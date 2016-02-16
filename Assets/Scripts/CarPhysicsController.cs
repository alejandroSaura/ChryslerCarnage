using UnityEngine;
using System.Collections;

public class CarPhysicsController : MonoBehaviour
{
    // Parameters



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
        input = gameObject.GetComponent<InputInterface>();

        // Set centerOfMass
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        centerOfMass = transform.parent.FindChild("CenterOfMass").localPosition;        
        mRigidbody.centerOfMass = centerOfMass;

        // Find all car parts
        frontLeftWheel = transform.parent.FindChild("FrontLeftWheel").GetComponent<PhysicsWheel>();
        frontRightWheel = transform.parent.FindChild("FrontRightWheel").GetComponent<PhysicsWheel>();
        backLeftWheel = transform.parent.FindChild("BackLeftWheel").GetComponent<PhysicsWheel>();
        backRightWheel = transform.parent.FindChild("BackRightWheel").GetComponent<PhysicsWheel>();
        FrontAxis = transform.parent.FindChild("FrontAxis");
        RearAxis = transform.parent.FindChild("RearAxis");

        distanceBetweenWheels = (transform.parent.FindChild("FrontLeftWheel").position - transform.parent.FindChild("FrontRightWheel").position).magnitude;
    }

    void OnDrawGizmos()
    {
        // Debugging gizmos
        Gizmos.color = Color.blue;
        if(weightPosition != null) Gizmos.DrawSphere(transform.parent.TransformPoint(weightPosition), 0.4f);
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

        Vector3 acceleration = transform.InverseTransformDirection((mRigidbody.velocity - lastVelocity)/Time.deltaTime);
        
        float tangentialAcceleration = acceleration.z;
        float normalAcceleration = acceleration.x;

        frontWeight = (distanceToRear / wheelBase) * mRigidbody.mass * Physics.gravity.y + (centerOfMass.y / wheelBase) * mRigidbody.mass * tangentialAcceleration;
        rearWeight = (distanceToFront / wheelBase) * mRigidbody.mass * Physics.gravity.y - (centerOfMass.y / wheelBase) * mRigidbody.mass * tangentialAcceleration;

        float lateralWeightTransfer = normalAcceleration / Physics.gravity.y * mRigidbody.mass * centerOfMass.y / distanceBetweenWheels;
        // weight transfer due to body roll ignored

        float weight = frontWeight + rearWeight;
        float frontWeightPercent = Mathf.Clamp01(frontWeight / weight);
        float rearWeightPercent = Mathf.Clamp01(rearWeight / weight);

        weightPosition = FrontAxis.localPosition * frontWeightPercent + RearAxis.localPosition * rearWeightPercent;

        // Test lateral weight transfer when truning is implemented
        // weightPosition += transform.right * distanceBetweenWheels / 2 * (lateralWeightTransfer/weight);


        lastVelocity = mRigidbody.velocity;
    }
}
