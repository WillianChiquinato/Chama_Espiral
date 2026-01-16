using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Se já existir, destrói o objeto duplicado
            Destroy(gameObject);
        }
    }
}
