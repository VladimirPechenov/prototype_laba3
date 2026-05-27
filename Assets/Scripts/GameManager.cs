using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private Text promptText;

    public int Score { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateScoreText();
        ShowPrompt(string.Empty);
    }

    public void AddScore(int value)
    {
        Score += value;
        UpdateScoreText();
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
}
