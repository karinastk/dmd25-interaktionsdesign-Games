using UnityEngine;

public class PowerUp : MonoBehaviour
{
	public enum Type { BigPaddle, DoubleBall, Explosion }
	public Type powerUpType;

	public float duration = 5f;
	public float scaleMultiplier = 1.5f;

	void Start() { gameObject.tag = "PowerUp"; }

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Paddle"))
		{
			Breakout.BreakoutPaddle paddle = other.GetComponent<Breakout.BreakoutPaddle>();
			if (paddle != null)
			{
				if (powerUpType == Type.BigPaddle)
					paddle.ActivateBigPaddle(duration, scaleMultiplier);
				else if (powerUpType == Type.DoubleBall)
					paddle.ActivateDoubleBall(duration);
				else if (powerUpType == Type.Explosion)
					BreakoutManager.instance.ActivateExplosion(10f);
			}
			Destroy(gameObject);
		}
	}
}