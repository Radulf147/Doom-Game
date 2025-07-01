using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    // O enum de tipo de munição e a quantidade foram removidos.
    // O script agora é um simples "gatilho" que avisa a arma para se reabastecer.

    [Header("Efeitos")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        // Procura por TODAS as armas que o jogador possa ter (se você tiver um sistema de inventário)
        // Por simplicidade, vamos procurar apenas um GunScript por enquanto.
        GunScript gun = other.GetComponentInChildren<GunScript>();

        if (gun != null)
        {
            // Chama o novo método AddAmmo(), que não precisa mais de parâmetros.
            // A própria arma saberá quanto de munição adicionar.
            gun.AddAmmo();

            // Toca efeitos sonoros e visuais
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // Destrói a caixa de munição após ser coletada.
            Destroy(gameObject);
        }
    }
}