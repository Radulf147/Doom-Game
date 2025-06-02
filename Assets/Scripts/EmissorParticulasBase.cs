using UnityEngine;

public abstract class EmissorParticulasBase : MonoBehaviour
{
    [Header("Configura��es Base do Emissor")]
    public GameObject particlePrefab;
    public Transform targetTransform; // Alvo que o emissor pode seguir (ex: jogador)
    public float emissionRate = 10f;
    public Vector3 offsetFromTarget = Vector3.zero; // Deslocamento em rela��o ao alvo

    protected float timeSinceLastEmission = 0f;

    protected virtual void Start()
    {
        if (particlePrefab == null)
        {
            Debug.LogError(GetType().Name + ": Particle Prefab n�o foi definido! Desabilitando emissor.");
            enabled = false;
            return;
        }
        // TargetTransform � opcional para alguns emissores, ent�o n�o desabilitamos se for nulo.
        // Classes filhas podem adicionar verifica��es mais estritas se necess�rio.
    }

    protected virtual void Update()
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + offsetFromTarget;
        }

        timeSinceLastEmission += Time.deltaTime;
        float timeBetweenEmissions = 1f / emissionRate;

        if (float.IsInfinity(timeBetweenEmissions) || float.IsNaN(timeBetweenEmissions))
        {
            if (emissionRate > 0) timeBetweenEmissions = 1f / emissionRate;
            else return; // Se emissionRate for 0 ou negativo, n�o emite.
        }

        while (timeSinceLastEmission >= timeBetweenEmissions)
        {
            EmitParticle();
            timeSinceLastEmission -= timeBetweenEmissions;
        }
    }

    protected abstract void EmitParticle();
}
