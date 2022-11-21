// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemySpawnManager : MonoBehaviour {
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

    private void Awake() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        map = tileMapControllerObject.GetComponent<TileMap>();
    }

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
    public void SpawnEnemy(int prefabIndex) {
        // iterate over every tile over a rough approximation of the island size
        int startingX = Random.Range(map.oceanSize, map.mapSize - map.oceanSize);
        int startingY = Random.Range(map.oceanSize, map.mapSize - map.oceanSize);
        for (int x = startingX; x <= map.mapSize - map.oceanSize; x++) {
            for (int y = startingY; y <= map.mapSize - map.oceanSize; y++) {
                // check if current tile is walkable
                Assert.IsTrue(map != null); // temporary
                // check if current tile is not viewable by the player and is walkable
                if (map.tileTypes[map.tiles[x, y]].isWalkable && !map.IsTileVisibleToPlayer(x, y)) {
                    // spawn enemy at (x, y)
                    // instantiate enemy
                    GameObject enemyObject = Instantiate(enemyPrefabs[prefabIndex], TileMap.TileCoordToWorldCoord(x, y, 0.125f), Quaternion.identity);

                    // add to list of all enemy objects
                    enemyGameObjects.Add(enemyObject);

                    // set reference to enemy in the tile its on
                    map.tilesObjects[x, y].GetComponent<ClickableTile>().currentCharacterOnTile = enemyObject;

                    // create enemy character
                    Character enemyCharacter = new Tank();
                    enemyList.Add(enemyCharacter);
                    return;
                }
            }
        }
    }
}
