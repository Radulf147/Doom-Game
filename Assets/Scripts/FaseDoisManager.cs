using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class FaseDoisManager : MonoBehaviour
{
    [Header("UI dos Itens")]
    public TextMeshProUGUI trilhoText;
    public TextMeshProUGUI fuelText;
    public TextMeshProUGUI radiatorText;

    public GameObject tremConsertado;

    [Header("Objetos do Cenário (Trilho)")]
    public GameObject trilhosQuebrados;
    public GameObject trilhoConsertado;

    [Header("Referências do Jogador e Câmeras")]
    public PlayerFPController playerController;
    public Camera cameraDoJogador;
    public Camera cameraDoTrem;

    [Header("Sistema de Animação e UI")]
    // A ÚNICA lista para toda a UI que deve sumir durante a animação
    public List<GameObject> uiParaEsconder; 
    public Image painelDeFade;
    public float duracaoFade = 0.5f;
    public AudioClip somDeConserto;
    
    [Header("Sistema de Objetivos")]
    // Apenas a referência para a mensagem final
    public TextMeshProUGUI mensagemFinalText;
    public GameObject listItens; 

    private AudioSource audioSource;
    private int itensColetados = 0;
    private bool animacaoEmAndamento = false;

    void Start()
    {
        trilhoConsertado.SetActive(false);
        trilhosQuebrados.SetActive(true);
        cameraDoTrem.gameObject.SetActive(false);
        audioSource = gameObject.AddComponent<AudioSource>();
        if(painelDeFade != null) { painelDeFade.gameObject.SetActive(false); }
        if(mensagemFinalText != null) { mensagemFinalText.gameObject.SetActive(false); }
    }

    public void ColetarItem(GameObject itemColetado)
    {
        if (animacaoEmAndamento) return;

        string itemTag = itemColetado.tag;
        itensColetados++;

        switch (itemTag)
        {
            case "Trilho":
                trilhoText.text = "<s>■ Trilho</s>";
                trilhoText.color = Color.gray;
                MecanicaEspecialTrilho();
                break;
            case "Fuel":
                fuelText.text = "<s>■ Combustível</s>";
                fuelText.color = Color.gray;
                break;
            case "Radiator":
                radiatorText.text = "<s>■ Radiador</s>";
                radiatorText.color = Color.gray;
                break;
        }

        StartCoroutine(ExecutarAnimacaoDoTrem(itemColetado));
    }

    void MecanicaEspecialTrilho()
    {
        trilhosQuebrados.SetActive(false);
        trilhoConsertado.SetActive(true);
    }

    IEnumerator ExecutarAnimacaoDoTrem(GameObject objetoParaDestruir)
    {
        animacaoEmAndamento = true;

        try
        {
            // --- PASSO 1: PREPARAÇÃO VISUAL ---
            // Desliga a UI e inicia o fade.
            foreach (GameObject obj in uiParaEsconder)
            {
                if (obj != null) obj.SetActive(false);
            }
            yield return StartCoroutine(Fade(true));

            // --- PASSO 2: CONGELAMENTO (Com a tela preta) ---
            if (objetoParaDestruir != null) Destroy(objetoParaDestruir);
            playerController.enabled = false;

            // --- PASSO 3: TROCAR CÂMERAS ---
            cameraDoJogador.GetComponent<AudioListener>().enabled = false;
            cameraDoJogador.gameObject.tag = "Untagged";
            cameraDoJogador.gameObject.SetActive(false);

            cameraDoTrem.gameObject.SetActive(true);
            cameraDoTrem.gameObject.tag = "MainCamera";
            cameraDoTrem.GetComponent<AudioListener>().enabled = true;

            // --- PASSO 4: EXIBIR A CENA ---
            if (somDeConserto != null) audioSource.PlayOneShot(somDeConserto);
            yield return StartCoroutine(Fade(false));
            yield return new WaitForSeconds(3.0f);

            // --- PASSO 5: PREPARAR RETORNO ---
            yield return StartCoroutine(Fade(true));

            // --- PASSO 6: RESTAURAR O JOGADOR ---
            cameraDoTrem.GetComponent<AudioListener>().enabled = false;
            cameraDoTrem.gameObject.tag = "Untagged";
            cameraDoTrem.gameObject.SetActive(false);
            cameraDoJogador.gameObject.SetActive(true);
            cameraDoJogador.gameObject.tag = "MainCamera";
            cameraDoJogador.GetComponent<AudioListener>().enabled = true;

            // --- PASSO 7: LÓGICA FINAL DA UI E DESCONGELAMENTO ---
            // Primeiro, devolvemos o controlo ao jogador.
            playerController.enabled = true;

            // Agora, decidimos o que mostrar na tela.
            if (itensColetados >= 3)
            {

                // Jogo terminado: mostra a mensagem final. A UI do jogo continua desligada.
                if (mensagemFinalText != null)
                {
                    mensagemFinalText.text = "O trem foi consertado!";
                    mensagemFinalText.gameObject.SetActive(true);
                }
                foreach (GameObject obj in uiParaEsconder)
                {
                    if (obj != null) obj.SetActive(true);
                }

                if (listItens != null)
                {
                    listItens.gameObject.SetActive(false);
                }

            }
            else
            {
                // Jogo NÃO terminado: reativa a UI normal do jogo.
                foreach (GameObject obj in uiParaEsconder)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }

            // Finalmente, clareia a tela, já com a UI correta visível e o jogador no controlo.
            yield return StartCoroutine(Fade(false));
        }
        finally
        {
            animacaoEmAndamento = false;
        }
        TrainExitController trainInteractor = tremConsertado.GetComponent<TrainExitController>();
        if (trainInteractor != null)
        {
        trainInteractor.enabled = true; // Ativa o script!
        }
    }

    IEnumerator Fade(bool paraPreto)
    {
        // ... (A função Fade continua exatamente a mesma) ...
        painelDeFade.gameObject.SetActive(true);
        float startAlpha = paraPreto ? 0f : 1f;
        float endAlpha = paraPreto ? 1f : 0f;
        Color corDoPainel = painelDeFade.color;
        float timer = 0f;
        while (timer < duracaoFade)
        {
            corDoPainel.a = Mathf.Lerp(startAlpha, endAlpha, timer / duracaoFade);
            painelDeFade.color = corDoPainel;
            timer += Time.deltaTime;
            yield return null;
        }
        corDoPainel.a = endAlpha;
        painelDeFade.color = corDoPainel;
        if (!paraPreto)
        {
            painelDeFade.gameObject.SetActive(false);
        }
    }
}