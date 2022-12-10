using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[SerializeField]
public class ShopItem {
    public Sprite icon;
    public int quantity;
    public string name;
    public string description;
    public int goldCost; // purchase price
    public bool soldOut;
    public GameObject shopItemObject;
    public Shop currentShop; // parent

    // reference
    public UIManager uIManager;

    public ShopItem() {
        soldOut = false;
    }
    public void SoldOut() {
        soldOut = true;
        shopItemObject.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = "SOLD OUT";
        shopItemObject.transform.Find("Quantity and cost").gameObject.SetActive(false);
        shopItemObject.transform.Find("Description").gameObject.SetActive(false);
        shopItemObject.transform.Find("Purchase Button").gameObject.SetActive(false);
    }

    public void UpdateColour(Character playerChar) {
        if (playerChar.gold - goldCost < 0 || soldOut) {
            // slightly transparent red
            Color32 newColour = new Color32(140, 0, 0, 100);
            shopItemObject.GetComponent<Image>().color = newColour;
        } else {
            // default (slightly transparent black)
            Color32 newColour = new Color32(0, 0, 0, 100);
            shopItemObject.GetComponent<Image>().color = newColour;
        }
    }

    public void UpdateItem(Character playerChar) {
        if (soldOut) {
            // disable item on screen
            SoldOut();
        } else {
            // set name
            shopItemObject.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name;

            // set quantity and gold cost
            string quantityAndCost = "";
            if (name == "Armour Piercing Rounds" || name == "Incendiary Rounds") {
                quantityAndCost = quantityAndCost + goldCost + " gold, max one";
            } else {
                quantityAndCost = quantityAndCost + goldCost + " gold, " + quantity + " in stock";
            }

            shopItemObject.transform.Find("Quantity and cost").GetComponent<TextMeshProUGUI>().text = quantityAndCost;

            // set description
            shopItemObject.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = description;
        }
    }

    // this will be overriden
    public virtual bool Purchase(Character playerChar) {
        return false;
    }

    public bool PurchaseCheck(Character playerChar) {
        if (playerChar.gold - goldCost >= 0 && quantity > 0) {
            // purchase successful
            playerChar.gold -= goldCost;
            quantity--;

            uIManager.SendMessageToLog("Purchased <color=#e19cff><b>" + name + "</b>");
            
            if (currentShop != null) {
                foreach (ShopItem item in currentShop.shopItems) {
                    if (item != null) {
                        item.UpdateColour(playerChar);
                    }
                }
            }

            if (quantity <= 0) {
                SoldOut();
            }

            UpdateItem(playerChar);
            UpdateColour(playerChar);

            return true;
        } else {
            return false;
        }
    }
}

[SerializeField]
public class Binoculars : ShopItem {
    public Binoculars (int quantity, GameObject shopItemObject) {
        this.quantity = quantity;
        this.shopItemObject = shopItemObject;
        name = "Binoculars";
        description = "Adds 1 tile to your view range.";
        goldCost = Random.Range(19, 24);
    }

    public override bool Purchase(Character playerChar) {
        if (PurchaseCheck(playerChar)) {
            playerChar.viewRange += 1;
            return true;
        } else {
            return false;
        }
    }
}

[SerializeField]
public class HermesBoots : ShopItem {
    public HermesBoots(int quantity, GameObject shopItemObject) {
        this.quantity = quantity;
        this.shopItemObject = shopItemObject;
        name = "Hermes Boots";
        description = "Increases max AP by 1. Gain 1 AP immediately.";
        goldCost = Random.Range(35, 55);
    }
    public override bool Purchase(Character playerChar) {
        if (PurchaseCheck(playerChar)) {
            playerChar.maxAP += 1;
            playerChar.AP += 1;
            return true;
        } else {
            return false;
        }
    }
}

[SerializeField]
public class ArmourPiercingRounds : ShopItem {
    public ArmourPiercingRounds(int quantity, GameObject shopItemObject) {
        this.quantity = quantity;
        this.shopItemObject = shopItemObject;
        name = "Armour Piercing Rounds";
        description = "Increases base damage on all weapons by 5.";
        goldCost = Random.Range(18, 25);
    }

    public override bool Purchase(Character playerChar) {
        if (PurchaseCheck(playerChar)) {
            if (playerChar.currentItems.Count == 2) {
                playerChar.currentItems[0].damage += 5;
                playerChar.currentItems[1].damage += 5;
            } else if (playerChar.currentItems.Count == 1) {
                playerChar.currentItems[0].damage += 5;
            }
            playerChar.armourPiercingRounds = true;
            SoldOut();
            return true;
        } else {
            return false;
        }     
    }
}

[SerializeField]
public class BandOfRegeneration : ShopItem {
    public BandOfRegeneration(int quantity, GameObject shopItemObject) {
        this.quantity = quantity;
        this.shopItemObject = shopItemObject;
        name = "Band Of Regeneration";
        description = "Increases health regeneration by 1 HP/sec.";
        goldCost = Random.Range(3, 20);
    }

    public override bool Purchase(Character playerChar) {
        if (PurchaseCheck(playerChar)) {
            playerChar.healRate++;
            return true;
        } else {
            return false;
        } 
    }
}

[SerializeField]
public class DiamondShield : ShopItem {
    public DiamondShield(int quantity, GameObject shopItemObject) {
        this.quantity = quantity;
        this.shopItemObject = shopItemObject;
        name = "Diamond Shield";
        description = "Increases max HP by 10. Regen 10 HP immediately.";
        goldCost = Random.Range(16, 28);
    }

    public override bool Purchase(Character playerChar) {
        if (PurchaseCheck(playerChar)) {
            playerChar.maxHP += 10;
            playerChar.HP += 10;
            return true;
        } else {
            return false;
        }     
    }
}

[SerializeField]
public class IncendiaryRounds : ShopItem {
    public IncendiaryRounds(int quantity, GameObject shopItemObject) {
        this.quantity = quantity;
        this.shopItemObject = shopItemObject;
        name = "Incendiary Rounds";
        description = "When you hit an enemy, set them on fire. Fire deals 10 damage/turn.";
        goldCost = Random.Range(17, 28);
    }

    public override bool Purchase(Character playerChar) {
        if (PurchaseCheck(playerChar)) {
            playerChar.incendiaryRounds = true;
            SoldOut();
            return true;
        } else {
            return false;
        }
    }
}

[SerializeField]
public class LuckyCharm : ShopItem {
    public LuckyCharm(int quantity, GameObject shopItemObject) {
        this.quantity = quantity;
        this.shopItemObject = shopItemObject;
        name = "Lucky Charm";
        description = "Increase crit chance by 1.5x.";
        goldCost = Random.Range(30, 40);
    }

    public override bool Purchase(Character playerChar) {
        if (PurchaseCheck(playerChar)) {
            playerChar.luckMultiplier *= 1.5f;
            return true;
        } else {
            return false;
        }
    }
}

[SerializeField]
public class BionicArm : ShopItem
{
    public BionicArm(int quantity, GameObject shopItemObject)
    {
        this.quantity = quantity;
        this.shopItemObject = shopItemObject;
        name = "Bionic Arm";
        description = "Increase base accuracy to 100%.";
        goldCost = Random.Range(30, 40);
    }

    public override bool Purchase(Character playerChar)
    {
        if (PurchaseCheck(playerChar))
        {
            playerChar.accuracy = 100f;
            return true;
        }
        else
        {
            return false;
        }
    }
}