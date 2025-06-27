using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para gerenciar cenas!

public class MenuManager : MonoBehaviour
{
    // Esta função será chamada pelo nosso botão "Novo Jogo"
    public void IniciarJogo()
{
    // Alterado para carregar a cena de seleção de personagem
    SceneManager.LoadScene("SelecaoDePersonagem");
}
    // Você pode adicionar outras funções para os outros botões aqui depois
}