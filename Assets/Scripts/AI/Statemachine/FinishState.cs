using UnityEngine;
using System.Collections;

public class FinishState : ILapState {

    private readonly StatePatternLap LapState;

    public FinishState(StatePatternLap statePatternLap)
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
