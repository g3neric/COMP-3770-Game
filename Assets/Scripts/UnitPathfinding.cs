using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UnitPathfinding : MonoBehaviour {

	// Represents the correct map-tile position
	// for this piece. Note that this doesn't necessarily mean
	// the world-space coordinates, because our map might be scaled
	// or offset or something of that nature.  Also, during movement
	// animations, we are going to be somewhere in between tiles.
	[HideInInspector] public int targetX;
	[HideInInspector] public int targetY;

	// UI
	public TextMeshProUGUI movementLeft_Text;

	public TileMap map;

	// Our pathfinding info.  Null if we have no destination ordered.
	public List<Node> currentPath = null;

	// How far this unit can move in one turn. Note that some tiles cost extra.
	private int moveSpeed = 2;
	private float remainingMovement = 0;

	void Start() {
		// Player spawns in middle of map
		targetX = (int)Mathf.Floor(map.mapSizeX / 2);
		targetY = (int)Mathf.Floor(map.mapSizeY / 2);
    }

	void FixedUpdate() {
		// This will go in the player's own class later on
		movementLeft_Text.text = "Movement left: " + remainingMovement;

		if(currentPath != null) {
			int currNode = 0;

			// Initialize (x, y) values of all nodes in currentPath using linked list
			while( currNode < currentPath.Count - 1 ) {
				Vector3 start = map.TileCoordToWorldCoord( currentPath[currNode].x, currentPath[currNode].y ) + new Vector3(0, 0, 0) ;
				Vector3 end = map.TileCoordToWorldCoord( currentPath[currNode+1].x, currentPath[currNode+1].y ) + new Vector3(0, 0, 0) ;
				
				currNode++;
			}
		}

		// Smoothly animate towards the correct map tile.
		transform.position = Vector3.Lerp(transform.position, map.TileCoordToWorldCoord(targetX, targetY), 5f * Time.fixedDeltaTime);
	}

	// Advances our pathfinding progress by one tile.
	private void AdvancePathing() {
		if(currentPath==null) {
			return;
		}

		if(remainingMovement <= 0) {
			return;
		}

		// Teleport us to our correct "current" position, in case we
		// haven't finished the animation yet.
		transform.position = map.TileCoordToWorldCoord(targetX, targetY);

		// Get cost from current tile to next tile
		remainingMovement -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y );

		// Move us to the next tile in the sequence
		targetX = currentPath[1].x;
		targetY = currentPath[1].y;

		// Remove the old animated line
		Destroy(map.createdLines[0]);
		map.createdLines.RemoveAt(0);
		// Remove the old "current" tile from the pathfinding list
		currentPath.RemoveAt(0);
		
		if(currentPath.Count == 1) {
			// We only have one tile left in the path, and that tile MUST be our ultimate
			// destination -- and we are standing on it!
			// So let's just clear our pathfinding info.
			currentPath = null;
		}
	}

	// The "Next Turn" button calls this.
	public void NextTurn() {
		// Make sure to wrap-up any outstanding movement left over.
		while(currentPath!=null && remainingMovement > 0) {
			AdvancePathing();
		}

		// Reset our available movement points.
		remainingMovement = moveSpeed;
	}
}
