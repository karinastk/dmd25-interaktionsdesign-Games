using UnityEngine;

public class PongGoal : MonoBehaviour
{
    public PongManager.Player enemyPlayer;


    private void OnTriggerExit2D(Collider2D other)
    {
        //Punkt vergeben und neuen Ball spawnen
        PongManager.instance.OnGoalScored(enemyPlayer);


        //Gameobject breaken, wir wollen das andere gameobject zerstören
        Destroy(other.gameObject);
    }

}
