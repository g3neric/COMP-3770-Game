using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ShopManager : MonoBehaviour {
    [HideInInspector] public  GameManager gameManager;
    [HideInInspector] public List<Shop> shopList;
    public GameObject shopPanel;
    public GameObject shopItemPrefab;

    // find shop at (x, y)
    public Shop FindShopAtPosition(int x, int y) {
        if (shopList.Count > 0) {
            Assert.IsTrue(shopList != null);
            foreach (Shop curShop in shopList) {
                if (curShop.shopCoords[0] == x &&
                    curShop.shopCoords[1] == y) {
                    return curShop;
                }
            }
        }
        print("shop not found");
        return null;
    }

    public void ToggleShop(int x, int y) {
        foreach (Shop curShop in shopList) {
            if (curShop.shopCoords[0] == x &&
                curShop.shopCoords[1] == y) {
                // found current shop
                if (!curShop.active) {
                    // not currently active, so set all items to active
                    foreach (ShopItem curShopItem in curShop.shopItems) {
                        curShopItem.shopItemObject.SetActive(true);

                        curShopItem.UpdateColour(gameManager.GetCharacterClass());

                        if (curShopItem.name == "Incendiary Rounds" && gameManager.GetCharacterClass().incendiaryRounds) {
                            curShopItem.SoldOut();
                        }

                        if (curShopItem.name == "Armour Piercing Rounds" && gameManager.GetCharacterClass().armourPiercingRounds) {
                            curShopItem.SoldOut();
                        }


                    }
                    curShop.active = true;
                } else {
                    // currently active, set all items to not active
                    foreach (ShopItem curShopItem in curShop.shopItems) {
                        curShopItem.shopItemObject.SetActive(false);
                    }
                    curShop.active = false;
                }
            }
        }
    }

    public bool ShopContains(string itemName, Shop shop) {
        foreach(ShopItem curShopItem in shop.shopItems) {
            if (curShopItem.name == itemName) {
                return true;
            }
        }
        return false;
    }

    public ShopItem RandomShopItem(int quantity, GameObject newShopItemObject) {
        int ranNum = Random.Range(1, 9);
        switch (ranNum) {
            case 1:
                return new Binoculars(quantity, newShopItemObject);
            case 2:
                return new HermesBoots(quantity, newShopItemObject);
            case 3:
                return new ArmourPiercingRounds(quantity, newShopItemObject);
            case 4:
                return new BandOfRegeneration(quantity, newShopItemObject);
            case 5:
                return new DiamondShield(quantity, newShopItemObject);
            case 6:
                return new IncendiaryRounds(quantity, newShopItemObject);
            case 7:
                return new LuckyCharm(quantity, newShopItemObject);
            case 8:
                return new BionicArm(quantity, newShopItemObject);
            default:
                print("error populating shop");
                return null;
        }
    }

    public void InitializeShop(int[] worldPos, GameObject shopTileObject) {
        // add shop to list
        Shop newShop = new Shop(worldPos, shopTileObject);
        // add between 1 and 6 items to the shop
        for (int i = 0; i < Random.Range(1, 7); i++) {
            // create new shop item from prefab
            GameObject newShopItemObject = Instantiate(shopItemPrefab,
                                                       Vector3.zero,
                                                       Quaternion.identity,
                                                       shopPanel.transform);
            // make current item UI initially inactive
            newShopItemObject.SetActive(false);

            // pick quantity to add
            int quantity = Random.Range(1, 3);
            // pick which item to add randomly
            ShopItem newShopItem = RandomShopItem(quantity, newShopItemObject);
            newShopItem.currentShop = newShop;

            // keep picking new item until we find an item that isn't already in the shop
            while (ShopContains(newShopItem.name, newShop)) {
                newShopItem = RandomShopItem(quantity, newShopItemObject);
            }

            // initialize reference for messages
            newShopItem.uIManager = gameManager.uiManager;

            newShop.shopItems.Add(newShopItem);

            // set up button
            Button newShopItemObjectButton = newShopItemObject.transform.Find("Purchase Button").GetComponent<Button>();

            // add functionality
            newShopItemObjectButton.onClick.AddListener(delegate { newShopItem.Purchase(gameManager.GetCharacterClass()); });
            newShopItemObjectButton.onClick.AddListener(delegate { gameManager.soundManager.PlayButtonClick(); });

            newShopItem.UpdateItem(gameManager.GetCharacterClass());

            // initialize name of new shop item
            newShopItemObject.name = "Shop (" + newShop.shopCoords[0] + ", " + newShop.shopCoords[1] + ") " + newShopItem.name;
        }
        shopList.Add(newShop);
    }
}
