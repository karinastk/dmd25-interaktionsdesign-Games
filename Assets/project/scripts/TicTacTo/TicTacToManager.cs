using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TicTacToManager : MonoBehaviour
{
	// ================= UI ELEMENTE =================
	public TextMeshProUGUI infoText; // Zeigt aktuelle Spielerinfo, Gewinn, Draw etc.
	public Button restartButton;     // Neustart-Button

	// Bomben-Buttons für Spieler 1 und Spieler 2
	public Button bombButtonPlayer1;
	public Button bombButtonPlayer2;

	// Feature-Buttons für Spieler 1 und Spieler 2
	public Button[] featureButtonsPlayer1;
	public Button[] featureButtonsPlayer2;

	// Alle Spielfeld-Buttons (16 Felder)
	public FieldButton[] fieldButtons;

	[Header("Start Banner")]
	public GameObject startBanner; // Banner zu Spielstart

	[Header("Sound")]
	public AudioSource audioSource;       // AudioSource für Effekte
	public AudioClip placeSound;          // Sound für X/O
	public AudioClip startBannerSound;    // Sound für Start-Banner
	public AudioClip bombSound;           // Sound beim Zerstören von Feldern
	public AudioClip winSound;            // Sound bei Sieg
	public AudioClip drawSound;           // Sound bei Unentschieden
	public AudioClip clickSound;          // Sound für Buttonklicks (Feature, Bomb, Restart)

	[Header("Background Music")]
	public AudioSource musicSource;       // Separate AudioSource für Musik
	public AudioClip backgroundMusic;     // Hintergrundmusikclip

	// ================= SPIELSTATUS =================
	int currentPlayer = 0;  // 0 = Spieler 1, 1 = Spieler 2
	bool gameEnded = false; // True wenn das Spiel gewonnen oder unentschieden ist
	bool gameStarted = false; // True wenn Spiel gestartet wurde

	// ================= FEATURES =================
	public enum FeatureType { BlockField, OverwriteEnemy, ExtraTurn }
	private List<FeatureType>[] playerFeatures = new List<FeatureType>[2]; // Liste der Features für jeden Spieler
	private FeatureType? activeFeature = null; // Momentan ausgewähltes Feature

	private FieldButton blockedField = null; // Feld, das blockiert wurde
	private bool blockPendingRelease = false; // Wird benutzt, um Block nach einem Zug zu entfernen
	private int extraTurnsRemaining = 0; // Extra-Züge von Feature "ExtraTurn"

	// ================= BOMB =================
	private bool isBombActive = false; // True, wenn Spieler eine Bombe aktiviert hat
	private bool[] bombUsed = new bool[2]; // Prüft, ob ein Spieler seine Bombe schon benutzt hat
	private int[] movesPerPlayer = new int[2]; // Zählt Züge pro Spieler, um Bombe freizuschalten

	// ================= WIN / DRAW =================
	int[][] winningCombinations; // Alle möglichen Gewinnlinien (4 in a row für 4x4 Feld)

	void Start()
	{
		// Start-Banner aktivieren
		if (startBanner != null)
			startBanner.SetActive(true);

		infoText.text = "Click Start";

		// Gewinnlinien vorbereiten
		SetUpWinningCombinations();

		// Features zufällig den Spielern zuweisen
		AssignRandomFeatures();

		// Feature-Buttons vorbereiten und beschriften
		SetupFeatureButtons();

		// Restart Button: Spiel neu starten mit Sound
		restartButton.onClick.AddListener(() => StartCoroutine(RestartWithSound()));

		// Bomb Buttons: Sound abspielen + Bombe aktivieren
		bombButtonPlayer1.onClick.AddListener(() => { PlayClickSound(); ActivateBomb(0); });
		bombButtonPlayer2.onClick.AddListener(() => { PlayClickSound(); ActivateBomb(1); });

		// Hintergrundmusik starten, falls gesetzt
		if (musicSource != null && backgroundMusic != null)
		{
			musicSource.clip = backgroundMusic;
			musicSource.loop = true;
			musicSource.Play();
		}
	}

	// ================= START =================
	public void StartGame()
	{
		gameStarted = true;

		// Banner ausblenden
		if (startBanner != null)
			startBanner.SetActive(false);

		infoText.text = "Player 1"; // Spieler 1 beginnt

		// Start Banner Sound
		if (audioSource != null && startBannerSound != null)
			audioSource.PlayOneShot(startBannerSound);
	}

	// ================= FEATURE SETUP =================
	void AssignRandomFeatures()
	{
		// Mögliche Features
		FeatureType[] all = { FeatureType.BlockField, FeatureType.OverwriteEnemy, FeatureType.ExtraTurn };
		System.Random rand = new System.Random();

		for (int p = 0; p < 2; p++)
		{
			playerFeatures[p] = new List<FeatureType>();
			List<FeatureType> pool = new List<FeatureType>(all);

			// Zwei verschiedene Features pro Spieler auswählen
			while (playerFeatures[p].Count < 2)
			{
				int r = rand.Next(pool.Count);
				playerFeatures[p].Add(pool[r]);
				pool.RemoveAt(r);
			}
		}
	}

	void SetupFeatureButtons()
	{
		// Buttons für beide Spieler einrichten
		SetupButtonsForPlayer(featureButtonsPlayer1, 0);
		SetupButtonsForPlayer(featureButtonsPlayer2, 1);
	}

	void SetupButtonsForPlayer(Button[] buttons, int player)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (i >= playerFeatures[player].Count) break;

			FeatureType feature = playerFeatures[player][i];
			TextMeshProUGUI tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();

			// Buttontext setzen
			tmp.text = feature == FeatureType.OverwriteEnemy ? "Overwrite\nEnemy" : feature.ToString();

			int index = i;
			buttons[i].onClick.AddListener(() =>
			{
				PlayClickSound(); // Sound beim Klicken eines Feature-Buttons
				ActivateFeature(player, feature, buttons[index]);
			});
		}
	}

	void ActivateFeature(int player, FeatureType feature, Button btn)
	{
		// Prüfen, ob Spieler dran ist und Spiel läuft
		if (currentPlayer != player || gameEnded || !gameStarted) return;

		activeFeature = feature;
		btn.interactable = false; // Button nach Nutzung deaktivieren

		if (feature == FeatureType.ExtraTurn && extraTurnsRemaining == 0)
			extraTurnsRemaining = 1;

		infoText.text = "Player " + (player + 1) + " uses " + feature;
	}

	// ================= GAME LOGIC =================
	public void OnButtonClickedInManager(FieldButton field)
	{
		if (!gameStarted || gameEnded) return;

		if (isBombActive)
		{
			HandleBomb(); // Wenn Bombe aktiv, Felder löschen
			return;
		}

		if (activeFeature.HasValue)
		{
			ApplyFeature(field); // Feature auf das Feld anwenden
			return;
		}

		// Normales Feld setzen, wenn frei
		if (field.Player != -1 || field.IsBlocked) return;

		field.SetField(currentPlayer);
		PlayPlaceSound(); // Sound für X oder O

		movesPerPlayer[currentPlayer]++;
		EndTurn(); // Nächster Spieler
	}

	void ApplyFeature(FieldButton field)
	{
		switch (activeFeature.Value)
		{
			case FeatureType.BlockField:
				if (field.Player == -1)
				{
					if (blockedField != null)
						blockedField.ReleaseBlock(); // Vorherigen Block entfernen

					field.SetBlocked(); // Feld blockieren
					blockedField = field;
					blockPendingRelease = false;

					PlayPlaceSound(); // Sound beim Block setzen
				}
				EndTurn();
				break;

			case FeatureType.OverwriteEnemy:
				if (field.Player != -1 && field.Player != currentPlayer)
				{
					field.ForceSetField(currentPlayer); // Gegnerfeld überschreiben
					PlayPlaceSound();
				}
				EndTurn();
				break;

			case FeatureType.ExtraTurn:
				field.SetField(currentPlayer);
				PlayPlaceSound();
				if (extraTurnsRemaining == 0)
					extraTurnsRemaining = 1;
				EndTurn();
				break;
		}

		activeFeature = null; // Feature zurücksetzen
	}

	// ================= SOUND =================
	void PlayPlaceSound() { if (audioSource != null && placeSound != null) { audioSource.pitch = Random.Range(0.95f, 1.05f); audioSource.PlayOneShot(placeSound); } }
	void PlayClickSound() { if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound); }
	void PlayBombSound() { if (audioSource != null && bombSound != null) audioSource.PlayOneShot(bombSound); }
	void PlayWinSound() { if (audioSource != null && winSound != null) audioSource.PlayOneShot(winSound); }
	void PlayDrawSound() { if (audioSource != null && drawSound != null) audioSource.PlayOneShot(drawSound); }

	// ================= TURN =================
	void EndTurn()
	{
		CheckForWinOrDraw();
		if (gameEnded) return;

		if (extraTurnsRemaining > 0)
		{
			extraTurnsRemaining--;
			infoText.text = "Extra Turn!"; // Extra-Zug anzeigen
			return;
		}

		currentPlayer = currentPlayer == 0 ? 1 : 0; // Spieler wechseln
		infoText.text = "Player " + (currentPlayer + 1);

		// Blockfeld nach einem Zug eventuell freigeben
		if (blockPendingRelease && blockedField != null)
		{
			blockedField.ReleaseBlock();
			blockedField = null;
			blockPendingRelease = false;
		}

		if (blockedField != null && !blockPendingRelease)
			blockPendingRelease = true;
	}

	// ================= BOMB =================
	void ActivateBomb(int player)
	{
		// Prüfen ob Bombe verfügbar ist
		if (bombUsed[player] || currentPlayer != player || movesPerPlayer[player] < 3)
			return;

		bombUsed[player] = true;
		isBombActive = true;

		if (player == 0) bombButtonPlayer1.interactable = false;
		else bombButtonPlayer2.interactable = false;

		infoText.text = "Player " + (player + 1) + " activated bomb!";
	}

	void HandleBomb()
	{
		int removed = 0;
		System.Random rand = new System.Random();

		// Drei zufällige Felder löschen
		while (removed < 3)
		{
			int r = rand.Next(fieldButtons.Length);
			FieldButton fb = fieldButtons[r];

			if (fb.Player != -1 && !fb.IsBlocked)
			{
				fb.ResetField();
				removed++;
			}
		}

		isBombActive = false;
		PlayBombSound(); // Sound nach dem Löschen der Felder
		EndTurn();
	}

	// ================= WIN / DRAW =================
	void CheckForWinOrDraw()
	{
		// Gewinn prüfen
		foreach (var c in winningCombinations)
		{
			int p = fieldButtons[c[0]].Player;
			if (p == -1) continue;

			if (fieldButtons[c[1]].Player == p &&
				fieldButtons[c[2]].Player == p &&
				fieldButtons[c[3]].Player == p)
			{
				infoText.text = "Player " + (p + 1) + " wins!";
				gameEnded = true;
				PlayWinSound(); // Sound beim Sieg
				return;
			}
		}

		// Prüfen auf Unentschieden
		foreach (var fb in fieldButtons)
			if (fb.Player == -1) return;

		infoText.text = "Draw!";
		gameEnded = true;
		PlayDrawSound(); // Sound beim Unentschieden
	}

	void SetUpWinningCombinations()
	{
		// Alle Gewinnlinien für 4x4-Feld (horizontal, vertikal, diagonal)
		winningCombinations = new int[][]
		{
			new int[]{0,1,2,3},
			new int[]{4,5,6,7},
			new int[]{8,9,10,11},
			new int[]{12,13,14,15},
			new int[]{0,4,8,12},
			new int[]{1,5,9,13},
			new int[]{2,6,10,14},
			new int[]{3,7,11,15},
			new int[]{0,5,10,15},
			new int[]{3,6,9,12}
		};
	}

	// ================= RESTART =================
	private IEnumerator RestartWithSound()
	{
		PlayClickSound(); // Sound vor Neustart
		if (audioSource != null && clickSound != null)
			yield return new WaitForSeconds(clickSound.length);

		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Szene neu laden
	}
}
