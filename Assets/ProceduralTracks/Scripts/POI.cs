using UnityEngine;
using System.Collections;

public class POI : MonoBehaviour
{

    public string connection;

	// Use this for initialization
	void Start ()
    {
        Track[] tracks = GetComponentsInChildren<Track>();
          
        for (int i = 0; i < tracks.Length; ++i)
        {
            tracks[i].SetPath(connection);
        }
    }	
}
