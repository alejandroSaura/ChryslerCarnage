using UnityEngine;
using System.Collections;

public class WheelDustScript : MonoBehaviour
{
    private ParticleSystem dustParticle;
    public PhysicsWheel slipRef;
    // Use this for initialization
    void Start()
    {
        dustParticle = transform.GetComponent<ParticleSystem>();
        slipRef = transform.parent.GetComponentInChildren<PhysicsWheel>();
    }

    // Update is called once per frame
    void Update()
    {
        //slipRatio = 1 + slipBrakeCurve.Evaluate(angularVelocity) * userBrakeWeight.Evaluate(input.userBrake) * (weightFactor - 1);
        //Debug.Log(slipRef.dustValueAcc);
        if (slipRef.dustValue > 0 || (slipRef.dustValue < 2 && slipRef.dustValue > 1) || slipRef.dustValueAcc > 1.164) 
        {
            dustParticle.maxParticles = 500;
        }
        else
        {
            dustParticle.maxParticles = 0;
        }

        //if (hpRef.carHP < 75)
        //{
        //smokeParticle.maxParticles = Mathf.RoundToInt(100 - hpRef.carHP) * 2;
        //  }
        //smokeParticle.maxParticles = 10;
    }
}
