using UnityEngine;

public class CharacterInitializer : MonoBehaviour
{
    [Header("Referências do Jogador")]
    public HUDManager hudManager; // Arraste seu Canvas aqui
    public GunScript gunScript;   // Ele já está no mesmo objeto
    
    // A referência ao SpriteRenderer da arma não é mais necessária aqui,
    // pois o HUDManager já a possui. Mas podemos manter para outras lógicas se precisar.
    // public SpriteRenderer spriteArmaRenderer; 

    void Start()
{
    Debug.Log("FASE 1 RECEBEU: O personagem que chegou do GameManager é: " + GameManager.Instance.personagemSelecionado.name);

    // Verificação 1: O GameManager existe?
    if (GameManager.Instance == null)
    {
        Debug.LogError("DIAGNÓSTICO FALHOU: A instância do GameManager é NULA. Verifique se o objeto _GameManager com o script está na cena de seleção.");
        return;
    }
    Debug.Log("DIAGNÓSTICO: GameManager.Instance - OK.");

    // Verificação 2: Um personagem foi selecionado?
    if (GameManager.Instance.personagemSelecionado == null)
    {
        Debug.LogError("DIAGNÓSTICO FALHOU: Nenhum personagem foi selecionado (personagemSelecionado é NULO). Verifique o script CharacterSelection.");
        return;
    }
    Debug.Log("DIAGNÓSTICO: Personagem Selecionado ('" + GameManager.Instance.personagemSelecionado.name + "') - OK.");

    CharacterData personagem = GameManager.Instance.personagemSelecionado;

    // Verificação 3: A referência ao HUDManager foi arrastada no Inspector?
    Debug.Log("VERIFICANDO: hudManager - " + (hudManager == null ? "ESTÁ NULO! (PROBLEMA AQUI)" : "OK."));

    // Verificação 4: A referência ao GunScript foi arrastada no Inspector?
    Debug.Log("VERIFICANDO: gunScript - " + (gunScript == null ? "ESTÁ NULO! (PROBLEMA AQUI)" : "OK."));

    // --- A partir daqui, o código tenta executar a lógica original ---
    Debug.Log("--- FIM DO DIAGNÓSTICO. TENTANDO EXECUTAR A LÓGICA... ---");


    // Lógica original (agora com verificações)
    if (hudManager != null)
    {
        hudManager.ConfigurarHUD(personagem.iconeRostoHUD, personagem.spriteArma2D);
    }
    else
    {
        // Se o hudManager for nulo, este erro apareceria antes da linha 41.
        Debug.LogError("Erro Crítico: Referência ao HUDManager está faltando no Inspector do CharacterInitializer.");
        return; // Para a execução para evitar mais erros.
    }

    if (gunScript != null)
    {
        if (personagem.dadosDaArma != null)
        {
            // A LINHA 41 (APROXIMADAMENTE) ESTÁ AQUI.
            // Se o erro acontece aqui, é porque algo DENTRO de CarregarDadosDaArma está nulo.
            gunScript.CarregarDadosDaArma(personagem.dadosDaArma);
        }
        else
        {
            gunScript.enabled = false;
        }
    }
    else
    {
        // Se o gunScript for nulo, este será o erro real.
        Debug.LogError("Erro Crítico: Referência ao GunScript está faltando no Inspector do CharacterInitializer.");
        return;
    }
}
}