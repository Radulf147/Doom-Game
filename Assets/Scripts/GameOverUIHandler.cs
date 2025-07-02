using UnityEngine;

// Este script serve apenas como um "intermediário" para os botões da UI.
public class GameOverUIHandler : MonoBehaviour
{
    // Esta função será chamada pelo botão "Tentar Novamente".
    public void BotaoTentarNovamente()
    {
        // Ele encontra a instância ATIVA do GameManager e chama a função dele.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TentarNovamente();
        }
    }

    // Esta função será chamada pelo botão "Menu Principal".
    public void BotaoVoltarAoMenu()
    {
        // Ele encontra a instância ATIVA do GameManager e chama a função dele.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.VoltarAoMenu();
        }
    }
}