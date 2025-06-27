using UnityEngine;

// A classe agora implementa IDamageable para ser "danificável" pela sua arma.
public class DestructibleCrate : MonoBehaviour, IDamageable
{
    // Enum para definir os tipos de loot de forma clara no Inspector.
    public enum LootType { None, Health, RevolverAmmo, ShotgunAmmo }

    [Header("Loot Settings")]
    [Tooltip("Qual tipo de item esta caixa deve dropar ao ser destruída?")]
    public LootType lootType = LootType.None; // Escolha o que a caixa vai dropar.

    [Header("Loot Prefabs")]
    [Tooltip("Arraste o prefab do Kit Médico aqui.")]
    public GameObject healthDropPrefab;
    [Tooltip("Arraste o prefab da munição de Revólver aqui.")]
    public GameObject revolverAmmoDropPrefab;
    [Tooltip("Arraste o prefab da munição de Escopeta aqui.")]
    public GameObject shotgunAmmoDropPrefab;
    
    [Header("Destruction Visuals")] 
    [Tooltip("O prefab da caixa quebrada em pedaços.")]
    [SerializeField] private GameObject destroyedCratePrefab;
    [Tooltip("A força aplicada aos pedaços quando a caixa explode.")]
    [SerializeField] private float explosionForce = 3f;
    [Tooltip("O raio da explosão a partir do centro da caixa.")]
    [SerializeField] private float explosionRadius = 2f;

    [Header("Effects")] 
    [Tooltip("Som que toca quando a caixa é destruída.")]
    [SerializeField] private AudioClip destroySound;
    
    private bool isDestroyed = false;

    /// <summary>
    /// Este é o método chamado pela arma (via interface IDamageable) quando a caixa é atingida.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDestroyed) return;
        DestroyCrate();
    }

    /// <summary>
    /// Orquestra todos os efeitos de destruição e o drop de loot.
    /// </summary>
    private void DestroyCrate()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (destroyedCratePrefab != null) 
        {
            GameObject shatteredCrate = Instantiate(destroyedCratePrefab, transform.position, transform.rotation);
            ApplyExplosionForceToPieces(shatteredCrate, transform.position);
        }
        
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        DropLoot();
         
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// Verifica qual tipo de loot foi escolhido e instancia o prefab correspondente com a rotação correta.
    /// </summary>
    private void DropLoot()
    {
        GameObject prefabToDrop = null;

        // Um 'switch' para decidir qual prefab usar baseado na escolha do lootType.
        switch (lootType)
        {
            case LootType.Health:
                prefabToDrop = healthDropPrefab;
                break;
            case LootType.RevolverAmmo:
                prefabToDrop = revolverAmmoDropPrefab;
                break;
            case LootType.ShotgunAmmo:
                prefabToDrop = shotgunAmmoDropPrefab;
                break;
            case LootType.None:
                return;
        }
        
        if (prefabToDrop != null)
        {
            // Posição: X e Z da caixa, mas com Y fixo em 0.348.
            Vector3 spawnPosition = new Vector3(transform.position.x, 0.348f, transform.position.z);

            // --- LÓGICA DE ROTAÇÃO CONDICIONAL ---
            Quaternion spawnRotation; // Declara a variável de rotação

            // Verifica se o tipo de loot é Health
            if (lootType == LootType.Health)
            {
                // Se for, aplica a rotação específica de -89 no eixo X para o kit médico.
                spawnRotation = Quaternion.Euler(-89f, 0f, 0f);
            }
            else
            {
                // Para todos os outros tipos de loot (munição), usa a rotação padrão do prefab.
                spawnRotation = Quaternion.identity;
            }

            // Instancia o item de loot na posição e rotação calculadas.
            Instantiate(prefabToDrop, spawnPosition, spawnRotation);
        }
    }

    /// <summary>
    /// Aplica uma força de explosão a todos os Rigidbodies filhos de um objeto.
    /// </summary>
    private void ApplyExplosionForceToPieces(GameObject shatteredCrateParent, Vector3 origin)
    {
        Rigidbody[] rbs = shatteredCrateParent.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.AddExplosionForce(explosionForce, origin, explosionRadius, 1f, ForceMode.Impulse);
        }
    }
}