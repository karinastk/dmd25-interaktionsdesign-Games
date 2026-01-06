using UnityEngine;
using UnityEngine;

[System.Serializable]
public class BreakoutControls
{
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public float paddleSpeed;
    public float minX = -7.5f;
    public float maxX = 7.5f;
}
