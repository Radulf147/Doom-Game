using UnityEngine;

public class CharacterInitializer : MonoBehaviour
{
    [Header("Referências aos Sistemas de Combate")]
    public GunScript gunScript;
    public MeleeScript meleeScript;

    private HUDManager hudManager;

    void Awake()
    {
        hudManager = FindFirstObjectByType<HUDManager>();
    }

    void Start()
    {
        if (GameManager.Instance == null || GameManager.Instance.personagemSelecionado == null)
        {
            Debug.LogError("Nenhum personagem selecionado! Carregue a cena de seleção.");
            return;
        }

        CharacterData personagem = GameManager.Instance.personagemSelecionado;
        
        if (hudManager != null)
        {
            hudManager.ConfigurarHUD(personagem.iconeRostoHUD, personagem.spriteArma2D);
        }
        
        if (personagem.dadosDaArma != null)
        {


            // --- LÓGICA DE DECISÃO CORRIGIDA ---
            if (personagem.dadosDaArma.tipoDeArma == WeaponData.TipoDeArma.Fogo) // Adicionado "WeaponData."
            {
                // Se for arma de fogo, ativa o GunScript e desativa o MeleeScript
                gunScript.enabled = true;
                meleeScript.enabled = false;
                gunScript.CarregarDadosDaArma(personagem.dadosDaArma);
            }
            else if (personagem.dadosDaArma.tipoDeArma == WeaponData.TipoDeArma.Branca) // Adicionado "WeaponData."
            {
                // Se for arma branca, ativa o MeleeScript e desativa o GunScript
                gunScript.enabled = false;
                meleeScript.enabled = true;
                meleeScript.CarregarDadosDaArma(personagem.dadosDaArma);
            }

            gunScript.SetCharacterAbilities(personagem);
        }
        else
        {
            // Se não tiver arma nenhuma, desativa os dois
            gunScript.enabled = false;
            meleeScript.enabled = false;
        }

        // --- Lógica das Passivas ---
        // Exemplo:
        // if (personagem.regeneraVida)
        // {
        //     GetComponent<PlayerHealth>().StartRegeneration();
        // }
    }
}