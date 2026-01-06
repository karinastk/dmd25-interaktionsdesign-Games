using UnityEngine;

public class VectorVisiuals : MonoBehaviour
{
    public VectorManager vectorManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(vectorManager.ballDebugString + vectorManager.ballPosition); 
    }
}
