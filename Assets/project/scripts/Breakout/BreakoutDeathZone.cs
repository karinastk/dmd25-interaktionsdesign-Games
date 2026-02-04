using UnityEngine;

public class BreakoutDeathZone : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Ball"))
			return;

		BreakoutManager.instance.OnDeath();
	}
}
