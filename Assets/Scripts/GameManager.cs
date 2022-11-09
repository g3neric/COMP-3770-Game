// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlState { Move, Item1, Item2, Deselected };

public class GameManager : MonoBehaviour {

	// I KNOW THIS IS A LIL MESSY I'LL CLEAN IT UP LATER

	[HideInInspector] public GameObject selectedUnit; // currently selected unit

	[HideInInspector] public int turnCount = 1;

	[HideInInspector] public ControlState cs = ControlState.Deselected;

	// Prefabs
	[Space]
	[Header("Prefabs")]
	[Space]
	public GameObject characterModel;
	public GameObject linePrefab;
	public GameObject tileOutlinePrefab;
	public GameObject tileHoverOutlinePrefab;
	public GameObject[] tilePossibleMovementOutlinePrefabs;
	public GameObject[] rangeOutlinePrefabs;

	// fog of war outline
	public Material fogOfWarOutlineMaterial;

	// scene object references
	[Space]
	[Header("Scene object references")]
	[Space]
	
	public GameObject Camera; // camera
	public GameObject tileMapController; // can't reference just components so i have to reference the game object first >:(
	[HideInInspector] public TileMap tileMap;

	// character class containing all the character's stats
	[HideInInspector] public Character characterClass = new Character(); // player character
	[HideInInspector] public List<Character> otherCharacters; // NPCs

	[HideInInspector] public bool pauseMenuEnabled;

	[HideInInspector] UnitPathfinding unitPathfinding;

	void Start() {
		InitiateGameSession();
	}

	// This function is called when a new game is created
	public void InitiateGameSession() {
		// initiate controller variables 
		tileMap = tileMapController.GetComponent<TileMap>();
		tileMap.gameManager = this;

		// Make this game object persistant throughout scenes
		DontDestroyOnLoad(gameObject);

		// Initiate all the tile map stuff
		tileMap.InitiateTileMap();

		// Create and spawn the player's character
		CreatePlayerCharacter();

		Camera.GetComponent<CameraController>().ToggleSnapToUnit();
	}

	public void SetControlState(ControlState newCS) {
		unitPathfinding.DestroyOutlines(unitPathfinding.createdRangeOutlines);
		unitPathfinding.DestroyOutlines(unitPathfinding.createdMovementOutlines);
		// check which state was clicked
		if (cs == newCS) {
			// deselect control state
			cs = ControlState.Deselected;
		} else if (newCS == ControlState.Move) {
			// switch to move state
			cs = ControlState.Move;
		} else if (newCS == ControlState.Item1) {
			// switch to item 1
			cs = ControlState.Item1;
			unitPathfinding.DrawTilesInRange();
		} else if (newCS == ControlState.Item2) {
			// switch to item 2
			cs = ControlState.Item2;
			unitPathfinding.DrawTilesInRange();
		}
		unitPathfinding.DrawPossibleMovements();
	}

	// I seperated this class for easy viewing
	public void CreatePlayerCharacter() {
		// Later on, when we get a menu, then "Character" below will be replaced 
		// by the player's chosen class which is inherited from the Character class
		characterClass.characterPrefab = characterModel; 

		// Create the player's unit model
		selectedUnit = Instantiate(characterClass.characterPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);

		// Give the player's character a pathfinding script
		unitPathfinding = selectedUnit.AddComponent<UnitPathfinding>();
		unitPathfinding.gameManager = this;
		unitPathfinding.map = tileMap;

		// Temporary dev stuff
		// These will be controlled by the class the player chooses later on
		characterClass.AP = 6;
		characterClass.maxAP = 6;
		characterClass.viewRange = 15;
		characterClass.attackRange = 7;

		// Check if chosen position is walkable
		int tempX = Mathf.FloorToInt(tileMap.mapSize / 2);
		int tempY = Mathf.FloorToInt(tileMap.mapSize / 2);
		while (!tileMap.tileTypes[tileMap.tiles[tempX, tempY]].isWalkable) {
			// Keep moving spawn position until spawn is walkable
			tempX += 1;
			tempY += 1;
		}
		unitPathfinding.SpawnPlayer(tempX, tempY);
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
		selectedUnit.GetComponent<UnitPathfinding>().DrawFogOfWar();

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
