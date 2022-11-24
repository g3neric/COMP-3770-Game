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
		assetHandler = GameObject.Find("AssetHandler").GetComponent<AssetHandler>();

		// default cursor
		Cursor.SetCursor(assetHandler.defaultCursorTexture, Vector2.zero, CursorMode.Auto);

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

		enemyManager.playerManager = playerManager;

		// spawn enemies

		for (int i = 0; i < 25; i++) {
			enemyManager.SpawnEnemy(0);
		}
		enemyManager.UpdateEnemiesVisibility();
	}

	public void SetControlState(ControlState newCS) {
		TileMap.DestroyOutlines(playerManager.createdRangeOutlines);
		TileMap.DestroyOutlines(playerManager.createdMovementOutlines);
		// check which state was clicked
		characterClass.selectedItemIndex = -1;
		// default cursor
		Cursor.SetCursor(assetHandler.defaultCursorTexture, Vector2.zero, CursorMode.Auto);
		if (cs == newCS) {
			// deselect control state
			cs = ControlState.Deselected;
		} else if (newCS == ControlState.Move) {
			// switch to move state
			cs = ControlState.Move;

			// update cursor to move cursor
			Cursor.SetCursor(assetHandler.moveCursorTexture, new Vector2(assetHandler.moveCursorTexture.width/2, assetHandler.moveCursorTexture.height / 2), CursorMode.Auto);
		} else if (newCS == ControlState.Item1) {
			// switch to item 1
			cs = ControlState.Item1;
			characterClass.selectedItemIndex = 0;
			playerManager.DrawTilesInRange();

			// update cursor to attack cursor
			Cursor.SetCursor(assetHandler.attackCursorTexture, Vector2.zero, CursorMode.Auto);
		} else if (newCS == ControlState.Item2) {
			// switch to item 2
			cs = ControlState.Item2;
			characterClass.selectedItemIndex = 1;
			playerManager.DrawTilesInRange();

			// update cursor to attack cursor
			Cursor.SetCursor(assetHandler.attackCursorTexture, Vector2.zero, CursorMode.Auto);
		}
		playerManager.DrawPossibleMovements();
	}

	// I seperated this class for easy viewing
	public void CreatePlayerCharacter() {
		// create player character
		characterClass = new Scout();

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
			CloseGame();
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
				enemyCharacter.TakeDamage(damageAmount);
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

		string message = "Dealt " + damageAmount + " damage to enemy " + enemyManager.enemyList[enemyIndex].className + ".";
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
			characterClass.TakeDamage(damageAmount);
			enemyCharacter.AP -= enemyCharacter.currentItems[itemIndex].APcost;

			string message = "Enemy " + enemyCharacter.className + " has dealt " + damageAmount + " damage to you.";
			uiManager.SendMessageToLog(message);
		}
	}

	void Update() {
		if (Input.GetKeyDown("e")) {
			FinishTurn();
		}
	}

	// passthrough function
	public void SendMessageToLog(string message) {
		uiManager.SendMessageToLog(message);
	}

	// exit the game completely
	public void CloseGame() {
		Application.Quit();
	}
}
