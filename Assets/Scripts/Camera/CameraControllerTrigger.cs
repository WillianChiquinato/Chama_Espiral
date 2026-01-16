using System.Collections;
using UnityEngine;

public class CameraControllerTrigger : MonoBehaviour
{
    public CustomInspector customInspectorObje;
    public bool PlayerDetect;

    [Header("Box Collider Mode")]
    [Tooltip("Usar o centro do BoxCollider2D como destino da câmera")]
    public bool useBoxColliderCenter = false;
    
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
        
        if (useBoxColliderCenter && boxCollider == null)
        {
            Debug.LogWarning($"[{gameObject.name}] useBoxColliderCenter está ativo mas não há BoxCollider2D no objeto!");
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

        PlayerDetect = true;

        if (!customInspectorObje.panCameraContact) return;

        var player = collision.GetComponent<PlayerController>();
        Vector2 pan;
        
        if (useBoxColliderCenter && boxCollider != null)
        {
            pan = GetPanToBoxCenter();
        }
        else
        {
            // Usa o sistema antigo de direções
            pan = GetPanValues(
                customInspectorObje.panDirection,
                customInspectorObje.panDistance,
                customInspectorObje.panDistance2
            );
        }

        CameraManager.instance.PanCamera(
            pan,
            customInspectorObje.panTime
        );
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerDetect = false;

        if (!customInspectorObje.panCameraContact) return;

        CameraManager.instance.PanCamera(
            Vector2.zero,
            customInspectorObje.panTime
        );
        
        // Reverte a expansão da câmera
        if (useBoxColliderCenter && cameraExpansionScale != 0f)
        {
            if (cameraTransitionCoroutine != null)
                StopCoroutine(cameraTransitionCoroutine);
            
            cameraTransitionCoroutine = StartCoroutine(TransitionCameraExpansion(
                originalOrthographicSize,
                originalTarget,
                customInspectorObje.panTime
            ));
        }
    }

    private Vector2 GetPanToBoxCenter()
    {
        // Calcula a posição do centro do BoxCollider2D no mundo
        Vector2 boxWorldCenter = (Vector2)transform.position + boxCollider.offset;
        Vector2 panToCenter = boxWorldCenter + centerOffset;
        
        // Aplica o multiplicador de expansão se necessário
        if (cameraExpansionScale != 0f)
        {
            if (cameraTransitionCoroutine != null)
                StopCoroutine(cameraTransitionCoroutine);
            
            cameraTransitionCoroutine = StartCoroutine(TransitionCameraExpansion(
                originalOrthographicSize * cameraExpansionScale,
                transform,
                customInspectorObje.panTime
            ));
        }
        
        Debug.Log($"[CameraTrigger] BoxCenter: {boxWorldCenter}, FinalPan: {panToCenter}");
        
        return panToCenter;
    }
    
    private IEnumerator TransitionCameraExpansion(float targetSize, Transform targetTransform, float duration)
    {
        var camera = GameManager.Instance.cinemachineCamera;
        float startSize = camera.Lens.OrthographicSize;
        float elapsed = 0f;
        
        camera.Target = new Unity.Cinemachine.CameraTarget { TrackingTarget = targetTransform };
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Lerp do tamanho da câmera
            camera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            
            yield return null;
        }
        
        // Garante valores finais
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
        Vector2 boxWorldCenter = (Vector2)transform.position + col.offset;
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