using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    PathDecisor pathDecisor;

	void Start ()
    {
        pathDecisor = gameObject.GetComponent<PathDecisor>();
    }
	
	void Update ()
    {
        bool lapCalculated = false;
        while (!lapCalculated)
        {
            lapCalculated = pathDecisor.CalculateLapPath();
        }
	}
}
