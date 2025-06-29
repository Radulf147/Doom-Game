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

    [Header("Referências")]
    public Transform[] spawnPoints;
    public TextMeshProUGUI waveTextUI;

    private int currentWaveIndex = -1;
    private List<EnemyNavigation> activeZombies = new List<EnemyNavigation>();

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

        // --- DIAGNÓSTICO 3 ---
        Debug.Log("StartNextWave foi chamado. Tentando iniciar a horda de índice: " + currentWaveIndex);

        if (currentWaveIndex >= waves.Length)
        {
            UpdateWaveUI("Todas as hordas concluídas!");
            Debug.Log("Parabéns! Você sobreviveu a todas as hordas.");
            this.enabled = false; // Desabilita o manager
            return;
        }

        StartCoroutine(WaveCoroutine());
    }

    IEnumerator WaveCoroutine()
    {
        Wave currentWave = waves[currentWaveIndex];

        // --- DIAGNÓSTICO 4 ---
        Debug.Log("Corrotina para '" + currentWave.waveName + "' iniciada. Esperando " + currentWave.timeBeforeThisWave + " segundos.");

        UpdateWaveUI("Próxima horda em " + currentWave.timeBeforeThisWave + "s...");

        yield return new WaitForSeconds(currentWave.timeBeforeThisWave);

        UpdateWaveUI(currentWave.waveName);
        SpawnWave(currentWave);
    }

    void SpawnWave(Wave wave)
    {
        Debug.Log("SPAWN WAVE: Criando " + wave.zombieCount + " zumbis.");
        activeZombies.Clear();

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("ERRO DE SPAWN: Nenhum ponto de spawn foi atribuído no HordeManager!");
            return;
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
        Debug.Log("SPAWN WAVE: " + activeZombies.Count + " zumbis adicionados à lista de ativos.");
    }

    void HandleEnemyDied(EnemyNavigation deadEnemy)
    {
        // --- DIAGNÓSTICO 1 ---
        // Este log deve aparecer CADA VEZ que um zumbi morre.
        Debug.Log("HandleEnemyDied foi chamado pelo zumbi: " + deadEnemy.name);

        if (activeZombies.Contains(deadEnemy))
        {
            activeZombies.Remove(deadEnemy);

            // --- DIAGNÓSTICO 2 ---
            // Este log nos diz se a contagem de zumbis está diminuindo corretamente.
            Debug.Log("Zumbi removido da lista. Zumbis restantes na horda: " + activeZombies.Count);
        }

        if (activeZombies.Count == 0 && currentWaveIndex < waves.Length)
        {
            Debug.Log("CONDIÇÃO ATINGIDA: Todos os zumbis da horda foram derrotados! Chamando StartNextWave...");
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
}