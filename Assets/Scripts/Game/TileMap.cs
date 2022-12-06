// Desgined and created by Andrew Simon and Tyler R. Renaud
// All rights belong to creator

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class TileMap : MonoBehaviour {
	// link to game manager
	[HideInInspector] public GameManager gameManager;

	// Map generation variables
	[Space]
	[Header("Map settings")]
	public int shoreSize;
	public float shoreVariation;
	[Range(5, 20)] public float biomeSize;
	[Range(15, 100)] public int oceanSize;
	[Space()]

	[Space]
	[Header("Up to index (numOfRandomlyGeneratedTiles) - 1 will")]
	[Header("be randomly generated. Every tile after that will")]
	[Header("not be included in the random tile generation.")]
	public int numOfRandomlyGeneratedTiles;
	[Space()]
	[Header("Frequencies in Tile Types MUST add up to 100!")]
	public TileType[] tileTypes;

	[Space]
	[Header("Prefabs")]

	// List of tile types, represented by integers
	public int[,] tiles;
	public int[,] originalTiles; // before mutation

	// List of the tile's game objects
	public GameObject[,] tilesObjects;
	public GameObject[,] roadObjects;
	public GameObject[,] shopTilesObjects;

	// graph of map for pathfinding
	[HideInInspector] public Node[,] fullMapGraph;

	// all tiles currently viewable to player
	// for fog of war
	[HideInInspector] public List<int[]> viewableTiles;

	// initiate assethandler reference
	private AssetHandler assetHandler;

    public void InitiateTileMap() {
		assetHandler = GameObject.Find("AssetHandler").GetComponent<AssetHandler>();

		// Initiate the map and pathfinding
		GenerateMap(); // generate map
		fullMapGraph = GeneratePathfindingGraph(gameManager.mapSize); // pathfinding
	}

	// Allocate our map tiles
	// I combined GenerateMapVisual() into this function becuz it didnt need to be seperate
	public void GenerateMap() {
		// quick reference
		int mapSize = gameManager.mapSize;

		// for later
		int ranY;
		int ranX;

		tiles = new int[mapSize, mapSize]; // all of the tile types
		originalTiles = new int[mapSize, mapSize];
		tilesObjects = new GameObject[mapSize, mapSize]; // all gameobjects of tiles
		viewableTiles = new List<int[]>(); // instantiate viewable tiles for fog of war
		roadObjects = new GameObject[mapSize, mapSize];
		shopTilesObjects = new GameObject[mapSize, mapSize];

		// set tile frequencies
		switch (gameManager.biomeSetting) {
			case BiomeSetting.Default:
				// swamp tile
				tileTypes[0].frequency = 15;
				// grass tile
				tileTypes[1].frequency = 30;
				// hill tile
				tileTypes[2].frequency = 25;
				// mountain tile
				tileTypes[3].frequency = 30;
				break;
			case BiomeSetting.Hilly:
				// swamp tile
				tileTypes[0].frequency = 10;
				// grass tile
				tileTypes[1].frequency = 20;
				// hill tile
				tileTypes[2].frequency = 35;
				// mountain tile
				tileTypes[3].frequency = 35;
				break;
			case BiomeSetting.Superflat:
				// swamp tile
				tileTypes[0].frequency = 50;
				// grass tile
				tileTypes[1].frequency = 50;
				// hill tile
				tileTypes[2].frequency = 0;
				// mountain tile
				tileTypes[3].frequency = 0;
				break;
			case BiomeSetting.Mountaineous:
				// swamp tile
				tileTypes[0].frequency = 10;
				// grass tile
				tileTypes[1].frequency = 15;
				// hill tile
				tileTypes[2].frequency = 30;
				// mountain tile
				tileTypes[3].frequency = 45;
				break;
			case BiomeSetting.Swampland:
				// swamp tile
				tileTypes[0].frequency = 45;
				// grass tile
				tileTypes[1].frequency = 25;
				// hill tile
				tileTypes[2].frequency = 15;
				// mountain tile
				tileTypes[3].frequency = 15;
				break;
		}

		// Test if frequencies attributes are messed up. If they are, generation will break
		// Must add up to 100%.
		int sum = 0;
		for (int x = 0; x < numOfRandomlyGeneratedTiles; x++) {
			sum += tileTypes[x].frequency;
		}
		// Throw error if frequencies don't add up to 100
		Assert.AreEqual(sum, 100, "Tile frequencies do not add up to 100%");

		// Throw error if ocean is bigger than map
		Assert.IsTrue(oceanSize < Mathf.Min(mapSize, mapSize), "Ocean is bigger than the map");

		// Look for which tile is sand and water because they can be accidentally moved around in editor
		int sandType = -1;
		int waterType = -1;
		int mountainType = -1;
		for (int i = 0; i < tileTypes.Length; i++) {
			if (tileTypes[i].name == "TileSand") {
				sandType = i;
			} else if (tileTypes[i].name == "TileWater") {
				waterType = i;
			} else if (tileTypes[i].name == "TileMountain") {
				mountainType = i;
			}
		}

		// Since perlin noise will generate the same values every time, we have to change the
		// part of the perlin noise map that we use each time. In order to do this we offset
		// the x and y values by different (random) amounts each time we generate our tilemap
		float mapSeed = Random.Range(1, 100);

		// Inverse biome size variable so that it makes more sense
		biomeSize = 25 - biomeSize;

		// iterate over every tile
		for (int x = 0; x < mapSize; x++) {
			for (int y = 0; y < mapSize; y++) {
				// formula for circular island:
				// (x - radius - oceanSize)^2 + (y - radius - oceanSize)^2 = r^2
				int radius = (mapSize / 2) - oceanSize;
				int center = radius + oceanSize; // circle inside square map, so center is at (center, center)
				float distance = Mathf.Sqrt(Mathf.Pow((x - center), 2) + Mathf.Pow((y - center), 2));
				distance = Random.Range(distance - shoreVariation, distance + shoreVariation); // spice it up a little

				if (distance > radius) {
					// Generate ocean tile

					// Look for which tile is water
					for (int l = 0; l < tileTypes.Length; l++) {

						if (tileTypes[l].name == "TileWater") {
							tiles[x, y] = l;
						}
					}
				} else if (distance > radius - shoreSize) {
					// Generate sand tile
					tiles[x, y] = sandType;
				} else {
					// Generate remaining tiles

					// Generate number using perlin noise
					float num = 100 * Mathf.PerlinNoise(mapSeed + ((float)biomeSize * x / mapSize), mapSeed + ((float)biomeSize * y / mapSize));

					// Initialize end points
					int[] endPoints = new int[numOfRandomlyGeneratedTiles];
					endPoints[0] = tileTypes[0].frequency;
					for (int m = 1; m < numOfRandomlyGeneratedTiles; m++) {
						endPoints[m] = endPoints[m - 1] + tileTypes[m].frequency;
					}

					// Below or equal to the first endpoint
					if (num <= endPoints[0]) {
						tiles[x, y] = 0;
					}

					// Above the first end point
					for (int z = 1; z < numOfRandomlyGeneratedTiles; z++) {
						if (num >= endPoints[z - 1] && num <= endPoints[z]) {
							tiles[x, y] = z;
						}
					}
				}
			}
		}

		// do some corrections
		// iterate over every tile
		for (int x = 0; x < mapSize; x++) {
			for (int y = 0; y < mapSize; y++) {
				// now we do some corrections
				// make sure we're not on a water tile
				if (tiles[x, y] != waterType) {
					// If current tile is surrounded by unwalkable tiles, then make it mountain
					if (!tileTypes[tiles[x - 1, y]].isWalkable &&
					!tileTypes[tiles[x + 1, y]].isWalkable &&
					!tileTypes[tiles[x, y - 1]].isWalkable &&
					!tileTypes[tiles[x, y + 1]].isWalkable) {
						tiles[x, y] = mountainType;
					}

					// If current tile touches a water tile, make it sand
					if ((tiles[x - 1, y] == waterType) ||
							   (tiles[x + 1, y] == waterType) ||
							   (tiles[x, y - 1] == waterType) ||
							   (tiles[x, y + 1] == waterType)) {

						tiles[x, y] = sandType;
					}
				}
			}
		}

		// Generate map visuals
		// iterate over every tile
		for (int x = 0; x < mapSize; x++) {
			for (int y = 0; y < mapSize; y++) {
				TileType tt = tileTypes[tiles[x, y]];
				GameObject currentTile = Instantiate(tileTypes[tiles[x, y]].tilePrefab,
													 new Vector3(x, 0f, y),
													 Quaternion.identity,
													 GameObject.Find("TileContainer").transform);
				tilesObjects[x, y] = currentTile;
				currentTile.name = tt.name + " (" + x + ", " + y + ")";

				// Randomly rotate tile a little so there's not as much reptition
				if (currentTile.name != "water_tile") {
					// but don't rotate water because that looks weird
					int phase = Mathf.RoundToInt(Random.Range(1, 3));
					currentTile.transform.Rotate(0f, 90f * phase, 0f, Space.World);
				}

				// add script to tile
				ClickableTile ct = currentTile.AddComponent<ClickableTile>();
				ct.map = this;
				ct.gameManager = gameManager;
				ct.x = x;
				ct.y = y;

				ChangeTileToFog(x, y);
			}
		}

		// copy tiles into originalTiles for storage
		for (int x = 0; x < mapSize; x++) {
			for (int y = 0; y < mapSize; y++) {
				originalTiles[x, y] = tiles[x, y];
			}
		}

		// generate roads
		for (int i = 0; i < Mathf.RoundToInt(mapSize / 15); i++) {
			// pick random point on the map that is walkable
			ranY = 0;
			ranX = 0;
			while (!tileTypes[tiles[ranY, ranX]].isWalkable || tiles[ranY, ranX] == 6) {
				ranY = Random.Range(oceanSize + 4, mapSize - oceanSize - 4);
				ranX = Random.Range(oceanSize + 4, mapSize - oceanSize - 4);
			}

			for (int x = ranX; x < mapSize - oceanSize - Random.Range(2, 8); x++) {
				if (tileTypes[tiles[x, ranY]].isWalkable) {
					// tile is walkable, so set it to road
					tiles[x, ranY] = 6; // 6 is road
					if (!tileTypes[tiles[x + Random.Range(2, 5), ranY]].isWalkable) {
						// mountain in the way, end road
						break;
                    }
				}
			}
		}

		// generate road visuals
		for (int x = 0; x < mapSize; x++) {
			for (int y = 0; y < mapSize; y++) {
				// check every tile if its road
				if (tiles[x, y] == 6) {
					// instantiate road outline a little bit overtop the current tile,
					// so that you can see the old tile underneath a lil bit
					GameObject currentTile = Instantiate(assetHandler.straightRoadPrefab,
														 new Vector3(x, 0.009f, y),
														 Quaternion.identity,
														 GameObject.Find("TileContainer").transform);
					currentTile.name = "Road (" + x + ", " + y + ")";
					currentTile.transform.Rotate(0f, 90f, 0f, Space.World);
					if (tilesObjects[x, y].transform.childCount > 0) {
						foreach (Transform child in tilesObjects[x, y].transform) {
							Destroy(child.gameObject);
						}
					}
					roadObjects[x, y] = currentTile;

					// make the road initially foggy
					ChangeTileToFog(x, y);

				} else {
					roadObjects[x, y] = null;
				}
			}
		}

		// generate shop tiles
		// Mathf.RoundToInt(mapSize / 50)
		for (int i = 0; i < 50; i++) {
			ranX = 0;
			ranY = 0;
			while (!tileTypes[tiles[ranY, ranX]].isWalkable || 
				   (tiles[ranY, ranX] == 6) || 
				   (tiles[ranY, ranX] == 7)) {
				// generate random point on the land; not on the sand
				ranY = Random.Range(oceanSize + shoreSize, mapSize - oceanSize - shoreSize);
				ranX = Random.Range(oceanSize + shoreSize, mapSize - oceanSize - shoreSize);
			}

			// set to shop tile
			tiles[ranX, ranY] = 7;

			GameObject currentTile = Instantiate(tileTypes[7].tilePrefab,
												 new Vector3(ranX, 0.00925f, ranY),
											     Quaternion.identity,
												 GameObject.Find("TileContainer").transform);
			currentTile.name = "Shop (" + ranX + ", " + ranY + ")";
			shopTilesObjects[ranX, ranY] = currentTile;

			// remove all children of original tile
			if (tilesObjects[ranX, ranY].transform.childCount > 0) {
				foreach (Transform child in tilesObjects[ranX, ranY].transform) {
					Destroy(child.gameObject);
				}
			}

			// make tile initially foggy
			ChangeTileToFog(ranX, ranY);
		}
	}

	// check if a given tile at (x, y) is viewable to the player
	public bool IsTileVisibleToPlayer(int x, int y) {
		// check each viewable tile, and if its the one given return true
		for (int i = 0; i < viewableTiles.Count; i++) {
			if (viewableTiles[i][0] == x && viewableTiles[i][1] == y) {
				return true;
			}
		}
		return false;
	}

	public void ChangeTileToFog(int x, int y) {
		// change material to fog
		// update overlay tiles
		if (tiles[x,y] == 6) {
			// road
			roadObjects[x, y].GetComponent<MeshRenderer>().sharedMaterial = assetHandler.fogOfWarOutlineMaterial;
		} else if (tiles[x,y] == 7) {
			// shop tile
			shopTilesObjects[x,y].GetComponent<MeshRenderer>().sharedMaterial = assetHandler.fogOfWarOutlineMaterial;
		}
		// update tile underneath
		tilesObjects[x, y].GetComponent<MeshRenderer>().sharedMaterial = assetHandler.fogOfWarOutlineMaterial;

		// disable all children
		foreach (Transform child in tilesObjects[x, y].transform) {
			child.gameObject.SetActive(false);
		}
	}

	public void RevertTileToDefault(int x, int y) {
		// revert material
		// update overlay tiles
		if (tiles[x, y] == 6) {
			// road
			roadObjects[x, y].GetComponent<MeshRenderer>().sharedMaterial = assetHandler.straightRoadPrefab.GetComponent<MeshRenderer>().sharedMaterial;
		} else if (tiles[x, y] == 7) {
			// shop tile
			shopTilesObjects[x, y].GetComponent<MeshRenderer>().sharedMaterial = tileTypes[7].tilePrefab.GetComponent<MeshRenderer>().sharedMaterial;
		}
		// update tile underneath
		tilesObjects[x, y].GetComponent<MeshRenderer>().sharedMaterial = tileTypes[originalTiles[x, y]].tilePrefab.GetComponent<MeshRenderer>().sharedMaterial;

		// enable all children
		foreach (Transform child in tilesObjects[x, y].transform) {
			child.gameObject.SetActive(true);
		}
	}

	public bool UnitCanEnterTile(int x, int y) {
		// check if tiletype is walkable
		if (tileTypes[tiles[x, y]].isWalkable && 
			tilesObjects[x, y].GetComponent<ClickableTile>().currentCharacterOnTile == null) {
			return true;
        } else {
			return false;
        }
	}

	// calculate distance between two points on the grid
	public static float DistanceBetweenTiles(int x, int y, int x2, int y2) {
		float c1 = Mathf.Pow(x2 - x, 2);
		float c2 = Mathf.Pow(y2 - y, 2);
		float c = Mathf.Sqrt(c1 + c2);
		return c;
    }

	public int CostToEnterTile(int x, int y) {
		if (UnitCanEnterTile(x, y) == false) {
			// unit cannot enter tile, so return big number
			return gameManager.mapSize;
		} else {
			return tileTypes[tiles[x, y]].movementCost;
		}
	}

	// Generates a graph of size rangeX by rangeY, starting at (offsetX, offsetY)
	public static Node[,] GeneratePathfindingGraph(int range) {
		// Initialize the array
		Node[,] graph = new Node[range, range];

		// Initialize a Node for each spot in the array
		for(int x = 0; x < range; x++) {
			for(int y = 0; y < range; y++) {
				graph[x,y] = new Node();
				graph[x,y].x = x;
				graph[x,y].y = y;
			}
		}

		// Now that all the nodes exist, calculate their neighbours
		for(int x = 0; x < range; x++) {
			for(int y = 0; y < range; y++) {
				// This is the 8-way connection version (allows diagonal movement)
				// Try left
				if(x > 0) {
					graph[x,y].neighbours.Add( graph[x-1, y] );
					if(y > 0)
						graph[x,y].neighbours.Add( graph[x-1, y-1] );
					if(y < range-1)
						graph[x,y].neighbours.Add( graph[x-1, y+1] );
				}

				// Try Right
				if(x < range-1) {
					graph[x,y].neighbours.Add( graph[x+1, y] );
					if(y > 0)
						graph[x,y].neighbours.Add( graph[x+1, y-1] );
					if(y < range-1)
						graph[x,y].neighbours.Add( graph[x+1, y+1] );
				}

				// Try straight up and down
				if (y > 0) {
					graph[x, y].neighbours.Add( graph[x, y - 1]);
				}
				if (y < range - 1) {
					graph[x, y].neighbours.Add( graph[x, y + 1]);
				}
				// This also works with 6-way hexes and n-way variable areas (like EU4)
			}
		} 
		return graph;
	}

	// Convert 2D tile (x, y) grid to 3D (x, 0, z) world coords
	public static Vector3 TileCoordToWorldCoord(int x, int y, float yOffset) {
		// 0.5 unit offset so that the unit pathfinds to the middle of the square
		return new Vector3(x, yOffset, y);
	}

	// Optimized for drawing possible movements
	public List<Node> BreadthFirstSearch(Node start, Node end, int maxDistance) {
		List<Node> result = new List<Node>();
		List<Node> visited = new List<Node>();
		Queue<Node> work = new Queue<Node>();

		start.history = new List<Node>();
		visited.Add(start);
		work.Enqueue(start);

		while (work.Count > 0) {
			Node current = work.Dequeue();
			if (current == end) {
				// Found the final node
				result = current.history;
				result.Add(current);
				for (int i = 0; i < visited.Count; i++) {
					visited[i].distance = 0;
				}
				return result;
			} else {
				// Not the final node
				// Go over each neighbour and check the distance to that neighbour.
				// If we can move to that neighbour, then add that neighbour to the list of
				// work tiles that we must still visit to check.
				foreach (Node currentNeighbor in current.neighbours) {
					// calculate distance to next node
					int dist = current.distance + CostToEnterTile(currentNeighbor.x, currentNeighbor.y);

					// Check if neighbour is walkable, in range, hasn't been visited yet and is viewable
					if (dist <= maxDistance &&
						!visited.Contains(currentNeighbor) &&
						tileTypes[tiles[currentNeighbor.x, currentNeighbor.y]].isWalkable &&
						IsTileVisibleToPlayer(currentNeighbor.x, currentNeighbor.y)) {
						// Neighbour is in range, walkable, hasn't been visited yet and is viewable. Yay!
						currentNeighbor.distance = dist;
						currentNeighbor.history = new List<Node>(current.history);
						currentNeighbor.history.Add(current);
						visited.Add(currentNeighbor);
						work.Enqueue(currentNeighbor);
					}
				}
			}
		}
		//Route not found, loop ends
		for (int i = 0; i < visited.Count; i++) {
			visited[i].distance = 0;
		}
		return null;
	}

	// Base function to generate path from [sourceX, sourceY] to [destX, destY]
	public List<Node> DijkstraPath (int sourceX, int sourceY, int destX, int destY, Node[,] graph) {
		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the queue - the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();

		// Find the source and target (destination) in the graph
		Node source = graph[sourceX, sourceY];
		Node target = graph[destX, destY];

		dist[source] = 0;
		prev[source] = null;

		// Populate the "unvisited" list of nodes, and initialize the distances for each node
		foreach (Node v in graph) {
			if (v != source) {
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}
			unvisited.Add(v);
		}

		// While unvisited is not empty
		while (unvisited.Count > 0) {
			// "u" is going to be the unvisited node with the smallest distance.
			Node u = null;

			foreach (Node possibleU in unvisited) {
				if (u == null || dist[possibleU] < dist[u]) {
					u = possibleU;
				}
			}

			if (u == target) {
				break;  // Exit the while loop!
			}

			unvisited.Remove(u);

			foreach (Node v in u.neighbours) {
				//float alt = dist[u] + u.DistanceTo(v);
				float alt = dist[u] + CostToEnterTile(v.x, v.y);
				if (alt < dist[v]) {
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		// If we get there, then either we found the shortest route
		// to our target, or there is no route at ALL to our target.

		if (prev[target] == null) {
			// No route between our target and the source
			return null;
		}

		List<Node> currentPath = new List<Node>();

		Node curr = target;

		// Step through the "prev" chain and add it to our path
		while (curr != null) {
			currentPath.Add(curr);
			curr = prev[curr];
		}

		// Right now, currentPath describes a route from our target to our source
		// So we need to invert it!
		currentPath.Reverse();

		return currentPath;
	}

	// Returns all tiles you can move to with AP action points as a list
	public List<int[]> CalculatePossibleMovements(int centerX, int centerY, int AP) {
		List<int[]> returnValues = new List<int[]>();
		// iterate over each tile in range of the player
		for (int x = centerX - AP - 1; x <= centerX + AP + 1; x++) {
			for (int y = centerY - AP - 1; y <= centerY + AP + 1; y++) {
				// check if there's a path to current tile from the player's current position
				// and if there's no character on the tile already
				List<Node> shortestPath = BreadthFirstSearch(fullMapGraph[centerX, centerY], fullMapGraph[x, y], AP);

				if (shortestPath != null && 
					(tilesObjects[x, y].GetComponent<ClickableTile>().currentCharacterOnTile == null ||
					tilesObjects[x, y].GetComponent<ClickableTile>().currentCharacterOnTile == gameManager.selectedUnit)) {
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

	// return all the grid squares that lie on a line
	public static List<int[]> BresenhamsAlgorithm(int x, int y, int x2, int y2) {
		List<int[]> result = new List<int[]>();
		int w = x2 - x;
		int h = y2 - y;
		int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
		if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
		if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
		if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
		int longest = Mathf.Abs(w);
		int shortest = Mathf.Abs(h);
		if (!(longest > shortest)) {
			longest = Mathf.Abs(h);
			shortest = Mathf.Abs(w);
			if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
			dx2 = 0;
		}
		int numerator = longest >> 1;
		for (int i = 0; i <= longest; i++) {
			int[] temp = { x, y };
			result.Add(temp);
			numerator += shortest;
			if (!(numerator < longest)) {
				numerator -= longest;
				x += dx1;
				y += dy1;
			} else {
				x += dx2;
				y += dy2;
			}
		}
		return result;
	}

	// Calculate which tiles are in attack range
	// Uses bresenham's line of sight algorithm to exlude results that are behind tiles which block vision
	public List<int[]> CalculateTilesInRange(int centerX, int centerY, int radius, bool includeFrontier) {
		List<int[]> returnValues = new List<int[]>();
		// Iterate over all tiles within range
		// radius = range
		for (int x = centerX - radius; x < centerX + radius; x++) {
			for (int y = centerY - radius; y < centerY + radius; y++) {
				float distance = Mathf.Sqrt(Mathf.Pow((x - centerX), 2) + Mathf.Pow((y - centerY), 2)) + 0.5f;
				if (distance <= radius) {
					// current tile is within radius
					// now, check if there's line of sight to the player

					// Draw a line to the center, and for every tile that lies on that line,
					// check whether it blocks vision.
					bool flag = false;

					List<int[]> result = BresenhamsAlgorithm(centerX, centerY, x, y);

					// iterate over each tile that lies on the line
					for (int i = 0; i < result.Count; i++) {

						int type = tiles[result[i][0], result[i][1]];
						// check if we're including the tile that is blocking the vision
						if (includeFrontier) {
							if (tileTypes[type].blocksVision && i != (result.Count - 1)) {
								// current tile blocks vision, therefore dont add (x, y) to list of viewable tiles
								flag = true;
							}
						} else {
							if (tileTypes[type].blocksVision) {
								// current tile blocks vision, therefore dont add (x, y) to list of viewable tiles
								flag = true;
							}
						}
					}
					if (!flag) {
						returnValues.Add(new int[] { x, y });
					}
				}
			}
		}
		return returnValues;
	}

	public bool IsTileInAttackRange(int x, int y, int x2, int y2, int range) {
		// calculate all tiles in range
		List<int[]> tilesInRange = CalculateTilesInRange(x2, y2, range, false);

		// check each tile to see if the enemy is in range
		for (int i = 0; i < tilesInRange.Count; i++) {
			if (tilesInRange[i][0] == x && tilesInRange[i][1] == y) {
				// given tile is in attack range
				return true;
			}
        }
		// given tile not found
		return false;
	}

	// Instantiate tile outlines that need to be oriented correctly
	// yOffset lets you put some outlines overtop others (precedence)
	// there's alot of if statements in here consider yourself warned
	public static List<GameObject> InstantiateOrientedOutlines(GameObject[] outlinePrefabs, List<int[]> tilesInRange, float yOffset) {
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
				GameObject tempObj = Instantiate(prefab, pos, Quaternion.identity, GameObject.Find("TileContainer").transform);
				// rotate outline
				tempObj.transform.Rotate(0f, yRotation, 0f, Space.World);
				// add outline to list of all created outlines
				createdOutlines.Add(tempObj);
			}
		}
		return createdOutlines;
	}

	// destroy all outlines in given list
	public static void DestroyOutlines(List<GameObject> outlines) {
		for (int i = 0; i < outlines.Count; i++) {
			Destroy(outlines[i]);
		}
		outlines.Clear();
	}

	// Passthrough helper function
	public List<Node> GeneratePathTo(int sourceX, int sourceY, int destX, int destY) {
		return DijkstraPath(sourceX, sourceY, destX, destY, fullMapGraph);
	}
}
