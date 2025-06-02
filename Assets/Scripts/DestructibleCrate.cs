using UnityEngine; // Necessário para funcionalidades do Unity como GameObject, Rigidbody, etc.

public class DestructibleCrate : MonoBehaviour // Faz deste script um componente que pode ser anexado a um GameObject
{
    // --- CAMPOS (Variáveis) que você vai configurar no Inspector do Unity ---

    // REMOVEMOS maxHealth e currentHealth, pois a destruição é em um acerto.
    // REMOVEMOS damagedCrateMaterial, pois não haverá estado de "pouca vida".

    [Header("Destruction Visuals")] // Título no Inspector
    // Prefab do caixote quebrado em vários pedaços (crie este Prefab e ARRASTE AQUI).
    [SerializeField] private GameObject destroyedCratePrefab;
    [SerializeField] private float explosionForce = 10f; // Força com que os pedaços são lançados. Ajuste no Inspector.
    [SerializeField] private float explosionRadius = 2f; // Raio da explosão dos pedaços. Ajuste no Inspector.

    [Header("Effects")] // Título no Inspector
    // Partículas de impacto (tiro na madeira). ARRASTE AQUI.
    // Embora o caixote destrua no primeiro hit, este é para o efeito do impacto inicial.
    [SerializeField] private GameObject impactParticlePrefab;
    // Partículas da explosão final (muita poeira). ARRASTE AQUI.
    [SerializeField] private GameObject finalDestroyParticlePrefab;
    [SerializeField] private AudioClip destroySound; // Som de destruição (opcional). ARRASTE AQUI.

    private bool isDestroyed = false; // Flag para garantir que a destruição aconteça apenas uma vez.

    // --- MÉTODOS ---

    // Awake é chamado uma vez quando o GameObject é iniciado.
    void Awake()
    {
        Debug.Log("DEBUG: DestructibleCrate Awake chamado para " + gameObject.name); // NOVO DEBUG
        // Não precisamos mais inicializar a saúde aqui.
    }

    /// <summary>
    /// FUNÇÃO PRINCIPAL: É CHAMADA POR OUTROS SCRIPTS (ex: seu GunScript ou Projectile) para aplicar dano/iniciar a destruição.
    /// </summary>
    /// <param name="damage">Quantidade de dano (não é usada para saúde, mas pode ser para impacto visual).</param>
    /// <param name="hitPoint">Ponto de impacto do dano (onde a bala acertou).</param>
    /// <param name="hitNormal">Normal da superfície atingida (direção para onde o efeito deve ir).</param>
    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log("DEBUG: TakeDamage chamado em " + gameObject.name + " por " + (damage > 0 ? "um tiro" : "uma causa desconhecida")); // NOVO DEBUG
        // Se o caixote já está no processo de destruição, ignore mais chamadas.
        if (isDestroyed)
        {
            Debug.Log("DEBUG: Caixote já marcado como destruído, ignorando esta chamada de TakeDamage."); // NOVO DEBUG
            return;
        }

        Debug.Log($"DEBUG: Caixote {gameObject.name} foi atingido! Iniciando destruição.");

        // Instancia partículas de impacto (lascas, poeira) no ponto onde a bala atingiu.
        if (impactParticlePrefab != null)
        {
            GameObject impactFX = Instantiate(impactParticlePrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(impactFX, 1f); // Destrói o efeito após 1 segundo.
            Debug.Log("DEBUG: Partículas de impacto instanciadas."); // NOVO DEBUG
        } else {
            Debug.LogWarning("WARNING: Impact Particle Prefab não atribuído no DestructibleCrate!"); // NOVO DEBUG
        }

        // Chama a função de destruição imediatamente no primeiro acerto.
        DestroyCrate(hitPoint);
    }

    /// <summary>
    /// FUNÇÃO DE DESTRUIÇÃO: Responsável por toda a lógica de quebra visual e física.
    /// </summary>
    /// <param name="destroyPoint">O ponto de impacto final que acionou a destruição.</param>
    private void DestroyCrate(Vector3 destroyPoint)
    {
        Debug.Log("DEBUG: Tentando destruir caixote " + gameObject.name + " via DestroyCrate()."); // NOVO DEBUG

        // Outra verificação de segurança.
        if (isDestroyed)
        {
            Debug.Log("DEBUG: isDestroyed já é true, abortando DestroyCrate() para " + gameObject.name); // NOVO DEBUG
            return;
        }
        isDestroyed = true; // Marca que a destruição está acontecendo agora.

        Debug.Log($"DEBUG: Caixote {gameObject.name} está sendo destruído AGORA! (Iniciando efeitos de destruição)"); // NOVO DEBUG

        // 1. Lógica Visual: Substitui o caixote por pedaços.
        if (destroyedCratePrefab != null) // Se temos um Prefab do caixote quebrado em pedaços...
        {
            Debug.Log("DEBUG: destroyedCratePrefab atribuído. Instanciando pedaços."); // NOVO DEBUG
            // Instancia o Prefab dos pedaços na mesma posição e rotação do caixote original.
            GameObject shatteredCrate = Instantiate(destroyedCratePrefab, transform.position, transform.rotation);
            // Aplica uma força para espalhar os pedaços.
            ApplyExplosionForceToPieces(shatteredCrate, destroyPoint);
        }
        else // Se não temos um Prefab de pedaços (o caixote deve apenas sumir)...
        {
            Debug.LogWarning("WARNING: destroyedCratePrefab NÃO atribuído. Apenas escondendo MeshRenderer."); // NOVO DEBUG
            // Esconde o modelo e o collider.
            MeshRenderer crateMeshRenderer = GetComponent<MeshRenderer>(); // Pega o renderer aqui, se não for um campo direto.
            if (crateMeshRenderer != null) crateMeshRenderer.enabled = false;
            else Debug.LogError("ERRO DEBUG: MeshRenderer não encontrado no caixote para esconder!"); // NOVO DEBUG
        }

        // 2. Lógica de Colisão: Desativa o collider do caixote original para liberar a passagem.
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            Debug.Log("DEBUG: Collider do caixote desativado."); // NOVO DEBUG
        } else {
            Debug.LogWarning("WARNING: Collider não encontrado no caixote para desativar!"); // NOVO DEBUG
        }


        // 3. Efeitos: Instancia partículas de destruição final e toca o som.
        if (finalDestroyParticlePrefab != null)
        {
            GameObject finalFX = Instantiate(finalDestroyParticlePrefab, transform.position, Quaternion.identity);
            Destroy(finalFX, 3f); // Destrói o efeito após 3 segundos.
            Debug.Log("DEBUG: Partículas de destruição final instanciadas."); // NOVO DEBUG
        } else {
            Debug.LogWarning("WARNING: Final Destroy Particle Prefab não atribuído no DestructibleCrate!"); // NOVO DEBUG
        }

        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position); // Toca o som no local.
            Debug.Log("DEBUG: Som de destruição tocado."); // NOVO DEBUG
        } else {
            Debug.LogWarning("WARNING: Destroy Sound não atribuído no DestructibleCrate!"); // NOVO DEBUG
        }

        // 4. Remover o Caixote Original: Destrói o GameObject do caixote da cena.
        // Um pequeno atraso é bom para dar tempo aos efeitos visuais/sonoros e aos pedaços voarem.
        Debug.Log("DEBUG: Destruindo GameObject original do caixote em 0.1s."); // NOVO DEBUG
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// FUNÇÃO AUXILIAR: Aplica uma força explosiva aos pedaços do caixote estilhaçado.
    /// Ela é chamada por DestroyCrate().
    /// </summary>
    /// <param name="shatteredCrateParent">O GameObject pai que contém todos os pedaços quebrados.</param>
    /// <param name="origin">O ponto de origem da "explosão" (onde a bala atingiu o fatal blow).</param>
    private void ApplyExplosionForceToPieces(GameObject shatteredCrateParent, Vector3 origin)
    {
        Debug.Log("DEBUG: Aplicando força explosiva aos pedaços do caixote."); // NOVO DEBUG
        Rigidbody[] rbs = shatteredCrateParent.GetComponentsInChildren<Rigidbody>();
        if (rbs.Length == 0)
        {
            Debug.LogWarning("WARNING: Nenhum Rigidbody encontrado nos filhos do shatteredCrateParent. Pedaços podem não voar."); // NOVO DEBUG
        }
        foreach (Rigidbody rb in rbs)
        {
            // Adiciona uma força imediata a cada pedaço, fazendo-o voar.
            rb.AddExplosionForce(explosionForce, origin, explosionRadius, 1f, ForceMode.Impulse);
        }
    }
}