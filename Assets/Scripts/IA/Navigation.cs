using UnityEngine;
using UnityEngine.AI;

// --- MODIFICAÇÃO AQUI ---
// Adicionamos ", IDamageable" para que este script oficialmente implemente a interface.
// Agora, outros scripts podem procurar por "IDamageable" e encontrarão este componente.
public class EnemyNavigation : MonoBehaviour, IDamageable
{
    public enum DetectionState { Idle, Detected }
    private DetectionState currentState = DetectionState.Idle;

    [Header("Referências")]
    public Transform player;
    private Animator animator;

    [Header("Navegação e Detecção")]
    public float detectionRange = 10f;
    public float loseChaseRange = 15f;
    public LayerMask collidableLayers;
    public float aiEyeHeight = 1.0f;
    public float playerTargetHeight = 1.0f;

    [Header("Atributos da IA")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Configurações de Ataque")]
    public float attackDamage = 15f;
    public float attackRange = 2.5f; 
    public float attackAngle = 90f; 
    public float attackCooldown = 2f;
    private float lastAttackTime;

    private NavMeshAgent agent;

    public DetectionState CurrentState => currentState;
    public float DetectionRadius => detectionRange;
    public float LoseChaseRadius => loseChaseRange;
    public float AttackRadius => attackRange;
    public float AttackAngle => attackAngle;
    public bool IsDead => isDead;


    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent não encontrado em " + gameObject.name, this);
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator não encontrado em " + gameObject.name, this);
        }
        
        isDead = false;
        currentHealth = maxHealth;
        if(GetComponent<Collider>() != null) GetComponent<Collider>().enabled = true;
        agent.enabled = true;
        this.enabled = true;

        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;
            animator.SetBool("IsMoving", isMoving);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;

        if (Application.isPlaying)
        {
            if (player == null)
            {
                Debug.LogError("O Transform do Player não foi atribuído!", this);
                enabled = false;
                return;
            }
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
                if (agent == null)
                {
                    Debug.LogError("NavMeshAgent não encontrado no Start!", this);
                    enabled = false; return;
                }
            }
            agent.isStopped = true;
            currentState = DetectionState.Idle;
        }
    }

    void Update()
    {
        if (!Application.isPlaying || player == null || agent == null) return;
        if (isDead) return; // Se estiver morto, para de executar a lógica.

        HandleDetectionAndChase();
        HandleAttacking();

        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;
            animator.SetBool("IsMoving", isMoving);
        }
    }

    void HandleDetectionAndChase()
    {
        float distanceToPlayerRoot = Vector3.Distance(transform.position, player.position);

        if (currentState != DetectionState.Detected)
        {
            if (distanceToPlayerRoot <= detectionRange)
            {
                if (HasLineOfSightToPlayer())
                {
                    currentState = DetectionState.Detected;
                    if (agent.isOnNavMesh) agent.isStopped = false;
                    if (agent.isOnNavMesh) agent.SetDestination(player.position);
                }
                else
                {
                    currentState = DetectionState.Idle;
                    if (agent.isOnNavMesh && agent.isActiveAndEnabled && !agent.isStopped) agent.isStopped = true;
                }
            }
        }
        else // currentState == DetectionState.Detected
        {
            if (distanceToPlayerRoot > loseChaseRange)
            {
                currentState = DetectionState.Idle;
                if (agent.isOnNavMesh) agent.isStopped = true;
            }
            else
            {
                if (agent.isOnNavMesh) agent.SetDestination(player.position);
                if (agent.isOnNavMesh && agent.isStopped) agent.isStopped = false;
            }
        }
    }

    bool HasLineOfSightToPlayer()
    {
        Vector3 aiEyePosition = transform.position + (Vector3.up * aiEyeHeight);
        Vector3 playerTargetPosition = player.position + (Vector3.up * playerTargetHeight);
        Vector3 directionToPlayer = (playerTargetPosition - aiEyePosition).normalized;
        float distanceToPlayerTarget = Vector3.Distance(aiEyePosition, playerTargetPosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(aiEyePosition, directionToPlayer, out hitInfo, distanceToPlayerTarget, collidableLayers))
        {
            return (hitInfo.transform == player || hitInfo.transform.IsChildOf(player.transform));
        }
        return true;
    }

    // Este método já existe e satisfaz a interface IDamageable
    public void TakeDamage(float amount)
    {
        if (isDead) return; // Não pode tomar dano se já estiver morto.

        currentHealth -= amount;
        Debug.Log(gameObject.name + " tomou " + amount + " de dano. Vida restante: " + currentHealth);

        // A lógica de morte já está aqui!
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " morreu.");

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false; // Desabilita o NavMeshAgent
        }
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // Desabilita o colisor
        }

        var visualizer = GetComponent<RangeVisualizer>();
        if (visualizer != null)
        {
            visualizer.displayVisualizers = false;
        }

        // --- ADIÇÃO CRÍTICA AQUI: COMUNICAR AO ANIMATOR ---
        if (animator != null)
        {
            animator.SetBool("IsDead", true); // Ativa o parâmetro "IsDead" no Animator
            // Opcional: Se você quer parar todas as outras animações imediatamente e ir para a morte.
            // Se a sua transição de 'Any State' para 'Die' já cuida disso, não precisa de mais nada aqui.
        }
        // --------------------------------------------------
    }

    void HandleAttacking()
    {
        if (currentState != DetectionState.Detected || Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position);
            directionToPlayer.y = 0;

            float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);

            if (angle <= attackAngle / 2f)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    void PerformAttack()
    {
        Debug.Log(gameObject.name + " ataca " + player.name + " causando " + attackDamage + " de dano!");
        
        PlayerHealth playerHealthComponent = player.GetComponent<PlayerHealth>();
        if (playerHealthComponent != null)
        {
            playerHealthComponent.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning("Player não possui o componente PlayerHealth para receber dano.");
        }

        if (animator != null)
        {
            animator.SetTrigger("IsAttacking");
        }
    }
}