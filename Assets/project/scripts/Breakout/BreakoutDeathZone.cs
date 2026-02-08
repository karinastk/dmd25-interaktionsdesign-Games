using UnityEngine;

public class BreakoutDeathZone : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Ball")) BreakoutManager.instance.OnDeath();
		else if (other.CompareTag("PowerUp")) Destroy(other.gameObject);
	}
}