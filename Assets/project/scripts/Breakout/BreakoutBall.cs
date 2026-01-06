using UnityEngine;

public class BreakoutBall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
        {        
           float startVelocity = BreakoutManager.instance.ballStartVelocity;

           GetComponent<Rigidbody2D>().linearVelocity =
                new Vector2(0, startVelocity);

        }

        // Update is called once per frame
        void Update()
    {
        
    }
}
