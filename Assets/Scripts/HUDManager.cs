using UnityEngine;
using UnityEngine.UI; // Necessário para o componente Image (rosto)
using TMPro;      // Necessário para o componente TextMeshPro (munição)

public class HUDManager : MonoBehaviour
{
    [Header("Referências Visuais da HUD")]
    // Arraste aqui o objeto de Imagem UI do rosto do personagem
    public Image iconeRostoImage;

    // Arraste aqui o objeto "spriteArmaFP" que é filho da sua câmera
    public SpriteRenderer spriteArmaRenderer;

    // Arraste aqui o mesmo "spriteArmaFP" (ele precisa ter um componente Animator)
    public Animator spriteArmaAnimator;

    // Arraste aqui o seu objeto de texto que mostra a munição
    public TextMeshProUGUI textoMunicao;

    /// <summary>
    /// Configura os elementos visuais estáticos da HUD no início da fase.
    /// Este método é chamado pelo CharacterInitializer.
    /// </summary>
    /// <param name="iconeRosto">O sprite do rosto do personagem escolhido.</param>
    /// <param name="spriteArma">O sprite base (idle) da arma do personagem.</param>
    public void ConfigurarHUD(Sprite iconeRosto, Sprite spriteArma)
    {
        // --- LINHA DE DEPURAÇÃO 2 ---
        Debug.Log("HUDMANAGER: Recebi a ordem para configurar a HUD.");
        Debug.Log("HUDMANAGER: Sprite do Rosto recebido: " + (iconeRosto != null ? iconeRosto.name : "NENHUM (NULL)"));
        Debug.Log("HUDMANAGER: Sprite da Arma recebido: " + (spriteArma != null ? spriteArma.name : "NENHUM (NULL)"));
        if (iconeRostoImage != null)
        {
            iconeRostoImage.sprite = iconeRosto;
            iconeRostoImage.gameObject.SetActive(true); // Garante que está visível
        }
        else
        {
            Debug.LogWarning("Referência para 'iconeRostoImage' não configurada no HUDManager.");
        }

        if (spriteArmaRenderer != null)
        {
            spriteArmaRenderer.sprite = spriteArma; // Define a imagem inicial da arma
        }
        else
        {
            Debug.LogWarning("Referência para 'spriteArmaRenderer' não configurada no HUDManager.");
        }
    }

    /// <summary>
    /// Atualiza o texto de munição na tela.
    /// Este método é chamado pelo GunScript sempre que a munição muda.
    /// </summary>
    /// <param name="municaoNoPente">Munição atual no pente.</param>
    /// <param name="municaoNaReserva">Munição total na reserva.</param>
    public void AtualizarTextoMunicao(int municaoNoPente, int municaoNaReserva)
    {
        if (textoMunicao != null)
        {
            textoMunicao.text = municaoNoPente + " / " + municaoNaReserva;
        }
    }

    /// <summary>
    /// Dispara a animação de tiro no Animator da arma.
    /// Este método é chamado pelo GunScript no momento do disparo.
    /// </summary>
    public void PlayAnimacaoTiro()
    {
        if (spriteArmaAnimator != null)
        {
            // "Tiro" deve ser o nome de um "Trigger" que você cria no seu Animator
            spriteArmaAnimator.SetTrigger("Tiro");
        }
    }

    // Opcional: Se suas armas tiverem diferentes conjuntos de animações
    public void CarregarAnimacoesDaArma(AnimatorOverrideController animController)
    {
        if (spriteArmaAnimator != null && animController != null)
        {
            spriteArmaAnimator.runtimeAnimatorController = animController;
        }
    }
}