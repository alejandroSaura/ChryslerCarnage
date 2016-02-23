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
    public float maxRPM = 13000.0f;
    public float[][] gearThresholds;
    public float[] gearRatios = { 2.9f, 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f }; // 0 = reverse

    // member variables ----------------------------------------------
    float engineRPM;
    float currentGearRatio;
    float rawEngineRPM;
    float engineTorque;
    public int currentGear = 0;
    public int appropriateGear;

    public bool gearShift = false;
    public float noiseRPM;
    public float rpmResetter;

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

        //gear thresholds
        gearThresholds = new float[7][];
        gearThresholds[1] = new float[2];
        gearThresholds[1][0] = 1000;
        gearThresholds[1][1] = 1200;
        gearThresholds[2] = new float[2];
        gearThresholds[2][0] = 1100;
        gearThresholds[2][1] = 2500;
        gearThresholds[3] = new float[2];
        gearThresholds[3][0] = 2300;
        gearThresholds[3][1] = 3100;
        gearThresholds[4] = new float[2];
        gearThresholds[4][0] = 2900;
        gearThresholds[4][1] = 3500;
        gearThresholds[5] = new float[2];
        gearThresholds[5][0] = 3300;
        gearThresholds[5][1] = 3800;
        gearThresholds[6] = new float[2];
        gearThresholds[6][0] = 3600;
        gearThresholds[6][1] = 4000;


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

        currentGear = 1;
        currentGearRatio = gearRatios[1]; // first gear
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

        GUI.TextArea(new Rect(15, 90, 290, 20), "engineRPM = " + engineRPM);
        GUI.TextArea(new Rect(15, 120, 290, 20), "rawEngineRPM = " + rawEngineRPM);
        GUI.TextArea(new Rect(15, 150, 290, 20), "Current Gear= " + currentGear);
        GUI.TextArea(new Rect(15, 180, 290, 20), "False RPM= " + noiseRPM);

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
        float meanAngularVel = (frontLeftWheel.angularVelocity + frontRightWheel.angularVelocity + backLeftWheel.angularVelocity + backRightWheel.angularVelocity) / 4;
        rawEngineRPM = meanAngularVel * gearRatios[1] * differentialRatio * 60.0f / (2.0f * Mathf.PI);
        //rawEngineRPM = Mathf.Clamp(rawEngineRPM, minRPM, maxRPM); // don't let the engine go under the minimun rpm


        //TODO: Fix RPM resetter (sort of working)
        //False RPM
        noiseRPM = rawEngineRPM-rpmResetter;
        if(noiseRPM < 0)
        {
            noiseRPM = 0;
        }
        shiftedGearAudio();

        // rawEngineRPM = (frontLeftWheel.angularVelocity + frontRightWheel.angularVelocity + backLeftWheel.angularVelocity + backRightWheel.angularVelocity) / 4 * gearRatios[currentGear];
        //currentEngineRPM =
        shiftGear();
        //currentGearRatio = currentGear;
        //Debug.Log(currentGearRatio);
        //Debug.Log(rawEngineRPM);

        // user sets a percentage of the maxEngineTorque
        float maxEngineTorque = enginePowerTorqueCurve.Evaluate(rawEngineRPM / maxRPM) * enginePower;
        engineTorque = input.userThrottle * maxEngineTorque;

        // TO-DO: calculate which gear to use
        //currentGearRatio = gearRatios[1]; // first gear
        //float driveTorque = engineTorque * currentGearRatio * differentialRatio * transmissionEfficiency; // / wheelRadius;
        float driveTorque = engineTorque * gearRatios[currentGear] * differentialRatio * transmissionEfficiency; // / wheelRadius;
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

    //false RPM function counter /w resetter.
    void shiftedGearAudio()
    {
        if (gearShift == true)
        {
            rpmResetter = rawEngineRPM;
            noiseRPM = 0;
            gearShift = false;
        }
    }

    void shiftGear()
    {
        switch (currentGear)
        {
            case 0:

                break;
            case 1:
                engineRPM = rawEngineRPM;
                currentGearRatio = gearRatios[1];
                if (rawEngineRPM > gearThresholds[1][1])
                {
                    gearShift = true;
                    currentGear = 2;
                }
                    break;
            case 2:
                engineRPM = rawEngineRPM;
                currentGearRatio = gearRatios[2];
                if (rawEngineRPM > gearThresholds[2][1])
                {
                    gearShift = true;
                    currentGear = 3;
                }
                if (rawEngineRPM < gearThresholds[2][0])
                {
                    gearShift = true;
                    currentGear = 1;
                }
                break;
            case 3:
                engineRPM = rawEngineRPM;
                currentGearRatio = gearRatios[3];
                if (rawEngineRPM > gearThresholds[3][1])
                {
                    gearShift = true;
                    currentGear = 4;
                }
                if (rawEngineRPM < gearThresholds[3][0])
                {
                    gearShift = true;
                    currentGear = 2;
                }
                break;
            case 4:
                engineRPM = rawEngineRPM;
                currentGearRatio = gearRatios[4];
                if (rawEngineRPM > gearThresholds[4][1])
                {
                    gearShift = true;
                    currentGear = 5;
                }
                if (rawEngineRPM < gearThresholds[4][0])
                {
                    gearShift = true;
                    currentGear = 3;
                }
                break;
            case 5:
                engineRPM = rawEngineRPM;
                currentGearRatio = gearRatios[5];
                if (rawEngineRPM > gearThresholds[5][1])
                {
                    gearShift = true;
                    currentGear = 6;
                }
                if (rawEngineRPM < gearThresholds[5][0])
                {
                    gearShift = true;
                    currentGear = 4;
                }
                break;
            //case 6:
            //    engineRPM = rawEngineRPM;
            //    currentGearRatio = gearRatios[3];
            //    if (rawEngineRPM > gearThresholds[4][1])
            //    {
            //        currentGear = 5;
            //    }
            //    if (rawEngineRPM < gearThresholds[4][0])
            //    {
            //        currentGear = 3;
            //    }
            //    break;
        }


    //    if (rawEngineRPM < gearThresholds[1][1])
    //    {
    //        engineRPM = rawEngineRPM;
    //        currentGear = 1;
    //        currentGearRatio = gearRatios[1];
    //    }
    //    else if (rawEngineRPM < gearThresholds[2][1])
    //    {
    //        engineRPM = rawEngineRPM - 2000;
    //        currentGear = 2;
    //        currentGearRatio = gearRatios[2];
    //        if(engineRPM <= gearThresholds[1][0])
    //        {
    //            currentGear = 1;
    //            currentGearRatio = gearRatios[1];
    //        }
    //    }
    //    else if (rawEngineRPM < 7000)
    //    {
    //        engineRPM = rawEngineRPM - 4000;
    //        currentGear = 3;
    //        currentGearRatio = gearRatios[3];
    //    }
    //    else if (rawEngineRPM < 9000)
    //    {
    //        engineRPM = rawEngineRPM - 6000;
    //        currentGear = 4;
    //        currentGearRatio = gearRatios[4];
    //    }
    //    else if (rawEngineRPM < 11000)
    //    {
    //        engineRPM = rawEngineRPM - 8000;
    //        currentGear = 5;
    //        currentGearRatio = gearRatios[5];
    //    }
    //    else if (rawEngineRPM < 13000)
    //    {
    //        engineRPM = rawEngineRPM - 10000;
    //        currentGear = 6;
    //        currentGearRatio = gearRatios[6];
    //    }
    }
        //if (engineRPM >= maxRPM)
        //{
        //    appropriateGear = currentGear;
        //    for (int i = 0; i < gears.Length; i++)
        //    {
        //        if ((backLeftWheel.angularVelocity * gearRatios[i] < maxRPM) || (backRightWheel.angularVelocity * gearRatios[i] < maxRPM) ||
        //            (frontLeftWheel.angularVelocity * gearRatios[i] < maxRPM) || (frontRightWheel.angularVelocity * gearRatios[i] < maxRPM))
        //        {
        //            appropriateGear = i;
        //            break;
        //        }
        //    }
        //    currentGear = appropriateGear;
        //}
        //if (engineRPM <= minRPM)
        //{
        //    appropriateGear = currentGear;
        //    for (int j = gears.Length - 1; j >= 0; j--)
        //    {
        //        if ((backLeftWheel.angularVelocity * gearRatios[j] > minRPM) || (backRightWheel.angularVelocity * gearRatios[j] > minRPM)
        //            || (frontLeftWheel.angularVelocity * gearRatios[j] > minRPM) || (frontRightWheel.angularVelocity * gearRatios[j] > minRPM))
        //        {
        //            appropriateGear = j;
        //            break;
        //        }
        //    }
        //    currentGear = appropriateGear;
        //}
    //}
}
