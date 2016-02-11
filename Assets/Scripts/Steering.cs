using UnityEngine;
using System.Collections;

public class Steering : MonoBehaviour {

    Transform wheelGeometry;

    // Use this for initialization
    void Start ()
    {
        wheelGeometry = transform.FindChild("wheel");
    }
	
	// Update is called once per frame
	void Update ()
    {
        Quaternion rot = Quaternion.Euler(50.0f, 0.0f, 0.0f);
        wheelGeometry.rotation.Set(rot.x, rot.y, rot.z, rot.w);
    }
}
