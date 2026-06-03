using UnityEngine;
using UnityEngine.UI;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string HasSaveKey = "WR_HasSave";
    private const string HasCheckpointKey = "WR_HasCheckpoint";
    private const string PrefixSave = "WR_Save_";
    private const string PrefixCheckpoint = "WR_Checkpoint_";

    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ResourceManager resources;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Shop shop;
    [SerializeField] private Text statusText;
    [SerializeField] private bool loadSaveOnStart;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResolveReferences();

        if (loadSaveOnStart && PlayerPrefs.GetInt(HasSaveKey, 0) == 1)
            LoadGame();
    }

    public void SaveGame()
    {
        ResolveReferences();
        SaveState(PrefixSave, player != null ? player.position : Vector3.zero);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();
        SetStatus("Progress saved");
    }

    public void LoadGame()
    {
        ResolveReferences();

        if (PlayerPrefs.GetInt(HasSaveKey, 0) != 1)
        {
            SetStatus("No saved progress");
            return;
        }

        LoadState(PrefixSave);
        SetStatus("Progress loaded");
    }

    public void SaveCheckpoint(Vector3 respawnPosition)
    {
        ResolveReferences();
        SaveState(PrefixCheckpoint, respawnPosition);
        PlayerPrefs.SetInt(HasCheckpointKey, 1);
        PlayerPrefs.Save();
        SetStatus("Checkpoint saved");
    }

    public bool LoadCheckpoint()
    {
        ResolveReferences();

        if (PlayerPrefs.GetInt(HasCheckpointKey, 0) != 1)
            return false;

        LoadState(PrefixCheckpoint);
        SetStatus("Respawned from checkpoint");
        return true;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.DeleteKey(HasCheckpointKey);
        DeleteState(PrefixSave);
        DeleteState(PrefixCheckpoint);
        PlayerPrefs.Save();
        SetStatus("Saved progress cleared");
    }

    private void SaveState(string prefix, Vector3 position)
    {
        PlayerPrefs.SetFloat(prefix + "X", position.x);
        PlayerPrefs.SetFloat(prefix + "Y", position.y);
        PlayerPrefs.SetFloat(prefix + "Z", position.z);

        if (playerHealth != null)
        {
            PlayerPrefs.SetInt(prefix + "MaxHealth", playerHealth.MaxHealth);
            PlayerPrefs.SetInt(prefix + "Health", playerHealth.CurrentHealth);
        }

        if (resources != null)
        {
            PlayerPrefs.SetInt(prefix + "Coins", resources.Coins);
            PlayerPrefs.SetInt(prefix + "KeyFragments", resources.KeyFragments);
        }

        if (gameManager != null)
        {
            PlayerPrefs.SetInt(prefix + "Score", gameManager.Score);
            PlayerPrefs.SetInt(prefix + "Remains", gameManager.RitualRemains);
        }

        if (playerStats != null)
        {
            PlayerPrefs.SetFloat(prefix + "DamageBonus", playerStats.DamageBonus);
            PlayerPrefs.SetFloat(prefix + "SpeedBonus", playerStats.SpeedBonus);
        }

        if (shop != null)
        {
            PlayerPrefs.SetInt(prefix + "HealthCost", shop.HealthUpgradeCost);
            PlayerPrefs.SetInt(prefix + "DamageCost", shop.DamageUpgradeCost);
            PlayerPrefs.SetInt(prefix + "SpeedCost", shop.SpeedUpgradeCost);
        }
    }

    private void LoadState(string prefix)
    {
        Vector3 position = new(
            PlayerPrefs.GetFloat(prefix + "X", player != null ? player.position.x : 0f),
            PlayerPrefs.GetFloat(prefix + "Y", player != null ? player.position.y : 1f),
            PlayerPrefs.GetFloat(prefix + "Z", player != null ? player.position.z : 0f));

        if (playerHealth != null)
        {
            int maxHealth = PlayerPrefs.GetInt(prefix + "MaxHealth", playerHealth.MaxHealth);
            int currentHealth = PlayerPrefs.GetInt(prefix + "Health", playerHealth.CurrentHealth);
            playerHealth.RestoreState(position, maxHealth, currentHealth);
        }
        else if (player != null)
        {
            player.position = position;
        }

        resources?.SetResources(
            PlayerPrefs.GetInt(prefix + "Coins", resources != null ? resources.Coins : 0),
            PlayerPrefs.GetInt(prefix + "KeyFragments", resources != null ? resources.KeyFragments : 0));

        gameManager?.SetProgress(
            PlayerPrefs.GetInt(prefix + "Score", gameManager != null ? gameManager.Score : 0),
            PlayerPrefs.GetInt(prefix + "Remains", gameManager != null ? gameManager.RitualRemains : 0));

        playerStats?.SetBonuses(
            PlayerPrefs.GetFloat(prefix + "DamageBonus", playerStats != null ? playerStats.DamageBonus : 0f),
            PlayerPrefs.GetFloat(prefix + "SpeedBonus", playerStats != null ? playerStats.SpeedBonus : 0f));

        shop?.SetCosts(
            PlayerPrefs.GetInt(prefix + "HealthCost", shop != null ? shop.HealthUpgradeCost : 50),
            PlayerPrefs.GetInt(prefix + "DamageCost", shop != null ? shop.DamageUpgradeCost : 40),
            PlayerPrefs.GetInt(prefix + "SpeedCost", shop != null ? shop.SpeedUpgradeCost : 30));
    }

    private static void DeleteState(string prefix)
    {
        string[] keys =
        {
            "X", "Y", "Z", "MaxHealth", "Health", "Coins", "KeyFragments",
            "Score", "Remains", "DamageBonus", "SpeedBonus", "HealthCost",
            "DamageCost", "SpeedCost",
        };

        foreach (string key in keys)
            PlayerPrefs.DeleteKey(prefix + key);
    }

    private void ResolveReferences()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (resources == null)
            resources = ResourceManager.Instance;

        if (gameManager == null)
            gameManager = GameManager.Instance;

        if (playerStats == null && player != null)
            playerStats = player.GetComponent<PlayerStats>();

        if (shop == null)
            shop = FindFirstObjectByType<Shop>();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        ResourceManager.Instance?.ShowFeedback(message);
    }
}
