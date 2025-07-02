using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; // ADICIONADO: Necessário para usar TextMeshPro

public class FaseUmManager : MonoBehaviour
{
    [Header("Referências da Cutscene Final")]
    public PlayerFPController playerController;
    public Camera cameraDoJogador;
    public Camera cameraDoTrem;
    public GameObject trem;
    public AudioClip somDoTremChegando;
    [Tooltip("Tempo em segundos que a câmera ficará mostrando o trem antes de voltar ao jogador.")]
    public float duracaoDaCutscene = 8.0f;

    [Header("Sistema de Fade e UI")]
    public List<GameObject> uiParaEsconder;
    public Image painelDeFade;
    public float duracaoFade = 1.5f;

    // ===== NOVO: CAMPO PARA A MENSAGEM FINAL =====
    [Header("Mensagem Final")]
    [Tooltip("O objeto de texto da UI que mostrará a mensagem final.")]
    public TextMeshProUGUI mensagemFinalText;


    private AudioSource audioSource;
    private bool cutsceneIniciada = false;

    // Se inscreve para ouvir o anúncio do HordeManager
    private void OnEnable()
    {
        HordeManager.OnAllHordesCompleted += IniciarCutsceneFinal;
    }

    // Se desinscreve para evitar erros
    private void OnDisable()
    {
        HordeManager.OnAllHordesCompleted -= IniciarCutsceneFinal;
    }

    void Start()
    {
        // Prepara os objetos da cutscene
        if (trem != null) trem.SetActive(false);
        if (cameraDoTrem != null) cameraDoTrem.gameObject.SetActive(false);
        if (painelDeFade != null) painelDeFade.gameObject.SetActive(false);

        // ===== NOVO: Garante que a mensagem final comece desligada =====
        if (mensagemFinalText != null)
        {
            mensagemFinalText.gameObject.SetActive(false);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        if (audioSource == null) { audioSource = gameObject.AddComponent<AudioSource>(); }
    }

    private void IniciarCutsceneFinal()
    {
        if (cutsceneIniciada) return;
        StartCoroutine(ExecutarCutsceneCoroutine());
    }

    private IEnumerator ExecutarCutsceneCoroutine()
    {
        cutsceneIniciada = true;
        
        // --- PARTE 1: INÍCIO DA CUTSCENE ---
        foreach (GameObject obj in uiParaEsconder)
        {
            if (obj != null) obj.SetActive(false);
        }
        yield return StartCoroutine(Fade(true));

        playerController.canMove = false;
        if (trem != null) trem.SetActive(true);
        if (somDoTremChegando != null) audioSource.PlayOneShot(somDoTremChegando);

        cameraDoJogador.enabled = false;
        cameraDoJogador.GetComponent<AudioListener>().enabled = false;
        cameraDoTrem.gameObject.SetActive(true);
        cameraDoTrem.GetComponent<AudioListener>().enabled = true;

        yield return StartCoroutine(Fade(false));

        // --- PARTE 2: RETORNO AO JOGADOR ---
        yield return new WaitForSeconds(duracaoDaCutscene);
        yield return StartCoroutine(Fade(true));

        cameraDoTrem.gameObject.SetActive(false);
        cameraDoTrem.GetComponent<AudioListener>().enabled = false;
        cameraDoJogador.enabled = true;
        cameraDoJogador.GetComponent<AudioListener>().enabled = true;
        playerController.canMove = true;

        foreach (GameObject obj in uiParaEsconder)
        {
            if (obj != null) obj.SetActive(true);
        }

        yield return StartCoroutine(Fade(false));

        // ===== NOVO: MOSTRAR A MENSAGEM FINAL =====
        if (mensagemFinalText != null)
        {
            mensagemFinalText.text = "O trem para o abrigo chegou";
            mensagemFinalText.gameObject.SetActive(true);
        }

        Debug.Log("Cutscene finalizada. O controle foi devolvido ao jogador.");
    }

    private IEnumerator Fade(bool paraPreto)
    {
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