using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This is probably not going to last until the finished game
// Just a dev feature so we can skip turn with E key

public class BasicInputController : MonoBehaviour {
    public GameObject unit;
    void Update() {
        if (Input.GetKeyDown("e")) {
            unit.GetComponent<UnitPathfinding>().NextTurn();
        }
    }
}
