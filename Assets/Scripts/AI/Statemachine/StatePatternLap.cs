using UnityEngine;
using System.Collections;

public class StatePatternLap : MonoBehaviour {


    [HideInInspector] public ILapState currentState;
    [HideInInspector] public StartState startState;
    [HideInInspector] public InLapState inLapState;
    [HideInInspector] public FinishState finishState;

    void Awake() {
        startState = new StartState(this);
        inLapState = new InLapState(this);
        finishState = new FinishState(this);
;    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
