using UnityEngine;

// Esta linha mágica permite criar "arquivos de dados" no menu da Unity
[CreateAssetMenu(fileName = "NovoPersonagem", menuName = "Jogo/Ficha de Personagem")]
public class CharacterData : ScriptableObject
{
    [Header("Informações Básicas")]
    public string nomeDoPersonagem;
    public Sprite iconeRostoHUD;
    public Sprite spriteArma2D; // A imagem da arma que fica na tela

    [Header("Atributos e Passivas")]
    public float velocidadeMovimento = 5f;
    public float taxaDeAtaque = 1f; // Modificador. 1.1f = 10% mais rápido
    public bool regeneraVida = false;

    [Header("Dados da Arma")]
    public WeaponData dadosDaArma; // Referência para outra ficha, só da arma!
}