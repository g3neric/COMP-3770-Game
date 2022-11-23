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

    // action points
    public int AP;
    public int maxAP;

    // other base stats
    public int attackRange; // for combat
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


    // constructor
    public Character() {
        killCount = 0;
        gold = 0;
    }

    public void FinishTurn() {
        AP = maxAP; // refresh AP
        if (HP > maxHP) {
            HP = maxHP;
        } else if (HP <= 0) {
            // Call to death/end game function will go here
            return;
        }
    }
}

public class Grunt : Character {
    public Grunt() {
        className = "Grunt";
        maxAP = 5;
        AP = maxAP;
        maxHP = 40;
        attackRange = 4; // tiles
        viewRange = 10;
        luckMultiplier = 1f;
    }
}

public class Engineer : Character {
    public Engineer() {
        className = "Engineer";
        maxAP = 6;
        AP = maxAP;
        maxHP = 45;
        attackRange = 6; // tiles
        viewRange = 10;
        luckMultiplier = 1f;
    }
}

public class Joker : Character {
    public Joker() {
        className = "Joker";
        maxAP = 6;
        AP = maxAP;
        maxHP = 35;
        attackRange = 4; // tiles
        viewRange = 10;
        luckMultiplier = 1.5f;
    }
}

public class Saboteur : Character {
    public Saboteur() {
        className = "Saboteur";
        maxAP = 6;
        AP = maxAP;
        maxHP = 45;
        attackRange = 6; // tiles
        viewRange = 10;
        luckMultiplier = 1f;
    }
}

public class Scout : Character {
    public Scout() {
        className = "Scout";
        maxAP = 8;
        AP = maxAP;
        maxHP = 45;
        attackRange = 4; // tiles
        viewRange = 10;
        luckMultiplier = 1f;
    }
}

public class Sharpshooter : Character {
    public Sharpshooter() {
        className = "Sharpshooter";
        maxAP = 7;
        AP = maxAP;
        maxHP = 30;
        attackRange = 10; // tiles
        viewRange = 10;
        luckMultiplier = 1f;
    }
}

public class Surgeon : Character {
    public Surgeon() {
        className = "Surgeon";
        maxAP = 6;
        AP = maxAP;
        maxHP = 70;
        attackRange = 2; // tiles
        viewRange = 10;
        luckMultiplier = 1f;
    }
}

public class Tank : Character {
    public Tank() {
        className = "Tank";
        maxAP = 6;
        AP = maxAP;
        maxHP = 80;
        attackRange = 4; // tiles
        viewRange = 10;
        luckMultiplier = 1f;
    }
}
