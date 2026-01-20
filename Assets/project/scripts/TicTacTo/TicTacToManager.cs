using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TicTacToManager : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public Button restartButton;

    public Button bombButtonPlayer1; // Linke Bombe
    public Button bombButtonPlayer2; // Rechte Bombe

    public GameObject buttonIndicator1;
    public GameObject buttonIndicator2;

    int currentPlayer = 0; // 0 = Player 1, 1 = Player 2
    private bool gameEnded = false;

    public FieldButton[] fieldButtons;

    int[] reihe1, reihe2, reihe3, reihe4;
    int[] spalte1, spalte2, spalte3, spalte4;
    int[] diagonale1, diagonale2, diagonale3, diagonale4;

    public int[][] winningCombinations;

    private int[] blockedFields = new int[3];
    private bool blockedFieldsReleased = false;

    // Bomben
    private bool isBombActive = false;
    private int bombOwner = -1; // 0 = Player 1, 1 = Player 2

    private int[] movesPerPlayer = new int[2]; // Züge pro Spieler zählen

    void Start()
    {
        infoText.text = "Player 1";
        SetUpWinningCombinations();
        BlockRandomFields();

        restartButton.onClick.AddListener(RestartGame);

        // Bombenbuttons verbinden
        bombButtonPlayer1.onClick.AddListener(() => ActivateBombMode(0));
        bombButtonPlayer2.onClick.AddListener(() => ActivateBombMode(1));
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(RestartGame);
        bombButtonPlayer1.onClick.RemoveAllListeners();
        bombButtonPlayer2.onClick.RemoveAllListeners();
    }

    public void OnButtonClickedInManager(FieldButton fieldButton)
    {
        if (gameEnded) return;

        // Bombenmodus
        if (isBombActive)
        {
            HandleBombClick(fieldButton);
            return;
        }

        // Gesperrte Felder
        if (System.Array.Exists(blockedFields, e => e == fieldButton.index))
        {
            infoText.text = "This field is blocked!";
            return;
        }

        // Feld besetzen
        fieldButton.SetField(currentPlayer);
        movesPerPlayer[currentPlayer]++;

        // Spielerwechsel
        currentPlayer = currentPlayer == 1 ? 0 : 1;
        infoText.text = "Player " + (currentPlayer + 1);

        // Nach beiden ersten Zügen gesperrte Felder freigeben
        if (!blockedFieldsReleased && currentPlayer == 0)
        {
            ReleaseBlockedFields();
            blockedFieldsReleased = true;
        }

        CheckForWin();
        CheckForDraw();
    }

    void HandleBombClick(FieldButton clickedField)
    {
        int removed = 0;
        System.Random rand = new System.Random();

        while (removed < 3)
        {
            int randomIndex = rand.Next(fieldButtons.Length);

            FieldButton fb = fieldButtons[randomIndex];

            // Nur Felder löschen, die gesetzt sind (nicht leer, nicht BLOCKED)
            if (fb.Player != -1 && !fb.IsBlocked)
            {
                fb.ResetField();
                removed++;
            }
        }

        infoText.text = "Player " + (bombOwner + 1) + " used bomb!";
        isBombActive = false; // Bombenmodus beenden
    }

    public void ActivateBombMode(int playerNumber)
    {
        if (currentPlayer != playerNumber)
        {
            infoText.text = "This bomb is only for Player " + (playerNumber + 1);
            return;
        }

        if (movesPerPlayer[playerNumber] >= 3)
        {
            isBombActive = true;
            bombOwner = playerNumber;
            infoText.text = "Player " + (playerNumber + 1) + " activated bomb! Click a field.";
        }
        else
        {
            infoText.text = "Bomb not ready yet!";
        }
        if (currentPlayer == 0)
        {
           buttonIndicator1.SetActive(false);
        }
        else if (currentPlayer == 1)
        {
              
           buttonIndicator2.SetActive(false);
        }
    }

    void CheckForWin()
    {
        foreach (var combination in winningCombinations)
        {
            if (fieldButtons[combination[0]].Player == fieldButtons[combination[1]].Player &&
                fieldButtons[combination[1]].Player == fieldButtons[combination[2]].Player &&
                fieldButtons[combination[2]].Player == fieldButtons[combination[3]].Player &&
                fieldButtons[combination[0]].Player != -1)
            {
                string winner = fieldButtons[combination[0]].Player == 0 ? "Player 1" : "Player 2";
                infoText.text = winner + " wins!";
                gameEnded = true;
                return;
            }
        }
    }

    void CheckForDraw()
    {
        foreach (var button in fieldButtons)
            if (button.Player == -1)
                return;

        infoText.text = "It's a draw!";
        gameEnded = true;
    }

    void SetUpWinningCombinations()
    {
        reihe1 = new int[] { 0, 1, 2, 3 };
        reihe2 = new int[] { 4, 5, 6, 7 };
        reihe3 = new int[] { 8, 9, 10, 11 };
        reihe4 = new int[] { 12, 13, 14, 15 };

        spalte1 = new int[] { 0, 4, 8, 12 };
        spalte2 = new int[] { 1, 5, 9, 13 };
        spalte3 = new int[] { 2, 6, 10, 14 };
        spalte4 = new int[] { 3, 7, 11, 15 };

        diagonale1 = new int[] { 0, 5, 10, 15 };
        diagonale2 = new int[] { 3, 6, 9, 12 };
        diagonale3 = new int[] { 0, 4, 8, 12 };
        diagonale4 = new int[] { 3, 7, 11, 15 };

        winningCombinations = new int[][]
        {
            reihe1, reihe2, reihe3, reihe4,
            spalte1, spalte2, spalte3, spalte4,
            diagonale1, diagonale2, diagonale3, diagonale4
        };
    }

    void BlockRandomFields()
    {
        System.Random rand = new System.Random();
        int blockedCount = 0;

        while (blockedCount < 3)
        {
            int randomIndex = rand.Next(fieldButtons.Length);

            if (System.Array.Exists(blockedFields, e => e == randomIndex))
                continue;

            blockedFields[blockedCount] = randomIndex;
            fieldButtons[randomIndex].SetField(-2);
            fieldButtons[randomIndex].SetButtonText("BLOCKED");
            blockedCount++;
        }
    }

    void ReleaseBlockedFields()
    {
        foreach (int index in blockedFields)
        {
            fieldButtons[index].SetField(-1);
            fieldButtons[index].SetButtonText("");
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
