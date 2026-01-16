using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Damage playerDamage;
    public PlayerController player;
    public Slider slider;

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        playerDamage = player.GetComponent<Damage>();
        slider.value = CalcularPorcentagem(playerDamage.Health, playerDamage.maxHealth);
        playerDamage.healthChange.AddListener(OnPlayerChange);
    }

    public void OnDisable()
    {
        playerDamage.healthChange.RemoveListener(OnPlayerChange);
    }

    public float CalcularPorcentagem(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth;
    }

    private void OnPlayerChange(int newHealth, int maxHealth)
    {
        slider.value = CalcularPorcentagem(newHealth, maxHealth);
    }
}
