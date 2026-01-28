using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public LevelTransicaoController levelTransicaoController;

    [Header("References")]
    public PlayerController player;
    public CinemachineCamera cinemachineCamera;
    public CinemachineBasicMultiChannelPerlin noise;
    public ShakeController shakeController;

    [Header("Flame Settings")]
    public int flameMaxAmmo = 1;
    public int currentFlameAmmo;

    [Header("Passive Mode Settings")]
    public float delayBeforeTension = 4f;
    public float tensionDuration = 5f;
    public float zoomDuration = 5f;
    public float zoomDurationReturn = 1.5f;
    private float targetZoom = 5f;
    private float defaultZoom;
    private bool tensionRunning = false;

    [Header("Damage Flash Settings")]
    public Material FullScreenDamageMaterial;
    public List<Light2D> luzesRef = new List<Light2D>();
    private float[] initialLightIntensity;
    private bool isFlashingDamage = false;

    void Awake()
    {
        Instance = this;
        currentFlameAmmo = flameMaxAmmo;
    }

    void Start()
    {
        levelTransicaoController = FindFirstObjectByType<LevelTransicaoController>();
        shakeController = FindFirstObjectByType<ShakeController>();
        player = FindFirstObjectByType<PlayerController>();
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        noise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        defaultZoom = cinemachineCamera.Lens.OrthographicSize;
        StopTension();

        initialLightIntensity = new float[luzesRef.Count];
        for (int i = 0; i < luzesRef.Count; i++)
        {
            initialLightIntensity[i] = luzesRef[i].intensity;
        }
    }

    void Update()
    {
        if (player.DamageScript.IsAlive)
        {
            if (currentFlameAmmo <= 0 && !tensionRunning)
            {
                StartCoroutine(PassiveTensionRoutine());
            }

            if (currentFlameAmmo > 0 && tensionRunning)
            {
                StopAllCoroutines();
                StartCoroutine(PassiveStopTensionRoutine());
                ResetCamera();
            }
        }
        else
        {
            StopAllCoroutines();
        }
    }

    IEnumerator PassiveTensionRoutine()
    {
        tensionRunning = true;

        yield return new WaitForSeconds(delayBeforeTension);
        StartTension();

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / zoomDuration;
            t = Mathf.Pow(t, 2.5f);

            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(
                defaultZoom,
                targetZoom,
                t
            );

            for (int i = 0; i < luzesRef.Count; i++)
            {
                luzesRef[i].intensity = Mathf.Lerp(
                    initialLightIntensity[i],
                    0.06f,
                    t
                );
            }

            yield return null;
        }

        yield return new WaitForSeconds(tensionDuration);

        tensionRunning = false;
        StopTension();

        player.DamageScript.Hit(100, Vector2.zero, null);
        Debug.LogWarning("Player Died");
    }

    IEnumerator PassiveStopTensionRoutine()
    {
        float elapsed = 0f;
        while (elapsed < zoomDurationReturn)
        {
            elapsed += Time.deltaTime;

            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(
                targetZoom,
                defaultZoom,
                elapsed / zoomDurationReturn
            );

            for (int i = 0; i < luzesRef.Count; i++)
            {
                luzesRef[i].intensity = Mathf.Lerp(
                    0.2f,
                    initialLightIntensity[i],
                    elapsed / zoomDurationReturn
                );
            }

            yield return null;
        }

        StopTension();
        tensionRunning = false;
    }

    void StartTension()
    {
        noise.AmplitudeGain = 0.5f;
        noise.FrequencyGain = 8f;

        StartCoroutine(FlashPulseDamagePassive());
    }

    void StopTension()
    {
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }

    void ResetCamera()
    {
        StopTension();
        cinemachineCamera.Lens.OrthographicSize = defaultZoom;
        tensionRunning = false;
    }

    public IEnumerator FlashPulseDamagePassive()
    {
        isFlashingDamage = true;
        FullScreenDamageMaterial.SetFloat("_IsPulseActive", 1);
        yield return new WaitForSeconds(0.35f);
        isFlashingDamage = false;

        // Se o player ainda estiver com 1 de vida, mantemos o pulse ativo
        if ((player.DamageScript.Health <= 1 || tensionRunning) && player.DamageScript.IsAlive)
        {
            FullScreenDamageMaterial.SetFloat("_IsPulseActive", 1);
        }
        else
        {
            FullScreenDamageMaterial.SetFloat("_IsPulseActive", 0);
        }
    }

    public void PlayerDeath()
    {
        FullScreenDamageMaterial.SetFloat("_IsPulseActive", 0);

        for (int i = 0; i < luzesRef.Count; i++)
        {
            luzesRef[i].intensity = initialLightIntensity[i];
        }

        //Realod na msm cena.
        Invoke("ReloadScene", 2f);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        FullScreenDamageMaterial.SetFloat("_IsPulseActive", 0);
    }
#endif
}