using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public GameObject unitPrefab; // unit model

	public Button button;

	public GameObject Camera; // camera

	public GameObject tileMap;

	[HideInInspector] public GameObject selectedObject; // currently selected object

	// This is only temporarily a Start() function.
	// Later on, this method will be called when a
	// new game is started from the main menu.
	void Awake() {
		// Make this gameobject persistant throughout scenes
		DontDestroyOnLoad(gameObject);

		// Create the unit
		GameObject unit = Instantiate(unitPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		// Give the unit a pathfinding script and spawn it
		UnitPathfinding unitPathfinding = unit.AddComponent<UnitPathfinding>();
		selectedObject = unit;

		// Initiate all the tile map stuff
		tileMap.GetComponent<TileMap>().InitiateTileMap(unit);

		// Initiate camera controller
		Camera.GetComponent<CameraController>().selectedObject = this.selectedObject;

		// Initiate button
		button.GetComponent<Button>().onClick.AddListener(delegate { unitPathfinding.NextTurn(); });
	}

    void Update() {
        if (selectedObject != tileMap.GetComponent<TileMap>().selectedUnit) {
			// Disable pathfinding?
        }
    }
}
