using UnityEngine;

namespace Pong
{
    [System.Serializable]

    public class PongControls
    {
        public KeyCode upKey = KeyCode.W;
        public KeyCode downKey = KeyCode.S;
        public float paddleSpeed;
        public float minY = -3f;
        public float maxY = 3f;
        
    }

}