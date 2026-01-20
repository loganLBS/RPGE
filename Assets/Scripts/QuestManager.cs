using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public bool questStarted = false;
    public bool questCompleted = false;

    public TextMeshProUGUI questText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartQuest();
    }

    void StartQuest()
    {
        questStarted = true;
        questText.text = "Find the item";
    }

    public void CompleteQuest()
    {
        if (questCompleted) return;

        questCompleted = true;
        questText.text = "Quest Completed!";
    }
}
