using UnityEngine;
using System.Collections;

public class CarPhysicsController : MonoBehaviour
{
    // Parameters ----------------------------------------------------

    public AnimationCurve enginePowerTorqueCurve;
    public float enginePower = 10;
    public float brakePower = 1000;

    public float airDragConstant;
    public float rollDragConstant;

    public float differentialRatio = 3.42f;
    public float transmissionEfficiency = 0.7f; // guess

    public float minRPM = 1000.0f;
    public float maxRPM = 6000.0f;

    public float[] gearRatios = { 2.9f, 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f }; // 0 = reverse

    // member variables ----------------------------------------------

    float gearRatio;
    float rpm;
    float engineTorque;

    Rigidbody mRigidbody;
    Vector3 centerOfMass;

    Vector3 weightPosition; // affected by the dynamic weight transfer
    float frontWeight;
    float rearWeight;
    float rightWeight;
    float leftWeight;

    // User input object
    InputInterface input;

    // Wheels
    PhysicsWheel frontLeftWheel;
    PhysicsWheel frontRightWheel;
    PhysicsWheel backLeftWheel;
    PhysicsWheel backRightWheel;

    // Axis
    Transform FrontAxis;
    Transform RearAxis;
    float distanceBetweenWheels;

    // Integration variables
    Vector3 lastVelocity;

    void Start ()
    {
        // Get user input object
        input = transform.parent.gameObject.GetComponent<InputInterface>();

        // Set centerOfMass
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        centerOfMass = transform.parent.FindChild("CenterOfMass").localPosition;        
        mRigidbody.centerOfMass = centerOfMass;

        // Find all car parts
        FrontAxis = transform.parent.FindChild("FrontAxis");
        RearAxis = transform.parent.FindChild("RearAxis");
        frontLeftWheel = transform.parent.FindChild("FrontLeftWheel").GetComponent<PhysicsWheel>();
        frontRightWheel = transform.parent.FindChild("FrontRightWheel").GetComponent<PhysicsWheel>();
        backLeftWheel = transform.parent.FindChild("BackLeftWheel").GetComponent<PhysicsWheel>();
        backRightWheel = transform.parent.FindChild("BackRightWheel").GetComponent<PhysicsWheel>();

        frontLeftWheel.axisRigidBody = FrontAxis.GetComponent<Rigidbody>();
        frontRightWheel.axisRigidBody = FrontAxis.GetComponent<Rigidbody>();
        backLeftWheel.axisRigidBody = RearAxis.GetComponent<Rigidbody>();
        backRightWheel.axisRigidBody = RearAxis.GetComponent<Rigidbody>();


        distanceBetweenWheels = (transform.parent.FindChild("FrontLeftWheel").position - transform.parent.FindChild("FrontRightWheel").position).magnitude;

        gearRatio = gearRatios[1]; // first gear
    }

    void OnDrawGizmos()
    {
        // Debugging gizmos
        Gizmos.color = Color.blue;
        if(weightPosition != null) Gizmos.DrawSphere(transform.parent.TransformPoint(weightPosition), 0.4f);
    }

    void OnGUI()
    {
        float velocity_forward = Vector3.Dot(mRigidbody.velocity, transform.forward);

        GUI.Box(new Rect(10, 10, 300, 200), "Debug Data");
        GUI.TextArea(new Rect(15, 30, 290, 20), "Forward velocity = " + velocity_forward * 3.6f + " km/h");

        Vector3 acceleration = transform.InverseTransformDirection((mRigidbody.velocity - lastVelocity) / Time.deltaTime);
        float tangentialAcceleration = acceleration.z;
        GUI.TextArea(new Rect(15, 60, 290, 20), "Forward acceleration = " + tangentialAcceleration + " m/s^2");

        GUI.TextArea(new Rect(15, 90, 290, 20), "RPM = " + rpm);
        
    }

    void Update ()
    {
        
    }

    void FixedUpdate ()
    {
        // calculate weight dynamic transfer ----------------------

        Vector3 CenterOfMassAligned = centerOfMass; // put the three objects in the same plane (y=0)
        CenterOfMassAligned.y = 0.0f;
        Vector3 FrontAxisAligned = FrontAxis.localPosition;
        FrontAxisAligned.y = 0.0f;
        Vector3 RearAxisAligned = RearAxis.localPosition;
        RearAxisAligned.y = 0.0f;

        float distanceToFront = (CenterOfMassAligned - FrontAxisAligned).magnitude;
        float distanceToRear = (CenterOfMassAligned - RearAxisAligned).magnitude;
        float wheelBase = (FrontAxisAligned - RearAxisAligned).magnitude;

        Vector3 acceleration = Vector3.ClampMagnitude(transform.InverseTransformDirection((mRigidbody.velocity - lastVelocity)/Time.deltaTime),100);
        
        float tangentialAcceleration = acceleration.z;
        float normalAcceleration = acceleration.x;

        frontWeight = (distanceToRear / wheelBase) * mRigidbody.mass * Physics.gravity.y - (centerOfMass.y / wheelBase) * mRigidbody.mass * tangentialAcceleration;
        rearWeight = (distanceToFront / wheelBase) * mRigidbody.mass * Physics.gravity.y + (centerOfMass.y / wheelBase) * mRigidbody.mass * tangentialAcceleration;

        float lateralWeightTransfer = normalAcceleration / Physics.gravity.y * mRigidbody.mass * centerOfMass.y / distanceBetweenWheels;
        rightWeight = 0.5f * mRigidbody.mass * Physics.gravity.y + lateralWeightTransfer;
        leftWeight = 0.5f * mRigidbody.mass * Physics.gravity.y - lateralWeightTransfer;
        // weight transfer due to body roll ignored

        float longitudinalWeight = frontWeight + rearWeight;
        float frontWeightPercent = Mathf.Clamp01(frontWeight / longitudinalWeight);
        float rearWeightPercent = Mathf.Clamp01(rearWeight / longitudinalWeight);

        float normalWeight = rightWeight + leftWeight;
        float rightWeightPercent = Mathf.Clamp01(rightWeight / normalWeight);
        float leftWeightPercent = Mathf.Clamp01(leftWeight / normalWeight);

        weightPosition = (FrontAxis.localPosition * frontWeightPercent + RearAxis.localPosition * rearWeightPercent);        
        weightPosition += ((distanceBetweenWheels) * (rightWeightPercent) - (distanceBetweenWheels) * (leftWeightPercent)) * new Vector3(1,0,0) * 5;

        // transfer the weight to the wheels
        frontLeftWheel.supportedWeight = (frontWeightPercent + leftWeightPercent)/2;
        frontRightWheel.supportedWeight = (frontWeightPercent + rightWeightPercent)/2;
        backLeftWheel.supportedWeight = (rearWeightPercent + leftWeightPercent)/2;
        backRightWheel.supportedWeight = (rearWeightPercent + rightWeightPercent)/2;

        //---------------------------------------------------------------------

        // rpm calculus from wheels angular velocity
        float maxAngularVel = Mathf.Max(backLeftWheel.angularVelocity, backRightWheel.angularVelocity, frontLeftWheel.angularVelocity, frontRightWheel.angularVelocity);
        rpm = maxAngularVel * gearRatio * differentialRatio * 60.0f / (2.0f * Mathf.PI);
        if (rpm < minRPM) rpm = minRPM; // don't let the engine go under the minimun rpm

        // user sets a percentage of the maxEngineTorque
        float maxEngineTorque = enginePowerTorqueCurve.Evaluate(rpm / maxRPM) * enginePower;
        engineTorque = input.userThrottle * maxEngineTorque;

        // TO-DO: calculate which gear to use
        gearRatio = gearRatios[1]; // first gear
        float driveTorque = engineTorque * gearRatio * differentialRatio * transmissionEfficiency; // / wheelRadius;

        // transfer driveTorque to the wheels, they will apply the force (if not sliding)
        frontLeftWheel.driveTorque = driveTorque / 4;
        frontRightWheel.driveTorque = driveTorque / 4;
        backLeftWheel.driveTorque = driveTorque / 4;
        backRightWheel.driveTorque = driveTorque / 4;        

        // transfer brakeTorque to the wheels - brakeTorque is > 0.
        float brakeTorque = brakePower * input.userBrake;
        frontLeftWheel.brakeTorque = brakeTorque / 4;
        frontRightWheel.brakeTorque = brakeTorque / 4;
        backLeftWheel.brakeTorque = brakeTorque / 4;
        backRightWheel.brakeTorque = brakeTorque / 4;

        // Calculate air friction force
        float velocity = transform.InverseTransformDirection(mRigidbody.velocity).z;
        Vector3 dragForce = -transform.forward * (velocity * velocity) * airDragConstant;
        Debug.DrawLine(transform.position, transform.position + dragForce / 200.0f, Color.red);
        // Calculate axis roll friction force
        Vector3 rollingForce = -transform.forward * velocity * rollDragConstant;
        Debug.DrawLine(transform.position, transform.position + rollingForce / 200.0f, Color.yellow);

        Vector3 frictionForces = dragForce + rollingForce;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit) && (hit.distance) < 1.7f) // if the wheels are touching the ground
        {
            mRigidbody.AddForce(frictionForces);
        }

        lastVelocity = mRigidbody.velocity;        
    }
}
