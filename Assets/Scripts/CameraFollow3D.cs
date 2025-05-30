using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    public Transform target; // Arraste o objeto "Jogador" aqui pelo Inspector
    public float distance = 7.0f;      // Distância da câmera ao alvo
    public float height = 3.0f;        // Altura da câmera em relação ao alvo
    public float horizontalOffset = 0f; // Deslocamento lateral da câmera
    public float smoothSpeed = 10f;    // Velocidade de suavização da câmera
    public Vector3 lookAtOffset = new Vector3(0, 1.0f, 0); // Ponto para onde a câmera olha (um pouco acima da base do jogador)


    void LateUpdate() // Use LateUpdate para seguir, pois garante que o jogador já se moveu
    {
        if (target == null)
        {
            return;
        }

        // Posição desejada da câmera
        Vector3 targetPosition = target.position + Vector3.up * height - target.forward * distance + target.right * horizontalOffset;

        // Suaviza a transição da câmera
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Faz a câmera olhar para o alvo (com um pequeno ajuste de altura para o ponto de foco)
        transform.LookAt(target.position + lookAtOffset);
    }
}