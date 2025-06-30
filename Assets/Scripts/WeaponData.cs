using UnityEngine;

[CreateAssetMenu(fileName = "NovaArma", menuName = "Jogo/Ficha de Arma")]
public class WeaponData : ScriptableObject
{
    [Header("Identificação")]
    public AmmoPickup.AmmoType weaponAmmoType;

    [Header("Configurações de Tiro")]
    public float danoDoProjetil = 20f;
    public float cadenciaDeTiro = 0.5f;
    public float alcanceDaArma = 1000f;

    [Header("Configurações de Espingarda")]
    [Tooltip("Quantos raios (projéteis) são disparados por tiro. Use 1 para armas normais.")]
    public int projeteisPorTiro = 1;
    [Tooltip("O fator de espalhamento dos tiros. Use 0 para precisão perfeita.")]
    public float fatorDeDispersao = 0f;

    [Header("Configurações de Munição")]
    public int tamanhoDoPente = 6;
    public int municaoReservaMax = 60;
    public float reloadTime = 1.5f;

    [Header("Efeitos Visuais de Impacto")]
    public GameObject hitEffectPrefab;

    [Header("Animações")]
    // MODIFICAÇÃO: Este é o campo que guardará o conjunto de animações da arma.
    public AnimatorOverrideController animadorDaArma;
}