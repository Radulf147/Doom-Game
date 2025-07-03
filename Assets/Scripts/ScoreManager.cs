using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para gerenciar cenas
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Configuração de Pontuação")]
    public int currentScore = 0;

    
    
    // A referência agora pode ser privada, pois vamos encontrá-la via código
    private TextMeshProUGUI scoreText; 

    [Tooltip("Pontos concedidos para cada tipo de morte.")]
    public Dictionary<HitType, int> scorePerKillType = new Dictionary<HitType, int>()
    {
        { HitType.Headshot, 10 },
        { HitType.BodyShot, 5 },
        { HitType.Melee, 8 },
        { HitType.Unknown, 0 }
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // --- INÍCIO DAS NOVAS ALTERAÇÕES ---

    // Este método é chamado quando o objeto se torna ativo.
    // É uma boa prática inscrever-se em eventos aqui.
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Este método é chamado quando o objeto é desativado.
    // É crucial se desinscrever para evitar erros.
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Esta função será executada toda vez que uma nova cena for carregada.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("ScoreManager detectou o carregamento da cena: " + scene.name);
        // Procura e configura o texto da UI na nova cena.
        FindAndAssignScoreText();
    }

    // Nova função para encontrar e configurar o texto da pontuação.
    void FindAndAssignScoreText()
    {
        // Procura por um GameObject com a tag "ScoreTextUI".
        GameObject scoreTextObject = GameObject.FindWithTag("ScoreTextUI");

        if (scoreTextObject != null)
        {
            scoreText = scoreTextObject.GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
            {
                Debug.Log("SUCESSO: Texto da pontuação encontrado na nova cena!");
                UpdateScoreUI(); // Atualiza a UI com a pontuação que já temos.
            }
            else
            {
                Debug.LogError("ERRO: O objeto com a tag 'ScoreTextUI' não tem um componente TextMeshProUGUI!");
            }
        }
        else
        {
            Debug.LogWarning("AVISO: Nenhum objeto com a tag 'ScoreTextUI' foi encontrado nesta cena.");
            scoreText = null; // Garante que a referência antiga seja limpa.
        }
    }

    // --- FIM DAS NOVAS ALTERAÇÕES ---

    // O Start original não é mais necessário para a UI, mas pode ser mantido se fizer outras coisas.
    void Start()
    {
        // A chamada para encontrar a UI já acontece no OnSceneLoaded,
        // então podemos chamar aqui também para garantir que funcione na primeira cena.
        FindAndAssignScoreText();
    }

    public void AddScoreForEnemyKill(HitType killType)
    {
        if (scorePerKillType.ContainsKey(killType))
        {
            int points = scorePerKillType[killType];
            currentScore += points;
            UpdateScoreUI();
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Pontos: " + currentScore;
        }
    }
}