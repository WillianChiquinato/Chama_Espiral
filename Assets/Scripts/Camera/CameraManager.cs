using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private Coroutine panCoroutine;

    [Header("Cinemachine")]
    private CinemachinePositionComposer composer;
    private CameraController cameraController;

    private Vector3 startOffset;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        composer = GameManager.Instance.cinemachineCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;
        cameraController = GameManager.Instance.cinemachineCamera.GetComponent<CameraController>();
        startOffset = composer.TargetOffset;
    }

    public void PanCamera(Vector2 panOffset, float panTime)
    {
        if (panCoroutine != null)
            StopCoroutine(panCoroutine);

        Vector3 targetOffset = startOffset + (Vector3)panOffset;
        panCoroutine = StartCoroutine(PanRoutine(targetOffset, panTime));
    }

    public void ResetPan(float panTime, Vector2 positionPlayer)
    {
        if (panCoroutine != null)
            StopCoroutine(panCoroutine);

        if (positionPlayer.x > 0)
        {
            startOffset = new Vector3(-startOffset.x, 0, 0f);
            panCoroutine = StartCoroutine(PanRoutine(startOffset, panTime));
            return;
        }

        startOffset = new Vector3(startOffset.x, 0, 0f);
        panCoroutine = StartCoroutine(PanRoutine(startOffset, panTime));
    }

    private IEnumerator PanRoutine(Vector3 targetOffset, float panTime)
    {
        Vector3 start = composer.TargetOffset;
        float elapsed = 0f;

        while (elapsed < panTime)
        {
            elapsed += Time.deltaTime;
            composer.TargetOffset = Vector3.Lerp(start, targetOffset, elapsed / panTime);
            yield return null;
        }

        composer.TargetOffset = targetOffset;
    }

    public void AttackCameraDirection(Vector2 attackDirection, float panTime)
    {
        if (panCoroutine != null)
            StopCoroutine(panCoroutine);

        Vector3 targetOffset = startOffset + (Vector3)(attackDirection * 6.5f);
        panCoroutine = StartCoroutine(PanRoutine(targetOffset, panTime));
    }

    public IEnumerator ResetAttackCamera(float panTime, Vector2 positionPlayer)
    {
        yield return new WaitForSeconds(1.7f);

        if (panCoroutine != null)
            StopCoroutine(panCoroutine);

        if (positionPlayer.x > 0)
        {
            startOffset = new Vector3(-startOffset.x, 0, 0f);
            panCoroutine = StartCoroutine(PanRoutine(startOffset, panTime));
            yield break;
        }

        startOffset = new Vector3(startOffset.x, 0, 0f);
        panCoroutine = StartCoroutine(PanRoutine(startOffset, panTime));
    }
}

