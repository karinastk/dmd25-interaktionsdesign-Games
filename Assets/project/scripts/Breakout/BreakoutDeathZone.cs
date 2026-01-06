using UnityEngine;

public class BreakoutDeathZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        // Lebenspunkt abziehen und neuen Ball spawnen
        BreakoutManager.instance.OnDeath();

        // diesen Ball zerstören
        Destroy(other.gameObject);
    }
}