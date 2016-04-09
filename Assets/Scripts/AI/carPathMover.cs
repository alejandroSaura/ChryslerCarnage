using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class carPathMover : MonoBehaviour
{
    public Vector3 centerOfMass;
    public List<Transform> path;
    public Transform pathgroup;
    private float maxSteer = 15.0f;
    public float topSpeed = 10000.0f;
    public float maxTorque = 5000;
    public float currentSpeed;
    public float decellerationSpeed = 10;
    public int currentPathNode;
    public float distFromPath = 20;
    Rigidbody aiRigidBody;
    
    
    public PhysicsWheel FLwheel;
    public PhysicsWheel FRwheel;
    //public PhysicsWheel RLwheel;
    //public PhysicsWheel RRwheel;
    public float dir;
    // Use this for initialization
    void Start()
    {
        getPath();
        aiRigidBody = gameObject.GetComponent<Rigidbody>();
        aiRigidBody.centerOfMass = centerOfMass;
    }

    void getPath()
    {
        Transform[] path_objs = pathgroup.GetComponentsInChildren<Transform>();
        path = new List<Transform>();

        for (int i = 0; i < path_objs.Length; i++)
        {
            if (path_objs[i] != pathgroup)
            {
                path.Add(path_objs[i]);
            }
        }
        //Debug.Log(path.Count);
    }


    // Update is called once per frame
    void Update()
    {

    }

    //void Steering()
    //{
    //    Vector3 steerVector = transform.InverseTransformPoint(new Vector3(path[currentPathNode].position.x, transform.position.y, path[currentPathNode].position.z));
    //    float newSteer = maxSteer * (steerVector.x / steerVector.magnitude);
    //    dir = steerVector.x / steerVector.magnitude;
    //    // referenceWheel.wheelSteerAngleTarget = newSteer;
    //    //FLwheel.wheelSteerAngleTarget = newSteer;
    //    //FRwheel.wheelSteerAngleTarget = newSteer;

    //    if (steerVector.magnitude <= distFromPath)
    //    {
    //        currentPathNode++;
    //        if (currentPathNode >= path.Count)
    //        {
    //            currentPathNode = 0;
    //        }
    //    }
    //}

    //void Move()
    //{
    //      //currentSpeed = 2 * (22 / 7) * RLwheel.wheelRadius * RLwheel.angularVelocity * 60 / 10;
    //      //currentSpeed = Mathf.Round(currentSpeed);
    //    if (currentSpeed <= topSpeed)
    //    {
    //        //RLwheel.driveTorque = maxTorque;
    //        //RRwheel.driveTorque = maxTorque;
    //        //FLwheel.driveTorque = maxTorque;
    //        //FRwheel.driveTorque = maxTorque;
    //        //RLwheel.brakeTorque = 0;
    //        //RRwheel.brakeTorque = 0;
    //        //FLwheel.brakeTorque = 0;
    //        //FRwheel.brakeTorque = 0;
    //    }
    //    else
    //    {
    //        //RLwheel.driveTorque = 0;
    //        //RRwheel.driveTorque = 0;
    //        //RLwheel.brakeTorque = decellerationSpeed;
    //        //RRwheel.brakeTorque = decellerationSpeed;

    //    }
    //}
}


