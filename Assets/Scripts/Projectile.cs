using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private int damage = 20; // Dano que este projétil causa
    [SerializeField] private float lifetime = 5f; // Tempo de vida do projétil antes de ser destruído (evita balas perdidas infinitas)
    [SerializeField] private GameObject hitEffectPrefab; // Prefab de efeito quando o projétil atinge algo (opcional, como poeira, faíscas)

    void Start()
    {
        Debug.Log("DEBUG: Projétil criado e ativo. Tempo de vida: " + lifetime + "s"); // NOVO DEBUG
        // Destrói o projétil após o tempo de vida, caso não colida com nada.
        Destroy(gameObject, lifetime);
    }

    // Chamado quando o projétil colide com algo.
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"DEBUG: Projétil colidiu com: {collision.gameObject.name} (Layer: {LayerMask.LayerToName(collision.gameObject.layer)}). Objeto do projétil: {gameObject.name}"); // NOVO DEBUG: Mais detalhes da colisão

        // Instancia um efeito de impacto no ponto de colisão (opcional)
        if (hitEffectPrefab != null)
        {
            // Pega o ponto de contato do impacto
            ContactPoint contact = collision.contacts[0];
            GameObject hitFX = Instantiate(hitEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
            Debug.Log("DEBUG: Efeito de impacto instanciado."); // NOVO DEBUG
            Destroy(hitFX, 1f); // Destrói o efeito após um tempo
        }

        // Tenta pegar o script DestructibleCrate do objeto atingido ou de um de seus pais
        DestructibleCrate crate = collision.gameObject.GetComponentInParent<DestructibleCrate>();
        if (crate != null)
        {
            Debug.Log($"DEBUG: Projétil atingiu um caixote destrutível: {crate.gameObject.name}. Chamando TakeDamage."); // NOVO DEBUG
            // Chama a função TakeDamage do caixote, passando o dano, o ponto de impacto e a normal
            // Passamos o ponto de contato do Rigidbody, que é mais preciso que o transform.position do projétil.
            crate.TakeDamage(damage, collision.contacts[0].point, collision.contacts[0].normal);
        }
        else
        {
            Debug.Log($"DEBUG: Projétil colidiu com {collision.gameObject.name}, mas não é um caixote destrutível ou o script não foi encontrado."); // NOVO DEBUG
        }

        // Destrói o próprio projétil após a colisão
        Destroy(gameObject);
        Debug.Log("DEBUG: Projétil se autodestruiu após colisão."); // NOVO DEBUG
    }
}