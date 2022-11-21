// Desgined and created by Tyler R. Renaud
// All rights belong to creator

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
