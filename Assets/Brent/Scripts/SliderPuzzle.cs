using System;
using UnityEngine;
using UnityEngine.UI;

public class SliderPuzzle : MonoBehaviour
{
    public RectTransform movementArea;
    public RectTransform sliderBar;
    public RectTransform targetZone;

    public float speed = 400f;
    public Action onSuccess;
    public Action onFail;

    private bool movingRight = true;
    private bool active = false;

    public void StartPuzzle(Action success, Action fail)
    {
        onSuccess = success;
        onFail = fail;

        gameObject.SetActive(true);

        // force UI layout update so world positions are valid
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        float halfWidth = movementArea.rect.width * 0.5f;
        sliderBar.anchoredPosition = new Vector2(-halfWidth, 0);

        movingRight = true;
        active = true;
    }

    void Update()
    {
        if (!active) return;

        MoveSlider();

        if (Input.GetKeyDown(KeyCode.Space))
            CheckHit();
    }

    void MoveSlider()
    {
        float delta = speed * Time.deltaTime;
        if (!movingRight) delta = -delta;

        sliderBar.anchoredPosition += new Vector2(delta, 0);

        float halfWidth = movementArea.rect.width * 0.5f;

        if (sliderBar.anchoredPosition.x >= halfWidth)
            movingRight = false;

        if (sliderBar.anchoredPosition.x <= -halfWidth)
            movingRight = true;
    }

    void CheckHit()
    {
        if (IsSliderInsideTarget())
            onSuccess?.Invoke();
        else
            onFail?.Invoke();

        active = false;
        gameObject.SetActive(false);
    }

    bool IsSliderInsideTarget()
    {
        Vector3 sliderCenter = sliderBar.position;

        // get world corners of target
        Vector3[] corners = new Vector3[4];
        targetZone.GetWorldCorners(corners);
        float targetMinX = corners[0].x;
        float targetMaxX = corners[2].x;

        return sliderCenter.x >= targetMinX && sliderCenter.x <= targetMaxX;
    }
}