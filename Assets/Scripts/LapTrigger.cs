using UnityEngine;
using System.Collections;

public class LapTrigger : MonoBehaviour
{

    public GameManager gameManager;


    // Use this for initialization
    void Start()
    {

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {


    }

    void OnTriggerEnter(Collider other)
    {
        CarRaceController car = other.GetComponent<CarRaceController>();
        if (car == null) car = other.transform.parent.GetComponent<CarRaceController>();
        if (car != null)
        {
            if (Vector3.Dot(transform.forward, other.transform.forward) < 0)
            {
                Debug.Log("It went through");
                gameManager.LapCounter();
            }
        }
    }
}
