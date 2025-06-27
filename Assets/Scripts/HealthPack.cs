using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Configurações")]
    public float healthToRestore = 25f; // Quantidade de vida que este pacote restaura

    [Header("Efeitos")]
    public GameObject pickupEffectPrefab; // Efeito de partícula ao coletar (opcional)
    public AudioClip pickupSound;         // Som ao coletar (opcional)

    // OnTriggerEnter é chamado quando outro Collider entra no trigger deste objeto.
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que entrou no trigger tem o script PlayerHealth.
        // Esta é a melhor maneira de identificar o jogador.
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // Se for o jogador, chama a função para curar.
            playerHealth.Heal(healthToRestore);
            
            // Toca o som de coleta no local do item
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Instancia um efeito visual de coleta
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // Destrói o objeto do pacote de vida após ser coletado.
            Destroy(gameObject);
        }
    }
}