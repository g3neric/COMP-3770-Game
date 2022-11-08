// Desgined and created by Andrew Simon and Tyler R. Renaud
// All rights belong to creator

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

	// All the visual and outline stuff
	[HideInInspector] public List<GameObject> createdLines; // list containing all created animated pathfinding lines
	[HideInInspector] public GameObject tileOutline;
	[HideInInspector] public GameObject tileHoverOutline;
	[HideInInspector] public List<GameObject> createdMovementOutlines;
	[HideInInspector] public List<GameObject> createdRangeOutlines;
	[HideInInspector] public List<GameObject> enabledTiles; // for fog of war

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
		enabledTiles = new List<GameObject>();
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

		// Draw possible movements right when you spawn
		createdMovementOutlines = new List<GameObject>();
		createdRangeOutlines = new List<GameObject>();

		DrawOutlines();
	}

	public void DestroyOutlines(List<GameObject> outlines) {
		for (int i = 0; i < outlines.Count; i++) {
			Destroy(outlines[i]);
		}
		outlines.Clear();
	}

	// Instantiate tile outlines that need to be oriented correctly
	// yOffset lets you put some outlines overtop others (precedence)
	public List<GameObject> InstantiateOrientedOutlines(GameObject[] outlinePrefabs, List<int[]> tilesInRange, float yOffset) {
		List<GameObject> createdOutlines = new List<GameObject>();
		// insantiate possible movement outlines
		foreach (int[] i in tilesInRange) {
			// determine which neighbours are also in movement range
			int[] temp = new int[2];

			// [0] = above, [1] = right, [2] = left, [3] = below
			bool[] neighbourValues = new bool[4];

			// check if neighbours are also in the list of possible movements

			foreach (int[] j in tilesInRange) {
				// above neighbour
				if (j[0] == i[0] && j[1] == i[1] + 1) {
					neighbourValues[0] = true;
				}
				// right neighbour
				if (j[0] == i[0] + 1 && j[1] == i[1]) {
					neighbourValues[1] = true;
				}
				// left neighbour
				if (j[0] == i[0] - 1 && j[1] == i[1]) {
					neighbourValues[2] = true;
				}
				// below neighbour
				if (j[0] == i[0] && j[1] == i[1] - 1) {
					neighbourValues[3] = true;
				}
			}

			// count how many neighbours are in movement list
			int tempSum = 0;
			for (int j = 0; j < 4; j++) {
				if (neighbourValues[j]) {
					tempSum++;
				}
			}

			// instantiate outline - make sure current tile has between 1 and 3 neighbours in list.
			// if it doesn't, then we dont instantiate it
			if (tempSum <= 3 && tempSum > 0) {
				// position of outline
				Vector3 pos = new Vector3(i[0], yOffset, i[1]);
				// rotation of outline
				float yRotation = 0f;
				// default
				GameObject prefab = null;

				if (tempSum == 3) {
					// 3 neighbours in list
					prefab = outlinePrefabs[0];

					if (!neighbourValues[0]) {
						// no neighbour above
						yRotation = 180f;
					} else if (!neighbourValues[1]) {
						// no neighbour right
						yRotation = 270f;
					} else if (!neighbourValues[2]) {
						// no neighbour left
						yRotation = 90f;
					} else if (!neighbourValues[3]) {
						// no neighbour below
						yRotation = 0f;
					}
				} else if (tempSum == 2) {
					// 2 neighbours in list
					prefab = outlinePrefabs[1];

					// shape: |_
					if (!neighbourValues[0] && !neighbourValues[1]) {
						// no neighbour above and to right
						yRotation = 270f;
					} else if (!neighbourValues[0] && !neighbourValues[2]) {
						// no neighbour above and to left
						yRotation = 180f;
					} else if (!neighbourValues[3] && !neighbourValues[1]) {
						// no neighbour below and to right
						yRotation = 0f;
					} else if (!neighbourValues[3] && !neighbourValues[2]) {
						// no neighbour below and to left
						yRotation = 90f;

						// shape: | |
					} else if (!neighbourValues[1] && !neighbourValues[2]) {
						// no neighbour right and left
						yRotation = 90f;
						prefab = outlinePrefabs[3];
					} else if (!neighbourValues[0] && !neighbourValues[3]) {
						// no neighbour above and below
						yRotation = 180f;
						prefab = outlinePrefabs[3];
					}
				} else if (tempSum == 1) {
					// 1 neighbour in list
					prefab = outlinePrefabs[2];

					if (neighbourValues[0]) {
						// no neighbour above
						yRotation = 360f;
					} else if (neighbourValues[1]) {
						// no neighbour right
						yRotation = 90f;
					} else if (neighbourValues[2]) {
						// no neighbour left
						yRotation = 270f;
					} else if (neighbourValues[3]) {
						// no neighbour below
						yRotation = 180f;
					}
				}
				// instantiate outline
				GameObject tempObj = Instantiate(prefab, pos, Quaternion.identity);
				// rotate outline
				tempObj.transform.Rotate(0f, yRotation, 0f, Space.World);
				// add outline to list of all created outlines
				createdOutlines.Add(tempObj);
			}
		}
		return createdOutlines;
	}

	// Returns all tiles you can move to with AP action points as a list
	public List<int[]> CalculatePossibleMovements(int centerX, int centerY, int AP) {
		List<int[]> returnValues = new List<int[]>();
		// iterate over each tile in range of the player
		for (int x = centerX - AP; x <= centerX + AP; x++) {
			for (int y = centerY - AP; y <= centerY + AP; y++) {
				// check if there's a path to current tile from the player's current position
				if (map.BreadthFirstSearch(map.fullMapGraph[centerX, centerY], map.fullMapGraph[x, y], AP) != null) {
					// Put the current tile position in list
					int[] temp = new int[2];
					temp[0] = x;
					temp[1] = y;
					returnValues.Add(temp);
				}
			}
		}

		return returnValues;
	}

	// Outline the tiles that the player has enough AP to move to
	// This function is a little messy at the bottom with all the if statements
	// and hard coded values, but whatever IDK how to optimize it more than this
	public void DrawPossibleMovements() {
		// you can only move up to AP units away from your current position
		int AP = gameManager.characterClass.AP;

		// Clear out the old outlines and delete them
		DestroyOutlines(createdMovementOutlines);

		// calculate which tiles are in range, then draw the outlines on the screen
		List<int[]> tilePosInMovementRange = CalculatePossibleMovements(targetX, targetY, gameManager.characterClass.AP);
		createdMovementOutlines = InstantiateOrientedOutlines(gameManager.tilePossibleMovementOutlinePrefabs, tilePosInMovementRange, 0.011f);
	}

	// Algorithm to check which points lie on a line
	// WIP
	public static List<int[]> BresenhamsAlgorithm(int x1, int y1, int x2, int y2) {
		List<int[]> result = new List<int[]>();
		return result;
	}

	// Calculate which tiles are in attack range
	public List<int[]> CalculateTilesInRange(int centerX, int centerY, int radius) {
		List<int[]> returnValues = new List<int[]>();
		// Iterate over all tiles within range
		// radius = range
		for (int x = centerX - radius; x < centerX + radius; x++) {
			for (int y = centerY - radius; y < centerY + radius; y++) {
				float distance = Mathf.Sqrt(Mathf.Pow((x - centerX), 2) + Mathf.Pow((y - centerY), 2));
				if (distance <= radius) {
					// current tile is within radius
					// now, check if there's line of sight to the player

					// Draw a line to the center, and for every tile that lies on that line,
					// check whether it blocks vision. If it does, then break; and go to the
					// next tile. If none of the tiles on that line break vision, then the current
					// tile is added to the list of tiles in range.

					// WIP

					/*
					List<int[]> temp = BresenhamsAlgorithm(centerX, centerY, x, y);
					foreach(int[] i in temp) {
						if (!map.tileTypes[map.tiles[i[0], i[1]]].blocksVision) {
							break;
                        }
                    }*/

					returnValues.Add(new int[] { x, y });
				}
			}
		}
		return returnValues;
	}

	// Draw tile outlines showing the player how far they can attack or shoot
	public void DrawTilesInRange() {
		// Clear out the old outlines and delete them
		DestroyOutlines(createdRangeOutlines);

		List<int[]> tilePosInAttackRange = CalculateTilesInRange(targetX, targetY, gameManager.characterClass.attackRange);
		createdRangeOutlines = InstantiateOrientedOutlines(gameManager.rangeOutlinePrefabs, tilePosInAttackRange, 0.01f);
	}

	// Hide the tiles that the player is too far away to see
	// WIP
	public void DrawFogOfWar() {
		return;
	}

	// Advances our pathfinding progress by one tile.
	private void AdvancePathing() {
		if(currentPath == null || gameManager.characterClass.AP <= 0 ) {
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

		if (currentPath.Count == 1) {
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

	// Helper function to destroy animated line and tile selection outline
	private void DestroyAnimatedLine() {
		// Destroy old pathfinding visual lines
		for (int i = 0; i < createdLines.Count; i++) {
			Destroy(createdLines[i]);
		}
		createdLines.Clear();
		// Move outline to new end target
		tileOutline.transform.position = new Vector3(-100f, -100f, -100f);
	}

	public void PathToLocation(int x, int y, GameObject tileGameObject) {
		// This method encompasses everything that happens when you click a tile
		DestroyAnimatedLine();

		if (map.UnitCanEnterTile(x, y) == false) {
			return;
		}

		if (tilePlayerIsOn != null) {
			// player is moving from another tile, so reset the old tile
			tilePlayerIsOn.GetComponent<ClickableTile>().currentCharacterOnTile = null;
		}

		// keep track of the tile the player is moving to
		tilePlayerIsOn = tileGameObject;

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

	// Draws outlines based on current control state
	public void DrawOutlines() {
		DrawPossibleMovements();
		DrawTilesInRange();
		DrawFogOfWar();
	}

	// This method will keep moving the character in the path they have chosen
	// until they run out of movement points
	public void TakeMovement() {
		// Make sure to wrap-up any outstanding movement left over.
		while (currentPath != null && gameManager.characterClass.AP - map.CostToEnterTile(currentPath[1].x, currentPath[1].y) >= 0) {
			AdvancePathing();
		}
		DrawOutlines();
	}
}
