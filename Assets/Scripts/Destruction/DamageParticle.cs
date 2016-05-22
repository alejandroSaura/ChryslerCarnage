using UnityEngine;
using System.Collections;

public class DamageParticle : MonoBehaviour {
    private ParticleSystem smokeParticle;

    public DamageScript hpRef;
    public DamageScriptMikko hpRefMikko;

    // Use this for initialization
    void Start () {
        smokeParticle = transform.GetComponent<ParticleSystem>();
        hpRef = transform.parent.GetComponentInChildren<DamageScript>();
        hpRefMikko = transform.parent.GetComponentInChildren<DamageScriptMikko>();

    }

    // Update is called once per frame
    void Update () {
        //if (hpRef.carHP < 75)
        //{
            if(hpRef != null) smokeParticle.maxParticles = Mathf.RoundToInt(100 - hpRef.carHP)*2;
            if (hpRefMikko != null) smokeParticle.maxParticles = Mathf.RoundToInt(100 - hpRefMikko.carHP) * 2;
        //  }
        //smokeParticle.maxParticles = 10;
    }
}
