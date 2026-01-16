using Unity.Cinemachine;
using UnityEngine;

[System.Serializable]
public class CustomInspector
{
    public bool swapCameras = false;
    public bool panCameraContact = false;

    [HideInInspector] public CinemachineCamera cameraOnLeft;
    [HideInInspector] public CinemachineCamera cameraOnRight;

    [HideInInspector] public PanDirecao panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panDistance2 = 3f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirecao
{
    Up,
    Down,
    Left,
    Right,
    UpRight,
    UpLeft,
    DownRight,
    DownLeft
}

