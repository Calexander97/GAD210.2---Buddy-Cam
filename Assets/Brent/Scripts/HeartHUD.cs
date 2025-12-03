using System.Collections.Generic;
using UnityEngine;

public class HeartHUD : MonoBehaviour
{
    public HeroHealth playerHealth;
    public GameObject heartPrefab;
    public Transform heartsContainer;

    private List<HeartUI> hearts = new List<HeartUI>();

    void Start()
    {
        BuildHearts(playerHealth.maxHearts);

        playerHealth.onDamaged.AddListener(UpdateHeartsFromPlayer);
        playerHealth.onDeath.AddListener(UpdateHeartsFromPlayer);

        UpdateHeartsFromPlayer();
    }

    void BuildHearts(int max)
    {
        // clear any old hearts
        foreach (Transform child in heartsContainer)
            Destroy(child.gameObject);

        hearts.Clear();

        // build hearts equal to max health
        for (int i = 0; i < max; i++)
        {
            GameObject h = Instantiate(heartPrefab, heartsContainer);
            hearts.Add(h.GetComponent<HeartUI>());
        }
    }

    void UpdateHeartsFromPlayer()
    {
        int current = playerHealth.Current;
        int max = playerHealth.maxHearts;

        // rebuild if max hearts changed
        if (hearts.Count != max)
            BuildHearts(max);

        // toggle full/empty states
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < current)
                hearts[i].ShowFull();
            else
                hearts[i].ShowEmpty();
        }
    }
}
