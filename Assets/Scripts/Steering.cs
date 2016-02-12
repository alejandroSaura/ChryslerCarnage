using UnityEngine;
using System.Collections;

public class Steering : MonoBehaviour {

    public float maxSteeringAngle = 35.0f;
    public float steeringVelocityFactor = 10.0f; // TO-DO: maybe this could slightly depend on the car velocity.

    public float angle;
    public float target;
    public Vector3 targetVector;

    Transform wheelGeometry;
    InputInterface input;

    // Use this for initialization
    void Start ()
    {
        wheelGeometry = transform.FindChild("wheel");
        input = transform.parent.gameObject.GetComponent<InputInterface>();
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {    

        target = maxSteeringAngle * input.userLeftStickHorizontal;

        //angle = Vector3.Angle(transform.forward, transform.parent.forward);
        //angle *= Mathf.Sign(Vector3.Dot(transform.right, transform.parent.forward));        
        //float rotationAngle = target - angle;

        targetVector = transform.parent.forward;
        targetVector = Quaternion.Euler(0, target, 0) * targetVector;      

        transform.LookAt(transform.position + targetVector);        
    }
}
