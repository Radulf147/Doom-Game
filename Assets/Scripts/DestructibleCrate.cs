using UnityEngine; 
public class DestructibleCrate : MonoBehaviour 
{
    [Header("Destruction Visuals")] 
    [SerializeField] private GameObject destroyedCratePrefab;
    [SerializeField] private float explosionForce = 10f; 
    [SerializeField] private float explosionRadius = 2f;

    [Header("Effects")] 
    [SerializeField] private GameObject impactParticlePrefab;
    [SerializeField] private GameObject finalDestroyParticlePrefab;
    [SerializeField] private AudioClip destroySound;
    
    [Header("Item Drop")]
    [SerializeField] private GameObject healthPackDropPrefab; 
    
    private bool isDestroyed = false;
    
    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDestroyed) return;
        DestroyCrate(hitPoint);
    }

    private void DestroyCrate(Vector3 destroyPoint)
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (destroyedCratePrefab != null) 
        {
            GameObject shatteredCrate = Instantiate(destroyedCratePrefab, transform.position, transform.rotation);
            ApplyExplosionForceToPieces(shatteredCrate, destroyPoint);
        }
        
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        
        if (finalDestroyParticlePrefab != null)
        {
            GameObject finalFX = Instantiate(finalDestroyParticlePrefab, transform.position, Quaternion.identity);
            Destroy(finalFX, 3f);
        }

        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        // --- LÓGICA DE INSTANCIAÇÃO DO PACOTE DE VIDA MODIFICADA ---
        if (healthPackDropPrefab != null)
        {
            // 1. Define a posição X e Z para serem as mesmas da caixa, mas o Y para o valor exato de 0.348.
            Vector3 spawnPosition = new Vector3(transform.position.x, 0.348f, transform.position.z);

            // 2. Define a rotação exata desejada: -89 graus no eixo X.
            Quaternion spawnRotation = Quaternion.Euler(-89f, 0f, 0f);

            // 3. Instancia o pacote de vida na posição e rotação especificadas.
            Instantiate(healthPackDropPrefab, spawnPosition, spawnRotation);
        }
         
        Destroy(gameObject, 0.1f);
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