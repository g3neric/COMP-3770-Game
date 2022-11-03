using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableTile : MonoBehaviour {
	// These variables are used by TileMap.cs when instantiating each tile
	[HideInInspector] public int x;
	[HideInInspector] public int y;
	[HideInInspector] public int type;
	[HideInInspector] public TileMap map;
	[HideInInspector] public GameManager gameManager;
	[HideInInspector] private UnitPathfinding unitPathfinding;

	// This variable contains a reference to the game object of the current
	// character - player or enemy - on the tile.
	public GameObject currentCharacterOnTile = null;

	// Check whether the hover outline has already been moved to this tile
	bool completed = false;

	// When mouse is released, calculate the tile being clicked
	void OnMouseUp() {
		unitPathfinding = gameManager.selectedUnit.GetComponent<UnitPathfinding>();
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (currentCharacterOnTile == null) {
				// nobody on tile right now
				currentCharacterOnTile = gameManager.selectedUnit;

				unitPathfinding.PathToLocation(x, y, gameObject);
				unitPathfinding.TakeMovement();
			} else if (currentCharacterOnTile.tag == "enemy") {
				// do something - attack?
            }
		}
	}

	// Move hover outline to current tile
    void OnMouseEnter() {
		unitPathfinding = gameManager.selectedUnit.GetComponent<UnitPathfinding>();
		if (!EventSystem.current.IsPointerOverGameObject() && !completed) {
			unitPathfinding.tileHoverOutline.transform.position = transform.position + new Vector3(0, 0.011f, 0);
			completed = true;
		}
    }

	// Banish hover outline to the void
    void OnMouseExit() {
		unitPathfinding = gameManager.selectedUnit.GetComponent<UnitPathfinding>();
		if (!EventSystem.current.IsPointerOverGameObject()) {
			completed = false;
			unitPathfinding.tileHoverOutline.transform.position = new Vector3(-100f, -100f, -100f);
		}
	}

	// These next two methods are for optimization.
	// Note: OnBecameVisible() triggers when the object is viewed by ANY camera...
	// including the scene view camera. When in the editor, you have to point the scene
	// view camera AWAY from the tilemap for these two to work!

	// When tile is viewable by a camera, then set its children to active
	void OnBecameVisible() {
		// Set all children to active
		foreach (Transform child in transform) {
			child.gameObject.SetActive(true);
		}
		// Set mesh renderer to active
		//gameObject.GetComponent<MeshRenderer>().enabled = true;
	}

	// When a tile moves off screen, then set its children to inactive
    void OnBecameInvisible() {
		// Set all children to inactive
		foreach (Transform child in transform) {
			child.gameObject.SetActive(false);
		}
		// Set mesh renderer to inactive
		//gameObject.GetComponent<MeshRenderer>().enabled = false;
	}
}

