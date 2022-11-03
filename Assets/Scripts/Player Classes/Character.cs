using UnityEngine;
[System.Serializable]

// All player classes and enemies will be 
// inherited from this Character base class
public class Character {
    // character model
    public GameObject characterPrefab;

    [HideInInspector] public string name;

    // base stats - all classes will differ
    // health
    [HideInInspector] public int HP;
    [HideInInspector] public int maxHP;

    // action points
    [HideInInspector] public int AP;
    [HideInInspector] public int maxAP;

    // other base stats
    [HideInInspector] public int attackRange; // for combat
    [HideInInspector] public int viewRange; // for fog of war
    [HideInInspector] public float luckMultiplier; // used by joker class

    // variable stats - all units start with 0
    [HideInInspector] public int gold;
    [HideInInspector] public int killCount;

    public Character() {
        this.killCount = 0;
        this.gold = 0;
    }

    public void FinishTurn() {
        if (this.HP > this.maxHP) {
            this.HP = this.maxHP;
        } else if (this.HP <= 0) {
            // Call to death/end game function will go here
        }
        AP = maxAP;
    }
}
