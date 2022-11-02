using UnityEngine;
using System.Collections.Generic;
public class UnitPathfinding : MonoBehaviour {
	// link to game manager
	[HideInInspector] public GameManager gameManager;

	// Represents the correct map-tile position
	// for this piece. Note that this doesn't necessarily mean
	// the world-space coordinates, because our map might be scaled
	// or offset or something of that nature.  Also, during movement
	// animations, we are going to be somewhere in between tiles.
	[HideInInspector] public int targetX;
	[HideInInspector] public int targetY;

	public TileMap map;

	// All the visual tile line and outline stuff
	[HideInInspector] public List<GameObject> createdLines; // list containing all created animated pathfinding lines
	[HideInInspector] public GameObject tileOutline;
	[HideInInspector] public GameObject tileHoverOutline;

	// Our pathfinding info.  Null if we have no destination ordered.
	public List<Node> currentPath = null;

	private GameObject tilePlayerIsOn; // game object of player's current tile

    void Start() {
		// Instantiate tile outlines
		// There's only two of these so we can just move them around the scene
		tileOutline = Instantiate(gameManager.tileOutlinePrefab, new Vector3(-100f, -100f, -100f), Quaternion.identity);
		tileHoverOutline = Instantiate(gameManager.tileHoverOutlinePrefab, new Vector3(-100f, -100f, -100f), Quaternion.identity);

		// Initialize list for the line segments
		createdLines = new List<GameObject>();
	}
	void FixedUpdate() {
		// Smoothly animate towards the correct map tile.
		if (currentPath != null) {
			int currNode = 0;

			// Initialize (x, y) values of all nodes in currentPath using linked list
			while (currNode < currentPath.Count - 1) {
				Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y) + new Vector3(0, 0, 0);
				Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y) + new Vector3(0, 0, 0);

				currNode++;
			}
		}

		transform.position = Vector3.Lerp(transform.position, map.TileCoordToWorldCoord(targetX, targetY), 10f * Time.fixedDeltaTime);
	}

	public void SpawnPlayer(int x, int y) {
		targetX = x; 
		targetY = y;
		transform.position = map.TileCoordToWorldCoord(targetX, targetY);
		
	}

	private void DrawPossibleMovements() {
		// I'm trying to draw the possible movements you can take on the screen
		// This is a WIP

		// "range" is the side length of the character's total range square
		// The character can move maxAP units to either side, and plus 1 for the tile they're standing on
		int range = (gameManager.characterClass.maxAP * 2) + 1;

		// Generate a graph of size maxAP * 2 + 1 by maxAP * 2 + 1
		// This will be the furthest the character could conceivably move on an open map with no obstacles
		Node[,] graph = map.GeneratePathfindingGraph(range, targetX - gameManager.characterClass.maxAP);

		// Test each tile in range of the player to see if they can path there
		for (int x = 0; x < range; x++) {
			for (int y = 0; y < range; y++) {
				if (map.DijkstraPath(gameManager.characterClass.maxAP, gameManager.characterClass.maxAP, x, y, graph) != null) {
					// There is a path to the specified tile
					// Create visual on this tile
					print("x: " + x + "   y: " + y);
                } else {
					// No path to specified tile
					print("NULL! x: " + x + "   y: " + y);
				}
            }
        }
    }

	// Advances our pathfinding progress by one tile.
	private void AdvancePathing() {
		if(currentPath==null) {
			return;
		}

		if(gameManager.characterClass.AP <= 0) {
			return;
		}

		// Teleport us to our correct "current" position, in case we
		// haven't finished the animation yet.
		transform.position = map.TileCoordToWorldCoord(targetX, targetY);

		// Get cost from current tile to next tile
		gameManager.characterClass.AP -= map.CostToEnterTile(currentPath[1].x, currentPath[1].y );

		// Move us to the next tile in the sequence
		targetX = currentPath[1].x;
		targetY = currentPath[1].y;

		// Remove the old animated line
		Destroy(createdLines[0]);
		createdLines.RemoveAt(0);
		// Remove the old "current" tile from the pathfinding list
		currentPath.RemoveAt(0);
		
		if(currentPath.Count == 1) {
			// We only have one tile left in the path, and that tile MUST be our ultimate
			// destination -- and we are standing on it!
			// So let's just clear our pathfinding info.
			currentPath = null;
		}
	}

	// Helper function to create animated line to end target
	private GameObject CreateAnimatedLine(Vector3 start, Vector3 end, float delay, bool last) {
		// Create a line and its particle system effects
		GameObject line = Instantiate(gameManager.linePrefab, start, Quaternion.identity);

		// Rotate towards next position in path
		line.transform.LookAt(end);

		// You have to access particle system properties through particlesystem.main for some reason
		ParticleSystem ps = line.transform.GetComponent<ParticleSystem>();
		var main = ps.main;
		main.startDelay = delay;

		// If the line is on an angle, then it is slightly longer
		if (start.x != end.x && start.z != end.z) {
			// Therefore, use pythagorean theorem to extend this line!
			// I just hardcoded the results since they are constants 
			main.maxParticles = 16;

			// Fudging the numbers >:)
			if (last) {
				main.startLifetime = 0.125f;
			} else {
				main.startLifetime = 0.25f;
			}

			// Change the size of the actual line object
			line.transform.localScale = new Vector3(1.41421f, 0, 1.41421f);
		}

		// Last piece in the line; should be half as long
		if (last) {
			// Since the scale is halfed here...
			line.transform.localScale /= 2;
			// ...the particles will be half the size, and go half the speed.
			// So let's fix that:
			main.startLifetime = 0.125f;
			main.startSpeed = 8;
			main.startSize = 0.2f;
		}
		return line; // will be added to a list in the calling function
	}

	public void PathToLocation(int x, int y, GameObject tileGameObject) {
		// This method encompasses everything that happens when you click a tile

		if (tilePlayerIsOn != null) {
			// player is moving from another tile, so reset the old tile
			tilePlayerIsOn.GetComponent<ClickableTile>().currentCharacterOnTile = null;
		}

		// keep track of the tile the player is moving to
		tilePlayerIsOn = tileGameObject;

		// Destroy old pathfinding visual lines
		for (int i = 0; i < createdLines.Count; i++) {
			Destroy(createdLines[i]);
		}
		createdLines.Clear();

		if (map.UnitCanEnterTile(x, y) == false) {
			// We clicked on an unwalkable tile, so yeet the tile outline into the void
			tileOutline.transform.position = new Vector3(-100f, -100f, -100f);
			return;
		}

		// Move outline to new end target
		tileOutline.transform.position = map.TileCoordToWorldCoord(x, y) - new Vector3(0, -0.01f, 0);

		// [targetX, targetY] is the current position of the unit
		// [x, y] is the target position, which was just clicked on
		map.GeneratePathTo(targetX, targetY, x, y); // generate path

		// Draw animated line to end destination
		float startDelayCount = 0;
		for (int i = 0; i < currentPath.Count - 1; i++) {
			Vector3 start = new Vector3(currentPath[i].x, 0.05f, currentPath[i].y);
			Vector3 end = new Vector3(currentPath[i + 1].x, 0.05f, currentPath[i + 1].y);
			bool last = false;
			if (i == currentPath.Count - 2) {
				last = true;
			}
			createdLines.Add(CreateAnimatedLine(start, end, startDelayCount, last));
			startDelayCount += 0.25f;
		}
	}
	public void TakeMovement() {
		// Make sure to wrap-up any outstanding movement left over.
		while (currentPath != null && gameManager.characterClass.AP > 0) {
			AdvancePathing();
		}
	}
}
