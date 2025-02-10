using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;
    [SerializeField] GameCursor actionCursor; // Cursor for the action selector
    [SerializeField] GameCursor moveCursor;   // Cursor for the move selector

    // Fixed X and initial Y positions for the cursors
    private float actionCursorX = -4.85f;
    private float actionCursorInitialY = -44.62f;
    private float moveCursorX = -35.085f;
    private float moveCursorInitialY = 11.94f;

    // Y offset to fine-tune cursor positioning
    private float yOffset = -0.61f; // Adjust this value as needed for accurate positioning
    private float moveCursorYOffset = -1.35f;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
        actionCursor.SetActive(enabled); // Show or hide the action cursor

        if (enabled)
        {
            // Reset the action cursor to the initial Y position with offset
            actionCursor.SetPosition(actionCursorX, actionCursorInitialY + yOffset);
            UpdateActionOnSelection(0); // Reset to the first option when enabling
        }
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
        moveCursor.SetActive(enabled); // Show or hide the move cursor

        if (enabled)
        {
            // Reset the move cursor to the initial Y position with offset
            moveCursor.SetPosition(moveCursorX, moveCursorInitialY + yOffset);
            UpdateMoveSelection(0, null); // Reset to the first option when enabling
        }
    }

    public void UpdateActionOnSelection(int selectedAction)
    {
        if (selectedAction >= 0 && selectedAction < actionTexts.Count)
        {
            // Get the Y position from the selected action text
            float cursorY = actionTexts[selectedAction].rectTransform.anchoredPosition.y + yOffset;

            actionCursor.SetPosition(actionCursorX, cursorY);
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        // Position the move cursor based on the selected move
        if (selectedMove >= 0 && selectedMove < moveTexts.Count)
        {
            // Add yOffset specific to the move cursor
            float cursorY = moveTexts[selectedMove].rectTransform.anchoredPosition.y + moveCursorYOffset;

            moveCursor.SetPosition(moveCursorX, cursorY);
        }

        // Update move details if a move is passed
        if (move != null)
        {
            ppText.text = $"{move.PP}/{move.Base.PP}";
            typeText.text = move.Base.Type.ToString();
        }
    }

    public void SetMovesName(List<Move> moves)
    {
        for (int i = 0; i < moves.Count; ++i)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else
                moveTexts[i].text = "-";
        }
    }
}
