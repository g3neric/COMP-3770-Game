// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// state enums
public enum ControlState { Move, Item1, Item2, Deselected };
public enum DifficultyState { Ez, Mid, Impossible };

public class GameManager : MonoBehaviour {
	// I KNOW THIS IS A LIL MESSY I'LL CLEAN IT UP LATER

	// link to asset handler
	private AssetHandler assetHandler;
	[HideInInspector] public UIManager uiManager;
	[HideInInspector] public UnitPathfinding unitPathfinding;
	[HideInInspector] public EnemyManager enemyManager;

	public GameObject selectedUnit; // currently selected unit

	[HideInInspector] public int turnCount = 1;

	[HideInInspector] public ControlState cs = ControlState.Deselected;
	[HideInInspector] public DifficultyState difficulty = DifficultyState.Mid; // default to mid

	// Prefabs
	[Space]
	[Header("Prefabs")]
	[Space]

	// scene object references
	[Space]
	[Header("Scene object references")]
	[Space]
	
	public GameObject Camera; // camera
	public GameObject tileMapController; // can't reference just components so i have to reference the game object first >:(
	[HideInInspector] public TileMap tileMap;


	// the class the player has chosen
	[HideInInspector] public Character characterClass; // player character

	// misc
	[HideInInspector] public bool pauseMenuEnabled;

	

	void Start() {
		// initiate manager script references
		tileMap = tileMapController.GetComponent<TileMap>();
		tileMap.gameManager = this;
		enemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
		uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();

		// reference to asset handler
		assetHandler = GameObject.Find("AssetHandler").GetComponent<AssetHandler>();

		InitiateGameSession();
	}

	// This function is called when a new game is created
	public void InitiateGameSession() {
		// Make this game object persistant throughout scenes
		DontDestroyOnLoad(gameObject);

		// Initiate all the tile map stuff
		tileMap.InitiateTileMap();

		// Create and spawn the player's character
		CreatePlayerCharacter();

		Camera.GetComponent<CameraController>().ToggleSnapToUnit();

		enemyManager.unitPathfinding = unitPathfinding;

		// spawn enemies
		
		for (int i = 0; i < 10; i++) {
			enemyManager.SpawnEnemy(0);
		}
		enemyManager.UpdateEnemiesVisibility();
	}

	public void SetControlState(ControlState newCS) {
		TileMap.DestroyOutlines(unitPathfinding.createdRangeOutlines);
		TileMap.DestroyOutlines(unitPathfinding.createdMovementOutlines);
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
		// create player character
		characterClass = new Scout();

		// Create the player's unit model
		selectedUnit = Instantiate(assetHandler.ScoutPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		selectedUnit.tag = "Player";

		// Give the player's character a pathfinding script
		unitPathfinding = selectedUnit.AddComponent<UnitPathfinding>();
		unitPathfinding.gameManager = this;
		unitPathfinding.map = tileMap;

		// Start at middle of map and keep moving until we find a walkable spawn
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
		// finish up current turn
		// resolve the player's actions
		if (characterClass.AP > 0) {
			selectedUnit.GetComponent<UnitPathfinding>().TakeMovement();
		}
		characterClass.FinishTurn(); // update character stats

		// enemy turns

		enemyManager.ResolveAllEnemyTurns();
		enemyManager.UpdateEnemiesVisibility();

		// initiate new turn
		turnCount++;
		selectedUnit.GetComponent<UnitPathfinding>().DrawPossibleMovements();
		selectedUnit.GetComponent<UnitPathfinding>().DrawFogOfWar();
		unitPathfinding.DrawPossibleMovements(); // update possible movements at start of new turn

	}

	void Update() {
		if (Input.GetKeyDown("e")) {
			FinishTurn();
		} else if (Input.GetKeyDown("r")) {
			SendMessageToLog("test message " + Random.Range(0, 1000));
        }
	}

	// passthrough function
	public void SendMessageToLog(string message) {
		uiManager.SendMessageToLog(message);
	}
}
