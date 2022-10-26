using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ClickableTile : MonoBehaviour {
	// These variables are used by TileMap.cs when instantiating each tile
	[HideInInspector] public int x;
	[HideInInspector] public int y;
	[HideInInspector] public int type;
	[HideInInspector] public TileMap map;

	bool completed = false;

	void OnMouseUp() {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			map.GeneratePathTo(x, y);
		}
	}

    void OnMouseEnter() {
		if (!EventSystem.current.IsPointerOverGameObject() && !completed) {
			map.tileHoverOutline.transform.position = transform.position + new Vector3(0, 0.01f, 0);
			completed = true;
		}
    }

    void OnMouseExit() {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			completed = false;
			map.tileHoverOutline.transform.position = new Vector3(-100f, -100f, -100f);
		}
	}
}

