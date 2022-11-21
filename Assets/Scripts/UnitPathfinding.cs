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
	

	// Our pathfinding info.  Null if we have no destination ordered.
	public List<Node> currentPath = null;

	private GameObject tilePlayerIsOn; // game object of player's current tile

	// reference to asset handler
	private AssetHandler assetHandler;

	private void FixedUpdate() {
		// Smoothly animate towards the correct map tile.
		if (currentPath != null) {
			int currNode = 0;

			// Initialize (x, y) values of all nodes in currentPath using linked list
			while (currNode < currentPath.Count - 1) {
				Vector3 start = TileMap.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y, 0.125f) + new Vector3(0, 0, 0);
				Vector3 end = TileMap.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y, 0.125f) + new Vector3(0, 0, 0);

				currNode++;
			}
		}

		transform.position = Vector3.Lerp(transform.position, TileMap.TileCoordToWorldCoord(targetX, targetY, 0.125f), 10f * Time.fixedDeltaTime);
	}

	public void SpawnPlayer(int x, int y) {
		// instantiate reference to asset handler
		assetHandler = GameObject.Find("AssetHandler").GetComponent<AssetHandler>();
		// Instantiate tile outlines
		// There's only two of these so we can just move them around the scene
		tileOutline = Instantiate(assetHandler.tileOutlinePrefab, new Vector3(-100f, -100f, -100f), Quaternion.identity);
		tileHoverOutline = Instantiate(assetHandler.tileHoverOutlinePrefab, new Vector3(-100f, -100f, -100f), Quaternion.identity);

		targetX = x;
		targetY = y;
		transform.position = TileMap.TileCoordToWorldCoord(targetX, targetY, 0.125f);

		// create the lists
		createdMovementOutlines = new List<GameObject>();
		createdRangeOutlines = new List<GameObject>();
		createdLines = new List<GameObject>();
		

		// set reference to unit in the tile's script
		map.tilesObjects[x, y].GetComponent<ClickableTile>().currentCharacterOnTile = gameObject;

		// Draw possible movements and fog of war right when you spawn
		DrawPossibleMovements();
		DrawFogOfWar();
	}


	// Outline the tiles that the player has enough AP to move to
	// This function is a little messy at the bottom with all the if statements
	// and hard coded values, but whatever IDK how to optimize it more than this
	public void DrawPossibleMovements() {
		// you can only move up to AP units away from your current position
		int AP = gameManager.characterClass.AP;

		// Clear out the old outlines and delete them
		TileMap.DestroyOutlines(createdMovementOutlines);

		// calculate which tiles are in range, then draw the outlines on the screen
		List<int[]> tilePosInMovementRange = map.CalculatePossibleMovements(targetX, targetY, gameManager.characterClass.AP);
		createdMovementOutlines = TileMap.InstantiateOrientedOutlines(assetHandler.tilePossibleMovementOutlinePrefabs, tilePosInMovementRange, 0.01f);
	}


	// Draw tile outlines showing the player how far they can attack or shoot
	public void DrawTilesInRange() {
		// Clear out the old outlines and delete them
		TileMap.DestroyOutlines(createdRangeOutlines);

		List<int[]> tilePosInAttackRange = map.CalculateTilesInRange(targetX, targetY, gameManager.characterClass.attackRange, false);
		createdRangeOutlines = TileMap.InstantiateOrientedOutlines(assetHandler.rangeOutlinePrefabs, tilePosInAttackRange, 0.011f);
	}

	// Hide the tiles that the player is too far away to see or are behind a tile
	// that blocks vision
	public void DrawFogOfWar() {
		// reset tiles previously in sight to fog and clear the list
		foreach (int[] i in map.viewableTiles) {
			map.ChangeTileToFog(i[0], i[1]);
        }
		map.viewableTiles.Clear();

		// calculate tiles with line of sight and within view range
		map.viewableTiles = map.CalculateTilesInRange(targetX, targetY, gameManager.characterClass.viewRange, true);

		foreach (int[] tile in map.viewableTiles) {
			map.RevertTileToDefault(tile[0], tile[1]);
		}
	}

	// Advances our pathfinding progress by one tile.
	private void AdvancePathing() {
		if(currentPath == null || gameManager.characterClass.AP <= 0 ) {
			return;
		}

		// Teleport us to our correct "current" position, in case we
		// haven't finished the animation yet.
		transform.position = TileMap.TileCoordToWorldCoord(targetX, targetY, 0.125f);

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
	private GameObject CreateVisualPathfindingLine(Vector3 start, Vector3 end, float delay, bool last) {
		// Create a line and its particle system effects
		GameObject line = Instantiate(assetHandler.linePrefab, start, Quaternion.identity);

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
		if (!map.UnitCanEnterTile(x, y)) {
			return;
		}

		// This method encompasses everything that happens when you click a tile
		TileMap.DestroyOutlines(createdLines);
		tileOutline.transform.position = new Vector3(-100f, -100f, -100f);

		if (tilePlayerIsOn != null) {
			// player is moving from another tile, so reset the old tile
			tilePlayerIsOn.GetComponent<ClickableTile>().currentCharacterOnTile = null;
		}

		// keep track of the tile the player is moving to
		tilePlayerIsOn = tileGameObject;

		// Move outline to new end target
		tileOutline.transform.position = TileMap.TileCoordToWorldCoord(x, y, 0f) - new Vector3(0, -0.01f, 0);

		// [targetX, targetY] is the current position of the unit
		// [x, y] is the target position, which was just clicked on
		map.GeneratePathTo(targetX, targetY, x, y); // generate path

		// Draw animated line to end destination
		float startDelayCount = 0;
		if (currentPath != null) {
			for (int i = 0; i < currentPath.Count - 1; i++) {
				Vector3 start = new Vector3(currentPath[i].x, 0.05f, currentPath[i].y);
				Vector3 end = new Vector3(currentPath[i + 1].x, 0.05f, currentPath[i + 1].y);
				bool last = false;
				if (i == currentPath.Count - 2) {
					last = true;
				}
				createdLines.Add(CreateVisualPathfindingLine(start, end, startDelayCount, last));
				startDelayCount += 0.25f;
			}
		}

		TakeMovement();
	}

	// This method will keep moving the character in the path they have chosen
	// until they run out of movement points
	public void TakeMovement() {
		// Make sure to wrap-up any outstanding movement left over.
		while (currentPath != null && gameManager.characterClass.AP - map.CostToEnterTile(currentPath[1].x, currentPath[1].y) >= 0) {
			AdvancePathing();
			DrawFogOfWar();
		}

		// Update outlines
		DrawPossibleMovements();

		// If you're moving along your path with an item selected, the tiles in range will update
		// each time you move
		if (gameManager.cs == ControlState.Item1 || gameManager.cs == ControlState.Item2) {
			DrawTilesInRange();
        }

		// update fog of war every time you move
		DrawFogOfWar();

		// update enemy visibilty every time you move
		gameManager.enemySpawnManager.UpdateEnemiesVisibility();
	}
}
