using UnityEngine;
using System.Collections;

public class DamageParticle : MonoBehaviour {
    private ParticleSystem smokeParticle;
    public DamageScript hpRef;
	// Use this for initialization
	void Start () {
        smokeParticle = GetComponent<ParticleSystem>();
        hpRef = GetComponent<DamageScript>();
	}
	
	// Update is called once per frame
	void Update () {
        smokeParticle.maxParticles = Mathf.RoundToInt(100-hpRef.carHP);//Mathf.RoundToInt(hpRef.carHP)
        //smokeParticle.maxParticles = 10;
	}
}
