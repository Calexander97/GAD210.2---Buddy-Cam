using System.Collections.Generic;
using UnityEngine;

public class LogPageScroller : MonoBehaviour
{
    public List<GameObject> pages;
    public int currentPage = 0;

    public void Start()
    {
        ShowPage(0);
    }

    public void NextPage()
    {
        if (currentPage < pages.Count - 1)
        {
            currentPage++;
            ShowPage(currentPage);
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowPage(currentPage);
        }
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Count; i++)
            pages[i].SetActive(i == index);
    }
}