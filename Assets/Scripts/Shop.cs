using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private int healthUpgradeCost = 50;
    [SerializeField] private int damageUpgradeCost = 40;
    [SerializeField] private int speedUpgradeCost = 30;
    [SerializeField] private int healthCostStep = 25;
    [SerializeField] private int damageCostStep = 20;
    [SerializeField] private int speedCostStep = 20;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Text healthButtonText;
    [SerializeField] private Text damageButtonText;
    [SerializeField] private Text speedButtonText;
    [SerializeField] private Text statusText;

    private PlayerHealth playerHealth;
    private PlayerStats playerStats;

    public int HealthUpgradeCost => healthUpgradeCost;
    public int DamageUpgradeCost => damageUpgradeCost;
    public int SpeedUpgradeCost => speedUpgradeCost;

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerStats = FindFirstObjectByType<PlayerStats>();
        SetShopVisible(false);
        UpdateLabels();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            SetShopVisible(shopPanel == null || !shopPanel.activeSelf);
    }

    public void BuyHealthUpgrade()
    {
        if (!TrySpend(healthUpgradeCost))
            return;

        playerHealth?.IncreaseMaxHealth(20, true);
        healthUpgradeCost += healthCostStep;
        SetStatus("Max HP increased by 20");
        UpdateLabels();
    }

    public void BuyDamageUpgrade()
    {
        if (!TrySpend(damageUpgradeCost))
            return;

        playerStats?.AddDamage(5f);
        damageUpgradeCost += damageCostStep;
        SetStatus("Ritual power increased by 5");
        UpdateLabels();
    }

    public void BuySpeedUpgrade()
    {
        if (!TrySpend(speedUpgradeCost))
            return;

        playerStats?.AddSpeed(0.8f);
        speedUpgradeCost += speedCostStep;
        SetStatus("Movement speed increased");
        UpdateLabels();
    }

    public void SetCosts(int healthCost, int damageCost, int speedCost)
    {
        healthUpgradeCost = Mathf.Max(0, healthCost);
        damageUpgradeCost = Mathf.Max(0, damageCost);
        speedUpgradeCost = Mathf.Max(0, speedCost);
        UpdateLabels();
    }

    private bool TrySpend(int cost)
    {
        if (ResourceManager.Instance != null && ResourceManager.Instance.SpendCoins(cost))
            return true;

        SetStatus("Not enough coins");
        return false;
    }

    private void SetShopVisible(bool visible)
    {
        if (shopPanel != null)
            shopPanel.SetActive(visible);

        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }

    private void UpdateLabels()
    {
        if (healthButtonText != null)
            healthButtonText.text = $"+20 max HP - {healthUpgradeCost}";

        if (damageButtonText != null)
            damageButtonText.text = $"+5 ritual power - {damageUpgradeCost}";

        if (speedButtonText != null)
            speedButtonText.text = $"+0.8 speed - {speedUpgradeCost}";
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
