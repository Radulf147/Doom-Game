using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FaseTresManager : MonoBehaviour
{
    [Header("Referências da Cutscene Inicial")]
    public PlayerFPController playerController;
    public Camera cameraDoJogador;
    public Camera cameraDoBoss;
    public float duracaoDaCutscene = 6.0f;

    [Header("Configurações de Áudio")]
    public AudioClip musicaDeFundoFase3;
    public AudioClip somDeApresentacao;
    public AudioClip somDeVitoria; 
    [Range(0f, 1f)]
    public float volumeMaximoMusica = 0.8f;
    public float duracaoFadeMusica = 5.0f;

    [Header("Sistema de Fade e UI")]
    public Image painelDeFade;
    public float duracaoFade = 2.0f;
    public List<GameObject> uiParaEsconder;
    public Image telaDeVitoriaImage; 
    public TextMeshProUGUI textoPontuacaoFinal; // Referência para o texto da pontuação final
    
    // Antigas variáveis do Header "Sistema de Objetivos", movidas para melhor organização
    public TextMeshProUGUI mensagemFinalText;
    public GameObject listItens; 

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
        if (musicaDeFundoFase3 != null)
        {
            audioSource.clip = musicaDeFundoFase3;
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.Play();
        }
        
        if (telaDeVitoriaImage != null)
        {
            telaDeVitoriaImage.gameObject.SetActive(false);
        }

        StartCoroutine(ExecutarCutsceneApresentacao());
    }

    private IEnumerator ExecutarCutsceneApresentacao()
    {
        Debug.Log("Iniciando cutscene...");
        if (musicaDeFundoFase3 != null)
        {
            StartCoroutine(FadeAudio(volumeMaximoMusica, duracaoFadeMusica));
        }

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
            yield return StartCoroutine(Fade(true)); // Escurece
            if (cameraDoJogador != null) cameraDoJogador.enabled = false;
            if (cameraDoBoss != null) cameraDoBoss.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(false)); // Clareia
        }
        else
        {
            if (cameraDoJogador != null) cameraDoJogador.enabled = false;
            if (cameraDoBoss != null) cameraDoBoss.gameObject.SetActive(true);
        }

        if (somDeApresentacao != null) audioSource.PlayOneShot(somDeApresentacao, 1f);
        Debug.Log("Focando no chefe por " + duracaoDaCutscene + " segundos.");
        yield return new WaitForSeconds(duracaoDaCutscene);

        FinalizarCutscene();
    }
    
    private void FinalizarCutscene()
    {
        StartCoroutine(FadeAndSwitchBack());
    }

    private IEnumerator FadeAndSwitchBack()
    {
        if (painelDeFade != null) yield return StartCoroutine(Fade(true)); // Escurece

        if (cameraDoBoss != null) cameraDoBoss.gameObject.SetActive(false);
        if (cameraDoJogador != null) cameraDoJogador.enabled = true;
        if (playerController != null) playerController.canMove = true;

        foreach (GameObject obj in uiParaEsconder)
        {
            if (obj != null) obj.SetActive(true);
        }

        if (painelDeFade != null) yield return StartCoroutine(Fade(false)); // Clareia
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
        if (!paraPreto && endAlpha == 0f)
        {
            painelDeFade.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeAudio(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    // ===== FUNÇÕES DE VITÓRIA E BOTÕES =====

    public void ChefeFoiDerrotado()
    {
        Debug.Log("O CHEFE FOI DERROTADO! VITÓRIA!");
        audioSource.Stop();

        if (somDeVitoria != null)
        {
            audioSource.PlayOneShot(somDeVitoria);
        }

        // Esconde a UI do jogo antes de mostrar a tela de vitória
        foreach (GameObject obj in uiParaEsconder)
        {
            if (obj != null) obj.SetActive(false);
        }

        if (telaDeVitoriaImage != null)
        {
            telaDeVitoriaImage.gameObject.SetActive(true);
        }
        
        if (textoPontuacaoFinal != null && ScoreManager.Instance != null)
        {
            int pontuacao = ScoreManager.Instance.currentScore;
            textoPontuacaoFinal.text = "Pontuação Final: " + pontuacao;
            textoPontuacaoFinal.gameObject.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaInicial");
    }

    public void SairDoJogo()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}