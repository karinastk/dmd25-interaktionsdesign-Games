using TMPro;
using UnityEngine;

public class FieldButton : MonoBehaviour
{
    private TicTacToManager ticTacToManager;
    internal int index;

    public int Player { get; private set; } = -1;
    public bool IsBlocked { get; private set; } = false;

    private TextMeshProUGUI buttonText;

    void Start()
    {
        ticTacToManager = FindObjectOfType<TicTacToManager>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnButtonClicked()
    {
        if (Player != -1 || IsBlocked)
            return;

        ticTacToManager.OnButtonClickedInManager(this);
    }

    public void SetField(int currentPlayer)
    {
        if (IsBlocked) return;
        Player = currentPlayer;
        SetButtonText(currentPlayer == 0 ? "X" : "O");
    }

    public void SetButtonText(string text)
    {
        if (buttonText != null)
            buttonText.text = text;
    }

    public void SetBlocked()
    {
        IsBlocked = true;
        SetButtonText("BLOCKED");
    }

    public void ReleaseBlock()
    {
        IsBlocked = false;
        SetButtonText("");
    }

    public void ResetField()
    {
        Player = -1;
        IsBlocked = false;
        SetButtonText("");
    }
}
