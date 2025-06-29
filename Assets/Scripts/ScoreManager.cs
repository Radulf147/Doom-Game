// ScoreManager.cs
using UnityEngine;
using TMPro; // Para exibir a pontuação na UI
using System.Collections.Generic; // Para o dicionário de pontos

public class ScoreManager : MonoBehaviour
{
    // Singleton Pattern: Garante que só há uma instância do ScoreManager
    public static ScoreManager Instance { get; private set; }

    [Header("Configuração de Pontuação")]
    public int currentScore = 0;
    public TextMeshProUGUI scoreText; // Arraste seu TextMeshProUGUI aqui no Inspector

    [Tooltip("Pontos concedidos para cada tipo de morte.")]
    public Dictionary<HitType, int> scorePerKillType = new Dictionary<HitType, int>()
    {
        { HitType.Headshot, 10 },
        { HitType.BodyShot, 5 },
        { HitType.Melee, 8 },
        { HitType.Unknown, 0 } // Caso não seja possível determinar o tipo
    };

    void Awake()
    {
        // Implementação do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destrói se já existir outra instância
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: Mantém o ScoreManager entre cenas
        }
    }

    void Start()
    {
        UpdateScoreUI();
    }

    // Método chamado pelos inimigos ao morrerem
    public void AddScoreForEnemyKill(HitType killType)
    {
        if (scorePerKillType.ContainsKey(killType))
        {
            int points = scorePerKillType[killType];
            currentScore += points;
            Debug.Log($"Pontos adicionados: {points} por morte tipo {killType}. Pontuação total: {currentScore}");
            UpdateScoreUI();
        }
        else
        {
            Debug.LogWarning($"Tipo de morte '{killType}' não encontrado no dicionário de pontuações.");
        }
    }

    // Método para adicionar pontos de outras fontes (se necessário)
    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log($"Pontos adicionados: {amount}. Pontuação total: {currentScore}");
        UpdateScoreUI();
    }

    // Atualiza o texto da UI com a pontuação atual
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Pontos: " + currentScore;
        }
    }
}