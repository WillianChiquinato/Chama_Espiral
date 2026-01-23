using System;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public PlayerController player;
    public CinemachineCamera cinemachineCamera;
    public ShakeController shakeController;

    [Header("Flame Settings and Variables")]
    public int flameMaxAmmo = 1;
    public int currentFlameAmmo;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        shakeController = FindFirstObjectByType<ShakeController>();

        currentFlameAmmo = flameMaxAmmo;
    }
}
