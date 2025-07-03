using UnityEngine;
using UnityEngine.SceneManagement; // Importante para detectar mudança de cena

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        // "Se inscreve" para ser notificado toda vez que uma cena for carregada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Esta função será chamada automaticamente pelo Unity quando uma nova cena carregar
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se a nova cena for uma das fases do jogo...
        if (scene.name == "Fase1" || scene.name == "Fase 2" || scene.name == "Fase 3")
        {
            // ...o MusicManager do menu se autodestrói, parando a música.
            // Para garantir que não haja "flashes" de som, podemos fazer um fade out antes
            // Mas, por simplicidade, vamos apenas destruir.
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        // Boa prática: sempre se "desinscrever" de eventos ao ser destruído para evitar erros.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}