using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICarMovementV3 : InputInterface
{
    // State machine --------------------
    STATES currentState;
    enum STATES
    {
        FREEDRIVE,
        REVERSE
    }
    float statesCoolDownTimer = 0;
    // ----------------------------------

    public float damperConstant = 5;
    float lastSteer = 0;

    public CarRaceController otherCar = null;

    public Transform nodeToFollow;
    Vector3 lastPos;

    public float distToNode = 20;

    public float maxSteer = 3.0f;

    public AnimationCurve steeringDamper;

    public AnimationCurve moneouverSmooth;
    public float maneouverOffset = 0;

    float currentSpeed;
    public float topSpeed = 5000.0f;
    public float minSpeed = 175.0f;

    public float minDistFromNode = 5.0f;
    float stabilizer = 0.05f;

    // for releasing the throttle when in a closed curve
    float throttleModifier = 0; 

    void Start()
    {
        StartCoroutine(AIUpdate());
    }

    void Update()
    {
        currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        Accelerate();
        Steer();
    }

    void OnTriggerEnter (Collider other)
    {
        
        if (otherCar == null)
        {
            otherCar = other.transform.GetComponent<CarRaceController>();
            

        }
    }
    void OnTriggerExit (Collider other)
    {
        if (otherCar != null)
        {
           if (otherCar == other.transform.GetComponent<CarRaceController>()) otherCar = null;
        }
    }

    IEnumerator AIUpdate()
    {
        while (true)
        {
            // Something ahead?
            RaycastHit hit;
            Physics.Raycast(transform.position, transform.forward + transform.up * 0.3f, out hit);
            Debug.DrawLine(transform.position, transform.position + transform.forward * 5, Color.magenta);

            // Something behind?
            RaycastHit hitBehind;
            Physics.Raycast(transform.position, -transform.forward, out hitBehind);

            bool somethingAhead = false;
            if (hit.distance < 5)
            {
                //Debug.Log("something ahead!");
                somethingAhead = true;
            }

            bool somethingBehind = false;
            if (hitBehind.distance < 5)
            {
                //Debug.Log("something behind!");
                somethingBehind = true;
            }

            if (
                (Vector3.Dot(nodeToFollow.position - transform.position, transform.forward) < -0.1f)
                ||
                (GetComponent<Rigidbody>().velocity.magnitude < 2 && somethingAhead)
                &&
                !somethingBehind
                )
            {
                currentState = STATES.REVERSE;
            }
            else
            {
                currentState = STATES.FREEDRIVE;
            }

            maneouverOffset = 0;

            if (otherCar != null)
            {
                Vector3 relativePosition = transform.InverseTransformPoint(otherCar.transform.position);

                maneouverOffset = moneouverSmooth.Evaluate(Mathf.Abs(relativePosition.x)) * Mathf.Sign(relativePosition.x);
            }

            //if (hit.transform != null)
            //{
            //    CarRaceController otherCar = hit.transform.GetComponent<CarRaceController>();
            //    if (otherCar != null)
            //    {
            //        Vector3 relativePosition = transform.InverseTransformPoint(otherCar.transform.position);

            //        maneouverOffset = moneouverSmooth.Evaluate(relativePosition.x);

            //    }                
            //}



            yield return new WaitForSeconds(0.2f);
        }
    }

    void OnGUI()
    {
        //GUI.TextArea(new Rect(15, 180, 290, 20), "Current speed= " + currentSpeed);
    }

    void OnDrawGizmos()
    {
        
    }

    void Accelerate()
    {        
        if (currentSpeed <= topSpeed)
        {
            userThrottle = (1f * (topSpeed - currentSpeed) / topSpeed) - throttleModifier;
        }

        if (currentSpeed <= minSpeed)
        {
            userThrottle = 1;
        }

        if (currentState == STATES.REVERSE) userThrottle *= -1;
    }

    void Steer()
    {
        Vector3 steerVector = transform.InverseTransformPoint(new Vector3(nodeToFollow.position.x, transform.position.y, nodeToFollow.position.z)) - new Vector3(maneouverOffset, 0, 0);
        float newSteer = (steerVector.x) * maxSteer;

        //Debug.Log("steer vector.x = " + steerVector.x);
        //Debug.Log("final steer = " + newSteer * steeringDamper.Evaluate(Mathf.Abs(steerVector.x)));

        float derivate = (newSteer - lastSteer) / Time.deltaTime;

        float damperForce = damperConstant * derivate;
        newSteer += Mathf.Clamp(damperForce, -20, 20);


        userLeftStickHorizontal =  Mathf.Clamp(newSteer * steeringDamper.Evaluate(Mathf.Abs(steerVector.x)), -1, 1);
        if (currentState == STATES.REVERSE) userLeftStickHorizontal *= -1;

        // Debug.Log(newSteer);
        if (steerVector.magnitude <= minDistFromNode)
        {
            userThrottle = 0;
        }

        // this modifier will release throttle proportional to the deviation from the target (so we can turn better)
        throttleModifier = Mathf.Clamp(
            Mathf.Clamp(Vector3.Angle(new Vector3(0, 0, 1), steerVector), 0, 90) / 90,
            0, 0.8f);

        //Debug.Log(nodeToFollow.GetComponent<FollowPathV2>().GetCurrentCurvature());
        if (nodeToFollow.GetComponent<FollowPathV2>().GetCurrentCurvature() > 2) //throttleModifier = 1;
            throttleModifier += Mathf.Clamp(nodeToFollow.GetComponent<FollowPathV2>().GetCurrentCurvature(), 0, 0.6f);


        lastSteer = newSteer;
    }

}
