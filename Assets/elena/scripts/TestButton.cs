using UnityEngine;
using System.Collections;

public class TestButton : MonoBehaviour {
  

	public void Play () 
    {
        Application.LoadLevel(1);
    }

    public void Help()
    {
        // hide credits
        var credits = GameObject.Find("Cred").transform;
        credits.position = new Vector3(-6000, credits.position.y, 0);

        // show help
        var help = GameObject.Find("Help").transform;
        help.position = new Vector3(400, help.position.y, 0);
    }
	
	public void Quit () 
    {
        Application.Quit();
	}

    public void Credits()
    {
        // show credits
        var credits = GameObject.Find("Cred").transform;
        credits.position = new Vector3(400, credits.position.y, 0);

        // hide help
        var help = GameObject.Find("Help").transform;
        help.position = new Vector3(-6000, help.position.y, 100);
    }
}
