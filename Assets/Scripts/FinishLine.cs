using UnityEngine;
using System.Collections;

public class FinishLine : MonoBehaviour
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
        if (car != null && car.position == 1)
        {
            
                Debug.Log("FinishLine crossed");
                gameManager.FinishLineCrossed();

                gameObject.SetActive(false);
            
        }
    }
}
