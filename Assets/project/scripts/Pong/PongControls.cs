using UnityEngine;

namespace Pong
{
	[System.Serializable]
	public class PongControls
	{
		public KeyCode upKey = KeyCode.W;
		public KeyCode downKey = KeyCode.S;
		public float paddleSpeed = 10f;
		public float minY = -4.5f;
		public float maxY = 4.5f;
	}
}