using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PathDecisor : MonoBehaviour
{
    public POI startPOI = null;
    public string startingIncomingWay = "South";

    Dictionary<string, POI> PointsOfInterest;

	void Start ()
    {
        PointsOfInterest = new Dictionary<string, POI>();
        PointsOfInterest.Add("North", GameObject.Find("NORTH").GetComponent<POI>());
        PointsOfInterest.Add("South", GameObject.Find("SOUTH").GetComponent<POI>());
        PointsOfInterest.Add("East", GameObject.Find("EAST").GetComponent<POI>());
        PointsOfInterest.Add("West", GameObject.Find("WEST").GetComponent<POI>());
        PointsOfInterest.Add("Center", GameObject.Find("CENTER").GetComponent<POI>());

        //startPOI = PointsOfInterest["West"];
        startPOI.IncomingWay = startingIncomingWay;
        
    }    

    public bool CalculateLapPath()
    {
        // debug string
        string lap = "Lap : ";

        Random.seed = (int)System.DateTime.Now.Ticks;

        // Set all POIs as non visited
        foreach (KeyValuePair<string, POI> p in PointsOfInterest)
        {
            p.Value.Visited = false;
        }

        POI currentPOI = startPOI;
        do
        {
            List<POI> possiblePOIs = new List<POI>();
            foreach (KeyValuePair<string, POI> neighbour in currentPOI.neighbours)
            {// select the non visited neighbours
                if (neighbour.Value.Visited == false || neighbour.Value == startPOI) possiblePOIs.Add(neighbour.Value);
            }
            
            POI poiToDelete = currentPOI.neighbours[currentPOI.IncomingWay];
            possiblePOIs.Remove(poiToDelete);

            POI nextPOI;
            if (possiblePOIs.Count > 1)
            {
                int randomSelector = Random.Range(0, possiblePOIs.Count);
                nextPOI = possiblePOIs[randomSelector];
            }
            else
            {
                if (possiblePOIs.Count == 0)
                {
                    Debug.Log("Invalid Lap, calculate a new one.");
                    return false;
                }
                else {
                    nextPOI = possiblePOIs[0];
                }
            }

            currentPOI.OutgoingWay = currentPOI.neighbours.FirstOrDefault(n => n.Value == nextPOI).Key;
            nextPOI.IncomingWay = nextPOI.neighbours.FirstOrDefault(n => n.Value == currentPOI).Key;

            currentPOI.Visited = true;

            lap += currentPOI.name + ", ";

            currentPOI = nextPOI;

        } while (currentPOI != startPOI);

        Debug.Log(lap);

        //Propagate selected ways to all POIs
        foreach (KeyValuePair<string, POI> p in PointsOfInterest)
        {
            p.Value.SetPath();
        }

        return true;
    }
}
