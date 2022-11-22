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
