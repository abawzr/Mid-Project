using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    public int correctIndex;   // رقمها الصحيح (مثلاً 0,1,2,3...)
    [HideInInspector] public int currentIndex;

    private PuzzleManager manager;

    private void Start()
    {
        manager = FindObjectOfType<PuzzleManager>();
    }

    private void OnMouseDown()
    {
        manager.SelectPiece(this);
    }
}
