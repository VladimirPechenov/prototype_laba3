using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private Text coinText;
    [SerializeField] private Text keyText;
    [SerializeField] private Text feedbackText;

    private Coroutine feedbackRoutine;

    public int Coins { get; private set; }
    public int KeyFragments { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateUI();
        ShowFeedback(string.Empty);
    }

    public void AddCoins(int amount, Vector3 pickupPosition)
    {
        Coins += amount;
        UpdateUI();
        ShowFeedback($"+{amount} coins");
        FeedbackAudio.PlayPickup(pickupPosition);
    }

    public void AddKeyFragments(int amount, Vector3 pickupPosition)
    {
        KeyFragments += amount;
        UpdateUI();
        ShowFeedback($"+{amount} key fragment");
        FeedbackAudio.PlayPickup(pickupPosition);
    }

    public bool SpendCoins(int amount)
    {
        if (Coins < amount)
        {
            ShowFeedback("Not enough coins");
            return false;
        }

        Coins -= amount;
        UpdateUI();
        ShowFeedback($"-{amount} coins");
        return true;
    }

    public void ShowFeedback(string message)
    {
        if (feedbackText == null)
            return;

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        if (string.IsNullOrWhiteSpace(message))
        {
            feedbackText.text = string.Empty;
            feedbackText.enabled = false;
            feedbackRoutine = null;
            return;
        }

        feedbackRoutine = StartCoroutine(ShowFeedbackRoutine(message));
    }

    private IEnumerator ShowFeedbackRoutine(string message)
    {
        feedbackText.text = message;
        feedbackText.enabled = true;
        yield return new WaitForSeconds(1.4f);
        feedbackText.enabled = false;
        feedbackRoutine = null;
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {Coins}";

        if (keyText != null)
            keyText.text = $"Key fragments: {KeyFragments}/3";
    }
}
