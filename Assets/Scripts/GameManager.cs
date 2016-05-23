using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{

    public int lapNumber = 0;

    PathDecisor pathDecisor;
    List<CarRaceController> cars;

    FollowPathV2 deathWallFollower;
    FollowPathV2 carRespawner;

    public float deathWallDistance = 4;
    public float carRespawnerDistance = 2;

    public float[] positionPenalization = new float[4];

    GameObject finishLine;
    LapTrigger[] triggers;

    void Start ()
    {
        cars = new List<CarRaceController>(GameObject.Find("Cars").GetComponentsInChildren<CarRaceController>());

        deathWallFollower = GameObject.Find("DeathWall").GetComponent<FollowPathV2>();
        deathWallFollower.followFromBehind = true;
        deathWallFollower.desiredDistToFollower = deathWallDistance;

        finishLine = GameObject.Find("FinishLine");
        triggers = GameObject.Find("LapTriggers").GetComponentsInChildren<LapTrigger>();

        pathDecisor = gameObject.GetComponent<PathDecisor>();

        carRespawner = GameObject.Find("CarRespawner").GetComponent<FollowPathV2>();
        carRespawner.followFromBehind = true;
        carRespawner.desiredDistToFollower = carRespawnerDistance;

        bool lapCalculated = false;
        while (!lapCalculated)
        {
            lapCalculated = pathDecisor.CalculateLapPath();
        }
    }
	
	void Update ()
    {
        SortCars();
        UpdateDeathWall();

        //pathDecisor.CalculateLapPath();
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

    void UpdateDeathWall()
    {
        deathWallFollower.follower = cars[0].transform;
        carRespawner.follower = cars[0].transform;
    }

    public void RespawnCar(CarRaceController carToRespawn)
    {
        Debug.Log("Car is Respawning");
        carToRespawn.gameObject.transform.position = carRespawner.transform.position + carRespawner.transform.up * 2;

        if (carRespawner.reverse)
        {
            carToRespawn.gameObject.transform.rotation = carRespawner.transform.rotation * Quaternion.Euler(0, 180, 0);
        }
        else
        {
            carToRespawn.gameObject.transform.rotation = carRespawner.transform.rotation;
        }

        carToRespawn.transform.FindChild("NodeToFollow").GetComponent<FollowPathV2>().currentSpline = carRespawner.currentSpline;
        carToRespawn.transform.FindChild("NodeToFollow").GetComponent<FollowPathV2>().reverse = carRespawner.reverse;

    }

    public void LapCounter()
    {
        Debug.Log("Thats A Lap");
        pathDecisor.CalculateLapPath();

        finishLine.SetActive(true);
    }

    public void FinishLineCrossed()
    {
        lapNumber++;
        
        for (int i = 0; i < triggers.Length; i++)
        {
            triggers[i].gameObject.SetActive(true);
        }
    }
}
