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
    private EmissorSangue emissorSangue;

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
    private HitType lastHitType = HitType.Unknown;

    [Header("Configurações de Ataque")]
    public float attackDamage = 15f;
    public float attackRange = 2.5f; 
    public float attackAngle = 90f; 
    public float attackCooldown = 2f;
    private float lastAttackTime;

    private NavMeshAgent agent;

    // --- PROPRIEDADES ADICIONADAS DE VOLTA AQUI ---
    // Estas propriedades permitem que outros scripts leiam os valores do inimigo de forma segura.
    public DetectionState CurrentState => currentState;
    public float DetectionRadius => detectionRange;
    public float LoseChaseRadius => loseChaseRange;
    public float AttackRadius => attackRange;
    public float AttackAngle => attackAngle;
    public bool IsDead => isDead;
    // --- FIM DA ADIÇÃO ---

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        emissorSangue = GetComponent<EmissorSangue>();

        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            player = playerGameObject.transform;
        }
        else
        {
            Debug.LogError("Objeto com a tag 'Player' não encontrado! O inimigo não funcionará.", this);
            enabled = false;
        }

        if (agent != null)
        {
            agent.stoppingDistance = attackRange * 0.8f;
        }
    }

    // O resto do seu script (OnEnable, Start, Update, TakeDamage, Die, etc.) permanece o mesmo.
    // Incluído abaixo para que você possa copiar e colar tudo de uma vez.
    #region Métodos Inalterados
    void OnEnable()
    {
        isDead = false;
        currentHealth = maxHealth;
        lastHitType = HitType.Unknown;
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
        {
            col.enabled = true;
        }
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = true;
            agent.avoidancePriority = UnityEngine.Random.Range(40, 60);
        }
    }
    void Start()
    {
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;
        currentState = DetectionState.Idle;
    }
    void Update()
    {
        if (isDead || player == null || agent == null || !agent.enabled) return;
        HandleDetectionAndChase();
        HandleAttacking();
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f && agent.hasPath;
            animator.SetBool("IsMoving", isMoving);
        }
    }
    public bool TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, HitType hitType)
    {
        if (isDead) return false;
        currentHealth -= amount;
        lastHitType = hitType;
        if (emissorSangue != null)
        {
            emissorSangue.EmitirSangueEmPonto(hitPoint, -hitDirection);
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return true;
        }
        return false;
    }
    void Die()
    {
        if (isDead) return;
        isDead = true;
        OnEnemyDied?.Invoke(this);
        if (agent != null) agent.enabled = false;
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders) col.enabled = false;
        if (animator != null) animator.SetTrigger("IsDead");
        Destroy(gameObject, 5f);
    }
    void HandleDetectionAndChase()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (currentState == DetectionState.Detected)
        {
            if (distanceToPlayer > loseChaseRange)
            {
                currentState = DetectionState.Idle;
            }
            else
            {
                if (agent.isOnNavMesh) agent.SetDestination(player.position);
            }
        }
        else // Idle
        {
            if (distanceToPlayer <= detectionRange)
            {
                currentState = DetectionState.Detected;
            }
        }
        if (agent.isOnNavMesh)
        {
            if (agent.remainingDistance > agent.stoppingDistance)
            {
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }
    void HandleAttacking()
    {
        if (currentState != DetectionState.Detected || Time.time < lastAttackTime + attackCooldown || (agent != null && !agent.isStopped)) return;
        
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position);
            directionToPlayer.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
            
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
        if(animator != null) animator.SetTrigger("IsAttacking");
        PlayerHealth playerHealthComponent = player.GetComponent<PlayerHealth>();
        if (playerHealthComponent != null) playerHealthComponent.TakeDamage(attackDamage);
    }
    #endregion
}