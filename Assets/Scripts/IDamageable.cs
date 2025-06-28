// Adicionamos Vector3 para o ponto de impacto e a direção
using UnityEngine;

public interface IDamageable
{
    // Agora, qualquer objeto "danificável" DEVE ter um método TakeDamage
    // que aceita a quantidade, a posição e a direção do dano.
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection);
}