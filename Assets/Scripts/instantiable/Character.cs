// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using UnityEngine;
using System.Collections.Generic;
[System.Serializable]

// All player classes and enemies will be 
// inherited from this Character base class
public class Character {
    // character model
    public GameObject characterPrefab;

    public string className;

    // base stats - all classes will differ
    // health
    public int HP;
    public int maxHP;
    public int healRate;

    // action points
    public int AP;
    public int maxAP;

    // other base stats
    public int viewRange; // for fog of war
    public float luckMultiplier; // used by joker class

    // variable stats - all units start with 0
    public int gold;
    public int killCount;

    // current pathfinding path
    public List<Node> currentPath = null;

    // target position (x, y)
    public int currentX;
    public int currentY;

    // the items the player is holding
    [HideInInspector] public List<Item> currentItems = new List<Item>();
    [HideInInspector] public int selectedItemIndex; // 0 for item 1, 1 for item 2

    // state of death
    public bool dead;

    // constructor
    public Character() {
        killCount = 0;
        gold = 0;
        dead = false;
    }

    public void TakeDamage(int amount) {
        HP -= amount;
        if (HP <= 0) {
            // die
            HP = 0;
            Death();
        }
    }

    public void FinishTurn() {
        AP = maxAP; // refresh AP
        HP += healRate; // heal a little bit

        // clamp view range to the highest attack range you have
        // therefore you can't shoot farther than you can see
        // gonna have to do this when weapon is picked up!
        if (currentItems[0] != null) {
            if (currentItems[0].range > viewRange) {
                currentItems[0].range = viewRange;
            }
        } else if (currentItems[1] != null) {
            if (currentItems[1].range > viewRange) {
                currentItems[0].range = viewRange;
            }
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
}

public class Grunt : Character {
    public Grunt() {
        className = "Grunt";
        maxAP = 5;
        maxHP = 40;
        healRate = 3;
        viewRange = 10;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
    }
}

public class Engineer : Character {
    public Engineer() {
        className = "Engineer";
        maxAP = 6;
        maxHP = 45;
        healRate = 3;
        viewRange = 10;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
    }
}

public class Joker : Character {
    public Joker() {
        className = "Joker";
        maxAP = 6;
        maxHP = 35;
        healRate = 3;
        viewRange = 10;
        luckMultiplier = 1.5f;
        HP = maxHP;
        AP = maxAP;
    }
}

public class Saboteur : Character {
    public Saboteur() {
        className = "Saboteur";
        maxAP = 6;
        maxHP = 45;
        healRate = 3;
        viewRange = 10;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
    }
}

public class Scout : Character {
    public Scout() {
        className = "Scout";
        maxAP = 8;
        maxHP = 45;
        healRate = 3;
        viewRange = 10;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
    }
}

public class Sharpshooter : Character {
    public Sharpshooter() {
        className = "Sharpshooter";
        maxAP = 7;
        maxHP = 30;
        healRate = 3;
        viewRange = 10;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
    }
}

public class Surgeon : Character {
    public Surgeon() {
        className = "Surgeon";
        maxAP = 6;
        maxHP = 70;
        healRate = 7;
        viewRange = 10;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
    }
}

public class Tank : Character {
    public Tank() {
        className = "Tank";
        maxAP = 6;
        maxHP = 80;
        healRate = 2;
        viewRange = 10;
        luckMultiplier = 1f;
        HP = maxHP;
        AP = maxAP;
    }
}
