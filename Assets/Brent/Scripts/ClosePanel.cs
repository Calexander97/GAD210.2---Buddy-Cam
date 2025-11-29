using UnityEngine;

public class ClosePanel : MonoBehaviour
{
    [Header("Panel Settings")]
    public GameObject panelToClose;

    [Header("Optional Player")]
    public GameObject player;

    public void ClosePanelButton()
    {
        if (panelToClose != null)
            panelToClose.SetActive(false);

        if (player != null)
        {
            HeroNavAgent2D hero = player.GetComponent<HeroNavAgent2D>();
            if (hero != null)
                hero.agent.isStopped = false;
        }
    }
}
