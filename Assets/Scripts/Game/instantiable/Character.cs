// Desgined and created by Tyler R. Renaud
// All rights belong to creator 
//fax

using UnityEngine;
using System.Collections.Generic;
[System.Serializable]

// All player classes and enemies will be 
// inherited from this Character base class
public class Character {
    // character model
    public GameObject characterObject;

    public string className;

    // base stats - all classes will differ
    // health
    public float HP;
    public float maxHP;
    public float healRate;

    // action points
    public int AP;
    public int maxAP;

    // accuracy
    public float accuracy; // percentage to hit

    // other base stats
    public int viewRange; // for fog of war
    public float luckMultiplier; // used by joker class

    // variable stats - all units start with 0
    public int gold;
    public int killCount;

    // gold reward upon death
    // only for enemies
    public int goldOnDeath;

    // current pathfinding path
    public List<Node> currentPath = null;

    // target position (x, y)
    public int currentX;
    public int currentY;

    // upgrades
    public bool onFire;
    public int fireTimer;
    public bool incendiaryRounds = false;
    public bool armourPiercingRounds = false;

    // the items the player is holding
    [HideInInspector] public List<Item> currentItems;
    [HideInInspector] public int selectedItemIndex; // 0 for item 1, 1 for item 2

    // state of death
    public bool dead;

    // constructor
    public Character() {
        currentItems = new List<Item>();
        killCount = 0;
        gold = 5;
        dead = false;
        selectedItemIndex = 0;
    }

    public void TakeDamage(float amount) {
        HP -= amount;
        if (HP <= 0) {
            // die
            HP = 0;
            Death();
        }
    }

    public void SetOnFire() {
        onFire = true;
    }

    public void FinishTurn() {
        AP = maxAP; // refresh AP
        HP += healRate; // heal a little bit

        if (onFire) {
            TakeDamage(5);
        }

        // these are mostly failsafes
        if (HP > maxHP) {
            HP = maxHP;
        } else if (HP <= 0) {
            Death();
        }
    }

    public void Death() {
        dead = true;
    }

    public string GetStringCharacterStats() {
        string text = maxAP + " AP\n" +
                      maxHP + " HP\n" +
                      healRate + " HP per turn\n" +
                      (viewRange - 1) + " tiles\n" +
                      (GameManager.critChance * luckMultiplier) + "% per shot\n" +
                      (GameManager.epicCritChance * luckMultiplier) + "% per shot\n" +
                      accuracy.ToString("F2") + "%\n" +
                      currentItems[0].name;
        if (currentItems.Count > 1) {
            text = text + ", " + currentItems[1].name;
        }
        return text;


        
        
    }
}
[System.Serializable]
public class Grunt : Character {
    public Grunt() {
        className = "Grunt";
        maxAP = 7;
        maxHP = 60;
        healRate = 3;
        viewRange = 8;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 90f;
        goldOnDeath = Random.Range(5, 12);
        currentItems.Add(new AssaultRifle());
        currentItems.Add(new Pistol());
    }
}
[System.Serializable]
public class Engineer : Character {
    public Engineer() {
        className = "Engineer";
        maxAP = 7;
        maxHP = 65;
        healRate = 3;
        viewRange = 8;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 80f;
        goldOnDeath = Random.Range(5, 12);
        currentItems.Add(new Pistol());

    }
}
[System.Serializable]
public class Joker : Character {
    public Joker() {
        className = "Joker";
        maxAP = 7;
        maxHP = 55;
        healRate = 3;
        viewRange = 7;
        luckMultiplier = 3f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 60f;
        goldOnDeath = Random.Range(5, 12);
        currentItems.Add(new Pistol());
    }
}

[System.Serializable]
public class Scout : Character {
    public Scout() {
        className = "Scout";
        maxAP = 9;
        maxHP = 55;
        healRate = 3;
        viewRange = 9;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 90f;
        goldOnDeath = Random.Range(5, 12);
        currentItems.Add(new Pistol());
    }
}
[System.Serializable]
public class Sharpshooter : Character {
    public Sharpshooter() {
        className = "Sharpshooter";
        maxAP = 6;
        maxHP = 45;
        healRate = 3;
        viewRange = 11;
        luckMultiplier = .75f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 95f;
        goldOnDeath = Random.Range(5, 12);
        currentItems.Add(new SniperRifle());
        currentItems.Add(new Pistol());
    }
}
[System.Serializable]
public class Surgeon : Character {
    public Surgeon() {
        className = "Surgeon";
        maxAP = 6;
        maxHP = 80;
        healRate = 7;
        viewRange = 7;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 80f;
        goldOnDeath = Random.Range(5, 12);
        currentItems.Add(new Pistol());
    }
}
[System.Serializable]
public class Tank : Character {
    public Tank() {
        className = "Tank";
        maxAP = 7;
        maxHP = 100;
        healRate = 2;
        viewRange = 7;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 85f;
        goldOnDeath = Random.Range(5, 12);
        currentItems.Add(new AssaultRifle());
        currentItems.Add(new Pistol());
    }
}

public class Goblin : Character {
    public Goblin() {
        className = "Goblin";
        maxAP = 5;
        maxHP = 50;
        healRate = 0;
        viewRange = 6;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 75f;
        goldOnDeath = Random.Range(3, 6);
        currentItems.Add(new Pistol());
    }
}

public class Ghoul : Character {
    public Ghoul() {
        className = "Ghoul";
        maxAP = 4;
        maxHP = 35;
        healRate = 0;
        viewRange = 6;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
        accuracy = 75f;
        goldOnDeath = Random.Range(2, 3);
        currentItems.Add(new Pistol());
    }
}
