using UnityEngine;
using System.Collections;

public class CameraPivotController : MonoBehaviour
{
    public float smoothFactor = 0.4f;
    Transform car;
    Transform carBody;

	// Use this for initialization
	void Start ()
    {
        car = transform.parent;
        carBody = transform.parent.FindChild("Body");
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        transform.forward = Vector3.Lerp(transform.forward, car.GetComponent<Rigidbody>().velocity, Time.deltaTime * smoothFactor);
        transform.rotation *= Quaternion.Euler(0, 0, car.transform.localRotation.z);

    }
}
