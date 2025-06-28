using UnityEngine;

[CreateAssetMenu(fileName = "NovaArma", menuName = "Jogo/Ficha de Arma")]
public class WeaponData : ScriptableObject
{
    [Header("Identificação")]
    public AmmoPickup.AmmoType weaponAmmoType;

    [Header("Configurações de Tiro")]
    public GameObject projetilPrefab;
    public float danoDoProjetil = 20f;
    public float cadenciaDeTiro = 0.5f;
    public float alcanceDoProjetil = 100f; 

    [Header("Configurações de Espingarda")]
    public int projeteisPorTiro = 1;     
    public float fatorDeDispersao = 0f; 
    [Header("Configurações de Munição")]
    public int tamanhoDoPente = 6;
    public int municaoReservaMax = 60;
}