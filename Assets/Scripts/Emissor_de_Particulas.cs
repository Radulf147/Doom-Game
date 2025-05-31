using UnityEngine;

public class Emissor_de_Particular : MonoBehaviour
{
    public GameObject particlePrefab;
    public Transform playerTransform;
    
    [Header("Configurações da Emissão")]
    public float emissionRate = 100f;
    public Vector3 emissionAreaSize = new Vector3(20f, 0f, 20f);
    public Vector3 offsetFromPlayer = new Vector3(0f, 10f, 0f);

    [Header("Configurações da Partícula de Chuva")]
    public float particleFallSpeed = 12f;
    public float particleLifetime = 1.5f;
    public float groundConsiderationOffset = 1.0f;

    [Header("Melhorias Visuais")]
    public float minScaleMultiplier = 0.7f;         // Mínimo multiplicador de escala
    public float maxScaleMultiplier = 1.3f;         // Máximo multiplicador de escala
    public float minSpeedMultiplier = 0.9f;         // Mínimo multiplicador de velocidade de queda
    public float maxSpeedMultiplier = 1.1f;         // Máximo multiplicador de velocidade de queda
    public Vector2 windInfluence = new Vector2(1.0f, 0.0f); // Influência do vento (X: lateral, Y: frente/trás relativo à visão padrão)

    private float timeSinceLastEmission = 0f;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Emissor de Partículas: Player Transform não foi definido! Desabilitando emissor.");
            enabled = false; 
        }
        if (particlePrefab == null)
        {
            Debug.LogError("Emissor de Partículas: Particle Prefab não foi definido! Desabilitando emissor.");
            enabled = false;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        transform.position = playerTransform.position + offsetFromPlayer;

        timeSinceLastEmission += Time.deltaTime;
        float timeBetweenEmissions = 1f / emissionRate;

        if (float.IsInfinity(timeBetweenEmissions) || float.IsNaN(timeBetweenEmissions))
        {
            if (emissionRate > 0) timeBetweenEmissions = 1f / emissionRate;
            else return;
        }
        
        while (timeSinceLastEmission >= timeBetweenEmissions)
        {
            EmitParticle();
            timeSinceLastEmission -= timeBetweenEmissions;
        }
    }

    void EmitParticle()
    {
        float spawnX = transform.position.x + Random.Range(-emissionAreaSize.x / 2, emissionAreaSize.x / 2);
        float spawnZ = transform.position.z + Random.Range(-emissionAreaSize.z / 2, emissionAreaSize.z / 2);
        Vector3 spawnPosition = new Vector3(spawnX, transform.position.y, spawnZ);

        // Aplica variação de velocidade e vento
        float currentFallSpeed = particleFallSpeed * Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
        Vector3 initialVelocity = new Vector3(
            windInfluence.x + Random.Range(-0.5f, 0.5f), // Vento em X + pequena variação aleatória
            -currentFallSpeed,
            windInfluence.y + Random.Range(-0.5f, 0.5f)  // Vento em Z + pequena variação aleatória
        );

        // Aplica variação de escala
        float scaleMultiplier = Random.Range(minScaleMultiplier, maxScaleMultiplier);

        GameObject particleInstance = Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
        // Se você quiser que as gotas já tenham uma leve inclinação com o vento:
        // if (initialVelocity.x != 0 || initialVelocity.z != 0) {
        //     particleInstance.transform.rotation = Quaternion.LookRotation(initialVelocity);
        // }
        // Nota: A rotação acima pode conflitar com o BillboardSprite se a textura não for simétrica.
        // O Billboard vai forçar a encarar a câmera. Para gotas de chuva (streaks), o billboard geralmente é suficiente.

        Particula particleScript = particleInstance.GetComponent<Particula>();

        if (particleScript != null)
        {
            float groundLevelForParticle = playerTransform.position.y - groundConsiderationOffset;
            // Passa o multiplicador de escala para a partícula
            particleScript.Initialize(spawnPosition, initialVelocity, particleLifetime, groundLevelForParticle, scaleMultiplier);
        }
        else
        {
            Debug.LogError("Prefab da partícula (" + particlePrefab.name + ") não contém o script Particula.cs!");
            Destroy(particleInstance); 
        }
    }
}