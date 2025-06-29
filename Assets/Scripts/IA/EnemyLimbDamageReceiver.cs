// EnemyLimbDamageReceiver.cs
using UnityEngine;

public class EnemyLimbDamageReceiver : MonoBehaviour
{
    public EnemyNavigation enemyNavigationManager;
    [Tooltip("Multiplicador de dano para esta parte do corpo (ex: 2.0 para headshot, 0.5 para perna)")]
    public float damageMultiplier = 1f;
    [Tooltip("Tipo de acerto que este membro representa (Headshot, BodyShot)")]
    public HitType limbHitType = HitType.BodyShot; // Defina como BodyShot por padrão

    void Awake()
    {
        if (enemyNavigationManager == null)
        {
            enemyNavigationManager = GetComponentInParent<EnemyNavigation>();
            if (enemyNavigationManager == null)
            {
                Debug.LogError("EnemyNavigation não encontrado no pai do " + gameObject.name + ". Certifique-se de que o script está no GameObject principal do inimigo ou em um de seus pais.", this);
            }
        }
    }

    public void ReceiveHit(float baseDamage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (enemyNavigationManager == null) return;
        float finalDamage = baseDamage * damageMultiplier;
        enemyNavigationManager.TakeDamage(finalDamage, hitPoint, hitDirection, limbHitType);
    }
}