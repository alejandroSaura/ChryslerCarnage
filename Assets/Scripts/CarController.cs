using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour {

	// public
	public float enginePower;
	public float brakePower;
	public float dragConstant;
	public float rollConstant;

	// private

	Transform CenterOfMass;
	Transform FrontAxis;
	Transform RearAxis;

	Rigidbody _rigidbody;
	Vector3 resultForce;

	public float frontWeight;
	public float rearWeight;
	Vector3 gizmoPosition;

	// user controls
	public float userThrottle;
	public float userBrake;

	public float userLeftStickHorizontal;
	public float userLeftStickVertical;

	public float userRightStickHorizontal;
	public float userRightStickVertical;



	void Start () 
	{
		_rigidbody = GetComponent<Rigidbody>();
		CenterOfMass = transform.FindChild("CenterOfMass");
		FrontAxis = transform.FindChild("FrontAxis");
		RearAxis = transform.FindChild("RearAxis");
	}

	void OnGUI()
	{
		float velocity_forward = Vector3.Dot(_rigidbody.velocity, transform.forward);

		GUI.Box(new Rect(10,10,300,100), "Debug Data");
		GUI.TextArea(new Rect(15,30,200,40), "Forward velocity = " + velocity_forward*3.6f +" km/h");
	}

	void Update()
	{
		getUserInput();

		// calculate weight dynamic transfer ----------------------

		Vector3 CenterOfMassAligned = CenterOfMass.localPosition; // put the three objects in the same plane (y=0)
		CenterOfMassAligned.y = 0.0f;
		Vector3 FrontAxisAligned = FrontAxis.localPosition;
		FrontAxisAligned.y = 0.0f;
		Vector3 RearAxisAligned = RearAxis.localPosition;
		RearAxisAligned.y = 0.0f;

		float distanceToFront = (CenterOfMassAligned - FrontAxisAligned).magnitude;
		float distanceToRear = (CenterOfMassAligned - RearAxisAligned).magnitude;
		float wheelBase = (FrontAxisAligned - RearAxisAligned).magnitude;

		Vector3 acceleration = resultForce/_rigidbody.mass;
		float tangentialAcceleration = transform.InverseTransformDirection(acceleration).z;

		frontWeight = (distanceToRear/wheelBase) * _rigidbody.mass*Physics.gravity.y + (CenterOfMass.localPosition.y/wheelBase) * _rigidbody.mass * tangentialAcceleration;
		rearWeight = (distanceToFront/wheelBase) * _rigidbody.mass*Physics.gravity.y - (CenterOfMass.localPosition.y/wheelBase) * _rigidbody.mass * tangentialAcceleration;


		// for debugging
		float weight = _rigidbody.mass*Physics.gravity.y;
		float frontWeightPercent = frontWeight/weight;
		float rearWeightPercent = rearWeight/weight;
		gizmoPosition = FrontAxis.position*frontWeightPercent + RearAxis.position*rearWeightPercent;

		// ----------------------------------------------------------
	}

	void OnDrawGizmos()
	{
		// Debugging gizmos
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere( gizmoPosition + new Vector3(0.0f, 0.2f, 0.0f), 0.4f);
	}
	
	void FixedUpdate () 
	{
		float velocity_forward = Vector3.Dot(_rigidbody.velocity, transform.forward);

		// Calculate forces

		Vector3 tractionForce = transform.forward * enginePower * userThrottle; //TO-DO: don't affect engineForce with direct user input
		Vector3 brakeForce = - transform.forward * brakePower * userBrake;

		Debug.DrawLine(transform.position, transform.position + tractionForce/200.0f, Color.green);

		Vector3 dragForce = - transform.forward * (velocity_forward*velocity_forward) * dragConstant;
		Debug.DrawLine(transform.position, transform.position + dragForce/200.0f, Color.red);

		Vector3 rollingForce = - transform.forward * velocity_forward * rollConstant;
		Debug.DrawLine(transform.position, transform.position + rollingForce/200.0f, Color.yellow);

		resultForce = tractionForce + dragForce + rollingForce;

		if(velocity_forward > 0.1f) resultForce += brakeForce;
		if(Mathf.Abs(velocity_forward) < 0.1f && userBrake > 0.0f) _rigidbody.velocity = Vector3.zero; // to avoid small slidings

		// Apply forces
		_rigidbody.AddForce(resultForce);
	}


	void getUserInput ()
	{
		if (Input.GetJoystickNames ().Length != 0) 
		{ //XboxController
			userThrottle = Input.GetAxis ("RT");
			userBrake = Input.GetAxis ("LT");

			userLeftStickHorizontal = Input.GetAxis ("Horizontal");
			userLeftStickVertical = Input.GetAxis ("Vertical");

			userRightStickHorizontal = Input.GetAxis ("Horizontal2");
			userRightStickVertical = Input.GetAxis ("Vertical2");

//			if (Input.GetButton ("RB") && !Input.GetButton ("LB")) {
//				userRoll = -1f;
//			} else {
//				if (Input.GetButton ("LB") && !Input.GetButton ("RB")) {
//					userRoll = 1f;
//				} else {
//					userRoll = 0f;
//				}
//			}	
		} else 
		{ //Keyboard
			userThrottle = 0;
			userBrake = 0;
			if (Input.GetKey (KeyCode.W)) userThrottle = 1f;
			if(Input.GetKey (KeyCode.S)) userBrake = 1f;	

			if (Input.GetKey (KeyCode.D) && !Input.GetKey (KeyCode.A)) {
				userLeftStickHorizontal = 1f;
			} else {
				if (!Input.GetKey (KeyCode.D) && Input.GetKey (KeyCode.A)) {
					userLeftStickHorizontal = -1f;
				} else {
					userLeftStickHorizontal = 0f;
				}
			}	

//			if (Input.GetKey (KeyCode.W) && !Input.GetKey (KeyCode.S)) {
//				userLeftStickVertical = 1f;
//			} else {
//				if (!Input.GetKey (KeyCode.W) && Input.GetKey (KeyCode.S)) {
//					userLeftStickVertical = -1f;
//				} else {
//					userLeftStickVertical = 0f;
//				}
//			}	


			if (Input.GetKey (KeyCode.UpArrow) && !Input.GetKey (KeyCode.DownArrow)) {
				userRightStickVertical = 1f;
			} else {
				if (!Input.GetKey (KeyCode.UpArrow) && Input.GetKey (KeyCode.DownArrow)) {
					userRightStickVertical = -1f;
				} else {
					userRightStickVertical = 0f;
				}
			}	
			if (Input.GetKey (KeyCode.RightArrow) && !Input.GetKey (KeyCode.LeftArrow)) {
				userRightStickHorizontal = 1f;
			} else {
				if (!Input.GetKey (KeyCode.RightArrow) && Input.GetKey (KeyCode.LeftArrow)) {
					userRightStickHorizontal = -1f;
				} else {
					userRightStickHorizontal = 0f;
				}
			}	

//			if (Input.GetKey (KeyCode.Q) && !Input.GetKey (KeyCode.E)) {
//				userRoll = 1f;
//			} else {
//				if (!Input.GetKey (KeyCode.Q) && Input.GetKey (KeyCode.E)) {
//					userRoll = -1f;
//				} else {
//					userRoll = 0f;
//				}
//			}	
		}


	}


}
