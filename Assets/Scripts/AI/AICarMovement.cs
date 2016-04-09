using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICarMovement : InputInterface
{    

    public Transform nodeToFollow;

    public float distToNode = 20;

    public float maxSteer = 3.0f;
    float currentSpeed;
    public float topSpeed = 175.0f;
    public float minDistFromNode = 5.0f;
    float stabilizer = 0.05f;

    Vector3 maxPos;
    Vector3 minPos;
    Vector3 offsetPos;
    Vector3 throttlerOffset;
    Vector3 currentPosition;

    void Start ()
    {        
        Vector3 offsetPos = transform.forward*5;
	}
	
	void Update ()
    {
        ReallocateNode();

        currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        Move();
        steering();
        offsetPos = new Vector3(0,0,100);
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
        Gizmos.DrawSphere((transform.position + transform.forward * 10/2), 0.35f);        
        
    }    

    void Move()
    {
        //currentSpeed = 2 * (22 / 7) * RLwheel.wheelRadius * RLwheel.angularVelocity * 60 / 10;
        //currentSpeed = Mathf.Round(currentSpeed);
        if (currentSpeed <= topSpeed)
        {
            userThrottle = 1f * (topSpeed - currentSpeed)/topSpeed;
        }
        else if(Input.GetKeyDown(KeyCode.S))
        {
            userThrottle = 0;
            // Debug.Log("Should Brake");
            userBrake = 1.0f;
        }
        
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
    }

    void ReallocateNode()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + transform.forward*2+ new Vector3(0, 2, 0), -Vector3.up, out hit, 30))
        {
            //Debug.Log("HIT");

            BezierSpline spline = hit.collider.transform.GetComponent<BezierSpline>();
            if (spline != null) ReallocateNodeOverSpline(spline);

        }
    }

    void ReallocateNodeOverSpline(BezierSpline spline)
    {
        Vector3 newPosition;

        // Collide with spline that can belong to 2 possible objects, biffurcation or curve
        Curve curve = spline.transform.parent.GetComponent<Curve>();
        Bifurcation bifurcation = spline.transform.parent.GetComponent<Bifurcation>();

        if (curve != null)
        {// we are in a curve
            Debug.DrawLine(transform.position, nodeToFollow.position, Color.cyan);
        }

        if (bifurcation != null)
        {// we are in a bifurcation
            if (!spline.isPath)
            {
                if (bifurcation.splines[0] == spline) spline = bifurcation.splines[1];
                if (bifurcation.splines[1] == spline) spline = bifurcation.splines[0];

            }
        }

        newPosition = spline.GetClosestPoint(transform.position + transform.forward * distToNode, 0.0001f);
        nodeToFollow.position = newPosition;

        // end or beggining of spline reached, check next/previous spline
        if (newPosition == Vector3.zero)
        {// jump to previous spline
            RaycastHit hit;
            if (Physics.Raycast(spline.startNode.position - spline.startNode.transform.forward + new Vector3(0, 2, 0), -Vector3.up, out hit, 30))
            {
                spline = hit.collider.transform.GetComponent<BezierSpline>();
            }
            ReallocateNodeOverSpline(spline);
        }
        if (newPosition == new Vector3(1,1,1))
        {// jump to next spline
            RaycastHit hit;
            if (Physics.Raycast(spline.endNode.position + spline.endNode.transform.forward + new Vector3(0, 2, 0), -Vector3.up, out hit, 30))
            {
                spline = hit.collider.transform.GetComponent<BezierSpline>();
            }
            ReallocateNodeOverSpline(spline);
        }

        
    }


}
