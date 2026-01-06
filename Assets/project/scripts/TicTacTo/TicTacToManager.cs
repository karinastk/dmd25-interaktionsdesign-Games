using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TicTacToManager : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public Button restartButton;

    int currentPlayer = 0; // 0 = Spieler 1, 1 = Spieler 2
    private bool gameEnded = false; // Flag, ob das Spiel beendet ist

    public FieldButton[] fieldButtons; // Array von Feldern (Button-Komponenten)

    // Gewinnkombinationen für ein 4x4 Spielfeld
    int[] reihe1;
    int[] reihe2;
    int[] reihe3;
    int[] reihe4;
    int[] spalte1;
    int[] spalte2;
    int[] spalte3;
    int[] spalte4;
    int[] diagonale1;
    int[] diagonale2;
    int[] diagonale3;
    int[] diagonale4;

    public int[][] winningCombinations;

    // Liste der gesperrten Felder
    private int[] blockedFields = new int[3];
    private bool blockedFieldsReleased = false; // Flag, ob gesperrte Felder nach den ersten Zügen freigegeben wurden

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        infoText.text = "Player 1";
        SetUpWinningCombinations();

        // Sperre zufällige Felder
        BlockRandomFields();

        restartButton.onClick.AddListener(RestartGame);
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(RestartGame);
    }

    // Wird aufgerufen, wenn ein Button im Spielfeld geklickt wird
    public void OnButtonClickedInManager(FieldButton fieldButton)
    {
        if (gameEnded) return; // Wenn das Spiel schon zu Ende ist, nichts tun

        // Überprüfen, ob das Feld gesperrt ist
        if (System.Array.Exists(blockedFields, element => element == fieldButton.index))
        {
            infoText.text = "This field is blocked!";
            return; // Wenn das Feld gesperrt ist, nichts tun
        }

        // Den Button mit dem aktuellen Spieler belegen
        fieldButton.SetField(currentPlayer);
        currentPlayer = currentPlayer == 1 ? 0 : 1; // Wechsel zum nächsten Spieler

        string symbol = currentPlayer == 0 ? "Player 1" : "Player 2";
        infoText.text = symbol + ""; // Text aktualisieren

        // Wenn beide Spieler ihre Züge gemacht haben, hebe die Sperrung der Felder auf
        if (!blockedFieldsReleased && currentPlayer == 0)
        {
            ReleaseBlockedFields();
            blockedFieldsReleased = true;
        }

        CheckForWin(); // Überprüfen, ob der aktuelle Spieler gewonnen hat
        CheckForDraw(); // Überprüfen, ob es ein Unentschieden gibt
    }

    // Überprüft, ob ein Spieler gewonnen hat
    void CheckForWin()
    {
        foreach (var combination in winningCombinations)
        {
            if (fieldButtons[combination[0]].Player == fieldButtons[combination[1]].Player &&
                fieldButtons[combination[1]].Player == fieldButtons[combination[2]].Player &&
                fieldButtons[combination[2]].Player == fieldButtons[combination[3]].Player &&
                fieldButtons[combination[0]].Player != -1) // -1 bedeutet, das Feld ist leer
            {
                // Ein Spieler hat gewonnen
                string winner = fieldButtons[combination[0]].Player == 0 ? "Player 1" : "Player 2";
                infoText.text = winner + " wins!";
                gameEnded = true; // Spiel beendet
                return;
            }
        }
    }

    // Überprüft, ob das Spiel Unentschieden endet
    void CheckForDraw()
    {
        // Überprüfen, ob alle Felder belegt sind
        foreach (var button in fieldButtons)
        {
            if (button.Player == -1) // Wenn ein Feld noch leer ist
                return; // Kein Unentschieden, weiter spielen
        }

        // Wenn alle Felder belegt sind und kein Gewinner da ist, Unentschieden
        infoText.text = "It's a draw!";
        gameEnded = true; // Spiel beendet
    }

    // Setzt alle möglichen Gewinnkombinationen auf
    void SetUpWinningCombinations()
    {
        // Reihen
        reihe1 = new int[] { 0, 1, 2, 3 };
        reihe2 = new int[] { 4, 5, 6, 7 };
        reihe3 = new int[] { 8, 9, 10, 11 };
        reihe4 = new int[] { 12, 13, 14, 15 };

        // Spalten
        spalte1 = new int[] { 0, 4, 8, 12 };
        spalte2 = new int[] { 1, 5, 9, 13 };
        spalte3 = new int[] { 2, 6, 10, 14 };
        spalte4 = new int[] { 3, 7, 11, 15 };

        // Diagonalen
        diagonale1 = new int[] { 0, 5, 10, 15 };  // von oben links nach unten rechts
        diagonale2 = new int[] { 3, 6, 9, 12 };   // von oben rechts nach unten links
        diagonale3 = new int[] { 0, 4, 8, 12 };   // Diagonale von oben links nach unten links
        diagonale4 = new int[] { 3, 7, 11, 15 };  // Diagonale von oben rechts nach unten rechts

        // Alle Gewinnkombinationen zusammenstellen
        winningCombinations = new int[][]
        {
            reihe1, reihe2, reihe3, reihe4,
            spalte1, spalte2, spalte3, spalte4,
            diagonale1, diagonale2, diagonale3, diagonale4
        };
    }

    // Sperrt zufällig 3 Felder und zeigt "BLOCKED" an
    void BlockRandomFields()
    {
        System.Random rand = new System.Random();
        int blockedCount = 0;

        while (blockedCount < 3)
        {
            int randomIndex = rand.Next(fieldButtons.Length); // Zufälliger Index aus den Feldern

            // Überprüfen, ob das Feld bereits gesperrt ist
            if (System.Array.Exists(blockedFields, element => element == randomIndex))
                continue; // Wenn das Feld bereits gesperrt ist, überspringen

            blockedFields[blockedCount] = randomIndex; // Sperre das Feld
            fieldButtons[randomIndex].SetField(-2); // -2 könnte die Markierung für gesperrte Felder sein
            fieldButtons[randomIndex].SetButtonText("BLOCKED"); // Beschriftung des gesperrten Feldes mit "BLOCKED"
            blockedCount++;
        }
    }

    // Hebt die Sperrung der Felder nach den ersten Zügen auf
    void ReleaseBlockedFields()
    {
        foreach (int index in blockedFields)
        {
            fieldButtons[index].SetField(-1); // Setzt das Feld auf leer (-1)
            fieldButtons[index].SetButtonText(""); // Löscht die Beschriftung "BLOCKED"
        }
    }

    // Startet das Spiel neu
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // buildIndex ist eine Zahl. Man kann anstelle dessen aber auch .name schreiben
    }
}
