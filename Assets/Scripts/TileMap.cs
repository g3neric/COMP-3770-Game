﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileMap : MonoBehaviour {
	// Public variables
	// Change these in the editor
	public int mapSizeX;
	public int mapSizeY;

	public GameObject tileOutlinePrefab;
	public GameObject selectedUnit;
	public TileType[] tileTypes;
	public Unit[] units;
	public GameObject tilePrefab;

	// Private variables
	private int[,] tiles;
	private Node[,] graph;
	private GameObject tileOutline;

	void Start() {
		// Setup the selectedUnit's variable
		selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
		selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.z;
		selectedUnit.GetComponent<Unit>().map = this;
		tileOutline = (GameObject)Instantiate(tileOutlinePrefab, new Vector3(-10f, 0.001f, -10f), Quaternion.identity);

		GenerateMapData();
		GeneratePathfindingGraph();
		GenerateMapVisual();
	}


    void GenerateMapData() {
		// Allocate our map tiles
		tiles = new int[mapSizeX,mapSizeY];
		
		int x,y;

		// Initialize our map tiles randomly
		/*
		please mind that this cannot work this way as it will possible softlock the player with mountains
		a potential work around will be making a set-map, (see above commented out code) or having it check 
		if (lasttile) was mountain then dont place another mountain, but thats kinda lame, 
		in my opinion (Cheesey/Andrew) We should set certain areas that could be /mountain/swamp/grass/whateverelse

		or maybe even create a list of pre-made "chunks" and have it grab them from an array or smth, that way it'll always be playable Thanks 
		*/
		
		for(x=0; x < mapSizeX; x++) {
			for (y = 0; y < mapSizeX; y++) {
				if (x == 0 || y == 0 || y == mapSizeY - 1 || x == mapSizeX - 1) {
					tiles[x, y] = 0;
				} else {
					if (tiles[x-1, y] == 1 &&
						tiles[x+1, y] == 1 &&
						tiles[x, y-1] == 1 &&
						tiles[x, y+1] == 1) {
						tiles[x, y] = 1;
                    } else {
						int num = Random.Range(1, tileTypes.Length);
						if (num == 1) {
							int num2 = Random.Range(1, tileTypes.Length);
							tiles[x, y] = num2;
						} else {
							tiles[x, y] = num;
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
		for(int x=0; x < mapSizeX; x++) {
			for(int y=0; y < mapSizeX; y++) {
				graph[x,y] = new Node();
				graph[x,y].x = x;
				graph[x,y].y = y;
			}
		}

		// Now that all the nodes exist, calculate their neighbours
		for(int x=0; x < mapSizeX; x++) {
			for(int y=0; y < mapSizeX; y++) {
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
				GameObject currentTile = (GameObject)Instantiate(tilePrefab, new Vector3(x, 0f, y), Quaternion.identity);

				currentTile.transform.GetComponent<Renderer>().material = tt.tileMaterial;

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

	public void GeneratePathTo(int x, int y) {
		// Clear out our unit's old path.
		selectedUnit.GetComponent<Unit>().currentPath = null;
		tileOutline.transform.position = TileCoordToWorldCoord(x, y) - new Vector3(0, 0, 0.5f);

		if( UnitCanEnterTile(x,y) == false ) {
			// We probably clicked on a mountain or something, so just quit out.
			return;
		}

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();
		
		Node source = graph[
		                    selectedUnit.GetComponent<Unit>().tileX, 
		                    selectedUnit.GetComponent<Unit>().tileY
		                    ];
		
		Node target = graph[
		                    x, 
		                    y
		                    ];
		
		dist[source] = 0;
		prev[source] = null;

		// Initialize everything to have INFINITY distance, since
		// we don't know any better right now. Also, it's possible
		// that some nodes CAN'T be reached from the source,
		// which would make INFINITY a reasonable value
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

		selectedUnit.GetComponent<Unit>().currentPath = currentPath;
	}

}
