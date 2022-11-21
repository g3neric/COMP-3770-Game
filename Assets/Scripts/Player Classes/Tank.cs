// Desgined and created by Tyler R. Renaud
// All rights belong to creator

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
