#if UNITY_EDITOR
using UnityEditor;
using Unity.Cinemachine;
using UnityEngine;

[CustomEditor(typeof(CameraControllerTrigger))]
public class CameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var trigger = (CameraControllerTrigger)target;

        DrawDefaultInspector();

        if (trigger.customInspectorObje.swapCameras)
        {
            trigger.customInspectorObje.cameraOnLeft =
                (CinemachineCamera)EditorGUILayout.ObjectField(
                    "Camera on Left",
                    trigger.customInspectorObje.cameraOnLeft,
                    typeof(CinemachineCamera),
                    true
                );

            trigger.customInspectorObje.cameraOnRight =
                (CinemachineCamera)EditorGUILayout.ObjectField(
                    "Camera on Right",
                    trigger.customInspectorObje.cameraOnRight,
                    typeof(CinemachineCamera),
                    true
                );
        }

        if (trigger.customInspectorObje.panCameraContact)
        {
            trigger.customInspectorObje.panDirection =
                (PanDirecao)EditorGUILayout.EnumPopup(
                    "Pan Direction",
                    trigger.customInspectorObje.panDirection
                );

            trigger.customInspectorObje.panDistance =
                EditorGUILayout.FloatField(
                    "Pan Distance",
                    trigger.customInspectorObje.panDistance
                );

            if (trigger.customInspectorObje.panDirection is
                PanDirecao.UpLeft or PanDirecao.UpRight or
                PanDirecao.DownLeft or PanDirecao.DownRight)
            {
                trigger.customInspectorObje.panDistance2 =
                    EditorGUILayout.FloatField(
                        "Pan Distance 2",
                        trigger.customInspectorObje.panDistance2
                    );
            }

            trigger.customInspectorObje.panTime =
                EditorGUILayout.FloatField(
                    "Pan Time",
                    trigger.customInspectorObje.panTime
                );
        }

        if (GUI.changed)
            EditorUtility.SetDirty(trigger);
    }
}
#endif
