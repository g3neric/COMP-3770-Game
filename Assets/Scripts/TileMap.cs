using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileMap : MonoBehaviour {

	public GameObject selectedUnit;

	public TileType[] tileTypes;

	public Unit[] units;

	int[,] tiles;
	Node[,] graph;


	int mapSizeX = 10;
	int mapSizeY = 10;

	void Start() {
		// Setup the selectedUnit's variable
		selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
		selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
		selectedUnit.GetComponent<Unit>().map = this;

		GenerateMapData();
		GeneratePathfindingGraph();
		GenerateMapVisual();
	}

	void GenerateMapData() {
		// Allocate our map tiles
		tiles = new int[mapSizeX,mapSizeY];
		
		int x,y;
		int map = Random.Range(0,3);

		switch(map)
		{
			case 0:
				// Initialize our map tiles to be grass
				for(x=0; x < mapSizeX; x++) {
					for(y=0; y < mapSizeX; y++) {
						tiles[x,y] = 0;
					}
				}

				// Make a big swamp area
				for(x=3; x <= 5; x++) {
					for(y=0; y < 4; y++) {
						tiles[x,y] = 1;
					}
				}
				
				// Let's make a u-shaped mountain range
				tiles[4, 4] = 2;
				tiles[5, 4] = 2;
				tiles[6, 4] = 2;
				tiles[7, 4] = 2;
				tiles[8, 4] = 2;

				tiles[4, 5] = 2;
				tiles[4, 6] = 2;
				tiles[8, 5] = 2;
				tiles[8, 6] = 2;
				break;
			case 1:
			// Initialize our map tiles to be grass
				for(x=0; x < mapSizeX; x++) {
					for(y=0; y < mapSizeX; y++) {
						tiles[x,y] = 0;
					}
				}

				// Make a big swamp area
				for(x=3; x <= 8; x++) {
					for(y=5; y < 8; y++) {
						tiles[x,y] = 1;
					}
				}
				tiles[7,3] = 1;
				tiles[7,4] = 1;
				tiles[8,3] = 1;
				tiles[8,4] = 1;
				// Let's make a L-shaped mountain range
				tiles[1, 7] = 2;
				tiles[1, 6] = 2;
				tiles[1, 5] = 2;
				tiles[1, 4] = 2;
				tiles[1, 3] = 2;

				tiles[2, 3] = 2;
				tiles[3, 3] = 2;
				tiles[4, 3] = 2;
				tiles[5, 3] = 2;
				break;
			case 2:
				// Initialize our map tiles randomly
				/*
				please mind that this cannot work this way as it will possible softlock the player with mountains
				a potential work around will be making a set-map, (see above commented out code) or having it check 
				if (lasttile) was mountain then dont place another mountain, but thats kinda lame, 
				in my opinion (Cheesey/Andrew) We should set certain areas that could be /mountain/swamp/grass/whateverelse

				or maybe even create a list of pre-made "chunks" and have it grab them from an array or smth, that way it'll always be playable Thanks 
				*/

				for(x=0; x < mapSizeX; x++) {
					for(y=0; y < mapSizeX; y++) {
						tiles[x,y] = Random.Range(0,3);
					}
				}
				break;
		}
		
		
		

	}

	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY) {

		TileType tt = tileTypes[ tiles[targetX,targetY] ];

		if(UnitCanEnterTile(targetX, targetY) == false)
			return Mathf.Infinity;

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

				// This is the 4-way connection version:
/*				if(x > 0)
					graph[x,y].neighbours.Add( graph[x-1, y] );
				if(x < mapSizeX-1)
					graph[x,y].neighbours.Add( graph[x+1, y] );
				if(y > 0)
					graph[x,y].neighbours.Add( graph[x, y-1] );
				if(y < mapSizeY-1)
					graph[x,y].neighbours.Add( graph[x, y+1] );
*/

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
				if(y > 0)
					graph[x,y].neighbours.Add( graph[x, y-1] );
				if(y < mapSizeY-1)
					graph[x,y].neighbours.Add( graph[x, y+1] );

				// This also works with 6-way hexes and n-way variable areas (like EU4)
			}
		}
	}

	void GenerateMapVisual() {
		for(int x=0; x < mapSizeX; x++) {
			for(int y=0; y < mapSizeX; y++) {
				TileType tt = tileTypes[ tiles[x,y] ];
				GameObject go = (GameObject)Instantiate( tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity );

				ClickableTile ct = go.GetComponent<ClickableTile>();
				ct.tileX = x;
				ct.tileY = y;
				ct.map = this;
			}
		}
	}

	public Vector3 TileCoordToWorldCoord(int x, int y) {
		return new Vector3(x, y, 0);
	}

	public bool UnitCanEnterTile(int x, int y) {

		// We could test the unit's walk/hover/fly type against various
		// terrain flags here to see if they are allowed to enter the tile.

		return tileTypes[ tiles[x,y] ].isWalkable;
	}

	public void GeneratePathTo(int x, int y) {
		// Clear out our unit's old path.
		selectedUnit.GetComponent<Unit>().currentPath = null;

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

		// If we get there, the either we found the shortest route
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

		// Right now, currentPath describes a route from out target to our source
		// So we need to invert it!

		currentPath.Reverse();

		selectedUnit.GetComponent<Unit>().currentPath = currentPath;
	}

}
