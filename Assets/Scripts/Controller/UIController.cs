using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public TextMeshProUGUI contagemBullets;
    public Slider carregadorAttackSlider;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        carregadorAttackSlider.GetComponent<CanvasGroup>().alpha = 0f;

        carregadorAttackSlider.maxValue = GameManager.Instance.player.maxAttackForce;
        carregadorAttackSlider.minValue = GameManager.Instance.player.minAttackForce;
    }

    void Update()
    {
        UpdateUI();

        if (carregadorAttackSlider.gameObject.activeSelf)
        {
            carregadorAttackSlider.value =
                GameManager.Instance.player.currentSpeedTarget;
        }
    }

    public void UpdateUI()
    {
        contagemBullets.text = GameManager.Instance.currentFlameAmmo.ToString() + " / " + GameManager.Instance.flameMaxAmmo.ToString();
    }

    public IEnumerator FadeInCanvasGroup(GameObject obj, float delay = 0f, float duration = 1f)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }
    }

    public IEnumerator FadeOutCanvasGroup(GameObject obj, float delay = 0f, float duration = 1f)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1 - Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }
    }
}
