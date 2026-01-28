using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InteractObject : MonoBehaviour
{
    public bool isActive = false;
    public int identity;
    public InteractType interactType;

    public float radius = 3f;
    public float influenceStrength = 1.2f;
    
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

    void Update()
    {
        animator.SetBool("isInteracting", isActive);
        float dist = Vector2.Distance(transform.position, GameManager.Instance.player.transform.position);

        if (isActive)
        {
            if (dist <= radius)
            {
                float t = 1f - (dist / radius);
                GameManager.Instance.torchInfluence =
                    Mathf.Max(GameManager.Instance.torchInfluence, t * influenceStrength);
            }
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
