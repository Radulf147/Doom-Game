using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Encontra a c�mera principal pela tag "MainCamera"
        // Certifique-se que sua c�mera principal tenha essa tag.
        if (Camera.main != null)
        {
            mainCamera = Camera.main;
        }
        else
        {
            Debug.LogError("BillboardSprite: Main Camera not found! Make sure your main camera is tagged 'MainCamera'.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Faz o sprite olhar para a c�mera.
            // A segunda parte (mainCamera.transform.up) ajuda a manter o sprite "em p�" corretamente.
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}