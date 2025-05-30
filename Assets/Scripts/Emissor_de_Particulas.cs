using UnityEngine;

public class Emissor_de_Particular : MonoBehaviour // Certifique-se que o nome da classe é Emissor_de_Particular
{
    public GameObject particlePrefab;       // Arraste seu GotaChuvaPrefab aqui
    public Transform playerTransform;       // Arraste seu objeto Jogador aqui
    
    [Header("Configurações da Emissão")]
    public float emissionRate = 100f;       // Gotas por segundo
    public Vector3 emissionAreaSize = new Vector3(15f, 0f, 15f); // Área X,Z da chuva ao redor do jogador
    public Vector3 offsetFromPlayer = new Vector3(0f, 8f, 0f);  // Quão alto acima do jogador a chuva começa

    [Header("Configurações da Partícula")]
    public float particleFallSpeed = 10f;
    public float particleLifetime = 1.5f;     // Tempo para a gota sumir se não atingir o chão
    public float groundOffset = 0.5f;       // Quão abaixo dos pés do jogador consideramos "chão"

    private float timeSinceLastEmission = 0f;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Emissor de Partículas: Player Transform não foi definido!");
            enabled = false; // Desabilita o script se não houver jogador
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Faz o emissor seguir o jogador
        transform.position = playerTransform.position + offsetFromPlayer;

        timeSinceLastEmission += Time.deltaTime;
        float timeBetweenEmissions = 1f / emissionRate;

        while (timeSinceLastEmission >= timeBetweenEmissions)
        {
            EmitParticle();
            timeSinceLastEmission -= timeBetweenEmissions;
        }
    }

    void EmitParticle()
    {
        if (particlePrefab == null || playerTransform == null) return;

        // Posição X e Z aleatória dentro da área de emissão, relativa ao emissor (que está acima do jogador)
        float spawnX = transform.position.x + Random.Range(-emissionAreaSize.x / 2, emissionAreaSize.x / 2);
        float spawnZ = transform.position.z + Random.Range(-emissionAreaSize.z / 2, emissionAreaSize.z / 2);
        Vector3 spawnPosition = new Vector3(spawnX, transform.position.y, spawnZ);

        // Velocidade inicial (para baixo, com uma leve variação X,Z para simular vento/dispersão)
        float windX = Random.Range(-0.5f, 0.5f);
        float windZ = Random.Range(-0.5f, 0.5f);
        Vector3 initialVelocity = new Vector3(windX, -particleFallSpeed, windZ);

        GameObject particleInstance = Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
        Particula particleScript = particleInstance.GetComponent<Particula>();

        if (particleScript != null)
        {
            // Define o nível do chão um pouco abaixo dos pés do jogador
            float groundLevelForParticle = playerTransform.position.y - groundOffset;
            particleScript.Initialize(spawnPosition, initialVelocity, particleLifetime, groundLevelForParticle);
        }
        else
        {
            Debug.LogError("Prefab da partícula não contém o script Particula.cs!");
            Destroy(particleInstance); // Limpa se deu erro
        }
    }
}