// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

// state enums
public enum ControlState { Move, Item1, Item2, Deselected };
public enum DifficultyState { Ez, Mid, Impossible };

public enum BiomeSetting { Default, Hilly, Superflat, Mountaineous, Swampland };

public class GameManager : MonoBehaviour {
	// I KNOW THIS IS A LIL MESSY I'LL CLEAN IT UP LATER

	// constants
	// these are base %, joker is 3x chance
	[HideInInspector] public const int critChance = 25;
	[HideInInspector] public const int epicCritChance = 3;

	// link to asset handler
	[HideInInspector] public AssetHandler assetHandler;
	[HideInInspector] public SoundManager soundManager;
	[HideInInspector] public UIManager uiManager;
	[HideInInspector] public PlayerManager playerManager;
	[HideInInspector] public EnemyManager enemyManager;
	[HideInInspector] public ShopManager shopManager;
	[HideInInspector] public CameraController camController;
	

	public GameObject selectedUnit; // currently selected unit

	[HideInInspector] public int turnCount = 1;

	// enums
	[HideInInspector] public ControlState cs = ControlState.Deselected;
	[HideInInspector] public DifficultyState difficulty = DifficultyState.Mid; // default to mid
	[HideInInspector] public BiomeSetting biomeSetting;

	[HideInInspector] public int mapSize;

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
	[HideInInspector] private Character characterClass; // player character
	[HideInInspector] public int characterClassInt;

	// misc
	[HideInInspector] public bool pauseMenuEnabled = false;
	[HideInInspector] public bool shopMenuEnabled = false;

	void Awake() {
		cs = ControlState.Deselected;

		// Make this game object persistant throughout scenes
		DontDestroyOnLoad(gameObject);

		// initialize asset handler
		assetHandler = GameObject.Find("AssetHandler").GetComponent<AssetHandler>();
		DontDestroyOnLoad(assetHandler.gameObject);

		// initialize sound manager
		soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		DontDestroyOnLoad(soundManager.gameObject);
		soundManager.Initialize();

		soundManager.PlayMainMenuMusic();
		SceneManager.LoadScene("MainMenu");

		
	}

	public void StartNewGame() {
		// we have to wait a lil for some reason
		soundManager.PauseMusic();
		Invoke("InitiateGameSession", .05f);
    }

	// This function is called when a new game is created
	public void InitiateGameSession() {
		soundManager.PlayGameMusic();
		// initiate ui manager
		uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
		uiManager.gameManager = this;

		// initiate enemy manager
		enemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
		enemyManager.gameManager = this;

		// initiate shop manager
		// have to do it this way cuz it breaks if i dont create a new one each time lmao
		shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
		shopManager.gameManager = this;
		shopManager.shopList = new List<Shop>();

		// create player character before creating map
		characterClass = ClassIntToClass(characterClassInt);

		// Initiate all the tile map stuff
		tileMap = GameObject.Find("TileMapController").GetComponent<TileMap>();
		tileMap.gameManager = this;
		tileMap.InitiateTileMap();

		// Create and spawn the player's character
		CreatePlayerCharacter();

		// check if player spawned on shop tile
		uiManager.SetShopMenuButtonActive();

		enemyManager.playerManager = playerManager;

		// spawn enemies
		int enemyCount; // amount of enemies at start of game
		switch(difficulty) {
			case DifficultyState.Ez:
				enemyCount = 15;
				break;
			case DifficultyState.Mid:
				enemyCount = 25;
				break;
			case DifficultyState.Impossible:
				enemyCount = 35;
				break;
			default:
				// this section should never be reached
				print("error spawning enemies - difficulty state not set");
				return;
        }
		for (int i = 0; i < enemyCount; i++) {
			enemyManager.SpawnEnemy(0);
		}
		enemyManager.UpdateEnemiesVisibility();

		// reset pause bools
		pauseMenuEnabled = false;
		shopMenuEnabled = false;

		// create camera controller
		camController = GameObject.Find("Main Camera").AddComponent<CameraController>();
		camController.gameManager = this;
		// set camera variables
		camController.CamTargetSpeed = 7;
		camController.CamTargetRadius = 3.7f;
		camController.ObjectCamTargetMaxSpeed = 4;
		camController.CamRotationSpeed = 20;
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
		if (cs == newCS || newCS == ControlState.Deselected) {
			// deselect control state
			cs = ControlState.Deselected;
			selectedUnit.GetComponent<Animator>().SetBool("idleRifle", false);
		} else if (newCS == ControlState.Move) {
			// switch to move state
			cs = ControlState.Move;
			selectedUnit.GetComponent<Animator>().SetBool("idleRifle", false);
			// update outline
			playerManager.DrawPossibleMovements();
		} else if (newCS == ControlState.Item1) {
			// switch to item 1
			cs = ControlState.Item1;
			// update animation
			selectedUnit.GetComponent<Animator>().SetBool("idleRifle", true);
			// update selected item
			characterClass.selectedItemIndex = 0;
			// update outlines
			playerManager.DrawTilesInRange();
			playerManager.DrawPossibleMovements();
		} else if (newCS == ControlState.Item2) {
			// switch to item 2
			cs = ControlState.Item2;
			// update animation
			selectedUnit.GetComponent<Animator>().SetBool("idleRifle", true);
			// update selected item
			characterClass.selectedItemIndex = 1;
			// update outlines
			playerManager.DrawTilesInRange();
			playerManager.DrawPossibleMovements();
		}
	}

	// helper function to help with class selection
	// the dropdown gives int values, this converts that to an object of the correct type
	public static Character ClassIntToClass(int input) {
		switch (input) {
			case 0:
				return new Grunt();
			case 1:
				return new Engineer();
			case 2:
				return new Joker();
			case 3:
				return new Scout();
			case 4:
				return new Sharpshooter();
			case 5:
				return new Surgeon();
			case 6:
				return new Tank();
			default:
				return null;
		}
	}

    // I seperated this class for easy viewing
    public void CreatePlayerCharacter() {
		// Create the player's unit model
		selectedUnit = Instantiate(assetHandler.classPrefabs[characterClassInt], new Vector3(0f, 0f, 0f), Quaternion.identity);
		selectedUnit.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
		selectedUnit.tag = "Player";
		selectedUnit.GetComponent<Animator>().SetBool("idleRifle", false);

		// Give the player's character a pathfinding script
		playerManager = selectedUnit.AddComponent<PlayerManager>();
		playerManager.gameManager = this;
		playerManager.map = tileMap;

		// setup audio source
		var audioSource = selectedUnit.AddComponent<AudioSource>();
		audioSource.loop = false;

		// Start at middle of map and keep moving until we find a walkable spawn
		int tempX = Mathf.FloorToInt(mapSize / 2);
		int tempY = Mathf.FloorToInt(mapSize / 2);
		while (!tileMap.tileTypes[tileMap.tiles[tempX, tempY]].isWalkable) {
			// Keep moving spawn position until spawn is walkable
			tempX += 1;
			tempY += 1;
		}
		playerManager.SpawnPlayer(tempX, tempY);
	}

	// player ended their turn; on to the next
	// ALWAYS reference this version of the method, as it calls all the others!
	public void FinishTurn() {
		// finish up current turn
		// resolve the player's actions
		if (characterClass.AP > 0) {
			playerManager.TakeMovement();
		}

		// still have AP left
		// use it to regen extra HP
		int extraRegenAmount = 0;
		if (characterClass.AP > 0 && characterClass.HP < characterClass.maxHP) {
			if (characterClass.HP + characterClass.AP > characterClass.maxHP) {
				// would be regenerating over the max HP, so correct for this
				extraRegenAmount = Mathf.RoundToInt(characterClass.maxHP) - Mathf.RoundToInt(characterClass.HP);
			} else {
				// would not be regenerating over max HP, so don't correct
				extraRegenAmount = characterClass.AP;
			}
			
			characterClass.HP += extraRegenAmount;
			uiManager.SendMessageToLog("Used extra AP to regen an extra " + extraRegenAmount + " HP");
		}

		characterClass.FinishTurn(); // update character stats

		

		// enemy turns
		enemyManager.ResolveAllEnemyTurns();
		enemyManager.UpdateEnemiesVisibility();

		// determine chance of enemy spawning each turn based on difficulty
		int spawnChance;
		switch (difficulty) {
			case DifficultyState.Ez:
				spawnChance = 5;
				break;
			case DifficultyState.Mid:
				spawnChance = 7;
				break;
			case DifficultyState.Impossible:
				spawnChance = 10;
				break;
			default:
				return;
		}

		// roll dice to determine if enemy will spawn
		int spawnRandom = Random.Range(0, 100);
		if (spawnRandom < spawnChance) {
			enemyManager.SpawnEnemy(0);
        }

		// initiate new turn
		turnCount++;
		playerManager.DrawFogOfWar();
		// update possible movements at start of new turn if move state selected
		if (cs == ControlState.Move) {
			playerManager.DrawPossibleMovements();
		}
		

	}

	// player attacking enemy
	// includes checks for current item held so you dont have to check when you call this function
	public void AttackEnemy(GameObject targetEnemyObject) {
		float damageAmount = 0; // 0 temporarily
		int enemyIndex = enemyManager.enemyGameObjects.IndexOf(targetEnemyObject);
		Character enemyCharacter = enemyManager.enemyList[enemyIndex];

		int selectedItemIndex = characterClass.selectedItemIndex;
		Item currentItem = characterClass.currentItems[selectedItemIndex];
		// check if in range
		if (tileMap.IsTileInAttackRange(enemyCharacter.currentX, 
										enemyCharacter.currentY, 
										characterClass.currentX,
										characterClass.currentY,
										currentItem.range)) {
			// check if enough AP
			if (characterClass.AP - currentItem.APcost >= 0) {
				// use item 1
				damageAmount = currentItem.damage;
				characterClass.AP -= currentItem.APcost;
			} else {
				// not enough AP
				uiManager.SendMessageToLog("Not enough AP to attack enemy");
				return;
			}
		} else {
			// out of range
			uiManager.SendMessageToLog("Enemy " + enemyCharacter.className + " out of attack range");
			return;
		}

		// roll for accuracy
		int accuracyRoll = Random.Range(0, 100);
		// calculate distance between player and enemy
		float shotDistance = TileMap.DistanceBetweenTiles(characterClass.currentX,
														  characterClass.currentY,
														  enemyCharacter.currentX,
														  enemyCharacter.currentY);
		// lower character's accuracy the further away the enemy is
		if (accuracyRoll < characterClass.accuracy - (shotDistance * 2)) {
			// hit
			// base message
			string message = "";

			// crit?
			// 4% chance for crazy crit, 25% for regular crit
			// joker is 16% for crazy crit, 75% for regular crit
			int critChance = Random.Range(0, 100);
			if (critChance < epicCritChance * characterClass.luckMultiplier)
			{
				damageAmount += Random.Range(10, 30);
				message = "Epic crit! ";
			}
			else if (critChance < critChance * characterClass.luckMultiplier)
			{
				damageAmount += Random.Range(1, 10);
				message = "Crit! ";
			}
			message = message + "Dealt " + damageAmount + " damage to enemy " + enemyManager.enemyList[enemyIndex].className;
			// deal damage
			enemyCharacter.TakeDamage(damageAmount);

			// send damage message to log
			uiManager.SendMessageToLog(message);

			// check if player has incendiary rounds
			if (characterClass.incendiaryRounds)
			{
				if (!enemyCharacter.onFire)
				{
					uiManager.SendMessageToLog("Set enemy " + enemyCharacter.className + " on fire!");
				}
				enemyCharacter.SetOnFire();
				Instantiate(assetHandler.fireEffectsPrefab,
							targetEnemyObject.transform.position,
							Quaternion.identity,
							targetEnemyObject.transform);
			}

			// update animation
			selectedUnit.GetComponent<Animator>().SetTrigger("Shoot");

			if (enemyCharacter.dead)
			{
				// clean up enemy - delete it, reset its stats, etc.
				enemyManager.CleanUpEnemy(enemyManager.enemyList.IndexOf(enemyCharacter));

				// we killed the enemy pog
				characterClass.killCount++;
				int goldAmount = 0;

				// determine amount of gold to reward upon death
				switch (difficulty)
				{
					case DifficultyState.Ez:
						goldAmount = Random.Range(3, 10);
						break;
					case DifficultyState.Mid:
						goldAmount = Random.Range(5, 12);
						break;
					case DifficultyState.Impossible:
						goldAmount = Random.Range(7, 14);
						break;
				}
				uiManager.SendMessageToLog("Gained " + goldAmount + " gold");
				characterClass.gold += goldAmount;
			}
		} else {
			// miss
			uiManager.SendMessageToLog("Shot missed enemy at range " + shotDistance.ToString("F2") + " tiles");
		}

		// update possible movements because you use AP when you attack
		playerManager.DrawPossibleMovements();

		soundManager.PlayGunshot(currentItem.name);
	}

	// enemy attacking player
	public void AttackPlayer(Character enemyCharacter) {
		int itemIndex = enemyCharacter.selectedItemIndex;
		float damageAmount = enemyCharacter.currentItems[itemIndex].damage;
		// check if enough AP 
		if (enemyCharacter.AP - enemyCharacter.currentItems[itemIndex].APcost >= 0) {
			// subtract ap cost of current weapon
			enemyCharacter.AP -= enemyCharacter.currentItems[itemIndex].APcost;

			int accuracyRoll = Random.Range(0, 100);
			// calculate distance between player and enemy
			float shotDistance = TileMap.DistanceBetweenTiles(characterClass.currentX,
															  characterClass.currentY,
															  enemyCharacter.currentX,
															  enemyCharacter.currentY);
			if (accuracyRoll < (enemyCharacter.accuracy - shotDistance * 2)) {
				// hit
				string message = "";

				// crit?
				// 4% chance for crazy crit, 25% for regular crit
				// joker is 16% for crazy crit, 75% for regular crit
				int critChance = Random.Range(0, 100);
				if (critChance < epicCritChance * enemyCharacter.luckMultiplier)
				{
					damageAmount += Random.Range(10, 30);
					message = "Epic crit! ";
				}
				else if (critChance < critChance * enemyCharacter.luckMultiplier)
				{
					damageAmount += Random.Range(1, 10);
					message = "Crit! ";
				}
				message = message + "Enemy " + enemyCharacter.className + " has dealt " + damageAmount + " damage to you";
				// send message to log
				uiManager.SendMessageToLog(message);

				// deal damage
				characterClass.TakeDamage(damageAmount);
				if (characterClass.dead) {
					uiManager.SendMessageToLog("You have died");
					uiManager.GameOverMenu(damageAmount, enemyCharacter);
				}

				
			} else {
				// miss
				uiManager.SendMessageToLog("Enemy " + enemyCharacter.className + " missed shot at range " + shotDistance.ToString("F2") + " tiles");
			}
		}
	}

	void LateUpdate() {
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

	public Character GetCharacterClass() {
		if (SceneManager.GetActiveScene().name == "Game") {
			return characterClass;
        } else {
			characterClass = null;
			return null;
		}
    }
}
