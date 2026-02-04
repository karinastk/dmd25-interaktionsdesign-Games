using UnityEngine;

public class BreakoutBlock : MonoBehaviour
{
	[Header("Block Hits")]
	public int maxHits = 1;
	private int currentHits;
	private int startMaxHits;

	[Header("Crack Sprites (light to strong)")]
	public Sprite[] damageSprites;

	[Header("Block Color")]
	public Color blockColor = Color.white;

	private SpriteRenderer sr;
	private Sprite startSprite;

	void Awake()
	{
		sr = GetComponent<SpriteRenderer>();

		startMaxHits = maxHits;
		currentHits = maxHits;

		sr.color = blockColor;
		startSprite = sr.sprite;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!collision.gameObject.CompareTag("Ball"))
			return;

		TakeHit();
	}

	void TakeHit()
	{
		currentHits--;

		if (currentHits <= 0)
		{
			BreakoutManager.instance.OnBlockDestroyed();
			gameObject.SetActive(false);
			return;
		}

		UpdateCracks();
	}

	void UpdateCracks()
	{
		int index = maxHits - currentHits - 1;

		if (index >= 0 && index < damageSprites.Length)
			sr.sprite = damageSprites[index];
	}

	public void ResetBlock()
	{
		maxHits = startMaxHits;
		currentHits = maxHits;
		sr.sprite = startSprite;
		gameObject.SetActive(true);
	}
}
