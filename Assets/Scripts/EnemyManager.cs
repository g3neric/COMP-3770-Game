// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyManager : MonoBehaviour {
    // link to game manager
    [HideInInspector] private GameManager gameManager;

    // references set by game manager
    public GameObject tileMapControllerObject;
    [HideInInspector] public TileMap map;
    [HideInInspector] public UnitPathfinding unitPathfinding;

    // enemy prefabs
    public GameObject[] enemyPrefabs = new GameObject[1]; // 1 for now

    // list of enemy game objects
    [HideInInspector] public List<GameObject> enemyGameObjects = new List<GameObject>(); // up to maxEnemyCount

    // list of enemy objects
    [HideInInspector] public List<Character> enemyList = new List<Character>();

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        map = tileMapControllerObject.GetComponent<TileMap>();
    }

    // update enemie's visibility
    public void UpdateEnemiesVisibility() {
        // toggle enemy viewable
        foreach (GameObject i in enemyGameObjects) {
            if (map.IsTileVisibleToPlayer(Mathf.RoundToInt(i.transform.position.x), Mathf.RoundToInt(i.transform.position.z))) {
                i.GetComponent<MeshRenderer>().enabled = true;
            } else {
                i.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    // spawn a single enemy at a random pos
    public void SpawnEnemy(int enemyPrefabIndex) {
        // iterate over every tile over a rough approximation of the island size
        int startingX = Random.Range(map.oceanSize, map.mapSize - map.oceanSize);
        int startingY = Random.Range(map.oceanSize, map.mapSize - map.oceanSize);
        for (int x = startingX; x <= map.mapSize - map.oceanSize; x++) {
            for (int y = startingY; y <= map.mapSize - map.oceanSize; y++) {
                // check if current tile is not viewable by the player, is walkable, and nobody is on that tile rn
                if (map.UnitCanEnterTile(x, y) &&
                    !map.IsTileVisibleToPlayer(x, y)) {
                    // spawn enemy at (x, y)
                    // instantiate enemy
                    GameObject enemyObject = Instantiate(enemyPrefabs[enemyPrefabIndex], TileMap.TileCoordToWorldCoord(x, y, 0.125f), Quaternion.identity);

                    // set reference to enemy in the tile its on
                    map.tilesObjects[x, y].GetComponent<ClickableTile>().currentCharacterOnTile = enemyObject;

                    // create enemy character
                    Character enemyCharacter = new Tank(); // temporarily all tanks
                    // update target variables
                    enemyCharacter.currentX = x;
                    enemyCharacter.currentY = y;

                    // add enemy to lists
                    enemyList.Add(enemyCharacter);
                    enemyGameObjects.Add(enemyObject);

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

    // calculate enemy's movement for one turn
    private void ResolveEnemyTurn(int enemyIndex) {
        Character curEnemy = enemyList[enemyIndex];
        GameObject curEnemyObject = enemyGameObjects[enemyIndex];

        // create path if enemy doesnt have one
        if (curEnemy.currentPath == null) {
            // no path right now, so lets pick a point to path to
            // generate initital random point
            int x = 0;
            int y = 0;

            // generate end path destinations until one is valid
            // destination cannot be current tile
            while (!map.UnitCanEnterTile(x, y) || (curEnemy.currentX == x && curEnemy.currentY == y)) {
                // pick new point and try again
                x = Mathf.Clamp(curEnemy.currentX + Random.Range(-5, 6), map.oceanSize, map.mapSize - map.oceanSize);
                y = Mathf.Clamp(curEnemy.currentY + Random.Range(-5, 6), map.oceanSize, map.mapSize - map.oceanSize);
            }
            // create path to location
            PathEnemyToLocation(x, y, curEnemy, curEnemyObject);
        }

        // take movement until out of AP
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
        if (!map.UnitCanEnterTile(x, y) || (enemy.currentX == x && enemy.currentY == y)) {
            return;
        }

        // [currentX, currentY] is the current position of the unit
        // [x, y] is the target position, which was just clicked on
        enemy.currentPath = map.GeneratePathTo(enemy.currentX, enemy.currentY, x, y); // generate path
    }

    // wrap up any outstanding movement in the current path.
    private void TakeEnemyMovement(Character enemy, GameObject enemyObject) {
        // check if movement is blocked by someone or something new
        if (!map.UnitCanEnterTile(enemy.currentPath[1].x, enemy.currentPath[1].y)) {
            // there's somebody on the next tile we're trying to move to!
            // therefore set path to null for now
            print("there's somebody on the tile i'm tryna move to wtf");
            enemy.currentPath = null;
            return;
        }

        while (enemy.currentPath != null &&
               enemy.AP - map.CostToEnterTile(enemy.currentPath[1].x, enemy.currentPath[1].y) >= 0) {
            AdvanceEnemyPathing(enemy, enemyObject);
        }
    }
}
