using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button[]            levelButtons;    // one per level
    public TextMeshProUGUI[]   levelNames;
    public Image[][]           levelStarImages; // [levelIndex][starIndex]

    void OnEnable()
    {
        var mgr = GameModeManager.Instance;
        if (mgr == null) return;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;

            bool unlocked = mgr.IsLevelUnlocked(i);
            levelButtons[i].interactable = unlocked;

            if (levelNames != null && i < levelNames.Length && levelNames[i] != null)
                levelNames[i].text = unlocked
                    ? mgr.campaignLevels[i].levelName
                    : "LOCKED";

            int saved   = mgr.GetSavedStars(i);
            int capture = i;
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() =>
                GameModeManager.Instance.StartLevel(capture));
        }
    }
}