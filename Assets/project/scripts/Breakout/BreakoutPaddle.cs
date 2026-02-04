using UnityEngine;

namespace Breakout
{
	public class BreakoutPaddle : MonoBehaviour
	{
		public BreakoutControls controls;

		private Vector3 startPosition;

		public enum Direction
		{
			Left,
			Right
		}

		void Awake()
		{
			startPosition = transform.position;
		}

		void Update()
		{
			if (!BreakoutManager.instance.GameStarted)
				return;

			if (Input.GetKey(controls.leftKey))
				Move(Direction.Left);

			if (Input.GetKey(controls.rightKey))
				Move(Direction.Right);
		}

		void Move(Direction direction)
		{
			float moveDistance = controls.paddleSpeed * Time.deltaTime;
			moveDistance *= direction == Direction.Right ? 1 : -1;

			float newX = transform.position.x + moveDistance;

			if (newX > controls.maxX)
				newX = controls.maxX;
			else if (newX < controls.minX)
				newX = controls.minX;

			transform.position = new Vector3(newX, transform.position.y, transform.position.z);
		}

		public void ResetPaddle()
		{
			transform.position = startPosition;
		}
	}
}
