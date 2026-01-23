using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingVariable), typeof(Damage))]
public class PlayerController : MonoBehaviour
{
    public string currentScene;

    [Header("Instances")]
    public float currentZRotation;
    public float maxSpeed = 7f;
    public float speed = 2f;
    public float acelerationSpeed;
    public float airSpeed = 7f;
    public Vector2 moveInput;

    public bool Atacar;
    [SerializeField]
    private int numeroDeAttcks;
    public bool Reset = false;
    public float ResetTimer;
    public float ResetTimerLimite;
    private int ataqueCounterAtual;

    //Healing
    [Header("Healing")]
    public bool healing = false;
    public float healingTimer;

    [HideInInspector]
    public HealthBar healthBar;

    //Variaveis
    [Header("Variaveis")]
    public Damage DamageScript;
    private bool isFlashingDamage = false;
    public TouchingVariable touching;
    public bool entrar;

    [HideInInspector]
    public Animator animacao;
    [HideInInspector]
    public Rigidbody2D rb;

    public SpriteRenderer spriteRenderer;
    public Material originalMaterial;

    //Jump
    [Header("Jump")]
    public bool jumpInput = false;
    public bool IsJumping;
    public float jumpImpulso = 20f;
    public float ContagemJump = 0.05f;

    public float CoyoteTime = 0.2f;
    public float coyoteTimeContador;

    public float jumpBufferTimer = 0.2f;
    public float jumpBufferContador;
    public bool jumpBufferFinal;

    public PlayerInput playerInput;

    [HideInInspector]
    public bool tempo;
    public float targetTimeScale = 0.3f;
    public float duration = 1f;
    public float elapsedTime = 0f;
    public int facingDirecao = 1;

    [Header("CameraFollowAnimation")]
    [SerializeField] private GameObject _cameraFollow;
    [SerializeField] public CameraController cameraController;

    [Header("Attack Instances")]
    public GameObject chamaPrefab;
    public Transform attackPoint;
    public float speedTarget = 0;
    public float currentSpeedTarget = 0;
    [SerializeField] private float minAttackForce = 7f;
    [SerializeField] private float maxAttackForce = 20f;
    private float attackTime;
    public bool isAttacking = false;

    [Header("Attack Direction")]
    public Vector2 playerDirectionTarget = Vector2.right;
    private Vector2 lastMoveDirection = Vector2.right;

    public float CurrentMoveSpeed
    {
        get
        {
            if (canMove)
            {
                playerInput.enabled = true;
                if (IsMoving && !touching.IsOnWall)
                {
                    if (touching.IsGrouded && speed <= maxSpeed)
                    {
                        speed += Time.deltaTime * acelerationSpeed;
                        return speed;
                    }
                    else
                    {
                        return airSpeed;
                    }
                }
                else
                {
                    //idle speed é 0;
                    speed = 2f;
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    public float attackCooldown
    {
        get
        {
            return animacao.GetFloat("attackCooldown");
        }
        private set
        {
            animacao.SetFloat("attackCooldown", Mathf.Max(value, 0));
        }
    }


    [SerializeField]
    private bool _IsMoving = false;

    public bool IsMoving
    {
        get
        {
            return _IsMoving;
        }
        set
        {
            _IsMoving = value;
            animacao.SetBool("IsMoving", value);
        }
    }

    public bool _IsRight = true;
    public bool IsRight
    {
        get
        {
            return _IsRight;
        }
        set
        {
            if (_IsRight != value)
            {
                // Flipa para a posicao oposta
                transform.localScale *= new Vector2(-1, 1);
            }
            _IsRight = value;
        }
    }

    public bool canMove
    {
        get
        {
            return animacao.GetBool("canMove");
        }
        set
        {
            animacao.SetBool("canMove", value);
        }
    }

    public bool IsAlive
    {
        get
        {
            return animacao.GetBool("IsAlive");
        }
        set
        {
            animacao.SetBool("IsAlive", value);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animacao = GetComponent<Animator>();
        touching = GetComponent<TouchingVariable>();
        DamageScript = GetComponent<Damage>();
        playerInput = GetComponent<PlayerInput>();
        healthBar = GameObject.FindFirstObjectByType<HealthBar>();
        _cameraFollow = GameObject.FindGameObjectWithTag("CameraFollow");

        cameraController = _cameraFollow.GetComponent<CameraController>();

        //saber qual cena o jogador esta.
        currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("Nome da cena atual: " + currentScene);
    }

    void Start()
    {
        // if (GameManager.instance != null)
        // {
        //     GameManager.instance.FullScreenDamageMaterial.SetFloat("_IsPulseActive", 0);
        // }

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;

        canMove = true;
        IsAlive = true;
    }

    private void Update()
    {
        if (!DamageScript.IsAlive)
        {
            return;
        }

        animacao.SetBool("isAttacking", isAttacking);

        if (!canMove)
        {
            playerInput.enabled = false;
        }

        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        if (tempo)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;
            Time.timeScale = Mathf.Lerp(1f, targetTimeScale, t);
        }

        if (Reset)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ataqueCounterAtual++;
                ResetTimer = 0f;
            }

            if (ataqueCounterAtual > 0)
            {
                ResetTimer += Time.deltaTime;

                if (ResetTimer >= ResetTimerLimite)
                {
                    Reset = false;
                    ResetTimer = 0;
                    ataqueCounterAtual = 0;
                }
            }
        }

        if (healing == true)
        {
            healingTimer -= Time.deltaTime;
            if (healingTimer <= 0)
            {
                healing = false;
                healingTimer = 2;
            }
        }

        //Shoot chama.
        if (!isAttacking) return;
        currentSpeedTarget += speedTarget * Time.deltaTime;
        currentSpeedTarget = Mathf.Clamp(currentSpeedTarget, minAttackForce, maxAttackForce);

        // if (!isFlashingDamage && DamageScript.Health != lastHealth)
        // {
        //     if (DamageScript.Health <= 1 && DamageScript.IsAlive)
        //     {
        //         GameManager.Instance.FullScreenDamageMaterial.SetFloat("_IsPulseActive", 1);
        //     }
        //     else
        //     {
        //         GameManager.Instance.FullScreenDamageMaterial.SetFloat("_IsPulseActive", 0);
        //     }

        //     lastHealth = DamageScript.Health;
        // }
    }

    private void FixedUpdate()
    {
        if (!DamageScript.VelocityLock)
        {

            rb.linearVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.linearVelocity.y);

            if (touching.IsGrouded && rb.linearVelocity.y <= 0f)
            {
                IsJumping = false;
                coyoteTimeContador = CoyoteTime;
            }
            else
            {
                coyoteTimeContador -= Time.deltaTime;
            }

            if (jumpBufferFinal)
            {
                jumpBufferContador -= Time.deltaTime;
                if (jumpBufferContador <= 0f)
                {
                    jumpBufferFinal = false;
                }
            }
        }

        if (jumpBufferFinal && touching.IsGrouded && jumpInput)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulso);
            jumpBufferFinal = false;
        }

        animacao.SetFloat("yVelocity", rb.linearVelocity.y);

        //utilizar depois para verificar especifica.
        // var currentStateInfo = animacao.GetCurrentAnimatorStateInfo(0);
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("Move Chamado..");
        moveInput = context.ReadValue<Vector2>();


        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput.normalized;
        }

        if (IsAlive)
        {
            IsMoving = moveInput != Vector2.zero;

            setDirection(moveInput);
        }
        else
        {
            IsMoving = false;
        }
    }

    private void setDirection(Vector2 moveInput)
    {
        facingDirecao = transform.localScale.x == 1 ? 1 : -1;

        if (moveInput.x > 0 && !IsRight)
        {
            IsRight = true;
            cameraController.ChamarTurn(IsRight);
        }
        else if (moveInput.x < 0 && IsRight)
        {
            IsRight = false;
            cameraController.ChamarTurn(IsRight);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump Chamado...");
        if (context.started && !isAttacking)
        {
            jumpInput = true;

            if (coyoteTimeContador > 0f || jumpBufferFinal)
            {
                Jump();
            }
        }

        if (!touching.IsGrouded)
        {
            jumpBufferFinal = true;
            jumpBufferContador = jumpBufferTimer;
        }

        if (context.canceled)
        {
            jumpInput = false;

            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Lerp(rb.linearVelocity.y, 0f, 0.5f));
            }
        }
    }

    private void Jump()
    {
        animacao.SetTrigger("jump");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulso);
        coyoteTimeContador = 0f;
        IsJumping = true;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!touching.IsGrouded) return;

        // Começou a segurar
        if (context.started)
        {
            isAttacking = true;
            currentSpeedTarget = minAttackForce;
        }

        // Soltou o botão
        if (context.canceled)
        {
            isAttacking = false;
            ResolveAttackDirection();
            animacao.SetTrigger("releaseAttack");
        }
    }

    private void ShootChama()
    {
        GameObject fire = Instantiate(chamaPrefab, attackPoint.position, Quaternion.identity);

        Rigidbody2D rb = fire.GetComponent<Rigidbody2D>();
        rb.linearVelocity = playerDirectionTarget * currentSpeedTarget;

        // Rotação visual
        float angle = Mathf.Atan2(playerDirectionTarget.y, playerDirectionTarget.x) * Mathf.Rad2Deg;
        
        //Correção de 180 graus para não ficar de cabeça para baixo
        fire.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void ResolveAttackDirection()
    {
        // Prioridade vertical
        if (Mathf.Abs(lastMoveDirection.y) > Mathf.Abs(lastMoveDirection.x))
        {
            playerDirectionTarget = lastMoveDirection.y > 0
                ? Vector2.up
                : Vector2.down;
        }
        else
        {
            playerDirectionTarget = lastMoveDirection.x > 0
                ? Vector2.right
                : Vector2.left;
        }

        if (!cameraController.shouldFlip) return;

        CameraManager.instance.AttackCameraDirection(playerDirectionTarget, 0.2f);
        StartCoroutine(CameraManager.instance.ResetAttackCamera(0.4f, transform.position));
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        //KNOCKBACK.
        // if (DamageScript.Health != 1)
        // {
        //     StartCoroutine(FlashPulseDamage());
        // }
        // else
        // {
        //     StopCoroutine(FlashPulseDamage());
        // }

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);
        // GameManager.instance.shakeCamera.ShakeHitDamage();

        if (DamageScript.IsAlive)
        {
            //JUMP
            IsJumping = false;
        }

        StartCoroutine(OnHitPlayer());
    }

    IEnumerator OnHitPlayer()
    {
        originalMaterial.SetFloat("_HitIntensity", 0.7f);
        yield return new WaitForSeconds(0.1f);
        originalMaterial.SetFloat("_HitIntensity", 0f);
    }

    // IEnumerator FlashPulseDamage()
    // {
    //     isFlashingDamage = true;
    //     GameManager.Instance.FullScreenDamageMaterial.SetFloat("_IsPulseActive", 1);
    //     yield return new WaitForSeconds(0.35f);
    //     isFlashingDamage = false;

    //     // Se o player ainda estiver com 1 de vida, mantemos o pulse ativo
    //     if (DamageScript.Health <= 1 && DamageScript.IsAlive)
    //     {
    //         GameManager.instance.FullScreenDamageMaterial.SetFloat("_IsPulseActive", 1);
    //     }
    //     else
    //     {
    //         GameManager.instance.FullScreenDamageMaterial.SetFloat("_IsPulseActive", 0);
    //     }
    // }


    // public void OnLook(InputAction.CallbackContext context)
    // {
    //     if (context.started && !arcoEffect && !OpenCaderno)
    //     {
    //         entrar = true;
    //     }
    //     else
    //     {
    //         entrar = false;
    //     }
    // }
}
