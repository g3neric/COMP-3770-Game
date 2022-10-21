using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickTest : MonoBehaviour
{
    //seeing if I can make it so the selected unit can work like the clickabletile.cs

    public int unitX;
    public int UnitY;
    public TileMap map;

    void OnMouseUp() {
        Debug.Log("UnitClicked X:" +unitX +" Y:" +UnitY); //will be zero right now because this is held somewhere else

	if(EventSystem.current.IsPointerOverGameObject())
		return;

    //map.SelectedUnit(unitX,UnitY);
	}
}
