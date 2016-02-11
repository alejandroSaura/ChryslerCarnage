using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour {

	// public

	public AnimationCurve enginePowerTorqueCurve;

	public float brakePower;
	public float dragConstant;
	public float rollConstant;

	public float differentialRatio = 3.42f;
	public float transmissionEfficiency = 0.7f; // guess

	public float minRPM = 1000.0f;
	public float maxRPM = 6000.0f;

	public float[] gearRatios = {2.9f, 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f}; // 0 = reverse

	public float wheelFrictionCoefficient = 1.0f;

	// private

	float gearRatio;
	float rpm;
	float engineTorque;

	float driveTorque;

	Transform CenterOfMass;
	Transform FrontAxis;
	Transform RearAxis;

    Vector3 centerOfMassPos;

	WheelExperiment frontLeftWheel;
    WheelExperiment frontRightWheel;
    WheelExperiment backLeftWheel;
    WheelExperiment backRightWheel;

	Rigidbody _rigidbody;
	Vector3 frictionForces;
	Vector3 totalForceApplied;

	public float frontWeight;
	public float rearWeight;
	Vector3 gizmoPosition;

	// driver controls
	InputInterface input;


	void Start () 
	{
		input = gameObject.GetComponent<InputInterface>();

		_rigidbody = GetComponent<Rigidbody>();
		CenterOfMass = transform.FindChild("CenterOfMass");
        centerOfMassPos = CenterOfMass.position;
		FrontAxis = transform.FindChild("FrontAxis");
		RearAxis = transform.FindChild("RearAxis");

		// wheels initialization
		frontLeftWheel = transform.FindChild("frontLeftWheel").GetComponent<WheelExperiment>();
		frontLeftWheel.carRB = _rigidbody;
		frontRightWheel = transform.FindChild("frontRightWheel").GetComponent<WheelExperiment>();
		frontRightWheel.carRB = _rigidbody;
		backLeftWheel = transform.FindChild("backLeftWheel").GetComponent<WheelExperiment>();
		backLeftWheel.carRB = _rigidbody;
		backRightWheel = transform.FindChild("backRightWheel").GetComponent<WheelExperiment>();
		backRightWheel.carRB = _rigidbody;

		gearRatio = gearRatios[1]; // first gear
	}

	void OnGUI()
	{
		float velocity_forward = Vector3.Dot(_rigidbody.velocity, transform.forward);

		GUI.Box(new Rect(10,10,300,200), "Debug Data");
		GUI.TextArea(new Rect(15,30,290,20), "Forward velocity = " + velocity_forward*3.6f +" km/h");

		Vector3 acceleration = totalForceApplied/_rigidbody.mass;
		float tangentialAcceleration = transform.InverseTransformDirection(acceleration).z;
		GUI.TextArea(new Rect(15,60,290,20), "Forward acceleration = " + tangentialAcceleration +" m/s^2");

		GUI.TextArea(new Rect(15,90,290,20), "RPM = " + rpm);

		float tangentialForce = transform.InverseTransformDirection(totalForceApplied).z;
		GUI.TextArea(new Rect(15,120,290,20), "Total tangential Force = " + tangentialForce);
	}

	void Update()
	{
		//getUserInput();

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

		Vector3 acceleration = totalForceApplied/_rigidbody.mass;
		float tangentialAcceleration = transform.InverseTransformDirection(acceleration).z;

		frontWeight = (distanceToRear/wheelBase) * _rigidbody.mass*Physics.gravity.y + (CenterOfMass.localPosition.y/wheelBase) * _rigidbody.mass * tangentialAcceleration;
		rearWeight = (distanceToFront/wheelBase) * _rigidbody.mass*Physics.gravity.y - (CenterOfMass.localPosition.y/wheelBase) * _rigidbody.mass * tangentialAcceleration;

		// for debugging
		float weight = frontWeight + rearWeight;
		float frontWeightPercent = frontWeight/weight;
		float rearWeightPercent = rearWeight/weight;
        //centerOfMassPos = Vector3.Lerp(centerOfMassPos, FrontAxis.localPosition * frontWeightPercent + RearAxis.localPosition * rearWeightPercent, Time.deltaTime);

        //gizmoPosition = CenterOfMass.position + centerOfMassPos;
        gizmoPosition = FrontAxis.position* frontWeightPercent +RearAxis.position * rearWeightPercent;

        // ----------------------------------------------------------

        // transfer weight to the wheels (so they can check if they slide)

        frontLeftWheel.supportedWeight = frontWeight/2;
		frontRightWheel.supportedWeight = frontWeight/2;
		backLeftWheel.supportedWeight = rearWeight/2;
		backRightWheel.supportedWeight = rearWeight/2;

		// rpm calculus from wheels angular velocity
		rpm = backLeftWheel.angularVelocity * gearRatio * differentialRatio * 60.0f/(2.0f * Mathf.PI);
		if(rpm < minRPM) rpm = minRPM; // don't let the engine go under the minimun rpm

		// user uses a percentage of the maxEngineTorque
		float maxEngineTorque = enginePowerTorqueCurve.Evaluate(rpm/maxRPM) * 600.0f;
		engineTorque = input.userThrottle * maxEngineTorque;

		// TO-DO: calculate which gear to use
		gearRatio = gearRatios[1]; // first gear
		driveTorque = engineTorque * gearRatio * differentialRatio * transmissionEfficiency; // / wheelRadius;

		// transfer driveTorque to the wheels, they will apply the force (if not sliding)
		frontLeftWheel.driveTorque = driveTorque/4;
		frontRightWheel.driveTorque = driveTorque/4;
		backLeftWheel.driveTorque = driveTorque/4;
		backRightWheel.driveTorque = driveTorque/4;

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

		Vector3 brakeForce = - transform.forward * brakePower * input.userBrake; //TO-DO: this should be done by the wheels

		Vector3 dragForce = - transform.forward * (velocity_forward*velocity_forward) * dragConstant;
		Debug.DrawLine(transform.position, transform.position + dragForce/200.0f, Color.red);

		Vector3 rollingForce = - transform.forward * velocity_forward * rollConstant;
		Debug.DrawLine(transform.position, transform.position + rollingForce/200.0f, Color.yellow);

		frictionForces = dragForce + rollingForce;

		if(velocity_forward > 0.1f) frictionForces += brakeForce;
		//if(Mathf.Abs(velocity_forward) < 0.1f && input.userBrake > 0.0f) _rigidbody.velocity = Vector3.zero; // to avoid small slidings

		// Apply friction forces
		_rigidbody.AddForce(frictionForces);

		totalForceApplied = frictionForces 
			- (frontLeftWheel.tractionForce
			+ frontRightWheel.tractionForce
            + backLeftWheel.tractionForce
            + backRightWheel.tractionForce) * transform.forward;
	}


}
