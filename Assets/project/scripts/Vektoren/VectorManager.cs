using UnityEngine;

public class VectorManager : MonoBehaviour
{
    public GameObject ballObject;
    public GameObject ballFollowObject;


    public string ballDebugString;
    public Vector3 ballPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        //ballObject.transform.position = ballFollowObject.transform.position;

        ballObject.transform.position =
        Vector3.Lerp(
            ballObject.transform.position,
            ballFollowObject.transform.position,
            Time.deltaTime);
    }
}
 