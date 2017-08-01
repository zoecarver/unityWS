using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public int force = 10;

	private Rigidbody rigidBody;

	void Start ()
	{
		rigidBody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate ()
	{
		float horizontalForce = Input.GetAxis ("Horizontal");
		float verticalForce = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (horizontalForce, 0.0f, verticalForce);

		rigidBody.AddForce (movement * force);
	}
}
