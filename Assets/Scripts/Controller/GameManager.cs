using System;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController player;
    public CinemachineCamera cinemachineCamera;

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
    }
}
