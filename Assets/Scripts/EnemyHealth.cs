// Crie um novo script chamado EnemyHealth.cs e cole este código

using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float vida = 100f;

    public void TomarDano(float dano)
    {
        vida -= dano;
        Debug.Log(gameObject.name + " tomou " + dano + " de dano. Vida restante: " + vida);

        if (vida <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        Debug.Log(gameObject.name + " morreu.");
        Destroy(gameObject); // Destrói o objeto do inimigo
    }
}