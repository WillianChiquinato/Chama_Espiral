using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InteractObject : MonoBehaviour
{
    public int identity;
    public InteractType interactType;
    
    private Animator animator;
    public Light2D luzFlame;
    public ParticleSystem particleFlame;
    [HideInInspector] public float luzIntensityTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
        luzFlame = GetComponentInChildren<Light2D>();
        particleFlame = GetComponentInChildren<ParticleSystem>();

        particleFlame.Stop();
        luzIntensityTarget = luzFlame.intensity;
        luzFlame.intensity = 0f;
        if (interactType == null)
        {
            interactType = InteractType.None;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flame"))
        {
            InteractController.Instance.InteractToObject(identity, interactType);
        }
    }
}
