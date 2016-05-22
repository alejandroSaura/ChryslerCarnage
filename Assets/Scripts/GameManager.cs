﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

    PathDecisor pathDecisor;
    List<CarRaceController> cars;

    FollowPathV2 deathWallFollower;
    FollowPathV2 carRespawner;

    public float deathWallDistance = 4;
    public float carRespawnerDistance = 2;

    public float[] positionPenalization = new float[4];


    void Start ()
    {
        cars = new List<CarRaceController>(GameObject.Find("Cars").GetComponentsInChildren<CarRaceController>());

        deathWallFollower = GameObject.Find("DeathWall").GetComponent<FollowPathV2>();
        deathWallFollower.followFromBehind = true;
        deathWallFollower.desiredDistToFollower = deathWallDistance;

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
}
