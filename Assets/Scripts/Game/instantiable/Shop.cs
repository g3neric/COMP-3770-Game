using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class Shop {
    public List<ShopItem> shopItems;
    public int[] shopCoords;
    public GameObject shopTileObject;
    public bool active = false;

    public Shop(int[] shopCoords, GameObject shopTileObject) {
        shopItems = new List<ShopItem>();
        this.shopCoords = shopCoords;
        this.shopTileObject = shopTileObject;
    }
}
