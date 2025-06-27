using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    public enum DetectionState { Idle, Detected }
    private DetectionState currentState = DetectionState.Idle;

    [Header("Referências")]
    public Transform player;
    private Animator animator; // Adicionado: Referência ao Animator

    [Header("Navegação e Detecção")]
    public float detectionRange = 10f;
    public float loseChaseRange = 15f;
    public LayerMask collidableLayers; // Para LoS
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
    public float attackCooldown = 2f; // Tempo em segundos entre ataques
    private float lastAttackTime;      // Para controlar o cooldown

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

        animator = GetComponent<Animator>(); // Obtém a referência ao Animator
        if (animator == null)
        {
            Debug.LogError("Animator não encontrado em " + gameObject.name, this);
            // Continua, mas sem controle de animação, ou pode desabilitar o script
        }
        
        // Resetar estado ao reabilitar (útil para pooling de objetos, por exemplo)
        isDead = false;
        currentHealth = maxHealth;
        // Garante que componentes desabilitados na morte sejam reabilitados se o objeto for reutilizado
        if(GetComponent<Collider>() != null) GetComponent<Collider>().enabled = true;
        agent.enabled = true;
        this.enabled = true;

        // Resetar parâmetros do Animator se o objeto for reutilizado
        if(animator != null)
        {
            animator.SetBool("IsDead", false); // Garante que não esteja na animação de morte
            animator.SetTrigger("ResetAttack"); // Se você tiver um trigger de reset, ou apenas não defina o "Attack"
            animator.SetBool("IsMoving", false); // Começa parado
            // Certifique-se que o Animator volta para o estado Idle/Walk padrão
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown; // Permite atacar imediatamente se as condições forem atendidas

        if (Application.isPlaying)
        {
            if (player == null)
            {
                Debug.LogError("O Transform do Player não foi atribuído!", this);
                enabled = false; // Desabilita o script se não houver player
                return;
            }
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>(); // Tentativa extra
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
        if (isDead) return; // Se estiver morto, não faz mais nada

        HandleDetectionAndChase();
        HandleAttacking();

        // Controlar animação de movimento
        if (animator != null)
        {
            // O NavMeshAgent.velocity.magnitude é uma boa forma de saber se ele está se movendo.
            // Se a magnitude da velocidade for maior que um pequeno limiar, ele está se movendo.
            bool isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;
            animator.SetBool("IsMoving", isMoving); // Define o parâmetro IsMoving no Animator
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

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " tomou " + amount + " de dano. Vida restante: " + currentHealth);

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
            agent.enabled = false;
        }
        
        // Desabilita o colisor para não interagir mais
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Para o script de visualização parar de tentar acessar
        var visualizer = GetComponent<RangeVisualizer>();
        if (visualizer != null)
        {
            visualizer.displayVisualizers = false;
        }
        
        // ** Chamada da animação de morte **
        if (animator != null)
        {
            animator.SetBool("IsDead", true); // Define o parâmetro IsDead como true
        }

        Destroy(gameObject, 5f); // Destroi o GameObject após 5 segundos
    }

    void HandleAttacking()
    {
        // Só tenta atacar se o player estiver detectado e o cooldown tiver passado
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
        // Se o inimigo está parado e não atacando, certifique-se de que a animação de ataque não está ativa
        // if (animator != null) {
        //     // Dependendo da sua configuração, você pode querer resetar o trigger de ataque aqui
        //     // ou confiar que a transição de saída do estado de ataque cuida disso.
        // }
    }

    void PerformAttack()
    {
        Debug.Log(gameObject.name + " ataca " + player.name + " causando " + attackDamage + " de dano!");

        // Tenta aplicar dano ao player
        PlayerHealth playerHealthComponent = player.GetComponent<PlayerHealth>();
        if (playerHealthComponent != null)
        {
            playerHealthComponent.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning("Player não possui o componente PlayerHealth para receber dano.");
        }

        // ** Chamada da animação de ataque **
        if (animator != null)
        {
            animator.SetTrigger("IsAttacking"); // Ativa o Trigger "IsAttacking" no Animator
        }
    }
}