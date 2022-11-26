// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using UnityEngine;
using System.Collections.Generic;
[System.Serializable]

// All game weapons
public class Item {
    public GameObject icon;
    public string name;
    public int damage; // per hit
    public int APcost; // per use
    public int range;
    
}

public class AssaultRifle : Item {
    // constructor
    public AssaultRifle() {
        name = "Assault Rifle";
        damage = 15;
        APcost = 4;
        range = 7;
    }
}

public class SniperRifle : Item {
    // constructor
    public SniperRifle() {
        name = "Sniper Rifle";
        damage = 20;
        APcost = 6;
        range = 10;
    }
}

public class Pistol : Item {
    // constructor
    public Pistol() {
        name = "Pistol";
        damage = 5;
        APcost = 3;
        range = 5;
    }
}



