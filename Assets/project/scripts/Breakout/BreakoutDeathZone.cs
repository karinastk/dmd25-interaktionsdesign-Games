using UnityEngine;

public class BreakoutDeathZone : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		// Wenn der Hauptball reinfällt: Leben abziehen & Reset
		if (other.CompareTag("Ball"))
		{
			BreakoutManager.instance.OnDeath();
		}
		// Wenn ein Power-Up reinfällt: Einfach löschen (Aufräumen)
		else if (other.CompareTag("PowerUp"))
		{
			Destroy(other.gameObject);
		}
	}
}