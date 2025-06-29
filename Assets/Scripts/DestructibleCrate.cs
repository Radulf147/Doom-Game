using UnityEngine;

// A classe agora implementa a nova e única interface IDamageable
public class DestructibleCrate : MonoBehaviour, IDamageable
{
    public enum LootType { None, Health, RevolverAmmo, ShotgunAmmo }

    [Header("Loot Settings")]
    [Tooltip("Qual tipo de item esta caixa deve dropar ao ser destruída?")]
    public LootType lootType = LootType.None;

    [Header("Loot Prefabs")]
    public GameObject healthDropPrefab;
    public GameObject revolverAmmoDropPrefab;
    public GameObject shotgunAmmoDropPrefab;

    [Header("Destruction Visuals")]
    [SerializeField] private GameObject destroyedCratePrefab;
    [SerializeField] private float explosionForce = 3f;
    [SerializeField] private float explosionRadius = 2f;

    [Header("Effects")]
    [SerializeField] private AudioClip destroySound;

    private bool isDestroyed = false;

    // --- MÉTODO TakeDamage MODIFICADO PARA CORRESPONDER À INTERFACE ---
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, HitType hitType = HitType.Unknown)
    {
        if (isDestroyed) return;

        // A caixa destrói com qualquer quantidade de dano, então apenas chamamos DestroyCrate.
        // O hitPoint pode ser usado para o efeito de explosão se desejado.
        DestroyCrate(hitPoint);
    }

    private void DestroyCrate(Vector3 destructionOrigin)
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (destroyedCratePrefab != null)
        {
            GameObject shatteredCrate = Instantiate(destroyedCratePrefab, transform.position, transform.rotation);
            // Usa a origem da destruição para a força da explosão
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
            Vector3 spawnPosition = new Vector3(transform.position.x, 0.348f, transform.position.z);
            Quaternion spawnRotation;
            if (lootType == LootType.Health)
            {
                spawnRotation = Quaternion.Euler(-89f, 0f, 0f);
            }
            else
            {
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