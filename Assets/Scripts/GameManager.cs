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
	[HideInInspector] public PlayerManager playerManager;
	[HideInInspector] public EnemyManager enemyManager;
	[HideInInspector] public CameraController camController;

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

	[HideInInspector] public TileMap tileMap;

	// the class the player has chosen
	[HideInInspector] public Character characterClass; // player character

	// misc
	[HideInInspector] public bool pauseMenuEnabled;

	void Start() {
		// Make this game object persistant throughout scenes
		DontDestroyOnLoad(gameObject);

		assetHandler = GameObject.Find("AssetHandler").GetComponent<AssetHandler>();

		Cursor.SetCursor(assetHandler.defaultCursorTexture, Vector2.zero, CursorMode.Auto);
	}

	public void StartNewGame() {
		// we have to wait a sec for some reason
		Invoke("InitiateGameSession", .1f);
    }

	// This function is called when a new game is created
	public void InitiateGameSession() {
		// initiate ui manager
		uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
		uiManager.gameManager = this;

		// initiate enemy manager
		enemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
		enemyManager.gameManager = this;

		// Initiate all the tile map stuff
		tileMap = GameObject.Find("TileMapController").GetComponent<TileMap>();
		tileMap.gameManager = this;
		tileMap.InitiateTileMap();
		
		// Create and spawn the player's character
		CreatePlayerCharacter();

		enemyManager.playerManager = playerManager;

		// spawn enemies
		for (int i = 0; i < 25; i++) {
			enemyManager.SpawnEnemy(0);
		}
		enemyManager.UpdateEnemiesVisibility();

		// create camera controller
		camController = GameObject.Find("Main Camera").AddComponent<CameraController>();
		camController.gameManager = this;
		// set camera variables
		camController.CamTargetSpeed = 7;
		camController.CamTargetRadius = 3.7f;
		camController.ObjectCamTargetMaxSpeed = 4;
		camController.CamRotationSpeed = 40;
		camController.zoomSpeed = 0.2f;
		camController.camTargetDrag = 5;
		camController.maxZoom = 5;

		camController.InititateCamera();

		// snap to unit
		camController.ToggleSnapToUnit();
	}

	public void SetControlState(ControlState newCS) {
		TileMap.DestroyOutlines(playerManager.createdRangeOutlines);
		TileMap.DestroyOutlines(playerManager.createdMovementOutlines);
		// check which state was clicked
		characterClass.selectedItemIndex = -1;
		
		if (cs == newCS) {
			// deselect control state
			cs = ControlState.Deselected;
		} else if (newCS == ControlState.Move) {
			// switch to move state
			cs = ControlState.Move;
		} else if (newCS == ControlState.Item1) {
			// switch to item 1
			cs = ControlState.Item1;
			characterClass.selectedItemIndex = 0;
			playerManager.DrawTilesInRange();

			
		} else if (newCS == ControlState.Item2) {
			// switch to item 2
			cs = ControlState.Item2;
			characterClass.selectedItemIndex = 1;
			playerManager.DrawTilesInRange();
		}
		playerManager.DrawPossibleMovements();
	}

    // I seperated this class for easy viewing
    public void CreatePlayerCharacter() {
		// create player character
		characterClass = new Sharpshooter();

		// Create the player's unit model
		selectedUnit = Instantiate(assetHandler.ScoutPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		selectedUnit.tag = "Player";

		// Give the player's character a pathfinding script
		playerManager = selectedUnit.AddComponent<PlayerManager>();
		playerManager.gameManager = this;
		playerManager.map = tileMap;

		// Start at middle of map and keep moving until we find a walkable spawn
		int tempX = Mathf.FloorToInt(tileMap.mapSize / 2);
		int tempY = Mathf.FloorToInt(tileMap.mapSize / 2);
		while (!tileMap.tileTypes[tileMap.tiles[tempX, tempY]].isWalkable) {
			// Keep moving spawn position until spawn is walkable
			tempX += 1;
			tempY += 1;
		}
		playerManager.SpawnPlayer(tempX, tempY);

		// give player weapons for now
		characterClass.currentItems.Add(new AssaultRifle());
		characterClass.currentItems.Add(new SniperRifle());
	}

	// player ended their turn; on to the next
	// ALWAYS reference this version of the method, as it calls all the others!
	public void FinishTurn() {
		// finish up current turn
		// resolve the player's actions
		if (characterClass.AP > 0) {
			selectedUnit.GetComponent<PlayerManager>().TakeMovement();
		}
		characterClass.FinishTurn(); // update character stats

		// enemy turns

		enemyManager.ResolveAllEnemyTurns();
		enemyManager.UpdateEnemiesVisibility();

		// player has died
		if (characterClass.dead) {
			uiManager.ReturnToMainMenu();
        }

		// initiate new turn
		turnCount++;
		selectedUnit.GetComponent<PlayerManager>().DrawPossibleMovements();
		selectedUnit.GetComponent<PlayerManager>().DrawFogOfWar();
		playerManager.DrawPossibleMovements(); // update possible movements at start of new turn

	}

	// player attacking enemy
	// includes checks for current item held so you dont have to check when you call this function
	public void AttackEnemy(GameObject targetEnemyObject) {
		int damageAmount = 0; // 0 temporarily
		int enemyIndex = enemyManager.enemyGameObjects.IndexOf(targetEnemyObject);
		Character enemyCharacter = enemyManager.enemyList[enemyIndex];

		int selectedItemIndex = characterClass.selectedItemIndex;
		if (selectedItemIndex == -1) {
			// no item is actually even selected
			return;
        }

		// check if in range
		if (tileMap.IsTileInAttackRange(enemyCharacter.currentX, 
										enemyCharacter.currentY, 
										characterClass.currentX,
										characterClass.currentY,
										characterClass.currentItems[selectedItemIndex].range)) {
			// check if enough AP
			if (characterClass.AP - characterClass.currentItems[selectedItemIndex].APcost >= 0) {
				// use item 1
				damageAmount = characterClass.currentItems[selectedItemIndex].damage;
				characterClass.AP -= characterClass.currentItems[selectedItemIndex].APcost;
			} else {
				// not enough AP
				uiManager.SendMessageToLog("Not enough AP to attack enemy.");
				return;
			}
		} else {
			// out of range
			uiManager.SendMessageToLog("Enemy " + enemyCharacter.className + " out of attack range.");
			return;
		}

		// base message
		string message = "";

		// crit?
		int critChance = Random.Range(0, 100);
		if (critChance < 25 * characterClass.luckMultiplier) {
			damageAmount += Random.Range(1, 10);
			message = "Crit! ";
		}
		message = message + "Dealt " + damageAmount + " damage to enemy " + enemyManager.enemyList[enemyIndex].className + ".";
		// deal damage
		enemyCharacter.TakeDamage(damageAmount);

		// send message to log
		uiManager.SendMessageToLog(message);

		// update possible movements because you use AP when you attack
		playerManager.DrawPossibleMovements();
	}

	// enemy attacking player
	public void AttackPlayer(Character enemyCharacter) {
		int itemIndex = enemyCharacter.selectedItemIndex;
		int damageAmount = enemyCharacter.currentItems[itemIndex].damage;
		// check if enough AP 
		if (enemyCharacter.AP - enemyCharacter.currentItems[itemIndex].APcost >= 0) {
			// subtract ap cost of current weapon
			enemyCharacter.AP -= enemyCharacter.currentItems[itemIndex].APcost;

			string message = "";

			// crit?
			int critChance = Random.Range(0, 100);
			if (critChance < 25 * characterClass.luckMultiplier) {
				damageAmount += Random.Range(1, 10);
				message = "Crit! ";
			} 
			message = message + "Enemy " + enemyCharacter.className + " has dealt " + damageAmount + " damage to you.";
			// deal damage
			characterClass.TakeDamage(damageAmount);

			// send message to log
			uiManager.SendMessageToLog(message);
		}
	}

	void Update() {
		// update cursor based on game state
		if (cs == ControlState.Move) {
			// update cursor to move cursor
			Cursor.SetCursor(assetHandler.moveCursorTexture, new Vector2(assetHandler.moveCursorTexture.width / 2, assetHandler.moveCursorTexture.height / 2), CursorMode.Auto);
		} else if (cs == ControlState.Item1 || cs == ControlState.Item2) {
			// update cursor to attack cursor
			Cursor.SetCursor(assetHandler.attackCursorTexture, Vector2.zero, CursorMode.Auto);
		} else {
			// default cursor
			Cursor.SetCursor(assetHandler.defaultCursorTexture, Vector2.zero, CursorMode.Auto);
		}
	}

	// passthrough function
	public void SendMessageToLog(string message) {
		uiManager.SendMessageToLog(message);
	}
}
