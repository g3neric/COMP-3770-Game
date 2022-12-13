// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using UnityEngine;
using System.Collections.Generic;
[System.Serializable]

// All game weapons
public class Item {
    public string name;
    public float damage; // per hit
    public int APcost; // per use
    public int range;
}

[System.Serializable]
public class AssaultRifle : Item {
    // constructor
    public AssaultRifle() {

        name = "Assault Rifle";
        damage = 12;
        APcost = 5;
        range = 7;
    }
}

[System.Serializable]
public class SniperRifle : Item {
    // constructor
    public SniperRifle() {
        name = "Sniper Rifle";
        damage = 20;
        APcost = 6;
        range = 9;
    }
}

[System.Serializable]
public class Pistol : Item {
    // constructor
    public Pistol() {
        name = "Pistol";
        damage = 4;
        APcost = 2;
        range = 5;
    }
}



