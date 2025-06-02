using UnityEngine; // Necessário para funcionalidades do Unity como GameObject, Transform, Input, etc.

public class GunScript : MonoBehaviour // Faz deste script um componente que pode ser anexado a um GameObject
{
    [Header("Gun Settings")] // Título no Inspector
    [SerializeField] private float fireRate = 0.5f; // Tempo de espera entre os tiros (em segundos). Ajuste no Inspector.

    [Header("Projectile Settings")] // Título no Inspector
    [SerializeField] private GameObject projectilePrefab; // Prefab do seu projétil (a "bala"). ARRASTE AQUI!
    [SerializeField] private Transform muzzlePoint; // Ponto de onde o projétil vai sair. ARRASTE AQUI!
    [SerializeField] private float projectileSpeed = 30f; // Velocidade de lançamento do projétil. Ajuste no Inspector.

    private float nextFireTime; // Variável interna para controlar o tempo do próximo tiro.

    // Update é chamado uma vez por frame. É onde detectamos o input do jogador.
    void Update()
    {
        // NOVO DEBUG: Verifica se o Update está rodando em cada frame.
        // Cuidado: Isso pode gerar muitos logs se você deixar ativo por muito tempo.
        // Debug.Log("GunScript Update rodando.");

        // NOVO DEBUG: Verifica se o botão de tiro (esquerdo do mouse) foi pressionado.
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("DEBUG: Botão Fire1 (esquerdo do mouse) Pressionado!");
        }

        // Verifica se o botão esquerdo do mouse (Fire1) foi pressionado
        // E se já passou o tempo mínimo para o próximo tiro (fireRate)
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            // NOVO DEBUG: Confirma que a condição para atirar foi atendida.
            Debug.Log("DEBUG: Condição de Disparo Atendida! Chamando ShootProjectile.");
            ShootProjectile(); // Chama a função para lançar o projétil
            nextFireTime = Time.time + fireRate; // Define o tempo para o próximo tiro
        }
    }

    /// <summary>
    /// Lança um projétil físico na cena.
    /// </summary>
    void ShootProjectile()
    {
        // NOVO DEBUG: Marca o início da função de disparo.
        Debug.Log("DEBUG: Função ShootProjectile Iniciada.");

        // Verificações de segurança para garantir que os Prefabs e o ponto de saída estão configurados.
        if (projectilePrefab == null)
        {
            // NOVO DEBUG (ERRO): Informa que o Prefab do projétil está faltando.
            Debug.LogError("ERRO DEBUG: Projectile Prefab não atribuído no GunScript! Arraste o prefab do projétil para o slot no Inspector.", this);
            return;
        }
        if (muzzlePoint == null)
        {
            // NOVO DEBUG (ERRO): Informa que o Muzzle Point está faltando.
            Debug.LogError("ERRO DEBUG: Muzzle Point não atribuído no GunScript! Crie um GameObject vazio na ponta da arma/câmera e arraste para o slot.", this);
            return;
        }

        // 1. Instancia o projétil
        // Cria uma nova instância do projectilePrefab na posição e rotação do muzzlePoint.
        GameObject newProjectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        // NOVO DEBUG: Confirma que o projétil foi instanciado.
        Debug.Log("DEBUG: Projétil instanciado com sucesso.");

        // 2. Aplica força ao projétil
        // Pega o componente Rigidbody do projétil recém-criado.
        Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Aplica uma força na direção "forward" do muzzlePoint (que é a direção para onde a arma/câmera está olhando).
            // ForceMode.VelocityChange: Aplica uma mudança instantânea de velocidade.
            rb.AddForce(muzzlePoint.forward * projectileSpeed, ForceMode.VelocityChange);
            // NOVO DEBUG: Confirma que a força foi aplicada.
            Debug.Log($"DEBUG: Força de {projectileSpeed} aplicada ao projétil. Direção: {muzzlePoint.forward}");
        }
        else
        {
            // NOVO DEBUG (ERRO): Informa que o Rigidbody está faltando no Prefab do projétil.
            Debug.LogError("ERRO DEBUG: O Prefab do Projétil não tem um componente Rigidbody! O projétil não se moverá. Verifique o Prefab.", newProjectile);
        }

        // Você pode adicionar mais efeitos visuais ou sonoros de disparo aqui (ex: flash de fogo, som do tiro).
    }
}