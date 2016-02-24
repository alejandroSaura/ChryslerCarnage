using UnityEngine;
using System.Collections;

public class Wheel : MonoBehaviour 
{

    //for debug:
    public float numerator;

    public float wheelLinearVelocity;

    //Rigidbody _rigidbody;
	public float wheelRadius = 0.34f;
    public float mass = 75f;

    public float tractionFactor = 10f;
    public float tractionForce;
    public float slipRatio;

    public float angularAccel;

	public float supportedWeight;
	public float driveTorque;
	public Vector3 driveForce;
    float totalTorque;
    public float maxTractionForce = 100.0f;

	public float angularVelocity;
    public AnimationCurve slipToTractionForce;
	public Rigidbody carRB;

	Transform wheelGeometry;

	void Start()
	{
		wheelGeometry = transform.FindChild("wheel");
        angularVelocity = 0;
        //_rigidbody = wheelGeometry.GetComponent<Rigidbody>();
    }

	void Update()
	{
        
    }
	
	void FixedUpdate () 
	{
        // get the slip ratio
        //wheelLinearVelocity = transform.InverseTransformDirection(_rigidbody.velocity).z;
        wheelLinearVelocity = transform.InverseTransformDirection(carRB.velocity).z;

        numerator = angularVelocity * wheelRadius;

        slipRatio = (angularVelocity * wheelRadius - wheelLinearVelocity) / Mathf.Abs(wheelLinearVelocity);
        // 2 special cases:
        if (float.IsNaN(slipRatio)) // car stopped, wheel stopped
        {
            slipRatio = 0.0f;
        }
        if (float.IsInfinity(slipRatio)) // car stopped but wheel spinning
        {
            slipRatio = Mathf.Sign(slipRatio) * 1.0f;
        }

        if (driveTorque == 0)
        {
            angularVelocity = wheelLinearVelocity / wheelRadius;
            slipRatio = 0;
        }
        else
        {
            angularVelocity += angularAccel * Time.fixedDeltaTime;
        }

        // get the traction torque


        //Debug.Log("EvaluatedCurve = " + slipToTractionForce.Evaluate(Mathf.Abs(slipRatio)) * Mathf.Sign(slipRatio));
        tractionForce = slipToTractionForce.Evaluate(Mathf.Abs(slipRatio)) * Mathf.Sign(slipRatio) * supportedWeight * tractionFactor;
        // Clamp it
        maxTractionForce = -1.5f * supportedWeight;
        tractionForce = Mathf.Clamp(tractionForce, -maxTractionForce, maxTractionForce);
        float tractionTorque = tractionForce * wheelRadius;


        //tractionTorque *= -1 * Mathf.Sign(driveTorque);

        totalTorque = tractionTorque + driveTorque;
        float wheelInertia = mass * wheelRadius * wheelRadius / 2;

        angularAccel = totalTorque / wheelInertia;

        

        


        //Vector3 totalForce = (totalTorque/wheelRadius) * transform.forward;

        // Apply forces to the car
        carRB.AddForce(-tractionTorque/wheelRadius * transform.forward);
        //Debug.DrawLine(transform.position, transform.position + transform.forward*(driveTorque/wheelRadius)/200.0f, Color.green);

        


        wheelGeometry.Rotate(0.0f, -angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f);


    }
}
