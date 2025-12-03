using UnityEngine;

public class HeartUI : MonoBehaviour
{
    public GameObject fullHeart;
    public GameObject emptyHeart;

    public void ShowFull()
    {
        fullHeart.SetActive(true);
        emptyHeart.SetActive(false);
    }

    public void ShowEmpty()
    {
        fullHeart.SetActive(false);
        emptyHeart.SetActive(true);
    }
}
