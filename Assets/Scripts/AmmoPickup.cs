using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    // Usamos um enum para definir os tipos de munição de forma clara.
    public enum AmmoType { Revolver, Shotgun }

    [Header("Configurações")]
    public AmmoType ammoType; // Escolha o tipo de munição no Inspector
    public int ammoAmount = 12; // Quantidade de munição que esta caixa fornece

    [Header("Efeitos")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        // Procura pelo GunScript no jogador.
        // Assumimos que o jogador tem o GunScript na hierarquia dele (geralmente na câmera ou em um objeto filho).
        GunScript gun = other.GetComponentInChildren<GunScript>();

        if (gun != null)
        {
            // Se encontrou a arma, adiciona a munição
            gun.AddAmmo(ammoAmount, ammoType);

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