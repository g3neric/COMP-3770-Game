// Desgined and created by Andrew Simon and Tyler R. Renaud
// All rights belong to creator

using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableTile : MonoBehaviour {
	// These variables are used by TileMap.cs when instantiating each tile
	[HideInInspector] public int x;
	[HideInInspector] public int y;
	[HideInInspector] public int type;
	[HideInInspector] public TileMap map;
	[HideInInspector] public GameManager gameManager;
	[HideInInspector] private PlayerManager playerManager;

	// This variable contains a reference to the game object of the current
	// character - player or enemy - on the tile.
	public GameObject currentCharacterOnTile = null;

	// Check whether the hover outline has already been moved to this tile
	bool completed = false;

    // When mouse is released, calculate the tile being clicked
    void OnMouseUp() {
		playerManager = gameManager.selectedUnit.GetComponent<PlayerManager>();
		// check if UI object in the way
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (gameManager.cs == ControlState.Move) {
				// move state
				playerManager.PathToLocation(x, y);
			} else {
				// not move state, therefore attack state
				if (currentCharacterOnTile != null) {
					if (currentCharacterOnTile.name.Substring(0, 5) == "Enemy") {
						gameManager.AttackEnemy(currentCharacterOnTile);
					}
				}
			}
		}
	}

	// Move hover outline to current tile
    void OnMouseEnter() {
		playerManager = gameManager.selectedUnit.GetComponent<PlayerManager>();
		if (!EventSystem.current.IsPointerOverGameObject() && !completed) {
			playerManager.tileHoverOutline.transform.position = transform.position + new Vector3(0, 0.012f, 0);
			completed = true;

			// update cursor text
			if (gameManager.tileMap.UnitCanEnterTile(x, y) &&
				gameManager.tileMap.IsTileVisibleToPlayer(x,y)) {
				gameManager.uiManager.UpdateCursorMovementCost(x, y);
			}
		}
    }

	// Banish hover outline to the void
    void OnMouseExit() {
		playerManager = gameManager.selectedUnit.GetComponent<PlayerManager>();
		if (!EventSystem.current.IsPointerOverGameObject()) {
			completed = false;
			playerManager.tileHoverOutline.transform.position = new Vector3(-100f, -100f, -100f);
		}
	}

	// These next two methods are for optimization.
	// Note: OnBecameVisible() triggers when the object is viewed by ANY camera...
	// including the scene view camera. When in the editor, you have to point the scene
	// view camera AWAY from the tilemap for these two to work!

	// NOTE: may not need these for optimization when fog of war is being used,
	// but we'll see.


	/*
	// When tile is viewable by a camera, then set its children to active
	void OnBecameVisible() {
		// Set all children to active
		if (transform.gameObject.GetComponent<MeshRenderer>().material != gameManager.fogOfWarOutlineMaterial) {
			foreach (Transform child in transform) {
				child.gameObject.SetActive(true);
			}
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
	}*/
}

