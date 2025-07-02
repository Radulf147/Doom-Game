using UnityEngine;

public interface IDamageable
{
    // O método agora retorna um 'bool' (true se o alvo morreu, false se não)
    bool TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, HitType hitType);
}