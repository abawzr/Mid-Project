using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("Put pieces in their CURRENT order from left->right, top->bottom")]
    public PuzzlePiece[] pieces;

    private PuzzlePiece firstSelected;

    private void Start()
    {
        // نحدّث currentIndex حسب ترتيب المصفوفة
        for (int i = 0; i < pieces.Length; i++)
            pieces[i].currentIndex = i;
    }

    public void SelectPiece(PuzzlePiece piece)
    {
        if (firstSelected == null)
        {
            firstSelected = piece;
            // ممكن تضيفي highlight هنا
            return;
        }

        // لو ضغط نفس القطعة مرتين
        if (firstSelected == piece)
        {
            firstSelected = null;
            return;
        }

        SwapPieces(firstSelected, piece);
        firstSelected = null;

        if (IsSolved())
        {
            Debug.Log("Solved ✅");
        }
    }

    private void SwapPieces(PuzzlePiece a, PuzzlePiece b)
    {
        // تبديل المكان الفعلي
        Vector3 tempPos = a.transform.position;
        a.transform.position = b.transform.position;
        b.transform.position = tempPos;

        // تبديل currentIndex (عشان التحقق)
        int tempIndex = a.currentIndex;
        a.currentIndex = b.currentIndex;
        b.currentIndex = tempIndex;

        // وتحديث مصفوفة pieces نفسها (اختياري لكنه مفيد)
        int ai = a.currentIndex;
        int bi = b.currentIndex;
        pieces[ai] = a;
        pieces[bi] = b;
    }

    private bool IsSolved()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].correctIndex != i)
                return false;
        }
        return true;
    }
}
