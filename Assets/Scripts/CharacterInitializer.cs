using UnityEngine;

public class CharacterInitializer : MonoBehaviour
{
    // A referência ao MeleeScript foi removida.
    [Header("Referências aos Sistemas de Combate")]
    public GunScript gunScript;

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
        
        // LÓGICA FINAL E SIMPLIFICADA
        if (personagem.dadosDaArma != null && gunScript != null)
        {
            // Se o personagem tem uma arma (qualquer uma), ativamos o GunScript.
            gunScript.enabled = true;
            gunScript.CarregarDadosDaArma(personagem.dadosDaArma);
            gunScript.SetCharacterAbilities(personagem); // Se você ainda usa passivas
        }
        else if (gunScript != null)
        {
            // Se não tiver arma nenhuma, desativa o GunScript.
            gunScript.enabled = false;
        }

        // Se você tiver passivas que não dependem da arma, pode aplicá-las aqui.
        // Ex: GetComponent<PlayerHealth>().maxHealth = personagem.vidaMaxima;
    }
}