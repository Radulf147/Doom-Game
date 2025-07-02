using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class Wave
{
    public string waveName;
    public GameObject zombiePrefab;
    public int zombieCount;
    public float timeBeforeThisWave = 5f;
}

public class HordeManager : MonoBehaviour
{
    [Header("Configuração das Hordas")]
    public Wave[] waves;

    public static event System.Action OnAllHordesCompleted;
    
    [Header("Referências")]
    // --- CORREÇÃO AQUI: A variável 'spawnPoints' foi declarada novamente ---
    public Transform[] spawnPoints;
    public TextMeshProUGUI waveTextUI;
    public AudioClip hordeAnnouncementSound;
    private AudioSource audioSource;

    private int currentWaveIndex = -1;
    private List<EnemyNavigation> activeZombies = new List<EnemyNavigation>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        EnemyNavigation.OnEnemyDied += HandleEnemyDied;
    }

    void OnDisable()
    {
        EnemyNavigation.OnEnemyDied -= HandleEnemyDied;
    }

    void Start()
    {
        StartNextWave();
    }

    void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            UpdateWaveUI("Todas as hordas concluídas!");
            OnAllHordesCompleted?.Invoke();
            this.enabled = false;
            return;
        }
        
        StartCoroutine(WaveAnnouncementCoroutine());
    }

    IEnumerator WaveAnnouncementCoroutine()
    {
        Wave currentWave = waves[currentWaveIndex];
        string waveNumberText = GetWaveNumberAsText(currentWaveIndex + 1);
        UpdateWaveUI($"A {waveNumberText} horda está vindo...");

        if (hordeAnnouncementSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hordeAnnouncementSound);
        }

        yield return new WaitForSeconds(8f);

        UpdateWaveUI("");
        SpawnWave(currentWave);
    }

    // --- MÉTODO SpawnWave CORRIGIDO ---
    void SpawnWave(Wave wave)
    {
        Debug.Log("SPAWN WAVE: Criando " + wave.zombieCount + " zumbis.");
        activeZombies.Clear();

        // A verificação agora é feita uma vez, antes do loop, e usa 'return'
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("ERRO DE SPAWN: Nenhum ponto de spawn foi atribuído no HordeManager!");
            return; // 'return' é o comando correto para sair de um método 'void'
        }

        for (int i = 0; i < wave.zombieCount; i++)
        {
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            GameObject zombieInstance = Instantiate(wave.zombiePrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
            EnemyNavigation enemyScript = zombieInstance.GetComponent<EnemyNavigation>();

            if (enemyScript != null)
            {
                activeZombies.Add(enemyScript);
            }
        }
    }

    void HandleEnemyDied(EnemyNavigation deadEnemy)
    {
        if (activeZombies.Contains(deadEnemy))
        {
            activeZombies.Remove(deadEnemy);
        }

        if (activeZombies.Count == 0 && currentWaveIndex < waves.Length)
        {
            StartNextWave();
        }
    }

    void UpdateWaveUI(string message)
    {
        if (waveTextUI != null)
        {
            waveTextUI.text = message;
        }
    }
    
    private string GetWaveNumberAsText(int number)
    {
        switch (number)
        {
            case 1: return "primeira";
            case 2: return "segunda";
            case 3: return "terceira";
            default: return number.ToString() + "ª";
        }
    }
}