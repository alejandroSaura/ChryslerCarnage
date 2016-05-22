using UnityEngine;
using System.Collections;

public class ParticleAutoDestroy : MonoBehaviour {
    private ParticleSystem parSys;
	// Use this for initialization
	void Start () {
        parSys = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
	if(parSys)
        {
            if(!parSys.IsAlive())
            {
               Destroy(gameObject);
            }
        }
	}
}
