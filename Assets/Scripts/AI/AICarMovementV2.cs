using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICarMovementV2 : InputInterface
{
    public float curvatureWeight = 1000;

    public Transform nodeToFollow;
    Vector3 lastPos;

    public float distToNode = 20;

    public float maxSteer = 3.0f;

    float currentSpeed;
    public float topSpeed = 5000.0f;
    public float minSpeed = 175.0f;

    public float minDistFromNode = 5.0f;
    float stabilizer = 0.05f;

    float throttleModifier = 0;

    Vector3 maxPos;
    Vector3 minPos;
    public Vector3 offsetPos;
    Vector3 throttlerOffset;
    Vector3 currentPosition;

    void Start()
    {
        //ReallocateNode();
        lastPos = nodeToFollow.position;
        offsetPos = Vector3.zero;
    }

    void Update()
    {
        //ReallocateNode();

        currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        Move();
        steering();
        throttlerOffset = new Vector3(0, 30, 0);
        maxPos = transform.forward * 10;
        minPos = transform.position;

        //  Debug.DrawLine(transform.position, transform.position + transform.forward * 10 ,Color.blue);
        //  currentPosition = transform.position;
        //transform.LookAt(path);


    }

    void OnGUI()
    {
        GUI.TextArea(new Rect(15, 180, 290, 20), "Current speed= " + currentSpeed);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 10);
        Gizmos.DrawSphere((transform.position + transform.forward * 10 / 2), 0.35f);

    }

    void Move()
    {
        //currentSpeed = 2 * (22 / 7) * RLwheel.wheelRadius * RLwheel.angularVelocity * 60 / 10;
        //currentSpeed = Mathf.Round(currentSpeed);
        if (currentSpeed <= topSpeed)
        {
            userThrottle = (1f * (topSpeed - currentSpeed) / topSpeed) - throttleModifier;
        }

        if (currentSpeed <= minSpeed)
        {
            userThrottle = 1;
        }

        //else if (Input.GetKeyDown(KeyCode.S))
        //{
        //    userThrottle = 0;
        //    // Debug.Log("Should Brake");
        //    userBrake = 1.0f;
        //}

    }

    void steering()
    {

        Vector3 steerVector = transform.InverseTransformPoint(new Vector3(nodeToFollow.position.x, transform.position.y, nodeToFollow.position.z));
        float newSteer = (steerVector.x / steerVector.magnitude) * maxSteer;

        //if(newSteer <= 0)
        //{
        //    newSteer += stabilizer;
        //}
        //else { newSteer -= stabilizer; }
        userLeftStickHorizontal = Mathf.Clamp(newSteer, -1, 1);

        // Debug.Log(newSteer);
        if (steerVector.magnitude <= minDistFromNode)
        {
            userThrottle = 0;
        }

        // this modifier will release throttle proportional to the deviation from the target (so we can turn better)
        throttleModifier = Mathf.Clamp(
            Mathf.Clamp(Vector3.Angle(new Vector3(0,0,1), steerVector), 0, 90) / 90,
            0, 0.8f);

        Debug.Log(nodeToFollow.GetComponent<FollowPathV2>().GetCurrentCurvature());
        if (nodeToFollow.GetComponent<FollowPathV2>().GetCurrentCurvature() > 2) //throttleModifier = 1;
            throttleModifier += Mathf.Clamp(nodeToFollow.GetComponent<FollowPathV2>().GetCurrentCurvature(), 0, 1f);

    }

    void ReallocateNode()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + transform.forward * 2f + new Vector3(0, 2, 0), -transform.up, out hit, 30))
        {
            //Debug.Log("HIT");

            BezierSpline spline = hit.collider.transform.GetComponent<BezierSpline>();
            if (spline != null) ReallocateNodeOverSpline(nodeToFollow, spline, 0);

        }

        // Offset action
        offsetPos = Vector3.Lerp(offsetPos, Vector3.Project((lastPos - nodeToFollow.position), nodeToFollow.right), Time.deltaTime * 2);
        if (Mathf.Abs(offsetPos.magnitude) > 30) offsetPos = Vector3.zero;

        lastPos = nodeToFollow.position;

        nodeToFollow.position += offsetPos * curvatureWeight;
    }

    void ReallocateNodeOverSpline(Transform node, BezierSpline spline, int depth)
    {
        float newPosition;

        // Collide with spline that can belong to 2 possible objects, biffurcation or curve
        Curve curve = spline.transform.parent.GetComponent<Curve>();
        Bifurcation bifurcation = spline.transform.parent.GetComponent<Bifurcation>();

        if (curve != null)
        {// we are in a curve
            Debug.DrawLine(transform.position, nodeToFollow.position, Color.cyan);
        }

        if (bifurcation != null)
        {// we are in a bifurcation

            if (bifurcation.splines[0].isPath) spline = bifurcation.splines[0];
            else spline = bifurcation.splines[1];
        }

        newPosition = spline.GetClosestPoint(transform.position + transform.forward * distToNode, 0.01f);
        nodeToFollow.position = spline.GetPoint(newPosition);
        nodeToFollow.rotation = spline.GetOrientation(newPosition, transform.up);

        // end or beggining of spline reached, check next/previous spline
        if (newPosition == 2)
        {// jump to previous spline

            if (depth == 1)
            {// avoid infinite recursive loops
                nodeToFollow.position = spline.startNode.transform.position;
                nodeToFollow.rotation = spline.startNode.transform.rotation;
                return;
            }

            RaycastHit hit;

            Vector3 dir = -spline.startNode.transform.forward;
            //if (spline.startNode.reverse == true) dir *= -1;

            if (Physics.Raycast(spline.startNode.position + dir + new Vector3(0, 2, 0), -transform.up, out hit, 30))
            {
                spline = hit.collider.transform.GetComponent<BezierSpline>();
            }
            ReallocateNodeOverSpline(nodeToFollow, spline, 1);
        }
        if (newPosition == 3)
        {// jump to next spline

            if (depth == 1)
            {// avoid infinite recursive loops
                nodeToFollow.position = spline.endNode.transform.position;
                nodeToFollow.rotation = spline.endNode.transform.rotation;
                return;
            }

            RaycastHit hit;

            Vector3 dir = spline.endNode.transform.forward;
            //if (spline.endNode.reverse == true)
            //    dir *= -1;

            if (Physics.Raycast(spline.endNode.position + dir + new Vector3(0, 2, 0), -transform.up, out hit, 30))
            {
                spline = hit.collider.transform.GetComponent<BezierSpline>();
            }

            ReallocateNodeOverSpline(nodeToFollow, spline, 1);
        }

    }


}
