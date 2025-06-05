using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitEffectPrefab; // Efeito de faíscas/poeira

    [Header("Decal Settings")] // <<< NOVO HEADER
    public GameObject bulletHoleDecalPrefab; // <<< ADICIONE ESTA LINHA: Arraste seu prefab de decalque aqui
    public float decalOffset = 0.03f; // <<< ADICIONE ESTA LINHA: Pequeno offset para evitar Z-fighting

    void Start()
    {
        Debug.Log("DEBUG: Projétil criado e ativo. Tempo de vida: " + lifetime + "s");
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"DEBUG: Projétil colidiu com: {collision.gameObject.name} (Layer: {LayerMask.LayerToName(collision.gameObject.layer)}). Objeto do projétil: {gameObject.name}");

        ContactPoint contact = collision.contacts[0];
        Vector3 hitPoint = contact.point;
        Vector3 hitNormal = contact.normal;

        // Instancia efeito de impacto (faíscas, poeira)
        if (hitEffectPrefab != null)
        {
            GameObject hitFX = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            Debug.Log("DEBUG: Efeito de impacto instanciado.");
            Destroy(hitFX, 1f);
        }

        // --- LÓGICA PARA CRIAR DECALQUE DE MARCA DE BALA --- // <<< ADICIONADO AQUI
        if (bulletHoleDecalPrefab != null && collision.rigidbody == null) // Só cria decalques em objetos estáticos/cenário
        {
            // Cria o decalque um pouco à frente da superfície para evitar Z-fighting (briga de polígonos)
            Vector3 decalPosition = hitPoint + hitNormal * decalOffset;
            // Orienta o decalque para ficar "deitado" na superfície, olhando para fora dela
            // Quaternion decalRotation = Quaternion.LookRotation(hitNormal); // Faz o Z+ do decalque apontar na direção da normal
            // Para que a face do decalque fique visível, e assumindo que a face é o +Z do decalque,
            // queremos que ele olhe na direção OPOSTA da normal (para "ver" a superfície),
            // ou que o Z+ do decalque aponte junto com a normal e o decalque seja renderizado dos dois lados ou sua face visível seja o -Z.
            // A rotação mais comum é fazer o decalque "olhar" na direção da normal da superfície.
            // Se o seu Quad está com a face visível no seu eixo +Z, então LookRotation(hitNormal) o alinhará com a superfície.
            // Adicionamos uma rotação aleatória em torno da normal para variar a aparência dos buracos.
            Quaternion decalRotation = Quaternion.LookRotation(hitNormal) * Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            GameObject decalInstance = Instantiate(bulletHoleDecalPrefab, decalPosition, decalRotation);
            Debug.Log("DEBUG: Decalque de marca de bala instanciado.");

            // Opcional: Define o objeto atingido como pai do decalque, para que se mova com ele.
            // Cuidado com objetos com escala não uniforme, pode distorcer o decalque.
            decalInstance.transform.SetParent(collision.transform);

            Destroy(decalInstance, 10f); // Destrói o decalque depois de um tempo
        }
        // -------------------------------------------------- //

        DestructibleCrate crate = collision.gameObject.GetComponentInParent<DestructibleCrate>();
        if (crate != null)
        {
            Debug.Log($"DEBUG: Projétil atingiu um caixote destrutível: {crate.gameObject.name}. Chamando TakeDamage.");
            crate.TakeDamage(damage, hitPoint, hitNormal);
        }
        else
        {
            Debug.Log($"DEBUG: Projétil colidiu com {collision.gameObject.name}, mas não é um caixote destrutível ou o script não foi encontrado.");
        }

        Destroy(gameObject);
        Debug.Log("DEBUG: Projétil se autodestruiu após colisão.");
    }
}