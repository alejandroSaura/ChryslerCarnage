using UnityEngine;
using System.Collections;

public class PhysicsWheel : MonoBehaviour
{
    // exposed to be set by carController
    public float supportedWeight;
    public float angularVelocity;
    public float driveTorque;
    public float wheelRadius = 0.7f;

    //debug
    public float latVelocity;

    Rigidbody mRigidbody;

    void Start ()
    {
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        
    }

    void FixedUpdate ()
    {
        // Add forces
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit) && (hit.distance) < wheelRadius) //if the wheel is touching the ground
        {
            mRigidbody.AddForce(-100 * transform.forward);

            // lateral force

            Vector3 velocity = mRigidbody.velocity;
            float carLinearVelocity = Vector3.Project(transform.parent.GetComponent<Rigidbody>().velocity, transform.parent.forward).magnitude;

            latVelocity = transform.InverseTransformDirection(velocity).y;
            if (carLinearVelocity > 0.1f)
            {
                mRigidbody.AddForceAtPosition(-latVelocity * transform.up * (1 + mRigidbody.mass * 9.8f) * (carLinearVelocity / 5), transform.position);
                Debug.DrawLine(transform.position, transform.position + -latVelocity * transform.up * (1 + mRigidbody.mass * 9.8f) * (carLinearVelocity / 5));
            }
            else
            {
                Vector3 v = mRigidbody.velocity;
                v.x = 0;
                mRigidbody.velocity = v;
            }
        }

    }
}
