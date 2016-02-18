using UnityEngine;
using System.Collections;

public class PhysicsWheel : MonoBehaviour
{
    // parameters
    public float inputSensitivity = 0.5f;
    public float maxLateralForce = 1000;
    public float maxSteerAngle = 35;
    public AnimationCurve tractionCurve;
    public AnimationCurve slipCurve;
    public AnimationCurve userThrottleWeight;
    public AnimationCurve slipBrakeCurve;
    public AnimationCurve userBrakeWeight;
    public AnimationCurve lowSpeedLateralCompensation;
    public AnimationCurve velocityToSideSlip;
    public AnimationCurve slipToSideSlip;
    public AnimationCurve maxSteerAngleCurve;


    public float directionDeviationCorrection = -0.01f;

    // exposed to be set by carController
    public float supportedWeight;
    public float angularVelocity;
    public float driveTorque;
    public float brakeTorque;
    public float wheelRadius = 0.7f;
    public Rigidbody axisRigidBody;


    // debug
    public float latVelocity;
    public float wheelLinearVelocity;
    public float tangentialVelocity;
    public float tractionFactor;
    public float weightFactor;
    public float slipRatio;
    public float latForce_velocityFactor;
    public float sideSlipAngleRatio;
    public float sideSlipAngle;

    Rigidbody mRigidbody;
    Transform wheelGeometry;

    // User input object
    InputInterface input;
    
    float meanWeightSupported;
    float tractionTorque;
    float tractionForce;

    Vector4 slipColor;
    Vector4 tractionColor;

    Vector3 normal;
    Vector3 tangent;

    float wheelSteerAngleTarget = 0;



    void Start ()
    {
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        wheelGeometry = transform.FindChild("wheelGeometry");

        // Get user input object
        input = transform.parent.gameObject.GetComponent<InputInterface>();

        meanWeightSupported = transform.parent.gameObject.GetComponent<Rigidbody>().mass * Physics.gravity.y / 4.0f;
    }

    void OnDrawGizmos()
    {
        // Debugging gizmos
        Gizmos.color = slipColor;
        Gizmos.DrawSphere(transform.position + new Vector3(0.0f, 1f, 0.0f), 0.2f);

        Gizmos.color = tractionColor;
        Gizmos.DrawSphere(transform.position + new Vector3(0.0f, 1.6f, 0.0f), 0.2f);
    }

    void FixedUpdate ()
    {
        Vector3 velocity = mRigidbody.velocity;
        tangentialVelocity = transform.InverseTransformDirection(velocity).z;
        latVelocity = transform.InverseTransformDirection(velocity).y;

        //weightFactor = -((supportedWeight - meanWeightSupported) / meanWeightSupported) * 5.0f;
        //if (weightFactor < -1) weightFactor = -1;

        weightFactor = (supportedWeight-0.4f)/0.2f;
        Mathf.Clamp01(weightFactor);

        // get the slip ratio
        wheelLinearVelocity = angularVelocity * wheelRadius;

        // loss of traction. Depends on the supported weight and the velocity.
        //tractionFactor = 1 - (tractionCurve.Evaluate(angularVelocity)) * (1-weightFactor);
        tractionFactor = (weightFactor);

        //depends on traction and its own curve.
        slipRatio = 1 + slipCurve.Evaluate(angularVelocity) * userThrottleWeight.Evaluate(input.userThrottle) * (1-tractionFactor);

        // Rotate the geometry
        angularVelocity = slipRatio * tangentialVelocity / wheelRadius;
        //if (slipRatio < wheelsBlockFactor) angularVelocity = 0; // block the wheels

        wheelGeometry.Rotate(0.0f, -angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f);

        // Add forces:

        tractionTorque = tractionFactor * driveTorque;
        tractionForce = (-tractionTorque / wheelRadius);       

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit) && (hit.distance) < wheelRadius // if the wheel is touching the ground
            && Vector3.Angle(transform.right, -normal)>120) //and the angle of collision is reasonable
        {
            

            // particular cases ----------------------------------------------
            if (tangentialVelocity < 0.1f && brakeTorque > 0) // car stopped and brake and throttle pressed
            {
                tractionTorque = 0;
                slipRatio = 1;
            }
            if (driveTorque == 0) // No driveTorque applied
            {
                angularVelocity = tangentialVelocity / wheelRadius;
            }
            //-----------------------------------------------------------------

            // brake
            if (tangentialVelocity > 1f && brakeTorque > 0)
            {
                mRigidbody.AddForce(-brakeTorque / wheelRadius * (1 + weightFactor) * transform.forward);
                slipRatio = 1 + slipBrakeCurve.Evaluate(angularVelocity) * userBrakeWeight.Evaluate(input.userBrake) * -(weightFactor);
                if (slipRatio > 1) slipRatio = 1; // we dont want the wheel to be spinning faster than the velocity of the ground when braking    

                Debug.DrawLine(transform.position, transform.position + -brakeTorque / wheelRadius * (1 - weightFactor) * transform.forward, Color.red);
            }

            // accelerate 

            //float rotation = maxSteerAngle * (input.userLeftStickHorizontal); //add an offset to correct the deviation bug
            //Quaternion q = new Quaternion();
            //q.eulerAngles = new Vector3(0, rotation, 0);
            //Vector3 headingDirection = q * transform.parent.forward;

            mRigidbody.AddForceAtPosition(tractionTorque / wheelRadius * tangent * 5, transform.position);
            Debug.DrawLine(transform.position, transform.position + tractionTorque / wheelRadius * transform.forward, Color.green);

            // lateral force ---------------------------------------------------

            latForce_velocityFactor = velocityToSideSlip.Evaluate(velocity.magnitude); // speed penalizer
            float latForce_slipFactor = slipToSideSlip.Evaluate(slipRatio); // tangentialSlip penalizer
            //Debug.Log(velocity.magnitude);
            Vector3 direction = Vector3.Cross(transform.forward, normal).normalized;

            sideSlipAngle = Vector3.Angle(transform.forward, velocity);
            //if (sideSlipAngle > 180) sideSlipAngle = 360 - sideSlipAngle;
            sideSlipAngleRatio = 1 - sideSlipAngle / 180;
            //Debug.Log(sideSlipAngle);


            // force calculus
            Vector3 lateralForce = Vector3.zero;
            if ((transform.parent.GetComponent<Rigidbody>().velocity.magnitude > 10)) // high speed turning
            {                
                //lateralForce = direction * sideSlipToForce.Evaluate(sideSlipAngle) * maxLateralForce;

                float maxLateralForce =
                    (
                    (Mathf.Abs(latVelocity) * Mathf.Sign(latVelocity))
                    //* (supportedWeight)
                    * mRigidbody.mass*9.8f
                    * Mathf.Clamp(tangentialVelocity / 8, 1, float.MaxValue)
                    * latForce_slipFactor 
                    * sideSlipAngleRatio
                    * latForce_velocityFactor
                    * 0.8f
                    );

                lateralForce = direction * maxLateralForce; //* sideSlipToForce.Evaluate(sideSlipAngle);
            }
            else if ((transform.parent.GetComponent<Rigidbody>().velocity.magnitude) < 10f) // low speed turning
            {
                lateralForce =
                    (
                    (1 + Mathf.Abs(latVelocity) * 1.5f * Mathf.Sign(latVelocity))
                    //* (supportedWeight)
                    * Mathf.Clamp(tangentialVelocity / 8, 1, float.MaxValue)
                    * mRigidbody.mass*9.8f
                    * 4                    
                    ) * direction;

                mRigidbody.drag = 5;
            }
            if (velocity.magnitude < 2)
            {
                // if the speed is too low just stop the car with Unity's drag
                lateralForce = Vector3.zero;
                mRigidbody.drag = 40;
            }

            // Apply force
            if ((tangentialVelocity) > 0f) // going forward
            {                
                mRigidbody.AddForceAtPosition(-lateralForce, transform.position);
                Debug.DrawLine(transform.position, transform.position + -lateralForce);
            }
            else if((tangentialVelocity) < 0f) // backwards
            {                
                mRigidbody.AddForceAtPosition(-lateralForce, transform.position);
                Debug.DrawLine(transform.position, transform.position + -lateralForce);
            }
           
            if(driveTorque > 0) mRigidbody.drag = 0;
            //-------------------------------------------------------------------

            // debug
            slipColor = new Vector4(1, 1 - (slipRatio - 1), 1 - (slipRatio - 1), 1);
            tractionColor = new Vector4(1 - tractionFactor, 1 - tractionFactor, 1, 1);
        }
        // TO-DO: if not touching the ground set slipRatio to 1 and let the wheel spin freely.

        // Steer the wheel
        maxSteerAngle = maxSteerAngleCurve.Evaluate(velocity.magnitude);
        wheelSteerAngleTarget = maxSteerAngle * input.userLeftStickHorizontal;
        HingeJoint joint = gameObject.GetComponent<HingeJoint>();
        if (joint != null)
        {
            JointSpring spring = joint.spring;
            spring.targetPosition = Mathf.Lerp(spring.targetPosition, wheelSteerAngleTarget, Time.deltaTime*3);
            joint.spring = spring;
        }

    }

    void OnCollisionStay(Collision collision)
    {
        normal = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            normal += contact.normal;
        }
        normal = (normal / collision.contacts.Length).normalized;
        tangent = Vector3.Cross(normal, transform.up);
    }

}
