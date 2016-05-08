using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class POI : MonoBehaviour
{
    string connection;

    string incomingWay;
    string outgoingWay;  
    public string IncomingWay
    {
        get
        {
            return incomingWay;
        }

        set
        {
            incomingWay = value;
        }
    }
    public string OutgoingWay
    {
        get
        {
            return outgoingWay;
        }

        set
        {
            outgoingWay = value;
        }
    }

    public bool Visited
    {
        get
        {
            return visited;
        }

        set
        {
            visited = value;
        }
    }
    bool visited;

    public string northNeighbour;
    public string southNeighbour;
    public string eastNeighbour;
    public string westNeighbour;

    public Dictionary<string, POI> neighbours;

    // Use this for initialization
    void Awake ()
    {
        neighbours = new Dictionary<string, POI>();

        if (northNeighbour != "") neighbours.Add("North", GameObject.Find(northNeighbour).GetComponent<POI>());
        if (southNeighbour != "") neighbours.Add("South", GameObject.Find(southNeighbour).GetComponent<POI>());
        if (eastNeighbour != "") neighbours.Add("East", GameObject.Find(eastNeighbour).GetComponent<POI>());
        if (westNeighbour != "") neighbours.Add("West", GameObject.Find(westNeighbour).GetComponent<POI>());        
    }     
      

    public void SetPath()
    {
        connection = "";

        // Set connection from incoming and outgoing directions
        if ( incomingWay == "North" || outgoingWay == "North")
        {
            if (incomingWay == "South" || outgoingWay == "South")
            {
                connection = "north_south";
            }
            else if (incomingWay == "East" || outgoingWay == "East")
            {
                connection = "north_east";
            }
            else if (incomingWay == "West" || outgoingWay == "West")
            {
                connection = "north_west";
            }
        }
        else if (incomingWay == "South" || outgoingWay == "South")
        {
            if (incomingWay == "East" || outgoingWay == "East")
            {
                connection = "south_east";
            }
            else if (incomingWay == "West" || outgoingWay == "West")
            {
                connection = "south_west";
            }
        }
        else if (incomingWay == "East" || outgoingWay == "East")
        {
            connection = "east_west";
        }

        if (visited == false) connection = ""; 


        Track[] tracks = GetComponentsInChildren<Track>();

        for (int i = 0; i < tracks.Length; ++i)
        {
            tracks[i].SetPath(connection);
        }
    }
}
