using UnityEngine;
using System.Collections;

public class PhysicsWheel : MonoBehaviour
{
    // parameters
    public AnimationCurve inputSensitivityCurve;
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

    // animation adjustment params
    public float upAmplitude = 0.5f;
    public float downAmplitude = 0.5f;
    


    public float directionDeviationCorrection = -0.01f;

    // exposed to be set by carController
    public Animator wheelAnimator;
    public float supportedWeight;
    public float angularVelocity;
    public float driveTorque;
    public float brakeTorque;
    public float wheelRadius = 0.7f;
    public Rigidbody axisRigidBody;
    public Rigidbody body;


    // debug
    public float latVelocity;
    public float wheelLinearVelocity;
    public float tangentialVelocity;
    public float weightFactor;
    public float slipRatio;
    public float latForce_velocityFactor;
    public float sideSlipAngleRatio;
    public float sideSlipAngle;
    public float wheelHeight;

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

    bool goingBackwards = true;

    
    float restHeight;
    float lastHeight;

    void Start ()
    {
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        wheelGeometry = transform.FindChild("wheelGeometry");

        if (gameObject.GetComponent<FixedJoint>() != null)
        {
            axisRigidBody = gameObject.GetComponent<FixedJoint>().connectedBody;
        }
        else if (gameObject.GetComponent<HingeJoint>() != null)
        {
            axisRigidBody = gameObject.GetComponent<HingeJoint>().connectedBody;
        }

        body = axisRigidBody.GetComponent<ConfigurableJoint>().connectedBody;        

        restHeight = body.transform.InverseTransformPoint(transform.position).y;
        lastHeight = restHeight;

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
        wheelLinearVelocity = angularVelocity * wheelRadius;

        if (Vector3.Dot(mRigidbody.velocity, transform.forward) < 0 && brakeTorque>0)
        {
            goingBackwards = true;
        }
        else
        {
            goingBackwards = false;
        }

        weightFactor = (supportedWeight-0.4f)/0.2f;
        Mathf.Clamp01(weightFactor);        

        // slip ratio. Will be modified below due to other effects 
        slipRatio = 1 + slipCurve.Evaluate(angularVelocity) * userThrottleWeight.Evaluate(input.userThrottle) * (1 - weightFactor);         

        tractionTorque = weightFactor * driveTorque;
        tractionForce = (-tractionTorque / wheelRadius);       

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit) && (hit.distance) < wheelRadius * 2f // if the wheel is touching the ground
            && Vector3.Angle(transform.right, -normal)>120) //and the angle of collision is reasonable
        {
            #region particularCases

            if (tangentialVelocity < 0.1f && brakeTorque > 0) // car stopped and brake and throttle pressed
            {
                tractionTorque = 0;                
            }
            if (driveTorque == 0) // No driveTorque applied
            {
                //slipRatio = 1;
                angularVelocity = tangentialVelocity / wheelRadius;
            }

            #endregion


            #region brake

            if (tangentialVelocity > 1f && brakeTorque > 0)
            {
                slipRatio = 1 + slipBrakeCurve.Evaluate(angularVelocity) * userBrakeWeight.Evaluate(input.userBrake) * (weightFactor - 1);
                if (slipRatio > 1) slipRatio = 1; // we dont want the wheel to be spinning faster than the velocity of the ground when braking 
                if (slipRatio < 0) slipRatio = 0; // block wheels when you brake too hard

                mRigidbody.AddForce(
                    -brakeTorque / wheelRadius
                    * (1 + weightFactor) // better braking with more weight
                    * slipRatio // affected by slip factor
                    * transform.forward) ;                 

                Debug.DrawLine(transform.position, transform.position + -brakeTorque / wheelRadius * (1 + weightFactor) * transform.forward, Color.red);
            }

            #endregion


            #region reverse gear

            if (tangentialVelocity < 1f && brakeTorque > 0)
            {                

                mRigidbody.AddForce(
                    brakeTorque / wheelRadius
                    * 1
                    * (1 + weightFactor)                    
                    * -transform.forward);
                mRigidbody.drag = 0;

                //Debug.DrawLine(transform.position, transform.position + -brakeTorque / wheelRadius * (1 + weightFactor) * transform.forward, Color.red);
            }

            #endregion


            #region forwardForce

            if(Vector3.Dot(mRigidbody.velocity, transform.forward) < 0)
            {
                tractionTorque *= 5;
            }

            axisRigidBody.AddForceAtPosition(tractionTorque / wheelRadius * tangent *5, transform.position);
            Debug.DrawLine(transform.position, transform.position + tractionTorque / wheelRadius * transform.forward, Color.green);

            #endregion


            #region lateralForces

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
                    //* (supportedWeight)*2
                    * mRigidbody.mass*9.8f
                    * Mathf.Clamp(tangentialVelocity / 8, 1, float.MaxValue)
                    * latForce_slipFactor 
                    * sideSlipAngleRatio
                    * latForce_velocityFactor
                    * 0.8f
                    );

                lateralForce = direction * maxLateralForce; //* sideSlipToForce.Evaluate(sideSlipAngle);
            }
            else if ((transform.parent.GetComponent<Rigidbody>().velocity.magnitude) <= 10f) // low speed turning
            {
                lateralForce =
                    (
                    (Mathf.Abs(latVelocity) * 1.5f * Mathf.Sign(latVelocity))
                    //* (supportedWeight)*2
                    * Mathf.Clamp(tangentialVelocity / 8, 1, float.MaxValue)
                    * mRigidbody.mass*9.8f
                    * 2                    
                    ) * direction;

                //mRigidbody.drag = 5;
            }
            //if (velocity.magnitude < 0.1f && !goingBackwards)
            //{
            //    // if the speed is too low just stop the car with Unity's drag
            //    //lateralForce = Vector3.zero;
            //    mRigidbody.drag = 40;
            //}

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
           
            if(driveTorque > 0 && !goingBackwards) mRigidbody.drag = 0;
            #endregion

            #region rotateGeometry

            angularVelocity = slipRatio * tangentialVelocity / wheelRadius
                + userThrottleWeight.Evaluate(input.userThrottle) * 20 / Mathf.Clamp(tangentialVelocity, 0.2f, 9999); // simulation of slip when 
            wheelGeometry.Rotate(angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f, 0.0f);

            #endregion

            // final slipRatio calculus
            slipRatio = angularVelocity / (tangentialVelocity / wheelRadius);
        }
        else
        {
            #region wheelInAir
            if (brakeTorque > 0 && driveTorque > 0)
            {
                angularVelocity = 0;
                slipRatio = 0;
            }
            else if (brakeTorque > 0 && driveTorque == 0)
            {
                angularVelocity = userBrakeWeight.Evaluate(input.userBrake) * 50;
                slipRatio = -1;
            }
            else if (brakeTorque == 0 && driveTorque > 0)
            {
                angularVelocity = userThrottleWeight.Evaluate(input.userThrottle) * 50;
                slipRatio = 2;
            }
            wheelGeometry.Rotate(angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f, 0.0f);
            #endregion
        }

        #region steering

        maxSteerAngle = maxSteerAngleCurve.Evaluate(velocity.magnitude);
        wheelSteerAngleTarget = maxSteerAngle * input.userLeftStickHorizontal * inputSensitivityCurve.Evaluate(velocity.magnitude);
        HingeJoint joint = gameObject.GetComponent<HingeJoint>();
        if (joint != null)
        {
            JointSpring spring = joint.spring;
            spring.targetPosition = Mathf.Lerp(spring.targetPosition, wheelSteerAngleTarget, Time.deltaTime*3);
            joint.spring = spring;
        }

        #endregion

        #region animation

        if (wheelAnimator != null)
        {
            float height = body.transform.InverseTransformPoint(transform.position).y;
            wheelHeight = height - restHeight;
            Mathf.Clamp(wheelHeight, -downAmplitude, upAmplitude);
            //if (wheelHeight > 0)
            {
                wheelHeight /= upAmplitude;
                wheelHeight += downAmplitude/(upAmplitude+downAmplitude);
            }
            //else
            //{
            //    wheelHeight /= downAmplitude;
            //    wheelHeight += upAmplitude / (upAmplitude + downAmplitude);
            //}         
            
            Mathf.Clamp01(wheelHeight);
            
            // Apply the animation time in LateUpdate to avoid Jerkyness and here to avoid desync with the physics in collisions.
            wheelAnimator.Play(Animator.StringToHash("UpDown"), 0, 1-wheelHeight);
            lastHeight = wheelHeight;
        }

        #endregion



        // debug ----------------------------------------------------------------------

        slipColor = new Vector4(1, 1 - (slipRatio - 1), 1 - (slipRatio - 1), 1);
        tractionColor = new Vector4(1 - weightFactor, 1 - weightFactor, 1, 1);

    }

    void LateUpdate()
    {
        if (wheelAnimator != null)
        {
            wheelAnimator.Play(Animator.StringToHash("UpDown"), 0, 1 - wheelHeight);
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
