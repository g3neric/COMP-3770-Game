// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyManager : MonoBehaviour {
    // link to game manager
    [HideInInspector] public GameManager gameManager;

    // references set by game manager
    public GameObject tileMapControllerObject;
    [HideInInspector] public TileMap map;
    [HideInInspector] public PlayerManager playerManager;

    // enemy prefabs
    public GameObject[] enemyPrefabs = new GameObject[1]; // 1 for now

    // list of enemy game objects
    [HideInInspector] public List<GameObject> enemyGameObjects = new List<GameObject>(); // up to maxEnemyCount

    // list of enemy objects
    [HideInInspector] public List<Character> enemyList = new List<Character>();

    // indexes of spotted enemies
    [HideInInspector] public List<int> spottedEnemies = new List<int>();

    private void Start() {
        map = tileMapControllerObject.GetComponent<TileMap>();
    }

    private void FixedUpdate() {
        // Check if any enemies have died
        for (int i = 0; i < enemyList.Count; i++) {
            if (enemyList[i].dead) {
                CleanUpEnemy(i);
            }
        }
    }

    // remove enemy from the game
    public void CleanUpEnemy(int enemyIndex) {
        Character enemyCharacter = enemyList[enemyIndex];
        GameObject enemyObject = enemyGameObjects[enemyIndex];

       
        // update the tile the enemy was on
        map.tilesObjects[enemyCharacter.currentX, enemyCharacter.currentY].GetComponent<ClickableTile>().currentCharacterOnTile = null;

        // destroy game object
        Destroy(enemyGameObjects[enemyIndex]);

        // remove from lists
        enemyList.RemoveAt(enemyIndex);
        enemyGameObjects.RemoveAt(enemyIndex);

        // remove UI objects
        gameManager.uiManager.RemoveEnemyUIObject(enemyIndex);

        // send death message to log
        gameManager.uiManager.SendMessageToLog("Killed enemy " + enemyCharacter.className + ".");

        // update possible movements when enemy dies because you can now move to where they were
        gameManager.playerManager.DrawPossibleMovements();
    }

    // update enemie's visibility
    public void UpdateEnemiesVisibility() {
        // toggle enemy viewable
        foreach (GameObject curEnemyObject in enemyGameObjects) {
            int enemyIndex = enemyGameObjects.IndexOf(curEnemyObject);
            if (map.IsTileVisibleToPlayer(Mathf.RoundToInt(curEnemyObject.transform.position.x), Mathf.RoundToInt(curEnemyObject.transform.position.z))) {
                // enemy is visible
                // update mesh visibility
                
                    //curEnemyObject.GetComponent<MeshRenderer>().enabled = true;
                
                curEnemyObject.SetActive(true);
                

                // update nametag visibility
                gameManager.uiManager.SetVisibilityOfEnemyUIObject(enemyIndex, true);

                // update health bar visibility

                // send message to log that enemy has been spotted if it hasn't already been sent
                if (!spottedEnemies.Contains(enemyIndex)) {
                    gameManager.SendMessageToLog("Spotted enemy " + enemyList[enemyIndex].className + ".");
                    spottedEnemies.Add(enemyIndex);
                }
            } else {
                // enemy is not visible
                // update mesh visibility
                
                    //curEnemyObject.GetComponent<MeshRenderer>().enabled = false;
                
                curEnemyObject.SetActive(false);
                

                // update nametag visibility
                gameManager.uiManager.SetVisibilityOfEnemyUIObject(enemyIndex, false);

                // update health bar visibility

                // remove enemy from list of spotted enemies
                spottedEnemies.Remove(enemyIndex);
            }
        }
    }

    // spawn a single enemy at a random pos
    public void SpawnEnemy(int enemyPrefabIndex) {
        // make sure we dont have too many enemies already
        int maxEnemies;
        switch(gameManager.difficulty) {
            case DifficultyState.Ez:
                maxEnemies = 25;
                break;
            case DifficultyState.Mid:
                maxEnemies = 35;
                break;
            case DifficultyState.Impossible:
                maxEnemies = 45;
                break;
            default:
                return;
        }

        if (enemyList.Count >= maxEnemies) {
            return;
        }

        // iterate over every tile over a rough approximation of the island size
        int startingX = Random.Range(map.oceanSize, gameManager.mapSize - map.oceanSize);
        int startingY = Random.Range(map.oceanSize, gameManager.mapSize - map.oceanSize);
        for (int x = startingX; x <= gameManager.mapSize - map.oceanSize; x++) {
            for (int y = startingY; y <= gameManager.mapSize - map.oceanSize; y++) {
                // check if current tile is not viewable by the player, is walkable, and nobody is on that tile rn
                if (map.UnitCanEnterTile(x, y) &&
                    !map.IsTileVisibleToPlayer(x, y)) {
                    
                    // create enemy character
                    Character enemyCharacter = null; // temporarily all tanks

                    // randomly choose one of the classes
                    // it's a shame i have to do it this way but it's the only way :(
                    int randomClass = Random.Range(0, 7);
                    switch(randomClass) {
                        case 0:
                            enemyCharacter = new Engineer();
                            break;
                        case 1:
                            enemyCharacter = new Grunt();
                            break;
                        case 2:
                            enemyCharacter = new Joker();
                            break;
                        case 3:
                            enemyCharacter = new Scout();
                            break;
                        case 4:
                            enemyCharacter = new Sharpshooter();
                            break;
                        case 5:
                            enemyCharacter = new Surgeon();
                            break;
                        case 6:
                            enemyCharacter = new Tank();
                            break;
                        default:
                            print("error selecting enemy class");
                            break;
                    }

                    // spawn enemy at (x, y)
                    // instantiate enemy
                    GameObject enemyObject = Instantiate(enemyPrefabs[randomClass], TileMap.TileCoordToWorldCoord(x, y, 0.125f), Quaternion.identity);

                    // set reference to enemy in the tile its on
                    map.tilesObjects[x, y].GetComponent<ClickableTile>().currentCharacterOnTile = enemyObject;


                    // update target variables
                    enemyCharacter.currentX = x;
                    enemyCharacter.currentY = y;

                    switch(gameManager.difficulty) {
                        case DifficultyState.Ez:
                            enemyCharacter.maxHP -= 20;
                            enemyCharacter.HP -= 20;
                            enemyCharacter.luckMultiplier -= 0.5f;
                            break;
                        case DifficultyState.Impossible:
                            enemyCharacter.maxHP += 10;
                            enemyCharacter.luckMultiplier *= 1.2f;
                            enemyCharacter.HP += 15;
                            enemyCharacter.maxAP += 2;
                            break;
                    }

                    // add enemy to lists
                    enemyList.Add(enemyCharacter);
                    enemyGameObjects.Add(enemyObject);

                    // name the object with it's index
                    // you would think the add function above would return the index, but it doesn't. whatever.
                    enemyObject.name = "Enemy ("  + enemyGameObjects.IndexOf(enemyObject) + ") " + enemyCharacter.className;

                    // create UI objects
                    gameManager.uiManager.CreateEnemyUIObjects(enemyCharacter);

                    return; // enemy has been created, stop the method now
                }
            }
        }
    }

    // resolve all enemy actions
    public void ResolveAllEnemyTurns() {
        for (int i = 0; i < enemyList.Count; i++) {
            ResolveEnemyTurn(i);
        }
    }

    // check if player is in range. if they are, return their (x, y) location
    private int[] IsPlayerInRange(Character curEnemy, int range) {
        // calculate all tiles in range
        List<int[]> tilesInRange = map.CalculateTilesInRange(curEnemy.currentX, curEnemy.currentY, range, false);

        // check each tile to see if the player is in range
        for (int i = 0; i < tilesInRange.Count; i++) {
            ClickableTile curTile = map.tilesObjects[tilesInRange[i][0], tilesInRange[i][1]].GetComponent<ClickableTile>();
            
            // check if the current tile has someone on it, if it's not the tile we're standing on, and if it's a player
            if (curTile.currentCharacterOnTile != null &&
                tilesInRange[i][0] != curEnemy.currentX &&
                tilesInRange[i][1] != curEnemy.currentY &&
                curTile.currentCharacterOnTile.tag == "Player") {
                // player is in range
                return new int[] { tilesInRange[i][0], tilesInRange[i][1]};
            }
        }
        return null;
    }

    // calculate enemy's movement for one turn
    private void ResolveEnemyTurn(int enemyIndex) {
        Character curEnemy = enemyList[enemyIndex];
        GameObject curEnemyObject = enemyGameObjects[enemyIndex];

        // check if player is in view range, if so move towards them. if not, move randomly
        int[] playerCoordIfInRange = IsPlayerInRange(curEnemy, curEnemy.viewRange);
        if (playerCoordIfInRange != null) {
            // player is in view range
            
            // calculate distance between enemy and player
            float dist = TileMap.DistanceBetweenTiles(curEnemy.currentX, curEnemy.currentY, playerCoordIfInRange[0], playerCoordIfInRange[1]);

            // select appropriate weapon
            /*
            if (dist >= Mathf.Min(curEnemy.currentItems[0].range, curEnemy.currentItems[1].range) {

            }
            */

            // switch to primary weapon
            curEnemy.selectedItemIndex = 0;

            // check if enemy can attack player with primary
            if (IsPlayerInRange(curEnemy, curEnemy.currentItems[0].range) != null) {
                // player is in attack range, so lets check if we can attack them
                while (curEnemy.AP - curEnemy.currentItems[0].APcost >= 0) {
                    gameManager.AttackPlayer(curEnemy);
                }
            }

            // check if enemy has a secondary
            if (curEnemy.currentItems.Count > 1) {
                // check if player is in range of their secondary
                if (IsPlayerInRange(curEnemy, curEnemy.currentItems[1].range) != null) {
                    curEnemy.selectedItemIndex = 1; // switch to secondary
                    // attack until out of AP
                    while (curEnemy.AP - curEnemy.currentItems[1].APcost >= 0) {
                        gameManager.AttackPlayer(curEnemy);
                    }
                }
            }

            // return a vector2 that is (attackRange - 1) units away from the player in the direction of the player
            Vector2 start = new Vector2(curEnemy.currentX, curEnemy.currentY);
            Vector2 end = new Vector2(playerCoordIfInRange[0], playerCoordIfInRange[1]);
            Vector2 directionVector = start - end;
            int attackRange = curEnemy.currentItems[curEnemy.selectedItemIndex].range;
            Vector2 result = ((attackRange - 2) * (Vector2)Vector3.Normalize(directionVector)) + end; // have to cast to vector2

            // path to chosen location
            PathEnemyToLocation(Mathf.RoundToInt(result.x), Mathf.RoundToInt(result.y), curEnemy, curEnemyObject);

        } else {
            // wander; player not in view range

            // move towards current path. if no current path, pick random point nearby to patrol to
            if (curEnemy.currentPath == null) {
                // no path right now, so lets pick a point to path to
                // generate initital random point
                int x = 0;
                int y = 0;

                // generate end path destinations until one is valid
                // destination cannot be current tile
                while (!map.UnitCanEnterTile(x, y) || (curEnemy.currentX == x && curEnemy.currentY == y)) {
                    // pick new point and try again
                    x = Mathf.Clamp(curEnemy.currentX + Random.Range(-5, 6), map.oceanSize, gameManager.mapSize - map.oceanSize);
                    y = Mathf.Clamp(curEnemy.currentY + Random.Range(-5, 6), map.oceanSize, gameManager.mapSize - map.oceanSize);
                }
                // create path to location
                PathEnemyToLocation(x, y, curEnemy, curEnemyObject);
            }
        }

        // move until out of ap
        TakeEnemyMovement(curEnemy, curEnemyObject);

        // update enemy stats at end of turn
        curEnemy.FinishTurn();
    }

    // enemy pathfinding methods - these are a little different from player ones
    // because they dont have to draw any visuals

    // advance pathfinding by one tile
    private void AdvanceEnemyPathing(Character enemy, GameObject enemyObject) {
        if (enemy.currentPath == null || enemy.AP <= 0) {
            return;
        }

        // Subtract the cost of moving
        enemy.AP -= map.CostToEnterTile(enemy.currentPath[1].x, enemy.currentPath[1].y);

        // set current tile's current character to null
        map.tilesObjects[enemy.currentPath[0].x, enemy.currentPath[0].y].GetComponent<ClickableTile>().currentCharacterOnTile = null;
        // set next tile's current character to current enemy
        map.tilesObjects[enemy.currentPath[1].x, enemy.currentPath[1].y].GetComponent<ClickableTile>().currentCharacterOnTile = enemyObject;

        // Move us to the next tile in the sequence
        enemy.currentX = enemy.currentPath[1].x;
        enemy.currentY = enemy.currentPath[1].y;
        // teleport enemy object to end position
        enemyObject.transform.position = TileMap.TileCoordToWorldCoord(enemy.currentX, enemy.currentY, 0.125f);

        // Remove the old current tile from the pathfinding list
        enemy.currentPath.RemoveAt(0);

        if (enemy.currentPath.Count == 1) {
            // We only have one tile left in the path, and that tile MUST be our ultimate
            // destination -- and we are standing on it!
            // So let's just clear our pathfinding info.
            enemy.currentPath = null;
        }
    }

    private void PathEnemyToLocation(int x, int y, Character enemy, GameObject enemyObject) {
        if (!map.tileTypes[map.tiles[x, y]].isWalkable || (enemy.currentX == x && enemy.currentY == y)) {
            return;
        }

        // [currentX, currentY] is the current position of the unit
        // [x, y] is the target position, which was just clicked on
        enemy.currentPath = map.GeneratePathTo(enemy.currentX, enemy.currentY, x, y); // generate path
    }

    // wrap up any outstanding movement in the current path.
    private void TakeEnemyMovement(Character enemy, GameObject enemyObject) {
        // check if movement is blocked by someone or something new
        if (enemy.currentPath != null) {
            if (enemy.currentPath.Count > 0 && !map.UnitCanEnterTile(enemy.currentPath[1].x, enemy.currentPath[1].y)) {
                // there's somebody on the next tile we're trying to move to!
                // therefore set path to null for now
                enemy.currentPath = null;
                return;
            }

            while (enemy.currentPath != null &&
                   enemy.AP - map.CostToEnterTile(enemy.currentPath[1].x, enemy.currentPath[1].y) >= 0) {
                // check if player is in attack range
                AdvanceEnemyPathing(enemy, enemyObject);

                // after moving, check if player is now in range and if so then attack them
                if (IsPlayerInRange(enemy, enemy.currentItems[enemy.selectedItemIndex].range) != null) {
                    // player is in attack range, so lets check if we can attack them
                    while (enemy.AP - enemy.currentItems[enemy.selectedItemIndex].APcost >= 0) {
                        gameManager.AttackPlayer(enemy);
                    }
                }
            }
        }
    }
}
