using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainExitController : MonoBehaviour
{
    // Método público para ser chamado pelo jogador ao interagir com o trem.
    // Agora ele é mais genérico e funciona para várias fases.
    public void EmbarcarNoTrem()
    {
        // Garante que o tempo do jogo esteja normal.
        Time.timeScale = 1f;

        // Pega a cena que está ativa no momento.
        Scene cenaAtual = SceneManager.GetActiveScene();

        // Verifica o nome da cena atual para decidir para onde ir.
        if (cenaAtual.name == "Fase1")
        {
            Debug.Log("Embarcando! Indo da Fase 1 para a Fase 2...");
            // Lembre-se que "Fase 2" deve estar nos Build Settings!
            SceneManager.LoadScene("Fase 2");
        }
        else if (cenaAtual.name == "Fase 2")
        {
            Debug.Log("Embarcando! Indo da Fase 2 para a Fase 3...");
            // Lembre-se que "Fase 3" deve estar nos Build Settings!
            SceneManager.LoadScene("Fase 3");
        }
        else
        {
            // Mensagem de erro caso o script esteja em uma cena não prevista.
            Debug.LogWarning("TrainExitController está em uma cena não configurada para transição: " + cenaAtual.name);
        }
    }
}
