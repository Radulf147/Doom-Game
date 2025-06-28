using UnityEngine;
using System.Collections.Generic;

// --- CORREÇÃO IMPORTANTE ---
// A classe agora herda de MonoBehaviour, e não de EmissorParticulasBase.
// Isso remove toda a lógica de emissão contínua que causava o problema.
public class EmissorSangue : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O prefab da partícula de sangue que será instanciado.")]
    public GameObject particulaSanguePrefab;

    [Header("Configurações da Emissão")]
    [Tooltip("Número de partículas a serem criadas por ponto de impacto.")]
    public int particulasPorPonto = 15;
    [Tooltip("Força com que as partículas são expelidas.")]
    public float forcaEmissao = 5f;
    [Tooltip("Ângulo de dispersão do 'espirro' de sangue.")]
    [Range(0f, 360f)]
    public float anguloDispersao = 120f;
    [Tooltip("Tempo de vida de cada partícula em segundos.")]
    public float duracaoParticula = 1.5f;
    [Tooltip("Escala mínima para uma partícula.")]
    public float escalaParticulaMin = 0.5f;
    [Tooltip("Escala máxima para uma partícula.")]
    public float escalaParticulaMax = 1.2f;

    /// <summary>
    /// Cria uma rajada de sangue a partir de um único ponto. Ideal para revólveres/pistolas.
    /// </summary>
    /// <param name="posicao">O ponto no espaço onde o sangue deve se originar.</param>
    /// <param name="direcaoImpacto">A direção de onde veio o dano (ex: -direção do tiro).</param>
    public void EmitirSangueEmPonto(Vector3 posicao, Vector3 direcaoImpacto)
    {
        if (particulaSanguePrefab == null)
        {
            Debug.LogError("EmissorSangue: O prefab da partícula de sangue não foi atribuído!", this);
            return;
        }

        for (int i = 0; i < particulasPorPonto; i++)
        {
            CriarUmaParticula(posicao, direcaoImpacto);
        }
    }

    /// <summary>
    /// Método privado que cria e configura uma única partícula de sangue.
    /// </summary>
    private void CriarUmaParticula(Vector3 posicao, Vector3 direcaoImpacto)
    {
        // Cria uma rotação aleatória dentro do cone de dispersão
        Quaternion randomRotation = Quaternion.LookRotation(direcaoImpacto);
        Quaternion randomConeRotation = Quaternion.Euler(Random.Range(-anguloDispersao / 2, anguloDispersao / 2), Random.Range(-anguloDispersao / 2, anguloDispersao / 2), 0);

        Vector3 direcaoParticula = randomRotation * randomConeRotation * Vector3.forward;

        float forcaAleatoria = forcaEmissao * Random.Range(0.7f, 1.3f);
        Vector3 velocidadeInicial = direcaoParticula.normalized * forcaAleatoria;

        GameObject instanciaParticula = Instantiate(particulaSanguePrefab, posicao, Quaternion.identity);
        ParticulaSangue scriptParticula = instanciaParticula.GetComponent<ParticulaSangue>();

        if (scriptParticula != null)
        {
            float escala = Random.Range(escalaParticulaMin, escalaParticulaMax);
            scriptParticula.Initialize(posicao, velocidadeInicial, duracaoParticula, escala);
        }
    }

    // Os métodos para Múltiplos Pontos e Arco podem ser adicionados aqui se você precisar deles para a espingarda ou katana.
    // Por simplicidade, esta versão foca no tiro de ponto único que já está sendo usado.
}