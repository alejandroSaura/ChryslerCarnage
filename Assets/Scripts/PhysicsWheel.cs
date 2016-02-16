using UnityEngine;
using System.Collections;

public class PhysicsWheel : MonoBehaviour {

    Rigidbody mRigidbody;

    void Start ()
    {
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        
    }

    void FixedUpdate ()
    {
        mRigidbody.AddForce( -100 * transform.forward);
        
    }
}
