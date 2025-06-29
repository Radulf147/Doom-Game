using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CharacterData personagemSelecionado; // Sua variável de personagem

    // A referência para a UI de Game Over
    public GameObject telaDeGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // "Inscreve" um método para ser chamado toda vez que uma cena for carregada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Esta é a função que será chamada sempre que uma cena carregar
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Só executa a lógica se estivermos na cena do jogo
        if (scene.name == "Fase1") // << VERIFIQUE SE O NOME DA SUA CENA É "Fase1"
        {
            // 1. Procura pelo Canvas PAI, que deve estar sempre ATIVO.
            GameObject canvasPai = GameObject.Find("Canvas_GameOver");

            if (canvasPai != null)
            {
                // 2. Procura pelo painel FILHO dentro do Canvas.
                // Este comando funciona mesmo que o "PainelGameOver" esteja desativado.
                Transform painelTransform = canvasPai.transform.Find("PainelGameOver");

                if (painelTransform != null)
                {
                    // 3. Guarda a referência e garante que ele comece desativado.
                    telaDeGameOver = painelTransform.gameObject;
                    telaDeGameOver.SetActive(false);
                    Debug.Log("SUCESSO! Tela de Game Over foi encontrada e configurada!");
                }
                else
                {
                    Debug.LogError("ERRO: O Canvas 'Canvas_GameOver' foi encontrado, mas não foi possível encontrar o filho 'PainelGameOver' dentro dele! Verifique o nome do painel.");
                }
            }
            else
            {
                Debug.LogError("ERRO CRÍTICO: Não foi possível encontrar o Canvas chamado 'Canvas_GameOver'. Verifique o nome do seu Canvas na Hierarchy.");
            }
        }
    }

    // Função que mostra a tela de Game Over
    public void ChamarGameOver()
    {
        if (telaDeGameOver != null)
        {
            telaDeGameOver.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
             Debug.LogError("ChamarGameOver foi chamado, mas a referência para telaDeGameOver é NULA!");
        }
    }

    // Funções dos botões
    public void TentarNovamente()
    {
        if (telaDeGameOver != null)
        {
            telaDeGameOver.SetActive(false);
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaInicial");
    }

    // Desinscreve o evento quando o GameManager for destruído
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}