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
    public float maxRPM = 5000.0f;
    public float[][] gearThresholds;
    public float[] gearRatios = { 2.9f, 2.66f, 1.78f, 1.3f, 1f, 1.75f, 0.5f }; // 0 = reverse

    public AudioClip gears;
    private AudioSource source;

    // member variables ----------------------------------------------

    float velocity_forward;

    float engineRPM;
    float rawEngineRPM;
    float engineTorque;
    public int currentGear = 0;

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

    public Transform frontLeftWheelGeometry;
    public Transform frontRightWheelGeometry;
    public Transform backLeftWheelGeometry;
    public Transform backRightWheelGeometry;

    public Transform rightSteeringBone;
    public Transform leftSteeringBone;


    // Axis
    Transform FrontAxisRight;
    Transform FrontAxisLeft;
    Transform RearAxisRight;
    Transform RearAxisLeft;
    float distanceBetweenWheels;

    // Integration variables
    Vector3 lastVelocity;

    void Awake()
    {
        //shiftGear();
        source = GetComponent<AudioSource>();
        // GetComponent<AudioSource>().PlayOneShot(gears);
    }

    void Start ()
    {
        //gear thresholds
        gearThresholds = new float[7][];
        gearThresholds[1] = new float[2];
        gearThresholds[1][0] = 500;
        gearThresholds[1][1] = 1000;
        gearThresholds[2] = new float[2];
        gearThresholds[2][0] = 1000;
        gearThresholds[2][1] = 2000;
        gearThresholds[3] = new float[2];
        gearThresholds[3][0] = 2000;
        gearThresholds[3][1] = 3000;
        gearThresholds[4] = new float[2];
        gearThresholds[4][0] = 3000;
        gearThresholds[4][1] = 4000;
        gearThresholds[5] = new float[2];
        gearThresholds[5][0] = 4000;
        gearThresholds[5][1] = 4600;
        gearThresholds[6] = new float[2];
        gearThresholds[6][0] = 4600;
        gearThresholds[6][1] = 7000;


        // Get user input object
        input = transform.parent.gameObject.GetComponent<InputInterface>();

        // Set centerOfMass
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        centerOfMass = transform.parent.FindChild("CenterOfMass").localPosition;        
        mRigidbody.centerOfMass = centerOfMass;

        // Find all car parts

        FrontAxisRight = transform.parent.FindChild("FrontAxisRight");
        RearAxisRight = transform.parent.FindChild("RearAxisRight");

        frontLeftWheel = transform.parent.FindChild("FrontLeftWheel").GetComponent<PhysicsWheel>();
        frontRightWheel = transform.parent.FindChild("FrontRightWheel").GetComponent<PhysicsWheel>();
        backLeftWheel = transform.parent.FindChild("BackLeftWheel").GetComponent<PhysicsWheel>();
        backRightWheel = transform.parent.FindChild("BackRightWheel").GetComponent<PhysicsWheel>();

        frontLeftWheel.wheelAnimator = transform.FindChild("l_frontWheel_system").GetComponent<Animator>();
        frontRightWheel.wheelAnimator = transform.FindChild("r_frontWheel_system").GetComponent<Animator>();
        backLeftWheel.wheelAnimator = transform.FindChild("l_backWheel_system").GetComponent<Animator>();
        backRightWheel.wheelAnimator = transform.FindChild("r_backWheel_system").GetComponent<Animator>();

        //frontLeftWheel.steeringBone = transform.FindChild("l_frontWheel_system").FindChild("joint2");
        //frontRightWheel.steeringBone = transform.FindChild("r_frontWheel_system").FindChild("joint2");

        frontLeftWheel.steeringBone = leftSteeringBone;
        frontRightWheel.steeringBone = rightSteeringBone;

        //frontLeftWheel.wheelGeometry = transform.FindChild("l_frontWheel_system").transform.FindChild("wheelGeometry");
        //frontRightWheel.wheelGeometry = transform.FindChild("r_frontWheel_system").transform.FindChild("wheelGeometry");
        //backLeftWheel.wheelGeometry = transform.FindChild("l_backWheel_system").transform.FindChild("wheelGeometry");
        //backRightWheel.wheelGeometry = transform.FindChild("r_backWheel_system").transform.FindChild("wheelGeometry");

        frontLeftWheel.wheelGeometry = frontLeftWheelGeometry;
        frontRightWheel.wheelGeometry = frontRightWheelGeometry;
        frontRightWheel.scaleX = -1;
        backLeftWheel.wheelGeometry = backLeftWheelGeometry;
        backRightWheel.wheelGeometry = backRightWheelGeometry;

        distanceBetweenWheels = (transform.parent.FindChild("FrontLeftWheel").position - transform.parent.FindChild("FrontRightWheel").position).magnitude;

        currentGear = 1; // first gear

    }

    void OnDrawGizmos()
    {
        // Debugging gizmos
        Gizmos.color = Color.blue;
        if(weightPosition != null) Gizmos.DrawSphere(transform.parent.TransformPoint(weightPosition), 0.4f);
    }

    void OnGUI()
    {      

        GUI.Box(new Rect(10, 10, 300, 200), "Debug Data");
        GUI.TextArea(new Rect(15, 30, 290, 20), "Forward velocity = " + velocity_forward * 3.6f + " km/h");

        Vector3 acceleration = transform.InverseTransformDirection((mRigidbody.velocity - lastVelocity) / Time.deltaTime);
        float tangentialAcceleration = acceleration.z;
        GUI.TextArea(new Rect(15, 60, 290, 20), "Forward acceleration = " + tangentialAcceleration + " m/s^2");

        GUI.TextArea(new Rect(15, 90, 290, 20), "engineRPM = " + engineRPM);
        GUI.TextArea(new Rect(15, 120, 290, 20), "rawEngineRPM = " + rawEngineRPM);
        GUI.TextArea(new Rect(15, 150, 290, 20), "Current Gear= " + currentGear);

    }


    void FixedUpdate ()
    {
        velocity_forward = Vector3.Dot(mRigidbody.velocity, transform.forward);

        // calculate weight dynamic transfer ----------------------

        Vector3 CenterOfMassAligned = centerOfMass; // put the three objects in the same plane (y=0)
        CenterOfMassAligned.y = 0.0f;
        Vector3 FrontAxisAligned = FrontAxisRight.localPosition;
        //FrontAxisAligned.y = 0.0f;
        FrontAxisAligned.x = 0.0f;
        Vector3 RearAxisAligned = RearAxisRight.localPosition;
        //RearAxisAligned.y = 0.0f;
        RearAxisAligned.x = 0.0f;

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

        weightPosition = (FrontAxisAligned * frontWeightPercent + RearAxisAligned * rearWeightPercent);        
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
        
        //shiftGear(); // sets the engineRPM based on rawEngineRPM
        

        // user sets a percentage of the maxEngineTorque
        float maxEngineTorque = enginePowerTorqueCurve.Evaluate(rawEngineRPM / maxRPM) * enginePower;
        engineTorque = input.userThrottle * maxEngineTorque;
        
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

    void Update()
    {
        shiftGear();        
    }

    void shiftGear()
    {
        float meanSlipRatio = Mathf.Abs(frontLeftWheel.slipRatio + frontRightWheel.slipRatio + backLeftWheel.slipRatio + backRightWheel.slipRatio) / 4;

        switch (currentGear)
        {
            case 0:

                break;
            case 1:
                engineRPM = Mathf.Clamp(rawEngineRPM * meanSlipRatio, minRPM, maxRPM);                
                if (rawEngineRPM > gearThresholds[1][1])
                {                    
                    currentGear = 2;
                    source.PlayOneShot(gears);
                }
                    break;
            case 2:
                engineRPM = Mathf.Clamp((rawEngineRPM - gearThresholds[2][0] + 1000) * meanSlipRatio, minRPM, maxRPM) ;
                if (rawEngineRPM > gearThresholds[2][1])
                {                    
                    currentGear = 3;
                    source.PlayOneShot(gears);
                }
                if (rawEngineRPM < gearThresholds[2][0])
                {                    
                    currentGear = 1;
                    source.PlayOneShot(gears);
                }
                break;
            case 3:
                engineRPM = Mathf.Clamp((rawEngineRPM - gearThresholds[3][0] + 1000) * meanSlipRatio, minRPM, maxRPM);
                if (rawEngineRPM > gearThresholds[3][1])
                {                    
                    currentGear = 4;
                    source.PlayOneShot(gears);
                }
                if (rawEngineRPM < gearThresholds[3][0])
                {                    
                    currentGear = 2;
                    source.PlayOneShot(gears);
                }
                break;
            case 4:
                engineRPM = Mathf.Clamp((rawEngineRPM - gearThresholds[4][0] + 1000) * meanSlipRatio, minRPM, maxRPM);
                if (rawEngineRPM > gearThresholds[4][1])
                {
                    currentGear = 5;
                    source.PlayOneShot(gears);
                }
                if (rawEngineRPM < gearThresholds[4][0])
                {
                    currentGear = 3;
                    source.PlayOneShot(gears);
                }
                break;
            case 5:
                engineRPM = Mathf.Clamp((rawEngineRPM - gearThresholds[5][0] + 1000) * meanSlipRatio, minRPM, maxRPM);
                if (rawEngineRPM > gearThresholds[5][1])
                {
                    currentGear = 6;
                    source.PlayOneShot(gears);
                }
                if (rawEngineRPM < gearThresholds[5][0])
                {
                    currentGear = 4;
                    source.PlayOneShot(gears);
                }
                break;
            case 6:
                engineRPM = Mathf.Clamp((rawEngineRPM - gearThresholds[6][0] + 1000) * meanSlipRatio, minRPM, maxRPM);
                if (rawEngineRPM < gearThresholds[6][0])
                {
                    currentGear = 5;
                    source.PlayOneShot(gears);
                }                
                break;


        }    
    }
        
}
