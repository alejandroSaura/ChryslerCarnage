using UnityEngine;
using System.Collections;

public class PhysicsWheel : MonoBehaviour
{
    // parameters
    public float maxSteerAngle = 35;
    public AnimationCurve tractionCurve;
    public AnimationCurve slipCurve;
    public AnimationCurve userThrottleWeight;
    public AnimationCurve slipBrakeCurve;
    public AnimationCurve userBrakeWeight;

    // exposed to be set by carController
    public float supportedWeight;
    public float angularVelocity;
    public float driveTorque;
    public float brakeTorque;
    public float wheelRadius = 0.7f;

    // debug
    public float latVelocity;
    public float wheelLinearVelocity;
    public float carLinearVelocity;
    public float tractionFactor;
    public float weightFactor;
    public float slipRatio;

    Rigidbody mRigidbody;
    // User input object
    InputInterface input;
    
    float meanWeightSupported;
    float tractionTorque;
    float tractionForce;


    void Start ()
    {
        mRigidbody = gameObject.GetComponent<Rigidbody>();

        // Get user input object
        input = transform.parent.gameObject.GetComponent<InputInterface>();

        meanWeightSupported = transform.parent.gameObject.GetComponent<Rigidbody>().mass * Physics.gravity.y / 4.0f;

    }

    void FixedUpdate ()
    {
        weightFactor = -((supportedWeight - meanWeightSupported) / meanWeightSupported) * 5.0f;
        if (weightFactor < -1) weightFactor = -1;

        // get the slip ratio
        carLinearVelocity = transform.parent.InverseTransformDirection(transform.parent.GetComponent<Rigidbody>().velocity).z;
        wheelLinearVelocity = angularVelocity * wheelRadius;

        // loss of traction. Depends on the supported weight and the velocity.
        tractionFactor = (1 - tractionCurve.Evaluate(angularVelocity)) * (1 - weightFactor);

        //depends on traction and its own curve.
        slipRatio = 1 + slipCurve.Evaluate(angularVelocity) * userThrottleWeight.Evaluate(input.userThrottle) * Mathf.Abs(1 - tractionFactor * 0.5f);
        
        tractionTorque = tractionFactor * driveTorque;
        tractionForce = (-tractionTorque / wheelRadius);
       

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit) && (hit.distance) < wheelRadius) // if the wheel is touching the ground
        {
            // Add forces:

            // particular cases ----------------------------------------------
            if (carLinearVelocity < 0.1f && brakeTorque > 0) // car stopped and brake and throttle pressed
            {
                tractionTorque = 0;
                slipRatio = 1;
            }
            if (driveTorque == 0) // No driveTorque applied
            {
                angularVelocity = carLinearVelocity / wheelRadius;
            }
            //-----------------------------------------------------------------

            // brake
            if (carLinearVelocity > 0.1f && brakeTorque > 0)
            {
                mRigidbody.AddForce(-brakeTorque / wheelRadius * (1 - weightFactor) * transform.forward);
                slipRatio = 1 + slipBrakeCurve.Evaluate(angularVelocity) * userBrakeWeight.Evaluate(input.userBrake) * -(weightFactor);
                if (slipRatio > 1) slipRatio = 1; // we dont want the wheel to be spinning faster than the velocity of the ground when braking    

                Debug.DrawLine(transform.position, transform.position + -brakeTorque / wheelRadius * (1 - weightFactor) * transform.forward, Color.red);
            }

            // accelerate
            mRigidbody.AddForce(tractionTorque / wheelRadius * transform.forward * 5);

            Vector3 velocity = mRigidbody.velocity;

            // lateral force
            latVelocity = transform.InverseTransformDirection(velocity).y;
            if (carLinearVelocity > 0.1f)
            {
                mRigidbody.AddForceAtPosition(-latVelocity *1.5f * transform.up * (1 + mRigidbody.mass * 9.8f) * (carLinearVelocity/5), transform.position);
                Debug.DrawLine(transform.position, transform.position + -latVelocity * transform.up * (1 + mRigidbody.mass * 9.8f) * (carLinearVelocity / 5));
            }
            else
            {
                Vector3 v = mRigidbody.velocity;
                v.x = 0;
                mRigidbody.velocity = v;
            }
        }
        // TO-DO: if not touching the ground set slipRatio to 1 and let the wheel spin freely.

        // Steer the wheel
        HingeJoint joint = gameObject.GetComponent<HingeJoint>();
        if (joint != null)
        {
            JointSpring spring = joint.spring;
            spring.targetPosition = maxSteerAngle * input.userLeftStickHorizontal;
            joint.spring = spring;
        }

    }
}
