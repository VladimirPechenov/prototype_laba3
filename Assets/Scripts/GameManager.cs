using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private Text promptText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text objectiveText;

    public int Score { get; private set; }
    public int RitualRemains { get; private set; }
    public int RequiredRemains { get; private set; } = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateScoreText();
        UpdateObjectiveText();
        ShowPrompt(string.Empty);
    }

    public void AddScore(int value)
    {
        Score += value;
        UpdateScoreText();
    }

    public void AddRitualRemains(int value)
    {
        RitualRemains += value;
        AddScore(value * 25);
        UpdateObjectiveText();
    }

    public void SetProgress(int score, int ritualRemains)
    {
        Score = Mathf.Max(0, score);
        RitualRemains = Mathf.Clamp(ritualRemains, 0, RequiredRemains);
        UpdateScoreText();
        UpdateObjectiveText();
    }

    public bool HasEnoughRemains()
    {
        return RitualRemains >= RequiredRemains;
    }

    public void SetObjective(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
    }

    public void UpdatePlayerStatus(int health, int maxHealth, float stamina01, float battery01, bool flashlightOn, bool crouching)
    {
        if (statusText == null)
            return;

        string lightState = flashlightOn ? "on" : "off";
        string stance = crouching ? "crouch" : "stand";
        statusText.text = $"HP: {health}/{maxHealth}\nStamina: {Mathf.RoundToInt(stamina01 * 100f)}%\nBattery: {Mathf.RoundToInt(battery01 * 100f)}%\nLight: {lightState}\nStance: {stance}";
    }

    public void ShowPrompt(string text)
    {
        if (promptText == null)
            return;

        promptText.text = text;
        promptText.enabled = !string.IsNullOrWhiteSpace(text);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {Score}";
    }

    private void UpdateObjectiveText()
    {
        SetObjective($"Collect remains: {RitualRemains}/{RequiredRemains}\nReach the white altar");
    }
}
