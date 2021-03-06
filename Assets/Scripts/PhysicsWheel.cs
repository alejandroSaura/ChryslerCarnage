﻿using UnityEngine;
using System.Collections;

public class PhysicsWheel : MonoBehaviour
{
    // parameters
    public bool boneVersion2 = false;
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
    public float maxTorqueflip=200.0f;
    public float dustValue;
    public float dustValueAcc;
    // animation adjustment params
    public float upAmplitude = 0.5f;
    public float downAmplitude = 0.5f;


    // exposed to be set by carController
    public Animator wheelAnimator;
    public float supportedWeight;
    public float angularVelocity;
    public float driveTorque;
    public float brakeTorque;
    public float wheelRadius = 0.7f;
    public Rigidbody axisRigidBody;
    public Rigidbody body;
    public Transform steeringBone = null;
    public Transform wheelGeometry = null;
    public int scaleX = 1;



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
    

    // User input object
    InputInterface input;
    
    float meanWeightSupported;
    float tractionTorque;
    float tractionForce;

    float flipTorque;
    bool touchingGround;

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
        //wheelGeometry = transform.FindChild("wheelGeometry");

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

        // slip ratio. Will be modified below due to other effects  //acceleration
        slipRatio = 1 + slipCurve.Evaluate(angularVelocity) * userThrottleWeight.Evaluate(input.userThrottle) * (1 - weightFactor);
        dustValueAcc = slipRatio;

        tractionTorque = weightFactor * driveTorque;
        tractionForce = (-tractionTorque / wheelRadius);       

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit) && (hit.distance) < wheelRadius * 2f
            && Vector3.Angle(transform.right, -normal) > 120) // if the wheel is touching the ground //and the angle of collision is reasonable
        {
            #region particularCases
            touchingGround = true;
            if (tangentialVelocity < 0.1f && brakeTorque > 0) // car stopped and brake and throttle pressed
            {
                tractionTorque = 0;
                
            }
            if (driveTorque == 0) // No driveTorque applied
            {
                //slipRatio = 1;
                dustValue = 0; //value needed to reset the dust particles
                angularVelocity = tangentialVelocity / wheelRadius;
            }

            #endregion


            #region brake

            if (tangentialVelocity > 1f && brakeTorque > 0)
            {
                slipRatio = 1 + slipBrakeCurve.Evaluate(angularVelocity) * userBrakeWeight.Evaluate(input.userBrake) * (weightFactor - 1);
                dustValue = slipRatio;
                if (slipRatio > 1) slipRatio = 1; // we dont want the wheel to be spinning faster than the velocity of the ground when braking 
                if (slipRatio < 0) slipRatio = 0; // block wheels when you brake too hard

                mRigidbody.AddForce(
                    -brakeTorque / wheelRadius
                    * (1 + weightFactor) // better braking with more weight
                    * slipRatio // affected by slip factor
                    * transform.forward);

                Debug.DrawLine(transform.position, transform.position + -brakeTorque / wheelRadius * (1 + weightFactor) * transform.forward, Color.red);
            }

            #endregion


            #region reverse gear

            if (tangentialVelocity < 1f && Mathf.Abs(tangentialVelocity) < 30 && brakeTorque > 0)
            {

                mRigidbody.AddForce(
                    brakeTorque / wheelRadius
                    * 1
                    * (1 + weightFactor)
                    * -transform.forward
                    * 0.7f
                );
                mRigidbody.drag = 0;

                //Debug.DrawLine(transform.position, transform.position + -brakeTorque / wheelRadius * (1 + weightFactor) * transform.forward, Color.red);
            }

            #endregion


            #region forwardForce

            if (Vector3.Dot(mRigidbody.velocity, transform.forward) < 0)
            {
                tractionTorque *= 5;
            }

            axisRigidBody.AddForceAtPosition(tractionTorque / wheelRadius * transform.forward * 5, transform.position);
            Debug.DrawLine(transform.position, transform.position + tractionTorque / wheelRadius * transform.forward, Color.green);

            #endregion


            // DownForce
            //transform.parent.GetComponent<Rigidbody>().AddForceAtPosition(-transform.right * 20, transform.position);

            #region lateralForces

            latForce_velocityFactor = velocityToSideSlip.Evaluate(velocity.magnitude); // speed penalizer
            float latForce_slipFactor = slipToSideSlip.Evaluate(slipRatio); // tangentialSlip penalizer
            //Debug.Log(velocity.magnitude);
            Vector3 direction = Vector3.Cross(transform.forward, normal).normalized;
            //Vector3 direction = transform.up;

            sideSlipAngle = Vector3.Angle(transform.forward, velocity);
            //if (sideSlipAngle > 180) sideSlipAngle = 360 - sideSlipAngle;
            sideSlipAngleRatio = 1 - sideSlipAngle / 180;
            //Debug.Log(sideSlipAngle);


            // force calculus
            Vector3 lateralForce = Vector3.zero;
            if ((transform.parent.GetComponent<Rigidbody>().velocity.magnitude > 10)) // high speed turning
            {
                //lateralForce = direction * sideSlipToForce.Evaluate(sideSlipAngle) * maxLateralForce;
                
                float driftWeight = 1;
                if (Input.GetButton("drift") && gameObject.GetComponent<HingeJoint>() == null && Vector3.Angle(velocity, transform.forward) < 30) driftWeight = 0.6f;

                float maxLateralForce =
                    (
                    (Mathf.Abs(latVelocity) * Mathf.Sign(latVelocity))
                    //* (supportedWeight)*2
                    * mRigidbody.mass * 9.8f
                    * Mathf.Clamp(tangentialVelocity / 8, 1, float.MaxValue)
                    * latForce_slipFactor
                    * sideSlipAngleRatio
                    * latForce_velocityFactor
                    * driftWeight
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
                    * mRigidbody.mass * 9.8f
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
            else if ((tangentialVelocity) < 0f) // backwards
            {
                mRigidbody.AddForceAtPosition(-lateralForce, transform.position);
                Debug.DrawLine(transform.position, transform.position + -lateralForce);
            }

            if (driveTorque > 0 && !goingBackwards) mRigidbody.drag = 0;
            #endregion

            #region rotateGeometry

            angularVelocity = slipRatio * tangentialVelocity / wheelRadius
                + userThrottleWeight.Evaluate(input.userThrottle) * 20 / Mathf.Clamp(tangentialVelocity, 0.2f, 9999); // simulation of slip when 

            if (wheelGeometry != null)
                wheelGeometry.Rotate(angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f, 0.0f);



            #endregion

            // final slipRatio calculus
            slipRatio = angularVelocity / (tangentialVelocity / wheelRadius);


            // set joint to support soft hits
            {
                SoftJointLimitSpring spring = axisRigidBody.GetComponent<ConfigurableJoint>().linearLimitSpring;
                spring.spring = Mathf.Lerp(spring.spring, 450, Time.deltaTime * 5);
                axisRigidBody.GetComponent<ConfigurableJoint>().linearLimitSpring = spring;
            }
        }

        //else if(Vector3.Dot(transform.right,Vector3.down) > 0 ) //in air control, car is correct orientation
        //{


        //}
        #region AirControl
        if (Vector3.Dot(transform.right,Vector3.down) > 0) // car is upside down
        {
           // maxSteerAngle = maxSteerAngleCurve.Evaluate(velocity.magnitude);
            flipTorque = input.userLeftStickHorizontal * maxTorqueflip;

            body.AddRelativeTorque(new Vector3(0, 0, flipTorque));
        }
        #endregion
        else
        {
           // touchingGround = false;
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
            if (wheelGeometry != null)
                wheelGeometry.Rotate(angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f, 0.0f);

            // set joint to support hard hits
            {
                SoftJointLimitSpring spring = axisRigidBody.GetComponent<ConfigurableJoint>().linearLimitSpring;
                spring.spring = Mathf.Lerp(spring.spring, 9999, Time.deltaTime * 5);
                axisRigidBody.GetComponent<ConfigurableJoint>().linearLimitSpring = spring;
            }


            if (Vector3.Dot(transform.up, Vector3.up) > -0.2f)
            {// flipped car. Rotate around Z axis
                //input.userLeftStickHorizontal
            }

            #endregion
        }

        #region steering

        maxSteerAngle = maxSteerAngleCurve.Evaluate(velocity.magnitude);
        wheelSteerAngleTarget = maxSteerAngle * input.userLeftStickHorizontal * inputSensitivityCurve.Evaluate(velocity.magnitude);
        HingeJoint joint = gameObject.GetComponent<HingeJoint>();
        if (joint != null)
        {
            JointSpring spring = joint.spring;
            spring.targetPosition = Mathf.Lerp(spring.targetPosition, wheelSteerAngleTarget, Time.deltaTime*10);
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

        HingeJoint joint = gameObject.GetComponent<HingeJoint>();
        if (joint != null)
        {
            Vector3 forward;
            if (boneVersion2)
            {
                forward = Vector3.Reflect(-joint.transform.forward, body.transform.right);
                steeringBone.LookAt(steeringBone.transform.position - 3*forward, Quaternion.Euler(0, 0, 5) * body.transform.up);

                //Vector3 rot = steeringBone.transform.rotation.eulerAngles;
                //rot.y += -joint.transform.localRotation.eulerAngles.y;
                //steeringBone.transform.rotation = Quaternion.Euler(rot);

                //Vector3 rot = steeringBone.transform.rotation.eulerAngles;
                //rot.y += -joint.transform.localRotation.eulerAngles.y;
                //steeringBone.transform.rotation = Quaternion.Euler(rot);

            }
            else
            {
                forward = joint.transform.forward;
                steeringBone.LookAt(steeringBone.transform.position + forward, joint.transform.right);
            }


            if (scaleX < 0)
            {
                Vector3 rot = steeringBone.transform.localRotation.eulerAngles;
                if (boneVersion2)
                {
                    //rot.x = rot.x + 180;
                    //rot.y *= -1;
                }
                else
                {
                    rot.y *= -1;
                }
                steeringBone.transform.localRotation = Quaternion.Euler(rot);
            }
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

        //normal = transform.forward;
    }

}
