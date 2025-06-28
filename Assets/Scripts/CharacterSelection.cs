using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    // Arraste aqui no Inspector as suas "Fichas de Personagem"
    public CharacterData michelleData;
    public CharacterData ricardoData;
    public CharacterData carlinhosData;

    public void SelecionarPersonagem(CharacterData personagem)
    {
        // Guarda a ficha do personagem escolhido no nosso GameManager
        GameManager.Instance.personagemSelecionado = personagem;

        // Carrega a cena do jogo
        SceneManager.LoadScene("Fase1");
    }

    // Estas são as funções que os botões vão chamar
    public void OnMichelleClicked() => SelecionarPersonagem(michelleData);
    public void OnRicardoClicked() => SelecionarPersonagem(ricardoData);
    public void OnCarlinhosClicked() => SelecionarPersonagem(carlinhosData);
}