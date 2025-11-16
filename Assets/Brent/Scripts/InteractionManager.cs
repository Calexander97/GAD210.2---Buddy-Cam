using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public Button interactButton;

    [Header("Highlight Settings")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    [HideInInspector] public Interactable CurrentInteractable { get; private set; }

    private Image buttonImage;

    private void Start()
    {
        if (interactButton != null)
        {
            interactButton.onClick.AddListener(OnInteractButtonPressed);
            buttonImage = interactButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = normalColor;
        }
    }

    private void Update()
    {
        if (buttonImage == null) return;

        if (CurrentInteractable != null)
            buttonImage.color = highlightColor;
        else
            buttonImage.color = normalColor;
    }

    public void SetCurrentInteractable(Interactable interactable)
    {
        CurrentInteractable = interactable;
    }

    public void ClearCurrentInteractable(Interactable interactable)
    {
        if (CurrentInteractable == interactable)
            CurrentInteractable = null;
    }

    public void OnInteractButtonPressed()
    {
        if (CurrentInteractable != null && player != null)
        {
            CurrentInteractable.Interact(player);
        }
    }
}