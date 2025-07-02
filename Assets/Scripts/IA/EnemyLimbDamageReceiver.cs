using UnityEngine;

public class EnemyLimbDamageReceiver : MonoBehaviour
{
    public EnemyNavigation mainHealthController;
    public float damageMultiplier = 2.0f;
    public HitType limbHitType = HitType.Headshot;

    // --- CORREÇÃO AQUI: O método agora retorna um 'bool' ---
    public bool ReceiveHit(float baseDamage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (mainHealthController != null)
        {
            float finalDamage = baseDamage * damageMultiplier;
            // Retorna o resultado da chamada TakeDamage do controlador principal
            return mainHealthController.TakeDamage(finalDamage, hitPoint, hitDirection, limbHitType);
        }
        
        Debug.LogError("O Controlador de Vida Principal não foi atribuído no LimbReceiver de " + gameObject.name);
        return false; // Retorna false se não houver controlador de vida
    }
}