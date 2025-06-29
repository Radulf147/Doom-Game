// EnemyLimbDamageReceiver.cs
using UnityEngine;

public class EnemyLimbDamageReceiver : MonoBehaviour
{
    // Referência ao script EnemyNavigation do GameObject pai (o inimigo completo)
    public EnemyNavigation enemyNavigationManager;
    [Tooltip("Multiplicador de dano para esta parte do corpo (ex: 2.0 para headshot, 0.5 para perna)")]
    public float damageMultiplier = 1f; 

    void Awake()
    {
        // Tenta encontrar o EnemyNavigation no próprio GameObject ou em um pai
        if (enemyNavigationManager == null)
        {
            enemyNavigationManager = GetComponentInParent<EnemyNavigation>();
            if (enemyNavigationManager == null)
            {
                Debug.LogError("EnemyNavigation não encontrado no pai do " + gameObject.name + ". Certifique-se de que o script está no GameObject principal do inimigo ou em um de seus pais.", this);
            }
        }
    }

    // Este método será chamado pelo script de tiro do jogador (GunScript)
    // Ele recebe o dano base da arma e o ponto/direção do impacto.
    public void ReceiveHit(float baseDamage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (enemyNavigationManager == null) return;

        // Calcula o dano final usando o multiplicador específico desta parte do corpo
        float finalDamage = baseDamage * damageMultiplier;
        
        // Passa o dano final (já multiplicado) para o gerenciador de saúde do inimigo
        enemyNavigationManager.TakeDamage(finalDamage, hitPoint, hitDirection);
    }
}