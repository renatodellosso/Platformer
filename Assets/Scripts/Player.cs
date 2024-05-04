using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

	public static Player instance;

	public Vector3 checkpoint;
	public Image fader;
	public Text stats;

	public Rigidbody2D rb;

	public float walkSpeed, jumpForce, movingDrag, standingDrag, groundCheckDist = .65f, fadeSpeed, startTime, airTime = 0, rawScore;
	public int jumps, maxJumps = 1, groundCheckIncrement = 3, deaths = 0, jumpCount = 0, lastCheckpoint;
	public bool isGrounded = false, dying = false;

	// Start is called before the first frame update
	void Start()
	{
		if (instance != null) Destroy(gameObject);
		else
		{
			instance = this;

			rb = GetComponent<Rigidbody2D>();
			checkpoint = transform.position;

			//fader = GameObject.Find("Fader").GetComponent<Image>();
			fader.canvasRenderer.SetAlpha(0);
			fader.gameObject.SetActive(true);

			jumps = maxJumps;

			startTime = Time.time;

			fader.CrossFadeAlpha(1, 0, false);
			FadeOutFromDeath();
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			fader.CrossFadeAlpha(1, fadeSpeed / 2, false);
			Invoke(nameof(ReloadScene), fadeSpeed / 2);
		}
	}

	void FixedUpdate()
	{
		Vector2 move = new Vector2();

		if (Input.GetKey(KeyCode.D)) move.x += walkSpeed;
		if (Input.GetKey(KeyCode.A)) move.x += -walkSpeed;

		if (Input.GetKey(KeyCode.Space) && jumps > 0)
		{
			jumpCount++;
			move.y += jumpForce * Mathf.Sign(rb.gravityScale);
			jumps--;
		}

		isGrounded = false;
		for (float x = transform.position.x - transform.lossyScale.x / 2;
			x <= transform.position.x + transform.lossyScale.x / 2;
			x += transform.lossyScale.x / (groundCheckIncrement - 1))
		{
			RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, transform.position.y - (transform.lossyScale.y / 2 - 0.02f) * Mathf.Sign(rb.gravityScale)),
				new Vector2(0, -1 * Mathf.Sign(rb.gravityScale)), groundCheckDist);
			if (hit.collider != null)
			{
				//print("Raycasted. Hit: " + hit.collider.name);
				Debug.DrawRay(new Vector2(x, transform.position.y - transform.lossyScale.y / 2), new Vector2(0, -1) * groundCheckDist, Color.green);
				isGrounded = true;
			}
			else
			{
				//print("Raycasted. Hit: null");
				Debug.DrawRay(new Vector2(x, transform.position.y - transform.lossyScale.y / 2), new Vector2(0, -1) * groundCheckDist, Color.red);
			}
		}

		if (move.x != 0 || move.y != 0 || !isGrounded) rb.drag = movingDrag;
		else rb.drag = standingDrag;

		rb.AddForce(move);

		if (!isGrounded) airTime += Time.deltaTime;

		float score = rawScore;

		float time = Time.time - startTime;
		float timeMult = Mathf.Pow(0.95f, Mathf.Log10(time + 1));
		float deathMult = Mathf.Pow(0.95f, deaths);
		float jumpMult = Mathf.Pow(0.995f, jumpCount);
		float airTimeMult = Mathf.Pow(0.95f, Mathf.Log10(airTime + 1));

		float scoreMult = timeMult * deathMult * jumpMult * airTimeMult;
		score *= scoreMult;

		stats.text = $"Score: {score:#,##0} ({Mathf.Round(scoreMult * 100)}%)\n" +
					 $"Time: {time:0.0}s ({Mathf.Round(timeMult * 100)}%)\n" +
					 $"Deaths: {deaths} ({Mathf.Round(deathMult * 100)}%)\n" +
					 $"Jumps: {jumpCount} ({Mathf.Round(jumpMult * 100)}%)\n" +
					 $"Air Time: {airTime:0.0}s ({Mathf.Round(airTimeMult * 100)}%)";
	}

	public void OnCollisionEnter2D(Collision2D collision)
	{
		if (isGrounded) jumps = maxJumps;
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Killbox" && !dying)
		{
			print("Player hit killbox");
			dying = true;
			fader.CrossFadeAlpha(1, fadeSpeed / 2, false);
			Invoke(nameof(FadeOutFromDeath), fadeSpeed / 2);
		}
		else if (collision.gameObject.tag == "Checkpoint")
		{
			print("Player reached checkpoint " + collision.gameObject.name);
			int checkPoint = int.Parse(collision.gameObject.name);
			if (checkPoint > lastCheckpoint)
			{
				lastCheckpoint = checkPoint;
				rawScore += checkPoint * 1000;

				if (checkPoint == 6)
					rb.gravityScale *= -1;
				else if (checkPoint == 7)
					rb.gravityScale *= -1;
			}
			checkpoint = collision.gameObject.transform.position;
		}
	}

	public void FadeOutFromDeath()
	{
		print("Fading out...");
		deaths++;
		transform.position = checkpoint;
		if (CameraController.instance != null)
		{
			CameraController.instance.transform.position = new Vector3(transform.position.x, transform.position.y, CameraController.instance.transform.position.z);
			CameraController.instance.velocities = new List<Vector2>();
			CameraController.instance.prevVel = new Vector2(0, 0);
		}
		fader.CrossFadeAlpha(0, fadeSpeed / 2, false);
		dying = false;
	}

	private void ReloadScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

}