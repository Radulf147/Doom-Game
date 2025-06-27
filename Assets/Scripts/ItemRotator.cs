using UnityEngine;

public class ItemRotator : MonoBehaviour
{
    [Tooltip("Velocidade da rotação em graus por segundo.")]
    public float rotationSpeed = 45f;

    [Tooltip("Eixo em torno do qual o objeto irá girar (Y é para cima, para um giro horizontal).")]
    public Vector3 rotationAxis = Vector3.up;

    // Update é chamado a cada frame
    void Update()
    {
        // Rotaciona o transform deste objeto
        // Multiplicamos por Time.deltaTime para que a rotação seja suave e independente do framerate.
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }
}