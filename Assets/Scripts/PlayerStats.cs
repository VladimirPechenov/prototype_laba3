using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseSpeedBonus;
    [SerializeField] private Text statsText;

    public float DamageBonus { get; private set; }
    public float SpeedBonus { get; private set; }
    public float TotalDamage => baseDamage + DamageBonus;
    public float TotalSpeedBonus => baseSpeedBonus + SpeedBonus;

    private void Start()
    {
        UpdateUI();
    }

    public void AddDamage(float amount)
    {
        DamageBonus += amount;
        UpdateUI();
    }

    public void AddSpeed(float amount)
    {
        SpeedBonus += amount;
        UpdateUI();
    }

    public void SetStatsText(Text text)
    {
        statsText = text;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (statsText == null)
            return;

        statsText.text = $"Ritual power: {TotalDamage:0}\nSpeed bonus: +{TotalSpeedBonus:0.0}";
    }
}
