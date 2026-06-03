using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private CameraShake cameraShake;

    private int currentHealth;
    private bool isInvincible;
    private bool isDead;
    private Vector3 respawnPoint;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        respawnPoint = transform.position;
    }

    private void Start()
    {
        UpdateUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void TakeDamage(int amount)
    {
        if (isDead || isInvincible)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateUI();
        cameraShake?.Shake();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityFrames());
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
    }

    public void IncreaseMaxHealth(int amount, bool healByIncrease)
    {
        maxHealth += amount;

        if (healByIncrease)
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);

        UpdateUI();
    }

    public void SetRespawnPoint(Vector3 point)
    {
        respawnPoint = point;
    }

    public void RespawnAtCheckpoint()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.LoadCheckpoint())
            return;

        Time.timeScale = 1f;
        isDead = false;
        isInvincible = false;
        currentHealth = maxHealth;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        transform.position = respawnPoint;

        if (controller != null)
            controller.enabled = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UpdateUI();
    }

    public void RestoreState(Vector3 position, int restoredMaxHealth, int restoredCurrentHealth)
    {
        Time.timeScale = 1f;
        isDead = false;
        isInvincible = false;
        maxHealth = Mathf.Max(1, restoredMaxHealth);
        currentHealth = Mathf.Clamp(restoredCurrentHealth, 1, maxHealth);
        respawnPoint = position;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        transform.position = position;

        if (controller != null)
            controller.enabled = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UpdateUI();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void Die()
    {
        isDead = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = maxHealth <= 0 ? 0f : currentHealth / (float)maxHealth;
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}
