using UnityEngine;

public class ChamaController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    public CapsuleCollider2D ColliderFlame;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ColliderFlame = GetComponent<CapsuleCollider2D>();

        Destroy(this.gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Inimigos") || collision.CompareTag("Ground"))
        {
            animator.SetTrigger("Impacto");
            rb.bodyType = RigidbodyType2D.Static;
            ColliderFlame.enabled = false;
            Destroy(this.gameObject, 0.4f);
        }
    }
}
