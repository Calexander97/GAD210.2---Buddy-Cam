using UnityEngine;

public class SliderPuzzleTrigger : MonoBehaviour
{
    public SliderPuzzle sliderPuzzle;

    void Update()
    {
    // press E to test the puzzle
        if (Input.GetKeyDown(KeyCode.E))
        {
            sliderPuzzle.StartPuzzle(OnSuccess, OnFail);
        }
    }

    void OnSuccess()
    {
        Debug.Log("Puzzle success!");
    }

    void OnFail()
    {
        Debug.Log("Puzzle failed!");
    }
}
