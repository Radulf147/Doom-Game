// MenuPrincipalController.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Precisamos disso para interagir com componentes de UI, como o botão

public class MenuPrincipalController : MonoBehaviour
{
    // No Inspector, arraste seu botão "Continuar" para este campo.
    public Button botaoContinuar;

    void Start()
    {
        // Verifica se a "gaveta" com nosso jogo salvo existe.
        if (PlayerPrefs.HasKey("UltimaFaseSalva"))
        {
            // Se existe, o botão Continuar fica ativo e clicável.
            botaoContinuar.interactable = true;
        }
        else
        {
            // Se não existe, o botão fica desativado e "cinza".
            botaoContinuar.interactable = false;
        }
    }

    // Esta função será chamada pelo clique do botão "Continuar".
    public void ContinuarJogo()
    {
        // Lê o nome da última fase que salvamos.
        string ultimaFase = PlayerPrefs.GetString("UltimaFaseSalva");
        
        // Carrega a cena que estava salva.
        SceneManager.LoadScene(ultimaFase);
    }

    // Função para o botão "Novo Jogo"
    // Função para o botão "Novo Jogo"
public void NovoJogo()
{
    // 1. Apaga qualquer save anterior. (MUITO IMPORTANTE MANTER ESSA LINHA)
    PlayerPrefs.DeleteKey("UltimaFaseSalva");

    // 2. Carrega a sua cena de Seleção de Campeão. (AQUI ESTÁ A MUDANÇA)
    SceneManager.LoadScene("SelecaoDePersonagem"); // <-- TROQUE PELO NOME EXATO DA SUA CENA
}

    // Função para o botão "Sair do Jogo"
    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}