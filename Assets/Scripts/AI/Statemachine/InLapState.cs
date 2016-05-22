using UnityEngine;
using System.Collections;

public class InLapState : ILapState {

    private readonly StatePatternLap LapState;

    public InLapState(StatePatternLap statePatternLap)
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
