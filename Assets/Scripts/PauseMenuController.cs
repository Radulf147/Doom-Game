using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio; // Essencial para o AudioMixer

public class PauseMenuController : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("Referências da UI")]

    [Header("Referências Externas")]
    public PlayerFPController playerController;
    public GameObject painelPause;
    public Slider volumeSlider;
    public Slider sensibilidadeSlider;
    public AudioMixer audioMixer; // Vamos configurar isso no próximo passo

    void Start()
    {
        // Garante que o menu comece desativado
        painelPause.SetActive(false);

        // Carrega as configurações salvas quando o jogo começa
        CarregarConfiguracoes();
    }


    void Update()
    {
        // Verifica se a tecla ESC foi pressionada
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                RetomarJogo();
            }
            else
            {
                PausarJogo();
            }
        }
    }
    public void RetomarJogo()
{
    painelPause.SetActive(false);
    Time.timeScale = 1f; // Volta o tempo ao normal
    isPaused = false;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}
    public void PausarJogo()
    {
        painelPause.SetActive(true);
        Time.timeScale = 0f; // A mágica de pausar o jogo
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
// Adicione esta função completa dentro da sua classe PauseMenuController

public void SetSensibilidade(float sensibilidade)
{
    PlayerPrefs.SetFloat("SensibilidadeSalva", sensibilidade);

    if (playerController != null)
    {
        playerController.lookSpeed = sensibilidade;
        // NOVA LINHA PARA DEBUG:
        Debug.Log("Sensibilidade do jogador (lookSpeed) alterada para: " + sensibilidade); 
    }
    else
    {
        // NOVA LINHA PARA DEBUG:
        Debug.LogWarning("Tentou alterar a sensibilidade, mas a referência ao PlayerController é nula!"); 
    }
}

    public void VoltarAoMenu()
{
    // Garante que o jogo seja despausado antes de sair
    Time.timeScale = 1f; 
    isPaused = false;

    // Carrega a cena do menu principal
    SceneManager.LoadScene("TelaInicial");
}

    // ===== LÓGICA DAS CONFIGURAÇÕES =====

    public void SetVolume(float volume)
{
    AudioListener.volume = volume;
    PlayerPrefs.SetFloat("VolumeSalvo", volume);
    // NOVA LINHA PARA DEBUG:
    Debug.Log("Volume do AudioListener alterado para: " + volume); 
}


    void CarregarConfiguracoes()
    {
        // Carrega o volume ou usa 1 como padrão
        float volumeSalvo = PlayerPrefs.GetFloat("VolumeSalvo", 1f);
        volumeSlider.value = volumeSalvo;
        SetVolume(volumeSalvo); // Esta linha agora vai chamar a nova função e definir o AudioListener.volume

        // O código da sensibilidade continua o mesmo
        float sensibilidadeSalva = PlayerPrefs.GetFloat("SensibilidadeSalva", 1f);
        sensibilidadeSlider.value = sensibilidadeSalva;
        SetSensibilidade(sensibilidadeSalva);
    }
}