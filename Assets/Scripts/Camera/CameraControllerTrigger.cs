using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraControllerTrigger : MonoBehaviour
{
    public CustomInspector customInspectorObje;
    private CinemachinePositionComposer composer;
    public bool PlayerDetect;

    [Header("Box Collider Mode")]
    [Tooltip("Usar o centro do BoxCollider2D como destino da câmera")]
    public bool useBoxColliderCenter = false;
    private Transform boxCenterTarget;

    [Tooltip("Multiplicador para expandir a câmera baseado no tamanho do BoxCollider2D")]
    [Range(0f, 2f)]
    public float cameraExpansionScale = 1f;

    [Tooltip("Offset adicional em relação ao centro do BoxCollider2D")]
    public Vector2 centerOffset = Vector2.zero;

    private BoxCollider2D boxCollider;
    private float originalOrthographicSize;
    private Transform originalTarget;
    private Coroutine cameraTransitionCoroutine;

    private void Awake()
    {
        PlayerDetect = false;
        boxCollider = GetComponent<BoxCollider2D>();

        if (useBoxColliderCenter && boxCollider != null)
        {
            GameObject target = new GameObject($"{name}_CameraCenter");
            target.transform.SetParent(transform);
            boxCenterTarget = target.transform;
        }
    }

    void Start()
    {
        // Guarda os valores originais da câmera
        if (GameManager.Instance != null && GameManager.Instance.cinemachineCamera != null)
        {
            originalOrthographicSize = GameManager.Instance.cinemachineCamera.Lens.OrthographicSize;
            originalTarget = GameManager.Instance.cinemachineCamera.Target.TrackingTarget;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (!customInspectorObje.panCameraContact) return;

        PlayerDetect = true;

        if (useBoxColliderCenter && boxCollider != null)
        {
            GetPanToBoxCenter();
            return;
        }

        Vector2 pan = GetPanValues(
            customInspectorObje.panDirection,
            customInspectorObje.panDistance,
            customInspectorObje.panDistance2
        );

        CameraManager.instance.PanCamera(pan, customInspectorObje.panTime);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (!customInspectorObje.panCameraContact) return;

        PlayerDetect = false;

        // 🔹 Volta para o target original
        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);

        cameraTransitionCoroutine = StartCoroutine(
            TransitionCameraExpansion(
                originalOrthographicSize,
                originalTarget,
                customInspectorObje.panTime
            )
        );

        if (!useBoxColliderCenter)
        {
            CameraManager.instance.PanCamera(Vector2.zero, customInspectorObje.panTime);
        }
    }

    private void GetPanToBoxCenter()
    {
        Vector2 center = (Vector2)boxCollider.bounds.center + centerOffset;

        boxCenterTarget.position = center;

        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);

        cameraTransitionCoroutine = StartCoroutine(
            TransitionCameraExpansion(
                originalOrthographicSize * cameraExpansionScale,
                boxCenterTarget,
                customInspectorObje.panTime
            )
        );
    }

    private IEnumerator TransitionCameraExpansion(float targetSize, Transform targetTransform, float duration)
    {
        var camera = GameManager.Instance.cinemachineCamera;
        CameraManager.instance.ResetPan(duration, GameManager.Instance.player.transform.localScale);
    
        // 🔹 Troca o target logo no início
        camera.Target = new CameraTarget
        {
            TrackingTarget = targetTransform
        };

        float startSize = camera.Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            camera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        camera.Lens.OrthographicSize = targetSize;
    }

    private Vector2 GetPanValues(PanDirecao dir, float d1, float d2)
    {
        return dir switch
        {
            PanDirecao.Up => new Vector2(0, d1),
            PanDirecao.Down => new Vector2(0, -d1),
            PanDirecao.Left => new Vector2(-d1, 0),
            PanDirecao.Right => new Vector2(d1, 0),

            PanDirecao.UpLeft => new Vector2(-d2, d1),
            PanDirecao.UpRight => new Vector2(d2, d1),
            PanDirecao.DownLeft => new Vector2(-d2, -d1),
            PanDirecao.DownRight => new Vector2(d2, -d1),

            _ => Vector2.zero
        };
    }

    private void OnDrawGizmosSelected()
    {
        // Visualização do sistema no Editor
        if (!useBoxColliderCenter) return;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        // Desenha o centro do BoxCollider2D
        Vector2 boxWorldCenter = col.bounds.center;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(boxWorldCenter, 0.3f);

        // Desenha o centro com offset aplicado
        Vector2 centerWithOffset = boxWorldCenter + centerOffset;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerWithOffset, 0.2f);

        // Desenha a área de expansão da câmera
        Vector2 expansion = col.size * cameraExpansionScale;
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireCube(centerWithOffset, expansion);
    }
}