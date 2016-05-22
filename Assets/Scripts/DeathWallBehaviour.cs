using UnityEngine;
using System.Collections;

public class DeathWallBehaviour : MonoBehaviour {

    public GameManager gameManager; 

    // Use this for initialization
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

  	// Update is called once per frame
	void Update () {
       

    }

    void  OnTriggerEnter(Collider other)
    {
        CarRaceController car = other.GetComponent<CarRaceController>();
        if (car != null)
        {
            Debug.Log("It went through");
            gameManager.RespawnCar(car);
        }
    }
}
