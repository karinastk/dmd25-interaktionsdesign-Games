using UnityEngine;

public class PowerUp : MonoBehaviour
{
	public enum Type { BigPaddle, DoubleBall, Explosion }
	public Type powerUpType;

	public float duration = 5f;
	public float scaleMultiplier = 1.5f;

	void Start()
	{
		// Setzt den Tag automatisch beim Start
		gameObject.tag = "PowerUp";
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		// Prüft Kollision mit dem Paddle
		if (other.CompareTag("Paddle"))
		{
			// Sound über den Manager abspielen
			if (BreakoutManager.instance != null)
				BreakoutManager.instance.PlaySFX(BreakoutManager.instance.powerUpPickupSound);

			Breakout.BreakoutPaddle paddle = other.GetComponent<Breakout.BreakoutPaddle>();

			if (paddle != null)
			{
				// Aktiviert den entsprechenden Effekt je nach Typ
				if (powerUpType == Type.BigPaddle)
					paddle.ActivateBigPaddle(duration, scaleMultiplier);
				else if (powerUpType == Type.DoubleBall)
					paddle.ActivateDoubleBall(duration);
				else if (powerUpType == Type.Explosion)
					BreakoutManager.instance.ActivateExplosion(10f);
			}

			// Item nach dem Einsammeln löschen
			Destroy(gameObject);
		}
	}
}