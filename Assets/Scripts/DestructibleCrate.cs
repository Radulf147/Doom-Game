using UnityEngine;

// A classe continua implementando IDamageable
public class DestructibleCrate : MonoBehaviour, IDamageable
{
    // --- ENUM MODIFICADO ---
    // Agora temos apenas as opções de loot relevantes: Nada, Vida ou Munição.
    public enum LootType { None, Health, Ammo }

    [Header("Loot Settings")]
    [Tooltip("Qual tipo de item esta caixa deve dropar ao ser destruída?")]
    public LootType lootType = LootType.None;

    [Header("Loot Prefabs")]
    // --- VARIÁVEIS DE PREFAB MODIFICADAS ---
    // Consolidamos para apenas um prefab de vida e um de munição.
    public GameObject healthDropPrefab;
    public GameObject ammoDropPrefab;

    [Header("Destruction Visuals")]
    [SerializeField] private GameObject destroyedCratePrefab;
    [SerializeField] private float explosionForce = 3f;
    [SerializeField] private float explosionRadius = 2f;

    [Header("Effects")]
    [SerializeField] private AudioClip destroySound;

    private bool isDestroyed = false;

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, HitType hitType = HitType.Unknown)
    {
        if (isDestroyed) return;
        DestroyCrate(hitPoint);
    }

    private void DestroyCrate(Vector3 destructionOrigin)
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (destroyedCratePrefab != null)
        {
            GameObject shatteredCrate = Instantiate(destroyedCratePrefab, transform.position, transform.rotation);
            ApplyExplosionForceToPieces(shatteredCrate, destructionOrigin);
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

    private void DropLoot()
    {
        GameObject prefabToDrop = null;

        // --- SWITCH MODIFICADO ---
        // A lógica agora usa os novos tipos de loot simplificados.
        switch (lootType)
        {
            case LootType.Health:
                prefabToDrop = healthDropPrefab;
                break;
            case LootType.Ammo: // Novo caso para a munição universal.
                prefabToDrop = ammoDropPrefab;
                break;
            case LootType.None:
                return; // Sai do método se não houver loot.
        }

        if (prefabToDrop != null)
        {
            Vector3 spawnPosition = new Vector3(transform.position.x, 0.348f, transform.position.z);
            Quaternion spawnRotation;

            // A lógica de rotação especial para o kit médico continua a mesma.
            if (lootType == LootType.Health)
            {
                spawnRotation = Quaternion.Euler(-89f, 0f, 0f);
            }
            else // Isso agora se aplica à munição.
            {
                // A munição usará a rotação padrão do seu prefab (geralmente em pé).
                spawnRotation = Quaternion.identity;
            }
            Instantiate(prefabToDrop, spawnPosition, spawnRotation);
        }
    }

    private void ApplyExplosionForceToPieces(GameObject shatteredCrateParent, Vector3 origin)
    {
        Rigidbody[] rbs = shatteredCrateParent.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.AddExplosionForce(explosionForce, origin, explosionRadius, 1f, ForceMode.Impulse);
        }
    }
}