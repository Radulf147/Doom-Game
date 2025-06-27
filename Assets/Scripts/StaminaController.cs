using UnityEngine;
using UnityEngine.UI; // Necessário para interagir com componentes de UI como Slider

public class StaminaController : MonoBehaviour
{
    [Header("Referências")]
    public Slider staminaSlider; // Arraste seu Slider da estamina aqui
    private PlayerFPController playerController;

    [Header("Configurações da Estamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 15f; // Estamina gasta por segundo ao correr
    public float staminaRegenRate = 10f; // Estamina recuperada por segundo
    public float exhaustedRegenRate = 5f; // Recuperação mais lenta quando esgotado
    public float runInputDelay = 0.25f; // Pequeno delay antes da regeneração começar

    [Header("Configurações da Exaustão")]
    public float exhaustionDuration = 3f; // Tempo que o jogador fica cansado
    public float exhaustedSpeedMultiplier = 0.5f; // 50% da velocidade normal

    // Variáveis de estado internas
    private float currentStamina;
    private bool isRunning = false;
    private bool isExhausted = false;
    private float timeSinceStoppedRunning = 0f;
    private float exhaustionTimer = 0f;

    void Start()
    {
        playerController = GetComponent<PlayerFPController>();
        if(playerController == null)
        {
            Debug.LogError("StaminaController: Script PlayerFPController não encontrado no mesmo GameObject!");
            enabled = false;
            return;
        }

        currentStamina = maxStamina;
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    void Update()
    {
        HandleExhaustion();
        HandleStamina();
        UpdateUI();
    }

    private void HandleExhaustion()
    {
        if (!isExhausted) return;

        exhaustionTimer -= Time.deltaTime;
        if (exhaustionTimer <= 0)
        {
            isExhausted = false;
            playerController.ApplySpeedModifier(1f); // Restaura a velocidade normal
            Debug.Log("Jogador não está mais exausto. Velocidade restaurada.");
        }
    }

    private void HandleStamina()
    {
        // Verifica se o jogador está tentando correr (tecla Shift Esquerdo). A permissão real é verificada no PlayerFPController.
        // O estado 'isExhausted' ainda previne o início da corrida aqui.
        isRunning = Input.GetKey(KeyCode.LeftShift) && !isExhausted;

        // Usamos o método IsMovingOnGround() do playerController.
        if (isRunning && playerController.IsMovingOnGround()) 
        {
            timeSinceStoppedRunning = 0f; 
            if (currentStamina > 0)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    isExhausted = true;
                    exhaustionTimer = exhaustionDuration;
                    playerController.ApplySpeedModifier(exhaustedSpeedMultiplier); 
                    Debug.Log("Estamina esgotada! Jogador exausto por " + exhaustionDuration + " segundos.");
                }
            }
        }
        else // Jogador não está correndo ou está parado
        {
            timeSinceStoppedRunning += Time.deltaTime;
            if (timeSinceStoppedRunning >= runInputDelay && currentStamina < maxStamina)
            {
                float currentRegenRate = isExhausted ? exhaustedRegenRate : staminaRegenRate;
                currentStamina += currentRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina); 
            }
        }
    }

    private void UpdateUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }

    /// <summary>
    /// Permite que outros scripts verifiquem se o jogador pode correr.
    /// </summary>
    /// <returns>Verdadeiro se o jogador não está exausto e tem estamina.</returns>
    public bool CanRun()
    {
        // O jogador só pode correr se não estiver no estado de exaustão E se tiver mais que zero de estamina.
        return !isExhausted && currentStamina > 0;
    }
}