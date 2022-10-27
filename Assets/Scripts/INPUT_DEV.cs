using UnityEngine;

// This is probably not going to last until the finished game
// Just a dev feature so we can skip turn with E key

public class INPUT_DEV : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown("e")) {
            transform.GetComponent<UnitPathfinding>().NextTurn();
        }
    }
}
