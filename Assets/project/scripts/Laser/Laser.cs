using UnityEngine;

public class Laser : MonoBehaviour
{
    public GameObject linePoint1;
    public GameObject linePoint2;
    public LineRenderer lineRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, linePoint1.transform.position);
        lineRenderer.SetPosition(1, linePoint2.transform.position);
    }
}
 