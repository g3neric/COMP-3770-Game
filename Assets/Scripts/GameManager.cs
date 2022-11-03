using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	// I KNOW THIS IS A LIL MESSY I'LL CLEAN IT UP LATER

	[HideInInspector] public GameObject selectedUnit; // currently selected unit

	[HideInInspector] public int turnCount = 1;

	// Prefabs
	[Space]
	[Header("Prefabs")]
	[Space]
	public GameObject characterModel;
	public GameObject linePrefab;
	public GameObject tileOutlinePrefab;
	public GameObject tileHoverOutlinePrefab;
	public GameObject tilePossibleMovementOutlinePrefab;
	// scene object references
	[Space]
	[Header("Scene object references")]
	[Space]
	public Button button;
	public GameObject Camera; // camera
	public GameObject tileMapController; // can't reference just components so i have to reference the game object first >:(
	[HideInInspector] public TileMap tileMap;

	// character class containing all the character's stats
	[HideInInspector] public Character characterClass = new Character(); // player character
	[HideInInspector] public List<Character> otherCharacters; // NPCs

	// This is only temporarily a Start() function.
	// Later on, this method will be called when a
	// new game is started from the main menu.
	void Start() {
		// initiate controller variables 
		tileMap = tileMapController.GetComponent<TileMap>();
		tileMap.gameManager = this;

		// Make this game object persistant throughout scenes
		DontDestroyOnLoad(gameObject);

		// Initiate all the tile map stuff
		tileMap.InitiateTileMap();

		// Create and spawn the player's character
		CreatePlayerCharacter();

		// Temporary dev stuff
		// These will be controlled by the class the player chooses later on
		characterClass.AP = 3;
		characterClass.maxAP = 3;

		
	}

	// I seperated this class for easy viewing
	public void CreatePlayerCharacter() {
		// Later on, when we get a menu, then "Character" below will be replaced 
		// by the player's chosen class which is inherited from the Character class
		characterClass.characterPrefab = characterModel; 

		// Create the player's unit model
		selectedUnit = Instantiate(characterClass.characterPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);

		// Give the player's character a pathfinding script
		UnitPathfinding unitPathfinding = selectedUnit.AddComponent<UnitPathfinding>();
		unitPathfinding.gameManager = this;
		unitPathfinding.map = tileMap;

		// Check if chosen position is walkable
		int tempX = Mathf.FloorToInt(tileMap.mapSize / 2);
		int tempY = Mathf.FloorToInt(tileMap.mapSize / 2);
		while (!tileMap.tileTypes[tileMap.tiles[tempX, tempY]].isWalkable) {
			// Keep moving spawn position until spawn is walkable
			tempX += 1;
			tempY += 1;
		}
		unitPathfinding.SpawnPlayer(tempX, tempY);

		// Initiate next turn button
		button.GetComponent<Button>().onClick.AddListener(delegate { FinishTurn(); });
	}

	// player ended their turn; on to the next
	// ALWAYS reference this version of the method, as it calls all the others!
	public void FinishTurn() {
		// resolve the player's actions first
		if (characterClass.AP > 0) {
			selectedUnit.GetComponent<UnitPathfinding>().TakeMovement();
		}
		characterClass.FinishTurn(); // update character stats
		selectedUnit.GetComponent<UnitPathfinding>().DrawPossibleMovements();

		// resolve NPC actions
		for (int i = 0; i < otherCharacters.Count; i++) {
			// complete NPC actions here
        }
		turnCount++;
    }
	void Update() {
		if (Input.GetKeyDown("e")) {
			FinishTurn();
		}
	}
}
