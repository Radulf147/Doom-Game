// EnemyNavigation.cs
using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour, IDamageable
{
    public enum DetectionState { Idle, Detected }
    private DetectionState currentState = DetectionState.Idle;
    public static event Action<EnemyNavigation> OnEnemyDied;

    [Header("Referências")]
    private Transform player;
    private Animator animator;
    private EmissorSangue emissorSangue; // Referência para o emissor de sangue (garanta que este script exista)

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
    private HitType lastHitType = HitType.Unknown; // NOVO: Armazena o tipo do último acerto

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


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        emissorSangue = GetComponent<EmissorSangue>(); // Pega o componente do emissor de sangue

        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            player = playerGameObject.transform;
        }
        else
        {
            Debug.LogError("Objeto com a tag 'Player' não encontrado na cena! O inimigo não conseguirá se referenciar ao jogador.", this);
            enabled = false; // Desabilita o script se o jogador não for encontrado
        }
    }

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
        lastHitType = HitType.Unknown; // Reseta o tipo de acerto ao habilitar
        // Habilita os colisores, incluindo o do próprio inimigo (se existir)
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
        {
            col.enabled = true;
        }

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

    // Este método é chamado pelo EnemyLimbDamageReceiver ou por um ataque corpo a corpo do jogador
    public bool TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, HitType hitType)
{
    if (isDead) return false;

        currentHealth -= amount;
        lastHitType = hitType; // Armazena o tipo do último acerto que causou dano
        Debug.Log(gameObject.name + " tomou " + amount + " de dano. Vida restante: " + currentHealth + " (Tipo de Acerto: " + hitType + ")");

        // Se um emissor de sangue foi encontrado no Awake, chama o método para criar o efeito
        if (emissorSangue != null)
        {
            emissorSangue.EmitirSangueEmPonto(hitPoint, -hitDirection);
        }

        if (currentHealth <= 0)
        {
        currentHealth = 0;
        Die();
        return true; // Retorna true porque o inimigo morreu
        }

        return false; // Retorna false porque o inimigo sobreviveu
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log(gameObject.name + " morreu.");

        // Adiciona pontos com base no tipo de acerto que causou a morte
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScoreForEnemyKill(lastHitType);
        }
        else
        {
            Debug.LogWarning("ScoreManager.Instance não encontrado. Pontos não serão adicionados.");
        }

        OnEnemyDied?.Invoke(this);

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false; // Desabilita o NavMeshAgent
        }
        
        // Desabilita todos os colisores no inimigo e seus filhos
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
        {
            col.enabled = false;
        }

        var visualizer = GetComponent<RangeVisualizer>();
        if (visualizer != null)
        {
            visualizer.displayVisualizers = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("IsDead");
        }

        // Destrói o objeto após um tempo para a animação tocar
        Destroy(gameObject, 5f);
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
            // O inimigo atacando o jogador não precisa de dano localizado,
            // então TakeDamage pode ser chamado diretamente.
            // Se o PlayerHealth também usa IDamageable, ele deve ter um método TakeDamage similar.
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