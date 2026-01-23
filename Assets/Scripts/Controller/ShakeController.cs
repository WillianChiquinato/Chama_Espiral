using Unity.Cinemachine;
using UnityEngine;

public class ShakeController : MonoBehaviour
{
    [SerializeField]
    private CinemachineImpulseSource[] impulseSources;

    void Awake()
    {
        impulseSources = GetComponents<CinemachineImpulseSource>();
    }

    public void ShakeHitDamage()
    {
        impulseSources[0].GenerateImpulse();
    }

    public void ShakeAttackHitObject()
    {
        impulseSources[1].GenerateImpulse();
    }

    public void ShakeNoBullets()
    {
        impulseSources[2].GenerateImpulse();
    }
}
