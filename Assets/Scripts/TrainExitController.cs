using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainExitController : MonoBehaviour
{
    // Esta função será chamada pelo jogador quando ele interagir com o trem.
    public void IrParaFaseTres()
    {
        Debug.Log("EMBARCANDO! Indo para a Fase 3...");

        // Garante que o tempo do jogo esteja normal antes de carregar a cena.
        Time.timeScale = 1f;

        // Carrega a cena da Fase 3.
        // Lembre-se que "Fase 3" deve estar nos Build Settings!
        SceneManager.LoadScene("Fase 3"); // Use o nome exato do seu arquivo de cena.
    }
}