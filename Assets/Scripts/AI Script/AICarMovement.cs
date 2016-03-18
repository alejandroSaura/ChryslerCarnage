using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICarMovement : InputInterface {
    public List<Transform> path;
    public Transform pathgroup;
    public int currentPathNode;
    public float maxSteer = 3.0f;
    double currentSpeed;
    double topSpeed = 175.0;
    float distFromPath = 100.0f;
    float stabilizer = 0.05f;
    //carPathMover pathReference;
    Vector3 maxPos;
    Vector3 minPos;
    Vector3 offsetPos;
    Vector3 throttlerOffset;
    Vector3 currentPosition;
    // Use this for initialization
    void Start () {
        getPath();
        Vector3 offsetPos = transform.forward*5;
	}
	
	// Update is called once per frame
	void Update () {
        currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6;
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
        
        //if()
    }

    void getPath()
    {
        Transform[] path_objs = pathgroup.GetComponentsInChildren<Transform>(); //creates array of path nodes in the children
        path = new List<Transform>();

        for (int i = 0; i < path_objs.Length; i++)
        {
            if (path_objs[i] != pathgroup)
            {
                path.Add(path_objs[i]);
            }
        }
        Debug.Log(path.Count);
    }
    void Move()
    {

        //currentSpeed = 2 * (22 / 7) * RLwheel.wheelRadius * RLwheel.angularVelocity * 60 / 10;
        //currentSpeed = Mathf.Round(currentSpeed);
        if (currentSpeed <= topSpeed)
        {
            userThrottle = 1.0f;
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
        Vector3 steerVector = transform.InverseTransformPoint(new Vector3(path[currentPathNode].position.x, transform.position.y, path[currentPathNode].position.z));
        float newSteer = maxSteer * (steerVector.x / steerVector.magnitude);
        newSteer = newSteer / 10;
        //if(newSteer <= 0)
        //{
        //    newSteer += stabilizer;
        //}
        //else { newSteer -= stabilizer; }
        userLeftStickHorizontal = newSteer;
        
       // Debug.Log(newSteer);
        if (steerVector.magnitude <= distFromPath)
        {
            currentPathNode++;
            if (currentPathNode >= path.Count)
            {
                currentPathNode = 0;
            }
        }
    }
}
