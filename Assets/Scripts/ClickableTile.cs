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

	void FixedUpdate() {
		unitPathfinding = gameManager.selectedUnit.GetComponent<UnitPathfinding>();
    }

	void OnMouseUp() {
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
		if (!EventSystem.current.IsPointerOverGameObject() && !completed) {
			unitPathfinding.tileHoverOutline.transform.position = transform.position + new Vector3(0, 0.011f, 0);
			completed = true;
		}
    }

	// Banish hover outline to the void
    void OnMouseExit() {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			completed = false;
			unitPathfinding.tileHoverOutline.transform.position = new Vector3(-100f, -100f, -100f);
		}
	}

	void OnBecameVisible() {
		foreach (Transform child in transform) {
			child.gameObject.SetActive(true);
		}
	}

    void OnBecameInvisible() {
		foreach (Transform child in transform) {
			child.gameObject.SetActive(false);
		}
	}
}

