using UnityEngine;

public class EnemyLimbDamageReceiver : MonoBehaviour
{
    public EnemyNavigation mainHealthController;
    public float damageMultiplier = 2.0f;
    public HitType limbHitType = HitType.Headshot;

    public bool ReceiveHit(float baseDamage, Vector3 hitPoint, Vector3 hitDirection)
    {
        Debug.Log($"<color=#FFFF00><b>-- CHECKPOINT 3: ReceiveHit CHAMADO em '{gameObject.name}' --</b></color>"); // AMARELO

        if (mainHealthController != null)
        {
            Debug.Log($"<i>--> Delegando dano para: {mainHealthController.gameObject.name}</i>");
            float finalDamage = baseDamage * damageMultiplier;
            bool morteConfirmada = mainHealthController.TakeDamage(finalDamage, hitPoint, hitDirection, limbHitType);
            Debug.Log($"<color=#666666><i><-- CHECKPOINT 5: ReceiveHit recebeu de volta '{morteConfirmada}'. Retornando para GunScript...</i></color>"); // CINZA
            return morteConfirmada;
        }

        Debug.LogError("FALHA CRÍTICA! O Controlador de Vida Principal (mainHealthController) está NULO em " + gameObject.name);
        return false;
    }
}