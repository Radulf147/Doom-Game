using UnityEngine;
using UnityEngine.UI; // Necessário para UI

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    // MODIFICADO: Mudamos o tipo da variável de 'Image' para 'Slider'
    public Slider healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        // Configura o valor máximo do slider no início
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
        }
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log(transform.name + " (Player) tomou " + amount + " de dano. Vida atual: " + currentHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        // MODIFICADO: Agora usamos 'healthBar.value' para atualizar o slider
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    /// <summary>
    /// Restaura uma quantidade de vida para o jogador.
    /// </summary>
    /// <param name="amount">A quantidade de vida a ser restaurada.</param>
    public void Heal(float amount)
    {
        currentHealth += amount;
        // Mathf.Clamp garante que a vida não ultrapasse o valor máximo.
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log(transform.name + " (Player) se curou em " + amount + ". Vida atual: " + currentHealth);
        UpdateHealthBar(); // Atualiza a UI da barra de vida.
    }

    void Die()
    {
        Debug.Log(transform.name + " (Player) morreu!");
        // Adicione aqui a lógica de morte do jogador
    }
}