using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Referências Visuais da HUD (Arrastar no Inspector)")]
    public Image iconeRostoImage;
    public SpriteRenderer spriteArmaRenderer;
    public Animator spriteArmaAnimator;
    public TextMeshProUGUI textoMunicao;

    /// <summary>
    /// Configura os sprites estáticos iniciais da HUD.
    /// </summary>
    public void ConfigurarHUD(Sprite iconeRosto, Sprite spriteArma)
    {
        if (iconeRostoImage != null)
        {
            iconeRostoImage.sprite = iconeRosto;
            iconeRostoImage.gameObject.SetActive(true);
        }

        if (spriteArmaRenderer != null)
        {
            spriteArmaRenderer.sprite = spriteArma;
        }
    }

    /// <summary>
    /// Carrega o conjunto de animações (Tiro, Recarga, Idle) para a arma atual.
    /// </summary>
    public void CarregarAnimadorDaArma(AnimatorOverrideController overrideController)
    {
        if (spriteArmaAnimator != null && overrideController != null)
        {
            spriteArmaAnimator.runtimeAnimatorController = overrideController;
        }
    }

    /// <summary>
    /// Atualiza o texto de munição na tela.
    /// </summary>
    public void AtualizarTextoMunicao(int municaoNoPente, int municaoNaReserva)
    {
        if (textoMunicao != null)
        {
            textoMunicao.text = municaoNoPente + " / " + municaoNaReserva;
        }
    }

    /// <summary>
    /// Dispara a animação de tiro.
    /// </summary>
    public void PlayAnimacaoTiro()
    {
        if (spriteArmaAnimator != null)
        {
            spriteArmaAnimator.SetTrigger("Tiro");
        }
    }

    /// <summary>
    /// Dispara a animação de recarga.
    /// </summary>
    public void PlayAnimacaoRecarga()
    {
        if (spriteArmaAnimator != null)
        {
            spriteArmaAnimator.SetTrigger("Recarregar");
        }
    }
}