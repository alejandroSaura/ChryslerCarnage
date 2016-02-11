using UnityEngine;
using System.Collections;

public class WheelExperiment : MonoBehaviour
{
    
    public float wheelLinearVelocity;
    public float carLinearVelocity;

    //Rigidbody _rigidbody;
    public float wheelRadius = 0.34f;
    public float mass = 75f;
   
    public float slipRatio;
    public float supportedWeight;

    public float driveTorque;
    public float tractionTorque;
    public float tractionForce;

    public float angularVelocity;

    public AnimationCurve slipCurve;
    public AnimationCurve tractionCurve;


    public Rigidbody carRB;

    Transform wheelGeometry;
    InputInterface input;

    //debug:
    public float angularVelocityFactor;
    public float tractionFactor;


    void Start()
    {
        wheelGeometry = transform.FindChild("wheel");
        angularVelocity = 0;
        //_rigidbody = wheelGeometry.GetComponent<Rigidbody>();

        input = transform.parent.gameObject.GetComponent<InputInterface>();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        // get the slip ratio
        //wheelLinearVelocity = transform.InverseTransformDirection(_rigidbody.velocity).z;
        carLinearVelocity = transform.InverseTransformDirection(carRB.velocity).z;
        wheelLinearVelocity = angularVelocity * wheelRadius;

        angularVelocityFactor = 1 + slipCurve.Evaluate(angularVelocity) * input.userThrottle * 0.5f;
        angularVelocity = angularVelocityFactor  * carLinearVelocity/wheelRadius;
        if (driveTorque == 0) angularVelocity = carLinearVelocity / wheelRadius;

        tractionFactor = 1 - (tractionCurve.Evaluate(angularVelocity) * input.userThrottle);
        //if (angularVelocityFactor > 0)
        //{
        //    tractionFactor = 1-(angularVelocityFactor - (int)angularVelocityFactor);            
        //}
        //else
        //{
        //    tractionFactor = 1;
        //}
        //tractionFactor = Mathf.Clamp(tractionFactor, 0.0f, 1.0f);
        tractionTorque = tractionFactor * driveTorque;

        tractionForce = (-tractionTorque / wheelRadius) ;

        // Apply forces to the car
        carRB.AddForce(tractionTorque / wheelRadius * transform.forward);
        // Rotate the geometry
        wheelGeometry.Rotate(0.0f, -angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f);
    }
}
