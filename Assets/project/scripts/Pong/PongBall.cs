using UnityEngine;

public class PongBall : MonoBehaviour
{
    // Speed vom Ball
    public float velocity = 5;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //hiermit kann man die Random-Berechnung mehr random machen
        Random.InitState(System.DateTime.Now.Millisecond);

        float side = Random.Range(0, 2);
        side = side == 0 ? 1 : -1;
        float angle = Random.Range(-1f, 1f);

        float startVelocity = PongManager.instance.ballStartVelocity;

        GetComponent<Rigidbody2D>().linearVelocity = 
            new Vector2(side * startVelocity, angle * startVelocity);
         
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
