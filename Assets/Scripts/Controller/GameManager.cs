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
    private bool tensionDeathTriggered = false;

    public float delayBeforeTension = 4f;
    public float tensionDurationPlayerDeath = 2.5f;
    public float zoomDuration = 5f;
    public float zoomDurationReturn = 1.5f;
    private float targetZoom = 5f;
    private float defaultZoom;
    private bool tensionRunning = false;
    [Range(0f, 1f)]
    public float tensionLevel = 0f;
    [Header("Torch Influence")]
    public float torchInfluence = 0f;
    public float torchFadeSpeed = 1.5f;

    [Header("Delay returned Flame Ammo")]
    public float delayTargetReturnFlameAmmo = 1.8f;
    public float delayReturnFlameAmmo = 0f;

    [Header("Damage Flash Settings")]
    public Material FullScreenDamageMaterial;
    public List<Light2D> luzesRef = new List<Light2D>();
    private float[] initialLightIntensity;

    private float currentZoom;
    private float[] currentLightIntensity;

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
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;

        initialLightIntensity = new float[luzesRef.Count];
        for (int i = 0; i < luzesRef.Count; i++)
        {
            initialLightIntensity[i] = luzesRef[i].intensity;
        }

        //Initial Currents.
        currentZoom = defaultZoom;
        currentLightIntensity = new float[luzesRef.Count];
        for (int i = 0; i < luzesRef.Count; i++)
        {
            currentLightIntensity[i] = initialLightIntensity[i];
        }
    }

    void Update()
    {
        if (!player.DamageScript.IsAlive)
            return;

        if (currentFlameAmmo <= 0)
        {
            delayReturnFlameAmmo += Time.deltaTime;

            if (delayReturnFlameAmmo >= delayBeforeTension)
            {
                float targetTension = 1f - torchInfluence;

                tensionLevel = Mathf.MoveTowards(
                    tensionLevel,
                    targetTension,
                    Time.deltaTime / zoomDuration
                );

                if (tensionLevel >= 0.7f)
                {
                    tensionDurationPlayerDeath -= Time.deltaTime;

                    if (tensionDurationPlayerDeath <= 0f && !tensionDeathTriggered)
                    {
                        tensionDeathTriggered = true;

                        player.DamageScript.Hit(100, Vector2.zero, null);

                        noise.AmplitudeGain = 0f;
                        noise.FrequencyGain = 0f;
                        tensionLevel = 0f;
                    }
                }
                else
                {
                    tensionDurationPlayerDeath = 2.5f;
                }

            }
        }
        else
        {
            tensionLevel = Mathf.MoveTowards(
                tensionLevel,
                0f,
                Time.deltaTime / zoomDurationReturn
            );
        }

        torchInfluence = Mathf.MoveTowards(torchInfluence, 0f, Time.deltaTime * torchFadeSpeed);

        ApplyTensionEffects();
    }

    void ApplyTensionEffects()
    {
        cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(
            defaultZoom,
            targetZoom,
            tensionLevel
        );

        for (int i = 0; i < luzesRef.Count; i++)
        {
            luzesRef[i].intensity = Mathf.Lerp(
                initialLightIntensity[i],
                0.06f,
                tensionLevel
            );
        }

        noise.AmplitudeGain = Mathf.Lerp(0f, 0.8f, tensionLevel);
        noise.FrequencyGain = Mathf.Lerp(0f, 10f, tensionLevel);

        FullScreenDamageMaterial.SetFloat("_IsPulseActive", tensionLevel > 0.1f ? 1 : 0);
    }

    IEnumerator StopTension()
    {
        // Reduz o shake gradualmente
        float elapsed = 0f;
        float duration = 0.8f;

        float initialAmplitude = noise.AmplitudeGain;
        float initialFrequency = noise.FrequencyGain;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            noise.AmplitudeGain = Mathf.Lerp(
                initialAmplitude,
                0f,
                t
            );
            noise.FrequencyGain = Mathf.Lerp(
                initialFrequency,
                0f,
                t
            );

            yield return null;
        }
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
        Invoke("ReloadScene", 1.8f);
    }

    public void ReloadScene()
    {
        levelTransicaoController.Transicao(player.currentScene);
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        FullScreenDamageMaterial.SetFloat("_IsPulseActive", 0);
    }
#endif
}