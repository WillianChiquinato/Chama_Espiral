using TMPro;
using UnityEngine;

public class FPS : MonoBehaviour
{
    public TextMeshProUGUI textoDoFPS;

    void Start()
    {
        textoDoFPS = GetComponent<TextMeshProUGUI>();

        // Essa funcao chama um elemento a todo momento;
        InvokeRepeating(nameof(CalcularFPS), 0, 1);
    }

    private void CalcularFPS () 
    {
        textoDoFPS.text = (1f / Time.deltaTime).ToString("00");
    }
}
