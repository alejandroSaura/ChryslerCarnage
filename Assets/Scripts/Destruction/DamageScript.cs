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
    //public List<SphereCollider> colliderArray;
    public SphereCollider[] colliderArray;
    //1 - right door
    //2- left door
    //3- middle - front
    //4- left - front
    //5- right - front
    //6- back right
    //7- back left
	// Use this for initialization
	void Start () {
        damagedParticle.SetActive(false);
	}


    void OnCollisionEnter(Collision col)
    {
        //if (col.gameObject.tag == "FrontBumper")
        //{
        //    if (col.relativeVelocity.magnitude > 25)
        //    {
        //        GameObject.FindGameObjectsWithTag("FrontBumper").SetActive(false);
        //    }
        //}
        //Vector3 dir = (col.gameObject.transform.position - gameObject.transform.position).normalized;
        //if (Mathf.Abs(dir.z) < 0.05f)
        //{
        //    if(dir.x > 0)
        //    {
        //        Debug.Log("RIGHT");
        //    }
        //    else if (dir.x <0)
        //    {
        //        Debug.Log("LEFT");
        //    }
        //    else
        //    {
        //        if(dir.z >0)
        //        {
        //            Debug.Log("FRONT");
        //        }
        //        else if (dir.z < 0)
        //        {
        //            Debug.Log("BACK");
        //        }
        //    }
        //}

        if (col.gameObject.tag == "Track" || col.gameObject.tag == "Car")
        {
            //print("Points colliding: " + col.contacts.Length);
            //print("First point that collided: " + col.contacts[0].point);
            if (col.relativeVelocity.magnitude > 25)
            {
                impactAmount = col.relativeVelocity.magnitude * 0.2f;
                carHP -= impactAmount;
                ContactPoint contact = col.contacts[0];
                Quaternion rot = Quaternion.FromToRotation(contact.normal, Vector3.forward);
                Vector3 pos = contact.point;
                if (carHP > 0)
                {
                    //transform.DetachChildren();
                    Instantiate(collisionParticle, pos, rot);
                }
                if(carHP <75)
                {
                    damagedParticle.SetActive(true);

                    //col.collider 
                    if(col.collider==colliderArray[0])
                    {
                        GameObject door = GameObject.FindGameObjectWithTag("DoorRight");
                        door.transform.parent = null;
                    }
                }
            }
            if (carHP < 0)
            {
                //transform.DetachChildren();
                carObject.SetActive(false);
                Instantiate(DeathParticle, transform.position, transform.rotation);
                Instantiate(mainCamera,transform.position,transform.rotation);
               // Debug.Log("Destroyed!");
            }
        }
       // Debug.Log(col.relativeVelocity.magnitude);
        //Debug.Log("Collided");
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(carHP);
      //  Debug.Log(impactAmount);
    }
}
