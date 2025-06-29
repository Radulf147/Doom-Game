// IDamageable.cs
using UnityEngine;

public interface IDamageable
{
    // Adicionamos 'HitType hitType = HitType.Unknown' com um valor padrão,
    // para que chamadas existentes que não especificam o tipo ainda funcionem.
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, HitType hitType = HitType.Unknown);
}