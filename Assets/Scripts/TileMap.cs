using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class TileMap : MonoBehaviour {
	// Public variables
	// Change these in the editor
	[Space]
	[Header("Map settings")]
	[Header("These will only be used at runtime")]
	[Range(10, 100)] public int mapSizeX;
	[Range(10, 100)] public int mapSizeY;
	[Range(0, 99)] public int shoreSize;
	[Range(1, 100)] public int oceanSize;
	[Space(25)]

	[Space]
	[Header("Misc")]
	// Tile outlines
	public GameObject tileOutlinePrefab;
	public GameObject tileHoverOutlinePrefab;
	[HideInInspector] public GameObject tileOutline;
	[HideInInspector] public GameObject tileHoverOutline;

	public GameObject linePrefab;
	public GameObject selectedUnit;
	[Space(25)]

	[Space]
	[Header("Up to index (numOfRandomlyGeneratedTiles) - 1 will")]
	[Header("be randomly generated. Every tile after that will")]
	[Header("not be included in the random tile generation.")]
	public int numOfRandomlyGeneratedTiles;
	[Space(25)]
	[Header("Frequencies in Tile Types MUST add up to 100!")]
	public TileType[] tileTypes;

	// Private variables
	private int[,] tiles;
	private Node[,] graph;
	
	// list containing all created animated pathfinding lines
	public List<GameObject> createdLines;

	void Start() {
		// Setup the selectedUnit's variable
		selectedUnit.GetComponent<UnitPathfinding>().targetX = (int)selectedUnit.transform.position.x;
		selectedUnit.GetComponent<UnitPathfinding>().targetY = (int)selectedUnit.transform.position.z;
		selectedUnit.GetComponent<UnitPathfinding>().map = this;

		// Instantiate tile outlines
		// There's only two of these so we can just move it around the scene
		tileOutline = (GameObject)Instantiate(tileOutlinePrefab, new Vector3(-100f, -100f, -100f), Quaternion.identity);
		tileHoverOutline = (GameObject)Instantiate(tileHoverOutlinePrefab, new Vector3(-100f, -100f, -100f), Quaternion.identity);

		// Initialize list
		createdLines = new List<GameObject>();

		GenerateMapData();
		GeneratePathfindingGraph();
		GenerateMapVisual();
	}

	// Allocate our map tiles
	void GenerateMapData() {
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

	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY) {
		TileType tt = tileTypes[ tiles[targetX,targetY] ];

		if(UnitCanEnterTile(targetX, targetY) == false) {
			return Mathf.Infinity;
		}
			
		float cost = tt.movementCost;

		if( sourceX!=targetX && sourceY!=targetY) {
			// We are moving diagonally!  Fudge the cost for tie-breaking
			// Purely a cosmetic thing!
			cost += 0.001f;
		}
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
				GameObject currentTile = (GameObject)Instantiate(tileTypes[tiles[x,y]].tilePrefab, new Vector3(x, 0f, y), Quaternion.identity);

				ClickableTile ct = currentTile.GetComponent<ClickableTile>();
				ct.x = x;
				ct.y = y;
				ct.map = this;
			}
		}
	}

	// Convert 2D tile (x, y) grid to 3D (x, 0, z) world coords
	public Vector3 TileCoordToWorldCoord(int x, int y) {
		// 0.5 unit offset so that the unit pathfinds to the middle of the square
		return new Vector3(x, 0f, y + 0.5f);
	}

	public bool UnitCanEnterTile(int x, int y) {

		// We could test the unit's walk/hover/fly type against various
		// terrain flags here to see if they are allowed to enter the tile.

		return tileTypes[ tiles[x,y] ].isWalkable;
	}

	// Helper function to create animated line to end target
	private GameObject CreateAnimatedLine(Vector3 start, Vector3 end, float delay, bool last) {
		GameObject line = Instantiate(linePrefab, start, Quaternion.identity);
		// Rotate towards next position in path
		line.transform.LookAt(end);

		// You have to access particle system properties through particlesystem.main for some reason
		ParticleSystem ps = line.transform.GetComponent<ParticleSystem>();
		var main = ps.main;
		main.startDelay = delay;

		// If the line is on an angle, then it is slightly longer
		if (start.x != end.x && start.z != end.z) {
			// Therefore, use pythagorean theorem to extend this line!
			// I just hardcoded the results they are constants 
			main.maxParticles = 16;

			if (last) {
				main.startLifetime = 0.125f;
			} else {
				main.startLifetime = 0.25f;
			}

			line.transform.localScale = new Vector3(1.41421f, 0, 1.41421f);

		}

		// Last piece in the line; should be half as long
		if (last) {
			// Since the scale is halfed here...
			line.transform.localScale /= 2;
			// ...the particles will be twice as small, and go twice as slow
			main.startLifetime = 0.125f;
			main.startSpeed = 8;
			main.startSize = 0.2f;
		}
		return line;
	}

	public void GeneratePathTo(int x, int y) {
		// Clear out our unit's old path.
		selectedUnit.GetComponent<UnitPathfinding>().currentPath = null;

		if (UnitCanEnterTile(x,y) == false ) {
			// We clicked on an unwalkable tile
			for (int i = 0; i < createdLines.Count; i++) {
				Destroy(createdLines[i]);
			}
			createdLines.Clear();
			tileOutline.transform.position = new Vector3(-100f, -100f, -100f);
			return;
		}

		// Move outline to new end target
		tileOutline.transform.position = TileCoordToWorldCoord(x, y) - new Vector3(0, -0.01f, 0.5f);

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();
		
		Node source = graph[
		                    selectedUnit.GetComponent<UnitPathfinding>().targetX, 
		                    selectedUnit.GetComponent<UnitPathfinding>().targetY
		                    ];
		
		Node target = graph[
		                    x, 
		                    y
		                    ];
		
		dist[source] = 0;
		prev[source] = null;

		// Initialize everything to have infinity distance
		foreach(Node v in graph) {
			if(v != source) {
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add(v);
		}

		while(unvisited.Count > 0) {
			// "u" is going to be the unvisited node with the smallest distance.
			Node u = null;

			foreach(Node possibleU in unvisited) {
				if(u == null || dist[possibleU] < dist[u]) {
					u = possibleU;
				}
			}

			if(u == target) {
				break;	// Exit the while loop!
			}

			unvisited.Remove(u);

			foreach(Node v in u.neighbours) {
				//float alt = dist[u] + u.DistanceTo(v);
				float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
				if( alt < dist[v] ) {
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		// If we get there, then either we found the shortest route
		// to our target, or there is no route at ALL to our target.

		if(prev[target] == null) {
			// No route between our target and the source
			return;
		}

		List<Node> currentPath = new List<Node>();

		Node curr = target;

		// Step through the "prev" chain and add it to our path
		while(curr != null) {
			currentPath.Add(curr);
			curr = prev[curr];
		}

		// Right now, currentPath describes a route from our target to our source
		// So we need to invert it!
		currentPath.Reverse();

		// Update current path
		selectedUnit.GetComponent<UnitPathfinding>().currentPath = currentPath;

		// Destroy old line objects
		for (int i = 0; i < createdLines.Count; i++) {
			Destroy(createdLines[i]);
        }
		// Reset list of line objects
		createdLines.Clear();

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
}
