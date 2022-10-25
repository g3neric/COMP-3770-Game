using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ClickableTile : MonoBehaviour {
	// These variables are used by TileMap.cs when instantiating each tile
	[HideInInspector] public int x;
	[HideInInspector] public int y;
	[HideInInspector] public int type;
	[HideInInspector] public TileMap map;

	void OnMouseUp() {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			map.GeneratePathTo(x, y);
		}
	}
}

