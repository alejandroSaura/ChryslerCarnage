using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageScript : MonoBehaviour {
    public float carHP=100.0f;
    public Transform collisionParticle;
    public GameObject damagedParticle;
    public Transform DeathParticle;
    public GameObject mainCamera;
    public GameObject carObject;
    private float impactAmount;
    public SphereCollider[] colliderArray;
    public Transform hood;
    public Transform frontBumper;
    public Transform leftDoor;
    public Transform rightDoor;
    public Transform rearBumper;
    public Transform rearObject;
    public Transform engine;

    /// <summary>
    //Sphere collider numbers:
    //1 - front left
    //2 - front right
    //3 - front middle (left door for Mikko's car)
    //4 - right door (left door for Mikko's car)
    //5 - left door (right door for Mikko's car)
    //6 - back left (right door for Mikko's car)
    //7 - back right
    //8 - (back left Mikko car)
    /// </summary>

    // Use this for initialization
    void Start () {
        damagedParticle.SetActive(false);
	}

    void OnCollisionEnter(Collision col)
    {
        
            foreach (ContactPoint contact in col.contacts)
            {
                if (col.gameObject.tag == "Track" || col.gameObject.tag == "Car")
                {

                    if (col.relativeVelocity.magnitude > 25)
                    {
                        impactAmount = col.relativeVelocity.magnitude * 0.2f;
                        carHP -= impactAmount;
                        //ContactPoint contact = col.contacts[0];
                        Quaternion rot = Quaternion.FromToRotation(contact.normal, Vector3.forward);
                        Vector3 pos = contact.point;
                        if (carHP > 0)
                        {
                            //transform.DetachChildren();
                            Instantiate(collisionParticle, pos, rot);
                        }
                        if (transform.gameObject.name == "RobCar")
                        {
                        Debug.Log("Rob's car");
                            if (carHP < 75)
                            {
                                damagedParticle.SetActive(true);
                                if (contact.thisCollider == colliderArray[0] || contact.thisCollider == colliderArray[1])
                                {
                                    Debug.Log("Collided at the front");
                                    destroyHood();
                                }
                                if (contact.thisCollider == colliderArray[6] || contact.thisCollider == colliderArray[7])
                                {
                                    Debug.Log("Back collision");
                                    destroyBumper();
                                }
                                if (contact.thisCollider == colliderArray[5])
                                {
                                    Debug.Log("Left Door Broken");
                                    destroyLeftDoor();
                                }
                                if (contact.thisCollider == colliderArray[4])
                                {
                                    Debug.Log("Right Door Broken");
                                    destroyRightDoor();
                                }

                            }
                        }
                    else if (transform.gameObject.name == "MikkoCar Player")
                    {
                        Debug.Log("Mikko's car");
                        if (carHP < 75)
                        {
                            damagedParticle.SetActive(true);
                            if (contact.thisCollider == colliderArray[0] || contact.thisCollider == colliderArray[1] || contact.thisCollider == colliderArray[2])
                            {
                                Debug.Log("Collided at the front");
                                destroyHood();
                            }
                            if (contact.thisCollider == colliderArray[7] || contact.thisCollider == colliderArray[8])
                            {
                                Debug.Log("Back collision");
                                destroyBumper();
                            }
                            if (contact.thisCollider == colliderArray[5]|| contact.thisCollider == colliderArray[6])
                            {
                                Debug.Log("Left Door Broken");
                                destroyRightDoor();
                            }
                            if (contact.thisCollider == colliderArray[3]|| contact.thisCollider == colliderArray[4])
                            {
                                Debug.Log("Right Door Broken");
                                destroyLeftDoor();
                            }

                        }
                    }
                        if (carHP < 0)
                    {
                        //transform.DetachChildren();
                        carObject.SetActive(false);
                        Instantiate(DeathParticle, transform.position, transform.rotation);
                        Instantiate(mainCamera, transform.position, transform.rotation);
                        // Debug.Log("Destroyed!");
                    }
                }
            }
        }
        
        {

        }
       // Debug.Log(col.relativeVelocity.magnitude);
        //Debug.Log("Collided");
    }
	
	// Update is called once per frame
	void Update () {
        //if(Input.GetKeyUp(KeyCode.X))
        //    {
        //    destroyHood();
        //    destroyLeftDoor();
        //    destroyRightDoor();
        //    destroyBumper();
        //}
        Debug.Log(carHP);

    }
    void destroyRightDoor()
    {
        rightDoor.GetComponent<Rigidbody>().isKinematic = false;
        rightDoor.GetComponent<BoxCollider>().enabled = true;
        rightDoor.transform.parent = null;
        //GameObject leftDoor = GameObject.FindGameObjectWithTag("RightDoor");
        //Rigidbody rDoorRB = rightDoor.AddComponent<Rigidbody>();
        //BoxCollider doorCollider = rightDoor.AddComponent<BoxCollider>();
        //rDoorRB.mass = 0.01f;
        //rDoorRB.useGravity = true;
        //rightDoor.transform.parent = null;

    }
    void destroyLeftDoor()
    {
        leftDoor.GetComponent<Rigidbody>().isKinematic = false;
        leftDoor.GetComponent<BoxCollider>().enabled = true;
        leftDoor.transform.parent = null;
        //GameObject leftDoor = GameObject.FindGameObjectWithTag("LeftDoor");
        //Rigidbody lDoorRB = leftDoor.AddComponent<Rigidbody>();
        //BoxCollider doorCollider = leftDoor.AddComponent<BoxCollider>();
        //lDoorRB.mass = 0.01f;
        //lDoorRB.useGravity = true;
        //leftDoor.transform.parent = null;
    }
    void destroyHood()
    {
        hood.GetComponent<Rigidbody>().isKinematic = false;
        hood.GetComponent<Rigidbody>().AddForce(transform.up * 3);
        hood.GetComponent<BoxCollider>().enabled = true;
        hood.transform.parent = null;
        frontBumper.GetComponent<Rigidbody>().isKinematic = false;
        frontBumper.GetComponent<Rigidbody>().AddForce(transform.forward * 10);
        frontBumper.GetComponent<BoxCollider>().enabled = true;
        frontBumper.transform.parent = null;
        //GameObject hood = GameObject.FindGameObjectWithTag("FrontBumper");
        //Rigidbody hoodRB = hood.AddComponent<Rigidbody>();
        //hoodRB.AddForce(transform.forward * 2000);
        //BoxCollider doorCollider = hood.AddComponent<BoxCollider>();
        //hoodRB.mass = 0.01f;
        //hoodRB.useGravity = true;
        //hood.transform.parent = null;
    }
    void destroyBumper()
    {
        rearBumper.GetComponent<Rigidbody>().isKinematic = false;
        rearBumper.GetComponent<BoxCollider>().enabled = true;
        rearBumper.transform.parent = null;
        rearObject.GetComponent<Rigidbody>().isKinematic = false;
        rearObject.GetComponent<BoxCollider>().enabled = true;
        rearObject.transform.parent = null;
        engine.GetComponent<Rigidbody>().isKinematic = false;
        engine.GetComponent<Rigidbody>().AddForce(transform.up * 10);
        engine.GetComponent<BoxCollider>().enabled = true;
        engine.transform.parent = null;
        //GameObject bumper = GameObject.FindGameObjectWithTag("RearBumper");
        //Rigidbody bumperRB = bumper.AddComponent<Rigidbody>();
        ////bumperRB.AddForce(transform.forward * 2000);
        //BoxCollider doorCollider = bumper.AddComponent<BoxCollider>();
        //bumperRB.mass = 0.01f;
        //bumperRB.useGravity = true;
        //bumper.transform.parent = null;
    }
}
