using UnityEngine;
using System.Collections;

public class CameraPivotController : MonoBehaviour
{
    Transform car;

	// Use this for initialization
	void Start ()
    {
        car = transform.parent;
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.forward = car.GetComponent<Rigidbody>().velocity.normalized;
	}
}
