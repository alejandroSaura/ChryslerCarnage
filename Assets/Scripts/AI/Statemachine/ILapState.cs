using UnityEngine;
using System.Collections;

public interface ILapState {

    void UpdateState();

    void OnTriggerEnter(Collider other);

    void ToStartState();

    void ToInLapState();

    void ToFinishState();
}
