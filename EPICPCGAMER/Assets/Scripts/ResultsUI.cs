using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultsUI : MonoBehaviour
{
    [Header("Shared")]
    public TextMeshProUGUI accuracyLabel;
    public TextMeshProUGUI messageLabel;

    [Header("Campaign")]
    public GameObject   campaignPanel;
    public Image[]      stars;           // 3 star images
    public Sprite       starFilled;
    public Sprite       starEmpty;
    public Button       nextLevelButton;
    public Button       retryButton;
    public Button       menuButton;

    [Header("Ultimate Gamer")]
    public GameObject   ultimatePanel;
    public TextMeshProUGUI ultimateScoreLabel;
    public TextMeshProUGUI ultimateBestLabel;
    public TextMeshProUGUI ultimateTimeLabel;
    public Button       playAgainButton;
    public Button       menuButtonUltimate;

    public void Show(int starCount, float accuracy, int levelIndex, bool hasNext)
    {
        if (campaignPanel  != null) campaignPanel.SetActive(true);
        if (ultimatePanel  != null) ultimatePanel.SetActive(false);

        if (accuracyLabel != null)
            accuracyLabel.text = $"{accuracy:F1}%";

        if (messageLabel != null)
            messageLabel.text = starCount >= 1 ? "LEVEL CLEAR!" : "FAILED — Try Again";

        // Fill stars
        if (stars != null)
            for (int i = 0; i < stars.Length; i++)
                if (stars[i] != null)
                    stars[i].sprite = i < starCount ? starFilled : starEmpty;

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(hasNext && starCount >= 1);

        if (retryButton != null)
            retryButton.onClick.RemoveAllListeners();
        retryButton?.onClick.AddListener(() =>
            GameModeManager.Instance.StartLevel(levelIndex));

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveAllListeners();
        nextLevelButton?.onClick.AddListener(() =>
            GameModeManager.Instance.StartLevel(levelIndex + 1));

        if (menuButton != null)
            menuButton.onClick.RemoveAllListeners();
        menuButton?.onClick.AddListener(() =>
            GameModeManager.Instance.ShowModeSelect());
    }

    public void ShowUltimate(float score, float best, float time, float accuracy)
    {
        if (campaignPanel  != null) campaignPanel.SetActive(false);
        if (ultimatePanel  != null) ultimatePanel.SetActive(true);

        if (ultimateScoreLabel != null)
            ultimateScoreLabel.text = $"SCORE\n{score:F0}";

        if (ultimateBestLabel != null)
            ultimateBestLabel.text = score >= best
                ? "NEW BEST!"
                : $"BEST: {best:F0}";

        if (ultimateTimeLabel != null)
        {
            int mins = (int)(time / 60f);
            int secs = (int)(time % 60f);
            ultimateTimeLabel.text = $"TIME: {mins:00}:{secs:00}\nACCURACY: {accuracy:F1}%";
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(() =>
                GameModeManager.Instance.SelectUltimateGamer());
        }

        if (menuButtonUltimate != null)
        {
            menuButtonUltimate.onClick.RemoveAllListeners();
            menuButtonUltimate.onClick.AddListener(() =>
                GameModeManager.Instance.ShowModeSelect());
        }
    }
}