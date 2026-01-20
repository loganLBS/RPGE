using UnityEngine;

public class QuestItem : MonoBehaviour
{
    public GameObject questMarker;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!QuestManager.Instance.questCompleted)
        {
            QuestManager.Instance.CompleteQuest();

            if (questMarker != null)
                questMarker.SetActive(false);

            Destroy(gameObject, 0.1f);
        }
    }
}
