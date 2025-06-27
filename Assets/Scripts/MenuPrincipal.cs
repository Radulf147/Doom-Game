using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para gerenciar cenas!

public class MenuManager : MonoBehaviour
{
    // Esta função será chamada pelo nosso botão "Novo Jogo"
    public void IniciarJogo()
    {
        // Coloque aqui o nome EXATO do arquivo da sua cena de jogo!
SceneManager.LoadScene("Fase1");
    }

    // Você pode adicionar outras funções para os outros botões aqui depois
}