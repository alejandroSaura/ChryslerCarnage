using UnityEngine;
using System.Collections;

public class CameraPivotController : MonoBehaviour
{
    public float smoothFactor = 0.4f;
    Transform car;

	// Use this for initialization
	void Start ()
    {
        car = transform.parent;
	}
	
	// Update is called once per frame
	void LateUpdate ()
    {
        transform.forward = Vector3.Lerp(transform.forward, car.GetComponent<Rigidbody>().velocity, Time.deltaTime * smoothFactor);
	}
}
