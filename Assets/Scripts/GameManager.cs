using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CharacterData personagemSelecionado;

    // A referência para o painel da UI de Game Over
    private GameObject telaDeGameOver;

   private void Awake()
{
    if (Instance != null && Instance != this)
    {
        // Adicionamos um log para saber qual objeto está sendo destruído
        Debug.LogWarning("!!! GameManager DUPLICADO encontrado. O objeto '" + this.gameObject.name + "' será DESTRUÍDO. O GameManager original é o '" + Instance.gameObject.name + "'");
        Destroy(gameObject);
        return;
    }
    
    Instance = this;
    DontDestroyOnLoad(gameObject);
    // Adicionamos um log para saber qual objeto foi mantido
    Debug.Log(">>> GameManager Singleton CONFIGURADO com sucesso. O objeto mantido é o '" + this.gameObject.name + "'");

    SceneManager.sceneLoaded += OnSceneLoaded;
}
    // Esta função será chamada sempre que uma cena carregar
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // Vamos procurar a UI em qualquer cena que NÃO seja o menu principal.
    if (scene.name != "TelaInicial") // << VERIFIQUE SE O NOME DA SUA CENA DE MENU É "TelaInicial"
    {
        // Tenta encontrar a UI e configurá-la
        ConfigurarTelaGameOver();

        // ===== NOVA LÓGICA DE SALVAMENTO =====
        // Salva o nome da fase atual no PlayerPrefs.
        // A "Key" é como uma etiqueta para o nosso dado.
        PlayerPrefs.SetString("UltimaFaseSalva", scene.name);
        PlayerPrefs.Save(); // Garante que os dados sejam gravados em disco imediatamente.
        Debug.Log("JOGO SALVO! O jogador está na fase: " + scene.name);
    }
}

    // Nova função para organizar a busca pela UI
    void ConfigurarTelaGameOver()
    {
        // 1. Procura pelo Canvas PAI, que deve estar sempre ATIVO.
        GameObject canvasPai = GameObject.Find("Canvas_GameOver");

        if (canvasPai != null)
        {
            // 2. Procura pelo painel FILHO dentro do Canvas.
            // Este comando funciona mesmo que o painel esteja desativado.
            Transform painelTransform = canvasPai.transform.Find("PainelGameOver"); // << VERIFIQUE SE O NOME DO SEU PAINEL É "PainelGameOver"

            if (painelTransform != null)
            {
                // 3. Guarda a referência e garante que ele comece desativado.
                telaDeGameOver = painelTransform.gameObject;
                telaDeGameOver.SetActive(false);
                Debug.Log("SUCESSO! Tela de Game Over da cena '" + SceneManager.GetActiveScene().name + "' foi encontrada!");
            }
            else
            {
                Debug.LogError("ERRO: O Canvas 'Canvas_GameOver' foi encontrado, mas não foi possível encontrar o filho 'PainelGameOver' dentro dele! Verifique o nome do painel.");
            }
        }
        else
        {
            // Se não encontrar, a referência fica nula, mas o jogo não quebra.
            telaDeGameOver = null; 
            Debug.LogWarning("AVISO: Não foi possível encontrar o Canvas 'Canvas_GameOver' nesta cena. A tela de Game Over não funcionará aqui.");
        }
    }

    // Função que mostra a tela de Game Over
    public void ChamarGameOver()
    {
        if (telaDeGameOver != null)
        {
            telaDeGameOver.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
             Debug.LogError("ChamarGameOver foi chamado, mas a referência para a tela de Game Over não foi encontrada nesta cena!");
        }
    }

    // ===== FUNÇÕES DOS BOTÕES (COM A LÓGICA CORRIGIDA) =====

    public void TentarNovamente()
    {
        Debug.Log("FUNÇÃO TENTAR NOVAMENTE FOI CHAMADA!"); // <-- ADICIONE ESSA LINHA
        Time.timeScale = 1f; // Despausa o jogo
        // CORREÇÃO PRINCIPAL: Sempre carrega a "Fase1"
        SceneManager.LoadScene("Fase1");
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f; // Despausa o jogo
        SceneManager.LoadScene("TelaInicial"); // << VERIFIQUE SE O NOME DA SUA CENA DE MENU É "TelaInicial"
    }

    private void OnDestroy()
    {
        // É uma boa prática se desinscrever do evento para evitar erros.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}