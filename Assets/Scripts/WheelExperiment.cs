using UnityEngine;
using System.Collections;

public class WheelExperiment : MonoBehaviour
{

    

    public float wheelsBlockFactor = 0.65f;

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

    public float brakeTorque;

    public float angularVelocity;

    public AnimationCurve slipCurve;
    public AnimationCurve tractionCurve;
    public AnimationCurve userThrottleWeight;

    public AnimationCurve slipBrakeCurve;
    public AnimationCurve userBrakeWeight;


    public Rigidbody carRB;

    Transform wheelGeometry;
    InputInterface input;

    Vector3 lastPosition;

    // suspension parameters
    public float restLenght = 0.5f;
    public float suspensionK = 5000;
    public float suspensionRestPos = 0.5f;
    public float suspensionMaxX;
    public float suspensionDampingCoeficient = 100;


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

       suspensionMaxX = suspensionRestPos + mass * -Physics.gravity.y / suspensionK; // x = F/k
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
        if (driveTorque == 0) angularVelocity = carLinearVelocity / wheelRadius;
        slipRatio = angularVelocityFactor;
       
        tractionTorque = tractionFactor * driveTorque;
        tractionForce = (-tractionTorque / wheelRadius) ;

        // Apply forces to the car
        
        if (carLinearVelocity < 0.1f && brakeTorque > 0) // car stopped and brake and throttle pressed
        {
            tractionTorque = 0;
            slipRatio = 1;
        }

        carRB.AddForce(tractionTorque / wheelRadius * transform.forward * 5);

        if (carLinearVelocity > 0.1f && brakeTorque > 0)
        {
            carRB.AddForce(-brakeTorque / wheelRadius * (1 - weightFactor) * transform.forward);
            slipRatio = 1 + slipBrakeCurve.Evaluate(angularVelocity) * userBrakeWeight.Evaluate(input.userBrake) * -(weightFactor);
            if (slipRatio > 1) slipRatio = 1; // we dont want the wheel to be spinning faster than the velocity of the ground when braking             
        }

        // Rotate the geometry
        angularVelocity = slipRatio * carLinearVelocity / wheelRadius;
        if (slipRatio < wheelsBlockFactor) angularVelocity = 0; // block the wheels

        wheelGeometry.Rotate(0.0f, angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, 0.0f);


        slipColor = new Vector4(1, 1-(slipRatio - 1), 1-(slipRatio - 1), 1);  
        tractionColor = new Vector4(1-tractionFactor, 1-tractionFactor, 1, 1);


        // lateral force

        Vector3 velocity = (transform.position - lastPosition).normalized;
        float latVelocity = transform.InverseTransformDirection(velocity).x;

        if (carLinearVelocity > 0.1f)
        {
            carRB.AddForceAtPosition(latVelocity * transform.right * (supportedWeight + mass * 9.8f) * (10 + carLinearVelocity / 5), transform.position);
        }
        else
        {
            Vector3 v = carRB.velocity;
            v.x = 0;
            carRB.velocity = v;
        }

        // vertical force to keep the car away from the ground.       
       
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit) && (hit.distance) < suspensionMaxX+wheelRadius)
        {
            Debug.Log(hit.collider.name);
            float springForce = -suspensionK * (hit.point - (transform.position)).y + suspensionDampingCoeficient * (transform.position-lastPosition).magnitude/Time.deltaTime;
            carRB.AddForce( springForce * Vector3.up); // Apply Hooke's law
        }

        lastPosition = transform.position;
    }
}
