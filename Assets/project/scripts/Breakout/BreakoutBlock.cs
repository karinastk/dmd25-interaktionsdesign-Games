using UnityEngine;

public class BreakoutBlock : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }


}
