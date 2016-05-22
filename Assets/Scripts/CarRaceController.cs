using UnityEngine;
using System.Collections;

public class CarRaceController : MonoBehaviour
{
    FollowPathV2 nodeToFollow;
    CarPhysicsController body;

    public float distanceSinceLapStart = 0;

    public int position = 0;
    private float _penalization = 0;

    public float penalization
    {
        get
        {
            return _penalization;
        }

        set
        {
            _penalization = value;
            body.powerPenalization = _penalization;
        }
    }


    // Use this for initialization
    void Start ()
    {
        body = transform.GetComponentInChildren<CarPhysicsController>();
        nodeToFollow = transform.GetComponentInChildren<FollowPathV2>();
    }
	
	public float GetDistanceSinceLapStart()
    {
        distanceSinceLapStart = nodeToFollow.distanceSinceLapStart;
        return nodeToFollow.distanceSinceLapStart;
    }

    public void RestartDistanceSinceLapStart()
    {
        nodeToFollow.distanceSinceLapStart = 0;
    }
}
