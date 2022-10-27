// All player classes and enemies will be 
// inherited from this Character base class
public class Character {
    // base stats - all units will differ
    // health
    public int HP;
    public int maxHP;
    
    // action points
    public int AP;
    public int maxAP;

    // other base stats
    public int range;
    public string name;
    public float luckMultiplier;

    // variable stats - all units start with 0
    public int gold;
    public int killCount;

    public Character() {
        this.killCount = 0;
        this.gold = 0;
    }

    public void EndTurn() {
        if (this.HP > this.maxHP) {
            this.HP = this.maxHP;
        } else if (this.HP <= 0) {
            // Call to death/end game function will go here
        }
        AP = maxAP;
    }
}
