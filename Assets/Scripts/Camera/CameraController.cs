using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    private CinemachinePositionComposer composer;

    [Header("Flip Settings")]
    [SerializeField] private float flipTime = 0.5f;
    [SerializeField] private Vector3 offsetRight;

    private bool isFacingRight;
    private bool isFlipping;
    public bool shouldFlip = true;

    private CameraControllerTrigger[] triggers;

    private void Awake()
    {
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return null;

        composer = GameManager.Instance.cinemachineCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;

        triggers = FindObjectsByType<CameraControllerTrigger>(FindObjectsSortMode.None);

        isFacingRight = GameManager.Instance.player.IsRight;
        composer.TargetOffset = offsetRight;

        // Define o Follow corretamente
        GameManager.Instance.cinemachineCamera.Follow = GameManager.Instance.player.transform;
    }

    private void Update()
    {
        bool playerInsideTrigger = false;

        if (triggers != null)
        {
            foreach (var trigger in triggers)
            {
                if (trigger.PlayerDetect)
                {
                    playerInsideTrigger = true;
                    break;
                }
            }
        }

        shouldFlip = !playerInsideTrigger;
    }

    public void ChamarTurn()
    {
        if (!shouldFlip || isFlipping) return;
        StartCoroutine(FlipOffset());
    }

    IEnumerator FlipOffset()
    {
        isFlipping = true;

        float startX = composer.TargetOffset.x;
        isFacingRight = !isFacingRight;
        float endX = isFacingRight ? offsetRight.x : -offsetRight.x;

        float elapsed = 0f;

        while (elapsed < flipTime)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Lerp(startX, endX, elapsed / flipTime);

            composer.TargetOffset = new Vector3(
                x,
                composer.TargetOffset.y,
                composer.TargetOffset.z
            );

            yield return null;
        }

        isFlipping = false;
    }
}