// Desgined and created by Andrew Simon and Tyler R. Renaud
// All rights belong to creator

using UnityEngine;
using System.Collections;
[System.Serializable]
public class TileType {
	public string name;
	public GameObject tilePrefab;
	[Range(0, 100)] public int frequency; // Maximum of 100%
	public bool isWalkable = true;
	public int movementCost = 1;
}
