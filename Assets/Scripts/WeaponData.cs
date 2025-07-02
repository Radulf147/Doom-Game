using UnityEngine;

// O enum TipoDeArma não é mais estritamente necessário para a lógica, mas podemos manter para organização.
public enum TipoDeArma { Fogo, Branca }

[CreateAssetMenu(fileName = "NovaArma", menuName = "Jogo/Ficha de Arma")]
public class WeaponData : ScriptableObject
{
    [Header("Tipo da Arma")]
    public TipoDeArma tipoDeArma = TipoDeArma.Fogo;

    [Header("Configurações de Gameplay")]
    [Tooltip("Dano de cada projétil ou acerto.")]
    public float dano = 20f;
    [Tooltip("Tempo MÍNIMO em segundos entre cada tiro. Controla a LÓGICA.")]
    public float cadenciaDeTiro = 0.5f;
    [Tooltip("Tempo total em segundos que o jogador fica ESPERANDO a recarga. Controla a LÓGICA.")]
    public float tempoDeRecarga = 2f;
    [Tooltip("Alcance máximo da arma.")]
    public float alcanceDaArma = 100f;

    [Header("Configurações de Disparo Múltiplo")]
    [Tooltip("Quantos projéteis (raios) são disparados por tiro. Use 1 para armas normais.")]
    public int projeteisPorTiro = 1;
    [Tooltip("O fator de espalhamento aleatório (para espingardas). Use 0 para precisão.")]
    public float fatorDeDispersao = 0f;
    [Tooltip("O espaçamento entre projéteis verticais (para a katana). Use 0 para armas normais.")]
    public float espacamentoVertical = 0f; // <-- CAMPO ADICIONADO

    [Header("Configurações de Munição")]
    public int tamanhoDoPente = 6;
    public int municaoReservaMax = 60;
    public int municaoPorColeta = 12;

    [Header("Efeitos e Animações")]
    public GameObject hitEffectPrefab;
    public AnimatorOverrideController animadorDaArma;
    public AudioClip somDoTiro;
    public AudioClip somDaRecarga;
    
    [Header("Controle de Tempo Visual das Animações")]
    public float duracaoBaseAnimTiro = 1f;
    public float duracaoVisualTiro = 0.2f;
    public float duracaoBaseAnimRecarga = 1f;
    public float duracaoVisualRecarga = 1.8f;
}