using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public static CameraController instance;

	public Transform target;

	public float speed, velSpeed, velUpdateSpeed, lookMod;
	public int velDelay;
	public List<Vector2> velocities = new List<Vector2>();
	public Vector2 prevVel, currentVel;

	// Start is called before the first frame update
	void Start()
	{
		if (instance != null) Destroy(gameObject);
		else
		{
			instance = this;
			InvokeRepeating("UpdateVelocities", 0, velUpdateSpeed);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	void FixedUpdate()
	{
		Vector2 targetPos = target.position;
		prevVel = currentVel;
		if (velocities.Count > 0)
		{
			currentVel = (prevVel * velSpeed + velocities[0]) / (velSpeed + 1);
		}
		else currentVel = new Vector2(0, 0);
		targetPos += currentVel;

		if (Input.GetKey(KeyCode.S)) targetPos.y -= lookMod;
		if (Input.GetKey(KeyCode.W)) targetPos.y += lookMod;

		Debug.DrawRay(new Vector2(targetPos.x - 0.5f, targetPos.y - 0.5f), new Vector2(1f, 1f), Color.yellow);
		Debug.DrawRay(new Vector2(targetPos.x - 0.5f, targetPos.y + 0.5f), new Vector2(1f, -1f), Color.yellow);

		Vector2 dir = targetPos - (Vector2)transform.position;
		dir.Normalize();
		dir *= speed;

		if (Mathf.Abs(dir.x) > Mathf.Abs(targetPos.x - transform.position.x)) dir.x = targetPos.x - transform.position.x;
		if (Mathf.Abs(dir.y) > Mathf.Abs(targetPos.y - transform.position.y)) dir.y = targetPos.y - transform.position.y;

		transform.Translate(dir);
	}

	public void UpdateVelocities()
	{
		if (velocities.Count > velDelay) velocities.RemoveAt(0);

		velocities.Add(target.GetComponent<Rigidbody2D>().velocity);
	}
}
