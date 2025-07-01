using UnityEngine;

[CreateAssetMenu(fileName = "NovaArma", menuName = "Jogo/Ficha de Arma")]
public class WeaponData : ScriptableObject
{[Header("Identificação")]
    public AmmoPickup.AmmoType weaponAmmoType;

    [Header("Sons da Arma")]
    public AudioClip somDoTiro;
    public AudioClip somDaRecarga;
    [Header("Configurações de Gameplay")]
    [Tooltip("Dano de cada projétil ou acerto.")]
    public float dano = 20f;
    [Tooltip("Tempo MÍNIMO em segundos entre cada tiro. Controla a LÓGICA.")]
    public float cadenciaDeTiro = 0.5f;
    [Tooltip("Tempo total em segundos que o jogador fica ESPERANDO a recarga. Controla a LÓGICA.")]
    public float tempoDeRecarga = 2f;
    [Tooltip("Alcance máximo da arma.")]
    public float alcanceDaArma = 100f;

    [Header("Configurações de Espingarda")]
    public int projeteisPorTiro = 1;
    public float fatorDeDispersao = 0f;

    [Header("Configurações de Munição")]
    public int tamanhoDoPente = 6;
    public int municaoReservaMax = 60;
    
    [Header("Efeitos e Animações")]
    public GameObject hitEffectPrefab;
    public AnimatorOverrideController animadorDaArma;
    
    [Header("Controle de Tempo Visual das Animações")]
    [Tooltip("A duração original do seu clipe de animação de TIRO. (Selecione o .anim para ver)")]
    public float duracaoBaseAnimTiro = 1f;
    [Tooltip("Quanto tempo você QUER que a animação de tiro dure na tela.")]
    public float duracaoVisualTiro = 0.2f; // <-- NOVO: Controle visual

    [Tooltip("A duração original do seu clipe de animação de RECARGA. (Selecione o .anim para ver)")]
    public float duracaoBaseAnimRecarga = 1f;
    [Tooltip("Quanto tempo você QUER que a animação de recarga dure na tela.")]
    public float duracaoVisualRecarga = 1.8f; // <-- NOVO: Controle visual
}