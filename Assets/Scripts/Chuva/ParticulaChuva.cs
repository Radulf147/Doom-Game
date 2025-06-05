using UnityEngine;

public class ParticulaChuva : ParticulaBase
{
    private float groundHitY;

    // Sobrescrevemos Initialize para adicionar o parâmetro groundLevel
    public void Initialize(Vector3 startPosition, Vector3 initialVelocity, float life, float groundLevel, float scaleMultiplier)
    {
        // Chamamos a Initialize da classe base para reaproveitar a lógica comum
        base.Initialize(startPosition, initialVelocity, life, scaleMultiplier);
        this.groundHitY = groundLevel;
    }

    protected override void CheckLifetime()
    {
        // Chama a lógica base (verificação de idade vs lifetime)
        base.CheckLifetime();

        // Adiciona a verificação específica da chuva (nível do chão)
        // Precisamos verificar se o objeto ainda existe, pois base.CheckLifetime() pode tê-lo destruído
        if (this != null && gameObject != null && transform.position.y <= groundHitY)
        {
            Destroy(gameObject);
        }
    }
}