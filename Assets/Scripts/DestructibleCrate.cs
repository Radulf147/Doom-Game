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
    private bool isDestroyed = false;
    void Awake()
    {
        Debug.Log("DEBUG: DestructibleCrate Awake chamado para " + gameObject.name); 
    }


    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log("DEBUG: TakeDamage chamado em " + gameObject.name + " por " + (damage > 0 ? "um tiro" : "uma causa desconhecida")); 
        if (isDestroyed)
        {
            Debug.Log("DEBUG: Caixote já marcado como destruído, ignorando esta chamada de TakeDamage."); 
            return;
        }

        Debug.Log($"DEBUG: Caixote {gameObject.name} foi atingido! Iniciando destruição.");

        if (impactParticlePrefab != null)
        {
            GameObject impactFX = Instantiate(impactParticlePrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(impactFX, 1f); 
            Debug.Log("DEBUG: Partículas de impacto instanciadas."); 
        } else {
            Debug.LogWarning("WARNING: Impact Particle Prefab não atribuído no DestructibleCrate!"); 
        }
        DestroyCrate(hitPoint);
    }

    private void DestroyCrate(Vector3 destroyPoint)
    {
        Debug.Log("DEBUG: Tentando destruir caixote " + gameObject.name + " via DestroyCrate()."); 
        if (isDestroyed)
        {
            Debug.Log("DEBUG: isDestroyed já é true, abortando DestroyCrate() para " + gameObject.name); 
            return;
        }
        isDestroyed = true;

        Debug.Log($"DEBUG: Caixote {gameObject.name} está sendo destruído AGORA! (Iniciando efeitos de destruição)");

        if (destroyedCratePrefab != null) 
        {
            Debug.Log("DEBUG: destroyedCratePrefab atribuído. Instanciando pedaços.");
            
            GameObject shatteredCrate = Instantiate(destroyedCratePrefab, transform.position, transform.rotation);
            ApplyExplosionForceToPieces(shatteredCrate, destroyPoint);
        }
        else
        {
            Debug.LogWarning("WARNING: destroyedCratePrefab NÃO atribuído. Apenas escondendo MeshRenderer."); 
            MeshRenderer crateMeshRenderer = GetComponent<MeshRenderer>();
            if (crateMeshRenderer != null) crateMeshRenderer.enabled = false;
            else Debug.LogError("ERRO DEBUG: MeshRenderer não encontrado no caixote para esconder!"); 
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            Debug.Log("DEBUG: Collider do caixote desativado.");
        } else {
            Debug.LogWarning("WARNING: Collider não encontrado no caixote para desativar!"); 
        }

        if (finalDestroyParticlePrefab != null)
        {
            GameObject finalFX = Instantiate(finalDestroyParticlePrefab, transform.position, Quaternion.identity);
            Destroy(finalFX, 3f);
            Debug.Log("DEBUG: Partículas de destruição final instanciadas."); 
        } else {
            Debug.LogWarning("WARNING: Final Destroy Particle Prefab não atribuído no DestructibleCrate!");
        }

        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
            Debug.Log("DEBUG: Som de destruição tocado."); 
        } else {
            Debug.LogWarning("WARNING: Destroy Sound não atribuído no DestructibleCrate!");
        }

        Debug.Log("DEBUG: Destruindo GameObject original do caixote em 0.1s."); 
        Destroy(gameObject, 0.1f);
    }

    private void ApplyExplosionForceToPieces(GameObject shatteredCrateParent, Vector3 origin)
    {
        Debug.Log("DEBUG: Aplicando força explosiva aos pedaços do caixote."); 
        Rigidbody[] rbs = shatteredCrateParent.GetComponentsInChildren<Rigidbody>();
        if (rbs.Length == 0)
        {
            Debug.LogWarning("WARNING: Nenhum Rigidbody encontrado nos filhos do shatteredCrateParent. Pedaços podem não voar."); 
        }
        foreach (Rigidbody rb in rbs)
        {
            rb.AddExplosionForce(explosionForce, origin, explosionRadius, 1f, ForceMode.Impulse);
        }
    }
}