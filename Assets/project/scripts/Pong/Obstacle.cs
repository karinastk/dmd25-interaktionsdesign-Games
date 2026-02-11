using UnityEngine;

public class Obstacle : MonoBehaviour
{
	public enum MovementDirection { Vertical, Horizontal }

	[Header("Movement Settings")]
	public MovementDirection direction = MovementDirection.Vertical;
	public float speed = 3f;
	public float range = 3f;

	[Tooltip("Nutze 3.14 (PI) für die entgegengesetzte Richtung")]
	public float phaseOffset = 0f;

	private Vector3 startPos;

	void Start()
	{
		// Speichert die Ausgangsposition für die Pendelbewegung
		startPos = transform.position;
	}

	void Update()
	{
		// Berechnet die Hin-und-Her-Bewegung mittels Sinus-Kurve
		float offset = Mathf.Sin((Time.time * speed) + phaseOffset) * range;

		// Weist den Versatz je nach gewählter Richtung der X- oder Y-Achse zu
		if (direction == MovementDirection.Vertical)
		{
			transform.position = new Vector3(startPos.x, startPos.y + offset, startPos.z);
		}
		else
		{
			transform.position = new Vector3(startPos.x + offset, startPos.y, startPos.z);
		}
	}
}