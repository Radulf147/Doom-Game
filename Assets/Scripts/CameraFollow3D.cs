using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    public Transform target; // Arraste o objeto "Jogador" aqui pelo Inspector
    public float distance = 7.0f;      // Dist�ncia da c�mera ao alvo
    public float height = 3.0f;        // Altura da c�mera em rela��o ao alvo
    public float horizontalOffset = 0f; // Deslocamento lateral da c�mera
    public float smoothSpeed = 10f;    // Velocidade de suaviza��o da c�mera
    public Vector3 lookAtOffset = new Vector3(0, 1.0f, 0); // Ponto para onde a c�mera olha (um pouco acima da base do jogador)


    void LateUpdate() // Use LateUpdate para seguir, pois garante que o jogador j� se moveu
    {
        if (target == null)
        {
            return;
        }

        // Posi��o desejada da c�mera
        Vector3 targetPosition = target.position + Vector3.up * height - target.forward * distance + target.right * horizontalOffset;

        // Suaviza a transi��o da c�mera
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Faz a c�mera olhar para o alvo (com um pequeno ajuste de altura para o ponto de foco)
        transform.LookAt(target.position + lookAtOffset);
    }
}