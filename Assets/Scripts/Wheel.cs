using UnityEngine;
using System.Collections;

public class Wheel : MonoBehaviour 
{
	public float wheelRadius = 0.34f;

	public float supportedWeight;
	public float driveTorque;
	public Vector3 driveForce;

	public float angularVelocity;

	public Rigidbody carRB;

	Transform wheelGeometry;

	void Start()
	{
		wheelGeometry = transform.FindChild("wheel");
	}

	void Update()
	{
		// rotate the mesh
		angularVelocity = transform.InverseTransformDirection(carRB.velocity).z / wheelRadius;
		wheelGeometry.Rotate(0.0f, -angularVelocity * Time.deltaTime *Mathf.Rad2Deg ,0.0f);
	}
	
	void FixedUpdate () 
	{
		driveForce = (driveTorque/wheelRadius) * transform.forward;

		// Apply forces to the car
		carRB.AddForce(driveForce);
		Debug.DrawLine(transform.position, transform.position + transform.forward*(driveTorque/wheelRadius)/200.0f, Color.green);
	}
}
