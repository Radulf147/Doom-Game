using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Configurações de Shield")]
    public float maxShield = 50f;
    public float currentShield;

    [Header("Referências da UI")]
    public Slider healthBar;
    public Slider shieldBar;
    public TextMeshProUGUI shieldText;

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = 0; 
        
        if (healthBar != null) healthBar.maxValue = maxHealth;
        if (shieldBar != null) shieldBar.maxValue = maxShield;
        
        UpdateHealthBar();
        UpdateShieldBar();
    }

    public void TakeDamage(float amount)
    {
        if (currentShield > 0)
        {
            float damageToShield = Mathf.Min(amount, currentShield);
            currentShield -= damageToShield;
            amount -= damageToShield;
        }

        if (amount > 0)
        {
            currentHealth -= amount;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        UpdateHealthBar();
        UpdateShieldBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddShield(float amount)
    {
        currentShield += amount;
        currentShield = Mathf.Min(currentShield, maxShield);
        UpdateShieldBar();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }
    
    void UpdateShieldBar()
    {
        if (shieldBar != null)
        {
            shieldBar.value = currentShield;
            shieldBar.gameObject.SetActive(currentShield > 0);
        }
        if (shieldText != null)
        {
            shieldText.text = currentShield.ToString("F0"); 
            shieldText.gameObject.SetActive(currentShield > 0);
        }
    }

    void Die()
    {
        Debug.Log("Player morreu!");
        
        // --- CORREÇÃO AQUI ---
        // Chamando o método com o nome correto que está no seu GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChamarGameOver();
        }
    }
}