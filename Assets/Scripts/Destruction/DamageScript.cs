﻿using UnityEngine;
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

    /// <summary>
    //Sphere collider numbers:
    //1 - front left
    //2 - front right
    //3 - front middle
    //4 - right door
    //5 - left door 
    //6 - back left
    //7 - back right
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
                    if (carHP < 75)
                    {
                        damagedParticle.SetActive(true);
                        if (contact.thisCollider == colliderArray[0] || contact.thisCollider == colliderArray[1] || contact.thisCollider == colliderArray[2])
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
                        if(contact.thisCollider ==  colliderArray[4])
                        {
                            Debug.Log("Right Door Broken");
                            destroyRightDoor();
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
        Transform rightDoor = transform.Find("Door_Right");
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
        Transform leftDoor = transform.Find("Door_Left");
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
        Transform hood = transform.Find("Hood");
        hood.GetComponent<Rigidbody>().isKinematic = false;
        hood.GetComponent<Rigidbody>().AddForce(transform.forward * 10);
        hood.GetComponent<BoxCollider>().enabled = true;
        hood.transform.parent = null;
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
        Transform bumper = transform.Find("Bumper_Rear");
        bumper.GetComponent<Rigidbody>().isKinematic = false;
        bumper.GetComponent<BoxCollider>().enabled = true;
        bumper.transform.parent = null;
        //GameObject bumper = GameObject.FindGameObjectWithTag("RearBumper");
        //Rigidbody bumperRB = bumper.AddComponent<Rigidbody>();
        ////bumperRB.AddForce(transform.forward * 2000);
        //BoxCollider doorCollider = bumper.AddComponent<BoxCollider>();
        //bumperRB.mass = 0.01f;
        //bumperRB.useGravity = true;
        //bumper.transform.parent = null;
    }
}
