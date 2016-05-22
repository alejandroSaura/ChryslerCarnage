using UnityEngine;
using System.Collections;

public class StartState : ILapState {

    private readonly StatePatternLap LapState;

    public StartState(StatePatternLap statePatternLap)
    {
        LapState = statePatternLap;
    }

    public void UpdateState()
    {

    }

    public void OnTriggerEnter(Collider other)
    {

    }

    public void ToStartState()
    {

    }

    public void ToInLapState()
    {

    }

    public void ToFinishState()
    {

    }
}
