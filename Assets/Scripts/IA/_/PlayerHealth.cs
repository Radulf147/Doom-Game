using UnityEngine;
using UnityEngine.UI; // Se for usar UI para mostrar a vida

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    // Opcional: Referência a uma barra de vida na UI
    // public Image healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth); // Garante que a vida não fique negativa ou ultrapasse o máximo

        Debug.Log(transform.name + " (Player) tomou " + amount + " de dano. Vida atual: " + currentHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        // if (healthBar != null)
        // {
        //     healthBar.fillAmount = currentHealth / maxHealth;
        // }
    }

    void Die()
    {
        Debug.Log(transform.name + " (Player) morreu!");
        // Adicione aqui a lógica de morte do jogador (ex: tela de Game Over, reiniciar nível)
        // Por agora, podemos desabilitar o objeto ou um script de movimento
        // GetComponent<PlayerMovementScript>()?.enabled = false; // Substitua pelo nome do seu script de movimento
        // gameObject.SetActive(false); // Desativa o jogador
    }
}
