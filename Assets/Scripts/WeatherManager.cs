using UnityEngine;
using System.Collections; // Necess�rio para Coroutines

public class WeatherManager : MonoBehaviour
{
    public enum RainIntensityState { Fraca, Forte }
    private RainIntensityState currentState;

    [Header("Refer�ncias")]
    public EmissorChuva emissorChuvaPrincipal; // Seu emissor de chuva principal
    public EmissorChuva emissorChuvaFundo;   // Seu emissor de chuva de fundo (opcional)
    public AudioSource audioChuvaFraca;
    public AudioSource audioChuvaForte;
    public AudioSource audioVentoForte;

    [Header("Configura��es de Dura��o (Segundos)")]
    public float duracaoMinChuvaFraca = 30f;
    public float duracaoMaxChuvaFraca = 60f;
    public float duracaoMinChuvaForte = 20f;
    public float duracaoMaxChuvaForte = 40f;
    public float tempoDeTransicao = 5f; // Tempo para transi��o suave entre estados

    [Header("Chuva Fraca")]
    public float emissaoChuvaFraca = 50f;
    public Vector2 ventoChuvaFraca = new Vector2(0.5f, 0f);
    public float volumeChuvaFraca = 0.7f;

    [Header("Chuva Forte")]
    public float emissaoChuvaForte = 200f;
    public Vector2 ventoChuvaForte = new Vector2(5f, 1f); // Vento mais forte e talvez em outra dire��o
    public float volumeChuvaForte = 1f;
    public float volumeVentoForte = 0.8f;

    private float tempoNoEstadoAtual;
    private float duracaoFaseAtual;

    void Start()
    {
        // Garante que os AudioSources est�o configurados para loop
        if (audioChuvaFraca) audioChuvaFraca.loop = true;
        if (audioChuvaForte) audioChuvaForte.loop = true;
        if (audioVentoForte) audioVentoForte.loop = true;

        // Come�a com chuva fraca (ou outro estado inicial desejado)
        // Define volumes iniciais para n�o come�ar tocando tudo
        if (audioChuvaFraca) audioChuvaFraca.volume = 0;
        if (audioChuvaForte) audioChuvaForte.volume = 0;
        if (audioVentoForte) audioVentoForte.volume = 0;

        DefinirEstado(RainIntensityState.Fraca, true); // true para configura��o inicial instant�nea
    }

    void Update()
    {
        tempoNoEstadoAtual += Time.deltaTime;
        if (tempoNoEstadoAtual >= duracaoFaseAtual)
        {
            MudarEstado();
        }
    }

    void MudarEstado()
    {
        if (currentState == RainIntensityState.Fraca)
        {
            DefinirEstado(RainIntensityState.Forte);
        }
        else
        {
            DefinirEstado(RainIntensityState.Fraca);
        }
    }

    void DefinirEstado(RainIntensityState novoEstado, bool instantaneo = false)
    {
        currentState = novoEstado;
        tempoNoEstadoAtual = 0f;

        StopAllCoroutines(); // Para quaisquer transi��es anteriores

        if (novoEstado == RainIntensityState.Fraca)
        {
            duracaoFaseAtual = Random.Range(duracaoMinChuvaFraca, duracaoMaxChuvaFraca);

            // Efeitos Visuais
            if (emissorChuvaPrincipal != null)
            {
                StartCoroutine(TransicionarFloat(val => emissorChuvaPrincipal.emissionRate = val, emissorChuvaPrincipal.emissionRate, emissaoChuvaFraca, instantaneo ? 0f : tempoDeTransicao));
                StartCoroutine(TransicionarVector2(val => emissorChuvaPrincipal.windInfluence = val, emissorChuvaPrincipal.windInfluence, ventoChuvaFraca, instantaneo ? 0f : tempoDeTransicao));
            }
            if (emissorChuvaFundo != null) // Se tiver emissor de fundo
            {
                // Ajuste proporcional ou valores espec�ficos para o fundo
                StartCoroutine(TransicionarFloat(val => emissorChuvaFundo.emissionRate = val, emissorChuvaFundo.emissionRate, emissaoChuvaFraca * 0.75f, instantaneo ? 0f : tempoDeTransicao)); // Ex: 75% da principal
                StartCoroutine(TransicionarVector2(val => emissorChuvaFundo.windInfluence = val, emissorChuvaFundo.windInfluence, ventoChuvaFraca * 0.5f, instantaneo ? 0f : tempoDeTransicao)); // Ex: 50% do vento
            }

            // Efeitos Sonoros
            if (audioChuvaFraca) StartCoroutine(FadeAudio(audioChuvaFraca, volumeChuvaFraca, instantaneo ? 0f : tempoDeTransicao));
            if (audioChuvaForte) StartCoroutine(FadeAudio(audioChuvaForte, 0f, instantaneo ? 0f : tempoDeTransicao));
            if (audioVentoForte) StartCoroutine(FadeAudio(audioVentoForte, 0f, instantaneo ? 0f : tempoDeTransicao));
        }
        else // Chuva Forte
        {
            duracaoFaseAtual = Random.Range(duracaoMinChuvaForte, duracaoMaxChuvaForte);

            // Efeitos Visuais
            if (emissorChuvaPrincipal != null)
            {
                StartCoroutine(TransicionarFloat(val => emissorChuvaPrincipal.emissionRate = val, emissorChuvaPrincipal.emissionRate, emissaoChuvaForte, instantaneo ? 0f : tempoDeTransicao));
                StartCoroutine(TransicionarVector2(val => emissorChuvaPrincipal.windInfluence = val, emissorChuvaPrincipal.windInfluence, ventoChuvaForte, instantaneo ? 0f : tempoDeTransicao));
            }
            if (emissorChuvaFundo != null)
            {
                StartCoroutine(TransicionarFloat(val => emissorChuvaFundo.emissionRate = val, emissorChuvaFundo.emissionRate, emissaoChuvaForte * 0.75f, instantaneo ? 0f : tempoDeTransicao));
                StartCoroutine(TransicionarVector2(val => emissorChuvaFundo.windInfluence = val, emissorChuvaFundo.windInfluence, ventoChuvaForte * 0.5f, instantaneo ? 0f : tempoDeTransicao));
            }

            // Efeitos Sonoros
            if (audioChuvaFraca) StartCoroutine(FadeAudio(audioChuvaFraca, 0f, instantaneo ? 0f : tempoDeTransicao));
            if (audioChuvaForte) StartCoroutine(FadeAudio(audioChuvaForte, volumeChuvaForte, instantaneo ? 0f : tempoDeTransicao));
            if (audioVentoForte) StartCoroutine(FadeAudio(audioVentoForte, volumeVentoForte, instantaneo ? 0f : tempoDeTransicao));
        }
    }

    IEnumerator FadeAudio(AudioSource audioSource, float targetVolume, float duration)
    {
        if (audioSource == null) yield break;

        float currentTime = 0;
        float startVolume = audioSource.volume;

        if (targetVolume > 0.01f && !audioSource.isPlaying) // Se vai aumentar o volume e n�o est� tocando
        {
            audioSource.Play();
        }

        if (duration <= 0) // Mudan�a instant�nea
        {
            audioSource.volume = targetVolume;
            if (targetVolume < 0.01f && audioSource.isPlaying) audioSource.Stop();
            yield break;
        }

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;

        if (targetVolume < 0.01f && audioSource.isPlaying) // Se volume final � zero e estava tocando
        {
            audioSource.Stop();
        }
    }

    // Coroutine gen�rica para transicionar um valor float
    IEnumerator TransicionarFloat(System.Action<float> setter, float valorInicial, float valorFinal, float duracao)
    {
        if (duracao <= 0)
        {
            setter(valorFinal);
            yield break;
        }
        float tempo = 0;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            setter(Mathf.Lerp(valorInicial, valorFinal, tempo / duracao));
            yield return null;
        }
        setter(valorFinal);
    }

    // Coroutine gen�rica para transicionar um Vector2
    IEnumerator TransicionarVector2(System.Action<Vector2> setter, Vector2 valorInicial, Vector2 valorFinal, float duracao)
    {
        if (duracao <= 0)
        {
            setter(valorFinal);
            yield break;
        }
        float tempo = 0;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            setter(Vector2.Lerp(valorInicial, valorFinal, tempo / duracao));
            yield return null;
        }
        setter(valorFinal);
    }
}