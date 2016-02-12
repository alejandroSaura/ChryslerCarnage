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
    public AnimationCurve userThrottleWeight;


    public Rigidbody carRB;

    Transform wheelGeometry;
    InputInterface input;

    //debug:
    public float angularVelocityFactor;
    public float tractionFactor;

    public float meanWeightSupported;
    public float weightFactor;

    Vector4 slipColor;
    Vector4 tractionColor;


    void Start()
    {
        wheelGeometry = transform.FindChild("wheel");
        angularVelocity = 0;
        //_rigidbody = wheelGeometry.GetComponent<Rigidbody>();
        meanWeightSupported = transform.parent.gameObject.GetComponent<Rigidbody>().mass*Physics.gravity.y/4.0f;

        input = transform.parent.gameObject.GetComponent<InputInterface>();
    }

    void Update()
    {

    }

    void OnDrawGizmos()
    {
        // Debugging gizmos
        Gizmos.color = slipColor;
        Gizmos.DrawSphere(transform.position + new Vector3(0.0f, 0.6f, 0.0f), 0.2f);

        Gizmos.color = tractionColor;
        Gizmos.DrawSphere(transform.position + new Vector3(0.0f, 1f, 0.0f), 0.2f);
    }

        void FixedUpdate()
    {
        weightFactor = -((supportedWeight - meanWeightSupported) / meanWeightSupported) * 5.0f;
        if (weightFactor < -1) weightFactor = -1;

        // get the slip ratio
        //wheelLinearVelocity = transform.InverseTransformDirection(_rigidbody.velocity).z;
        carLinearVelocity = transform.InverseTransformDirection(carRB.velocity).z;
        wheelLinearVelocity = angularVelocity * wheelRadius;       

        // loss of traction at low speed to simulate slip, then depends on the supported weight
        tractionFactor = (1 - tractionCurve.Evaluate(angularVelocity)) * (1-weightFactor) ;        

        //depends on traction and its own curve.
        angularVelocityFactor = 1 + slipCurve.Evaluate(angularVelocity) * userThrottleWeight.Evaluate(input.userThrottle) * Mathf.Abs(1-tractionFactor*0.5f);
        angularVelocity = angularVelocityFactor * carLinearVelocity / wheelRadius;
        if (driveTorque == 0) angularVelocity = carLinearVelocity / wheelRadius;

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
        wheelGeometry.Rotate(0.0f, angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f);


        slipColor = new Vector4(1, 1-(angularVelocityFactor-1), 1-(angularVelocityFactor-1), 1);  
        tractionColor = new Vector4(1-tractionFactor, 1-tractionFactor, 1, 1);
    }
}
