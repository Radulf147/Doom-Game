using UnityEngine;
//using static UnityEditor.Rendering.CoreEditorDrawer<TData>;

public class EmissorChuva : EmissorParticulasBase
{
    [Header("Configura��es da Emiss�o de Chuva")]
    public Vector3 emissionAreaSize = new Vector3(20f, 0f, 20f);
    // O offsetFromTarget da classe base ser� usado para a altura da "nuvem" de chuva
    // Ex: new Vector3(0f, 10f, 0f)

    [Header("Configura��es da Part�cula de Chuva")]
    public float particleFallSpeed = 12f;
    public float particleLifetime = 1.5f;
    public float groundConsiderationOffset = 1.0f; // Qu�o abaixo do player/target o "ch�o" � considerado

    [Header("Melhorias Visuais da Chuva")]
    public float minScaleMultiplier = 0.7f;
    public float maxScaleMultiplier = 1.3f;
    public float minSpeedMultiplier = 0.9f;
    public float maxSpeedMultiplier = 1.1f;
    public Vector2 windInfluence = new Vector2(1.0f, 0.0f);

    protected override void Start()
    {
        base.Start(); // Chama o Start da classe base (verifica particlePrefab)

        // Verifica��o espec�fica do EmissorChuva
        if (targetTransform == null) // Para chuva, geralmente precisamos do jogador
        {
            Debug.LogError(GetType().Name + ": Player Transform (Target Transform) n�o foi definido! Desabilitando emissor.");
            enabled = false;
        }
    }

    protected override void EmitParticle()
    {
        if (particlePrefab == null) return;

        // Posi��o de spawn relativa � posi��o do emissor (que segue o targetTransform + offsetFromTarget)
        float spawnX = transform.position.x + Random.Range(-emissionAreaSize.x / 2, emissionAreaSize.x / 2);
        float spawnZ = transform.position.z + Random.Range(-emissionAreaSize.z / 2, emissionAreaSize.z / 2);
        Vector3 spawnPosition = new Vector3(spawnX, transform.position.y, spawnZ);

        float currentFallSpeed = particleFallSpeed * Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
        Vector3 initialVelocity = new Vector3(
            windInfluence.x + Random.Range(-0.5f, 0.5f),
            -currentFallSpeed,
            windInfluence.y + Random.Range(-0.5f, 0.5f)
        );

        float scaleMultiplier = Random.Range(minScaleMultiplier, maxScaleMultiplier);

        GameObject particleInstance = Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
        ParticulaChuva particleScript = particleInstance.GetComponent<ParticulaChuva>();

        if (particleScript != null)
        {
            // groundLevelForParticle precisa do targetTransform para saber a refer�ncia do "ch�o"
            float groundLevelForParticle = (targetTransform != null) ?
                                           targetTransform.position.y - groundConsiderationOffset :
                                           transform.position.y - offsetFromTarget.y - groundConsiderationOffset; // Fallback se n�o houver target

            particleScript.Initialize(spawnPosition, initialVelocity, particleLifetime, groundLevelForParticle, scaleMultiplier);
        }
        else
        {
            Debug.LogError("Prefab da part�cula (" + particlePrefab.name + ") n�o cont�m o script ParticulaChuva.cs!");
            Destroy(particleInstance);
        }
    }
}