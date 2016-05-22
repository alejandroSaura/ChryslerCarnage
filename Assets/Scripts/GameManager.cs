using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

    PathDecisor pathDecisor;
    List<CarRaceController> cars;

    public float[] positionPenalization = new float[4];


    void Start ()
    {
        cars = new List<CarRaceController>(GameObject.Find("Cars").GetComponentsInChildren<CarRaceController>());
        

        pathDecisor = gameObject.GetComponent<PathDecisor>();

        bool lapCalculated = false;
        while (!lapCalculated)
        {
            lapCalculated = pathDecisor.CalculateLapPath();
        }
    }
	
	void Update ()
    {
        SortCars();
    }

    void SortCars()
    {
        // Sort the cars
        cars = cars.OrderByDescending(car => car.GetDistanceSinceLapStart()).ToList();

        for (int i = 0; i < cars.Count; ++i)
        {
            cars[i].position = i + 1;
            cars[i].penalization = positionPenalization[i];
        }
    }
}
