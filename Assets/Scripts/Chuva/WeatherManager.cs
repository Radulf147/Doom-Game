using UnityEngine;
using System.Collections; 

public class WeatherManager : MonoBehaviour
{
    public enum RainIntensityState { Fraca, Forte }
    private RainIntensityState currentState;

    [Header("Referências")]
    public EmissorChuva emissorChuvaPrincipal;
    public EmissorChuva emissorChuvaFundo; 
    public AudioSource audioChuvaFraca;
    public AudioSource audioChuvaForte;
    public AudioSource audioVentoForte;

    [Header("Configurações de Duração (Segundos)")]
    public float duracaoMinChuvaFraca = 30f;
    public float duracaoMaxChuvaFraca = 60f;
    public float duracaoMinChuvaForte = 20f;
    public float duracaoMaxChuvaForte = 40f;
    public float tempoDeTransicao = 5f; 

    [Header("Chuva Fraca")]
    public float emissaoChuvaFraca = 50f;
    public Vector2 ventoChuvaFraca = new Vector2(0.5f, 0f);
    public float volumeChuvaFraca = 0.7f;

    [Header("Chuva Forte")]
    public float emissaoChuvaForte = 200f;
    public Vector2 ventoChuvaForte = new Vector2(5f, 1f);
    public float volumeChuvaForte = 1f;
    public float volumeVentoForte = 0.8f;

    private float tempoNoEstadoAtual;
    private float duracaoFaseAtual;

    void Start()
    {
   
        if (audioChuvaFraca) audioChuvaFraca.loop = true;
        if (audioChuvaForte) audioChuvaForte.loop = true;
        if (audioVentoForte) audioVentoForte.loop = true;

        if (audioChuvaFraca) audioChuvaFraca.volume = 0;
        if (audioChuvaForte) audioChuvaForte.volume = 0;
        if (audioVentoForte) audioVentoForte.volume = 0;

        DefinirEstado(RainIntensityState.Fraca, true);
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

        StopAllCoroutines();

        if (novoEstado == RainIntensityState.Fraca)
        {
            duracaoFaseAtual = Random.Range(duracaoMinChuvaFraca, duracaoMaxChuvaFraca);

            if (emissorChuvaPrincipal != null)
            {
                StartCoroutine(TransicionarFloat(val => emissorChuvaPrincipal.emissionRate = val, emissorChuvaPrincipal.emissionRate, emissaoChuvaFraca, instantaneo ? 0f : tempoDeTransicao));
                StartCoroutine(TransicionarVector2(val => emissorChuvaPrincipal.windInfluence = val, emissorChuvaPrincipal.windInfluence, ventoChuvaFraca, instantaneo ? 0f : tempoDeTransicao));
            }
            if (emissorChuvaFundo != null)
            {
                StartCoroutine(TransicionarFloat(val => emissorChuvaFundo.emissionRate = val, emissorChuvaFundo.emissionRate, emissaoChuvaFraca * 0.75f, instantaneo ? 0f : tempoDeTransicao)); // Ex: 75% da principal
                StartCoroutine(TransicionarVector2(val => emissorChuvaFundo.windInfluence = val, emissorChuvaFundo.windInfluence, ventoChuvaFraca * 0.5f, instantaneo ? 0f : tempoDeTransicao)); // Ex: 50% do vento
            }

            if (audioChuvaFraca) StartCoroutine(FadeAudio(audioChuvaFraca, volumeChuvaFraca, instantaneo ? 0f : tempoDeTransicao));
            if (audioChuvaForte) StartCoroutine(FadeAudio(audioChuvaForte, 0f, instantaneo ? 0f : tempoDeTransicao));
            if (audioVentoForte) StartCoroutine(FadeAudio(audioVentoForte, 0f, instantaneo ? 0f : tempoDeTransicao));
        }
        else
        {
            duracaoFaseAtual = Random.Range(duracaoMinChuvaForte, duracaoMaxChuvaForte);

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

        if (targetVolume > 0.01f && !audioSource.isPlaying) // Se vai aumentar o volume e não está tocando
        {
            audioSource.Play();
        }

        if (duration <= 0) // Mudança instantânea
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

        if (targetVolume < 0.01f && audioSource.isPlaying) // Se volume final é zero e estava tocando
        {
            audioSource.Stop();
        }
    }

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