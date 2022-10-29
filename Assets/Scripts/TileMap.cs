using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class TileMap : MonoBehaviour {
	// link to game manager
	[HideInInspector] public GameManager gameManager;

	// Public variables
	[Space]
	[Header("Map settings")]
	[Range(10, 100)] public int mapSizeX;
	[Range(10, 100)] public int mapSizeY;
	[Range(0, 99)] public int shoreSize;
	[Range(1, 100)] public int oceanSize;
	[Space(25)]

	[Space]
	[Header("Up to index (numOfRandomlyGeneratedTiles) - 1 will")]
	[Header("be randomly generated. Every tile after that will")]
	[Header("not be included in the random tile generation.")]
	public int numOfRandomlyGeneratedTiles;
	[Space(25)]
	[Header("Frequencies in Tile Types MUST add up to 100!")]
	public TileType[] tileTypes;

	[Space]
	[Header("Prefabs")]

	// List of tile types, represented by integers
	public int[,] tiles;

	// Private variables
	private Node[,] graph;
	
	public void InitiateTileMap() {
		// Initiate the map and pathfinding
		GenerateMapData(); // determine which tiles go where
		GeneratePathfindingGraph(); // pathfinding
		GenerateMapVisual(); // draw map
	}

	// Allocate our map tiles
	public void GenerateMapData() {
		tiles = new int[mapSizeX,mapSizeY];

		// Test if you messed up the frequency attributes in the editor
		// Must add up to 100%.
		int sum = 0;
		for (int x = 0; x < numOfRandomlyGeneratedTiles; x++) {
			sum += tileTypes[x].frequency;
        }
		// Throw error if frequencies don't add up to 100
		Assert.AreEqual(sum, 100, "Tile frequencies do not add up to 100%");

		// Throw error is ocean is bigger than map
		Assert.IsTrue(oceanSize < Mathf.Min(mapSizeY, mapSizeX), "Ocean is bigger than the map");

		for (int x=0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeX; y++) {
				
				if (x < oceanSize || y < oceanSize || y >= mapSizeY - oceanSize || x >= mapSizeX - oceanSize) {
					// Generate ocean
					// Look for which tile is water
					for (int l = 0; l < tileTypes.Length; l++) {
						if (tileTypes[l].name == "TileWater") {
							tiles[x, y] = l;
						}
					}
				} else if (x < oceanSize + shoreSize || y < oceanSize + shoreSize || y >= mapSizeY - shoreSize - oceanSize || x >= mapSizeX - shoreSize - oceanSize) {
					// Generate sand
					// Look for which tile is sand
					for (int l = 0; l < tileTypes.Length; l++) {
						if (tileTypes[l].name == "TileSand") {
							tiles[x, y] = l;
						}
					}
				} else {
					// If current tile is surrounded by unwalkable tiles, then make it mountain (for now)
					if (!tileTypes[tiles[x - 1, y]].isWalkable &&
						!tileTypes[tiles[x + 1, y]].isWalkable &&
						!tileTypes[tiles[x, y - 1]].isWalkable &&
						!tileTypes[tiles[x, y + 1]].isWalkable) {
						tiles[x, y] = 1;
                    } else {
						// Randomly generate number between 1 and 100
						float num = Mathf.Ceil(Random.Range(0, 100));
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
		}
	}

	public int CostToEnterTile(int targetX, int targetY) {
		TileType tt = tileTypes[ tiles[targetX,targetY] ];

		if (UnitCanEnterTile(targetX, targetY) == false) {
			// unit cannot enter tile, so return big unreachable number
			return int.MaxValue;
		}

		int cost = tt.movementCost;

		return cost;
	}

	void GeneratePathfindingGraph() {
		// Initialize the array
		graph = new Node[mapSizeX,mapSizeY];

		// Initialize a Node for each spot in the array
		for(int x = 0; x < mapSizeX; x++) {
			for(int y = 0; y < mapSizeX; y++) {
				graph[x,y] = new Node();
				graph[x,y].x = x;
				graph[x,y].y = y;
			}
		}

		// Now that all the nodes exist, calculate their neighbours
		for(int x = 0; x < mapSizeX; x++) {
			for(int y = 0; y < mapSizeX; y++) {
				// This is the 8-way connection version (allows diagonal movement)
				// Try left
				if(x > 0) {
					graph[x,y].neighbours.Add( graph[x-1, y] );
					if(y > 0)
						graph[x,y].neighbours.Add( graph[x-1, y-1] );
					if(y < mapSizeY-1)
						graph[x,y].neighbours.Add( graph[x-1, y+1] );
				}

				// Try Right
				if(x < mapSizeX-1) {
					graph[x,y].neighbours.Add( graph[x+1, y] );
					if(y > 0)
						graph[x,y].neighbours.Add( graph[x+1, y-1] );
					if(y < mapSizeY-1)
						graph[x,y].neighbours.Add( graph[x+1, y+1] );
				}

				// Try straight up and down
				if (y > 0) {
					graph[x, y].neighbours.Add(graph[x, y - 1]);
				}
				if (y < mapSizeY - 1) {
					graph[x, y].neighbours.Add(graph[x, y + 1]);
				}
				// This also works with 6-way hexes and n-way variable areas (like EU4)
			}
		}
	}

	// Instantiate the tiles
	void GenerateMapVisual() {
		for(int x=0; x < mapSizeX; x++) {
			for(int y=0; y < mapSizeX; y++) {
				TileType tt = tileTypes[ tiles[x,y] ];
				GameObject currentTile = Instantiate(tileTypes[tiles[x,y]].tilePrefab, new Vector3(x, 0f, y), Quaternion.identity);

				ClickableTile ct = currentTile.AddComponent<ClickableTile>();
				ct.map = this;
				ct.gameManager = gameManager;
				ct.x = x;
				ct.y = y;
			}
		}
	}

	// Convert 2D tile (x, y) grid to 3D (x, 0, z) world coords
	public Vector3 TileCoordToWorldCoord(int x, int y) {
		// 0.5 unit offset so that the unit pathfinds to the middle of the square
		return new Vector3(x, 0f, y);
	}

	public bool UnitCanEnterTile(int x, int y) {
		// check if tiletype is walkable
		return tileTypes[ tiles[x,y] ].isWalkable;
	}

	// Base function to generate path from [sourceX, sourceY] to [destX, destY]
	public List<Node> GeneratePath (int sourceX, int sourceY, int destX, int destY) {
		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();

		Node source = graph[
							sourceX,
							sourceY
							];

		Node target = graph[
							destX,
							destY
							];

		dist[source] = 0;
		prev[source] = null;

		// Initialize everything to have infinity distance
		foreach (Node v in graph) {
			if (v != source) {
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add(v);
		}

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

	// Passthrough helper function
	public void GeneratePathTo(int sourceX, int sourceY, int destX, int destY) {
		// Clear out our unit's old path.
		gameManager.selectedUnit.GetComponent<UnitPathfinding>().currentPath = null;
		// Update current path
		gameManager.selectedUnit.GetComponent<UnitPathfinding>().currentPath = GeneratePath(sourceX, sourceY, destX, destY);
	}
}
