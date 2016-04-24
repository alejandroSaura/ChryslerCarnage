using UnityEngine;
using System.Collections;

public class DamageParticle : MonoBehaviour {
    private ParticleSystem smokeParticle;
    public DamageScript hpRef;
	// Use this for initialization
	void Start () {
        smokeParticle = GetComponent<ParticleSystem>();
        hpRef = transform.parent.GetComponentInChildren<DamageScript>();
	}
	
	// Update is called once per frame
	void Update () {
        //if (hpRef.carHP < 75)
        //{
            smokeParticle.maxParticles = Mathf.RoundToInt(100 - hpRef.carHP)*2;//Mathf.RoundToInt(hpRef.carHP)
      //  }
        //smokeParticle.maxParticles = 10;
	}
}
