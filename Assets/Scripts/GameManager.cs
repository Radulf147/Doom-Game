using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CharacterData personagemSelecionado;

    // A referência ainda é pública, mas agora será preenchida automaticamente.
    public GameObject telaDeGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- LÓGICA NOVA ---
            // "Inscreve" o método OnSceneLoaded para ser chamado toda vez que uma cena carregar.
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    // --- FUNÇÃO NOVA ---
    // Esta função é chamada automaticamente pela Unity quando uma nova cena termina de carregar.
   // Esta função é chamada automaticamente pela Unity quando uma nova cena termina de carregar.
void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // Verifica se a cena carregada é a nossa cena de jogo
    if (scene.name == "Fase1") // << Verifique se "Fase1" é o nome exato da sua cena!
    {
        Debug.Log("Cena 'Fase1' carregada. Procurando pelo Canvas da UI...");
        
        // 1. Encontra o Canvas que está sempre ATIVO.
        //    Verifique se o seu Canvas se chama "Canvas_GameOver".
        GameObject canvasPai = GameObject.Find("Canvas_GameOver");

        if (canvasPai != null)
        {
            // 2. Procura pelo painel DENTRO do Canvas.
            //    Isto funciona mesmo que o 'PainelGameOver' esteja desativado!
            Transform painelTransform = canvasPai.transform.Find("PainelGameOver");

            if (painelTransform != null)
            {
                // 3. Guarda a referência e garante que está desativado.
                telaDeGameOver = painelTransform.gameObject;
                telaDeGameOver.SetActive(false);
                Debug.Log("SUCESSO! Tela de Game Over encontrada através do Canvas pai!");
            }
            else
            {
                Debug.LogError("ERRO: O Canvas 'Canvas_GameOver' foi encontrado, mas não foi possível encontrar o filho 'PainelGameOver' dentro dele! Verifique se o painel está dentro do canvas e se o nome está correto.");
            }
        }
        else
        {
            Debug.LogError("ERRO CRÍTICO: Não foi possível encontrar o Canvas chamado 'Canvas_GameOver' na cena! Verifique o nome do seu Canvas na Hierarchy.");
        }
    }
}

    // Função que será chamada quando o jogador morrer
    public void ChamarGameOver()
    {
        if (telaDeGameOver != null)
        {
            telaDeGameOver.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    // Função para o botão "Tentar Novamente"
    public void TentarNovamente()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Função para o botão "Voltar ao Menu"
    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaInicial");
    }
    
    // --- LÓGICA NOVA ---
    // É uma boa prática "desinscrever" o evento quando o objeto for destruído para evitar erros.
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}