using UnityEngine;

public class PongGoal : MonoBehaviour
{
	public PongManager.Player enemyPlayer;

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Ball"))
		{
			PongManager.instance.OnGoalScored(enemyPlayer);
			Destroy(other.gameObject);
		}
	}
}