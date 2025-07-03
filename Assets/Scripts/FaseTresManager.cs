using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class FaseTresManager : MonoBehaviour
{
    [Header("Referências da Cutscene Inicial")]
    public PlayerFPController playerController;
    public Camera cameraDoJogador;
    public Camera cameraDoBoss;
    public AudioClip somDeApresentacao;

    [Tooltip("Tempo em segundos que a câmera ficará mostrando o chefe.")]
    public float duracaoDaCutscene = 6.0f;

    [Header("Sistema de Fade e UI")]
    [Tooltip("Painel de Imagem UI preto que será usado para o efeito de fade.")]
    public Image painelDeFade;
    [Tooltip("Duração do fade in e fade out.")]
    public float duracaoFade = 2.0f;
    [Tooltip("Opcional: Elementos da UI principal que devem ser escondidos durante a cutscene.")]
    public List<GameObject> uiParaEsconder;

    private AudioSource audioSource;

    // Usamos Awake para garantir que a preparação aconteça antes de qualquer Start.
    void Awake()
    {
        // Garante que o AudioSource existe
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) { audioSource = gameObject.AddComponent<AudioSource>(); }
        
        // --- PREPARAÇÃO INICIAL ---
        // Desativa o controle do jogador IMEDIATAMENTE.
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        // Garante que a câmera do chefe comece desativada para evitar conflitos.
        if (cameraDoBoss != null)
        {
            cameraDoBoss.gameObject.SetActive(false);
        }
    }

    // Start é chamado quando a cena carrega, após o Awake.
    void Start()
    {
        // Inicia a cutscene de apresentação do chefe
        StartCoroutine(ExecutarCutsceneApresentacao());
    }

    private IEnumerator ExecutarCutsceneApresentacao()
    {
        Debug.Log("Iniciando cutscene...");

        // Esconde a UI principal
        foreach (GameObject obj in uiParaEsconder)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Coloca a tela em preto (fade in instantâneo)
        if (painelDeFade != null)
        {
            painelDeFade.gameObject.SetActive(true);
            Color corDoPainel = painelDeFade.color;
            corDoPainel.a = 1f;
            painelDeFade.color = corDoPainel;
        }
        else
        {
            Debug.LogError("Painel de Fade não foi atribuído no Inspector!", this);
            // Se não houver painel de fade, a cutscene não funcionará corretamente.
            // Vamos pular para o final para evitar que o jogador fique travado.
            FinalizarCutscene(); 
            yield break; // Encerra a coroutine
        }

        // Troca para a câmera da cutscene (enquanto a tela está preta)
        if (cameraDoJogador != null) cameraDoJogador.enabled = false;
        if (cameraDoBoss != null) cameraDoBoss.gameObject.SetActive(true);

        Debug.Log("Câmera trocada para a do chefe. Clareando a tela...");
        yield return StartCoroutine(Fade(false)); // Clareia a tela

        // Toca o som de apresentação do chefe
        if (somDeApresentacao != null) audioSource.PlayOneShot(somDeApresentacao);
        Debug.Log("Focando no chefe por " + duracaoDaCutscene + " segundos.");

        yield return new WaitForSeconds(duracaoDaCutscene);

        Debug.Log("Retornando ao jogador. Escurecendo a tela...");
        yield return StartCoroutine(Fade(true)); // Escurece a tela novamente

        // Chama o método para finalizar a cutscene e devolver o controle
        FinalizarCutscene();
    }
    
    /// <summary>
    /// Finaliza a cutscene e devolve o controle ao jogador.
    /// </summary>
    private void FinalizarCutscene()
    {
        // Troca de volta para a câmera do jogador
        if (cameraDoBoss != null) cameraDoBoss.gameObject.SetActive(false);
        if (cameraDoJogador != null) cameraDoJogador.enabled = true;

        // Devolve o controle ao jogador e reativa a UI
        if (playerController != null) playerController.canMove = true;
        foreach (GameObject obj in uiParaEsconder)
        {
            if (obj != null) obj.SetActive(true);
        }

        // Clareia a tela para o jogador
        StartCoroutine(Fade(false));
        
        Debug.Log("Cutscene do chefe finalizada. Batalha iniciada!");
    }

    /// <summary>
    /// Coroutine que controla o efeito de fade para preto ou a partir do preto.
    /// </summary>
    private IEnumerator Fade(bool paraPreto)
    {
        if (painelDeFade == null) yield break;

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