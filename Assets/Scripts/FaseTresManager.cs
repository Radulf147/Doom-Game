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
    
    [Tooltip("Tempo em segundos que a câmera ficará mostrando o chefe.")]
    public float duracaoDaCutscene = 6.0f;

    // --- ALTERAÇÃO AQUI: Novas variáveis para a música da fase ---
    [Header("Configurações de Áudio")]
    [Tooltip("Música que tocará durante toda a fase.")]
    public AudioClip musicaDeFundoFase3;
    [Tooltip("Som de impacto ou rugido durante a apresentação.")]
    public AudioClip somDeApresentacao;
    [Range(0f, 1f)]
    [Tooltip("O volume máximo que a música de fundo atingirá.")]
    public float volumeMaximoMusica = 0.8f;
    [Tooltip("Quanto tempo (em segundos) o volume levará para ir de 0 ao máximo.")]
    public float duracaoFadeMusica = 5.0f;
    // --- FIM DA ALTERAÇÃO ---

    [Header("Sistema de Fade e UI")]
    [Tooltip("Painel de Imagem UI preto que será usado para o efeito de fade.")]
    public Image painelDeFade;
    [Tooltip("Duração do fade in e fade out.")]
    public float duracaoFade = 2.0f;
    [Tooltip("Opcional: Elementos da UI principal que devem ser escondidos durante a cutscene.")]
    public List<GameObject> uiParaEsconder;

    private AudioSource audioSource;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) { audioSource = gameObject.AddComponent<AudioSource>(); }
        
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        if (cameraDoBoss != null)
        {
            cameraDoBoss.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        // --- ALTERAÇÃO AQUI: Prepara e inicia a música de fundo ---
        if (musicaDeFundoFase3 != null)
        {
            audioSource.clip = musicaDeFundoFase3;
            audioSource.loop = true; // Música de fundo deve ser em loop
            audioSource.volume = 0f; // Começa com volume zero
            audioSource.Play(); // Começa a tocar (silenciosamente no início)
        }
        // --- FIM DA ALTERAÇÃO ---

        StartCoroutine(ExecutarCutsceneApresentacao());
    }

    private IEnumerator ExecutarCutsceneApresentacao()
    {
        Debug.Log("Iniciando cutscene...");
        
        // --- ALTERAÇÃO AQUI: Inicia o fade da música ---
        // A música começará a aumentar o volume gradualmente ENQUANTO a cutscene acontece.
        if (musicaDeFundoFase3 != null)
        {
            StartCoroutine(FadeAudio(volumeMaximoMusica, duracaoFadeMusica));
        }
        // --- FIM DA ALTERAÇÃO ---

        foreach (GameObject obj in uiParaEsconder)
        {
                // Garante que o objeto não é nulo antes de continuar
            if (obj != null)
            {
                // VERIFICAÇÃO ADICIONADA:
                // Se o objeto NÃO tiver um componente de Câmera, desative-o.
                if (obj.GetComponent<Camera>() == null)
                {
                    obj.SetActive(false);
                }
                // Se tiver um componente de Câmera, esta condição será falsa
                // e o código dentro do 'if' será ignorado, deixando a câmera ativa.
            }
        }

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
            FinalizarCutscene(); 
            yield break;
        }

        if (cameraDoJogador != null) cameraDoJogador.enabled = false;
        if (cameraDoBoss != null) cameraDoBoss.gameObject.SetActive(true);

        Debug.Log("Câmera trocada para a do chefe. Clareando a tela...");
        yield return StartCoroutine(Fade(false));

        // Toca o som de apresentação (um efeito sonoro curto)
        if (somDeApresentacao != null) audioSource.PlayOneShot(somDeApresentacao, 1f); // Usamos PlayOneShot para não interferir na música
        Debug.Log("Focando no chefe por " + duracaoDaCutscene + " segundos.");

        yield return new WaitForSeconds(duracaoDaCutscene);

        Debug.Log("Retornando ao jogador. Escurecendo a tela...");
        yield return StartCoroutine(Fade(true));

        FinalizarCutscene();
    }
    
    private void FinalizarCutscene()
    {
        if (cameraDoBoss != null) cameraDoBoss.gameObject.SetActive(false);
        if (cameraDoJogador != null) cameraDoJogador.enabled = true;

        if (playerController != null) playerController.canMove = true;
        foreach (GameObject obj in uiParaEsconder)
        {
            if (obj != null) obj.SetActive(true);
        }

        StartCoroutine(Fade(false));
        
        Debug.Log("Cutscene do chefe finalizada. Batalha iniciada!");
    }

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

    // --- ALTERAÇÃO AQUI: Nova coroutine para o fade do áudio ---
    /// <summary>
    /// Coroutine que aumenta ou diminui o volume do AudioSource gradualmente.
    /// </summary>
    /// <param name="targetVolume">O volume final desejado (entre 0 e 1).</param>
    /// <param name="duration">A duração do fade em segundos.</param>
    private IEnumerator FadeAudio(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            // Calcula o novo volume baseado no tempo passado
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            timer += Time.deltaTime;
            yield return null; // Espera até o próximo frame
        }

        // Garante que o volume final seja exatamente o alvo
        audioSource.volume = targetVolume;
    }
    // --- FIM DA ALTERAÇÃO ---
}