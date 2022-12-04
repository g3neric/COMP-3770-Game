// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum PauseMenuState { Default, Controls, Settings };

public class UIManager : MonoBehaviour {
    [HideInInspector] public GameManager gameManager;

    [HideInInspector] public PauseMenuState currentPMState = PauseMenuState.Default;

    // UI text objects
    // bars
    public Slider HPBar;
    public Slider APBar;
    // misc info
    public TextMeshProUGUI turnCountText;
    public TextMeshProUGUI classNameText;
    public GameObject enemyUIObjectsPrefab;
    public GameObject messagePrefab;

    // enemy health bars
    [HideInInspector] public List<GameObject> enemyUIObjects = new List<GameObject>();

    // toolbarButtons[0] = move state
    // toolbarButtons[1] = item 1
    // toolbarButtons[2] = item 2
    public Button[] toolbarButtons = new Button[3];

    // currently selected item
    [HideInInspector] public Button selectedToolbarButton = null;

    // pauseMenuButtons[0] = return (unpause)
    // pauseMenuButtons[1] = controls
    // pauseMenuButtons[2] = settings
    // pauseMenuButtons[3] = exit to main menu
    public Button[] pauseMenuButtons = new Button[4];

    public Button controlMenuReturnButton;
    public Button settingsMenuReturnButton;

    public Button gameOverMenuButton;

    // menus
    public GameObject pauseMenu;
    public GameObject controlsMenu;
    public GameObject settingsMenu;
    public GameObject gameOverMenu;

    // log variables
    public GameObject messageLog;
    [HideInInspector] public List<Message> LogMessageList = new List<Message>();
    public const int maxMessages = 9;
    public const int messageLife = 30; // in seconds

    public Button buttonNextTurn;

    private ControlState previousCS;

    private void Start() {
        // default all menus to inactive
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        controlsMenu.SetActive(false);

        // Initiate next turn button
        buttonNextTurn.GetComponent<Button>().onClick.AddListener(delegate { gameManager.FinishTurn(); });

        // Initiate move state button
        toolbarButtons[0].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(0); });

        // Initiate item buttons
        toolbarButtons[1].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(1); });
        toolbarButtons[2].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(2); });

        // Initiate pause menu buttons
        pauseMenuButtons[1].GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Controls); });
        pauseMenuButtons[2].GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Settings); });

        // Initiate control menu button
        controlMenuReturnButton.GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Default); });

        // Initiate settings menu button
        settingsMenuReturnButton.GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Default); });
    }

    void LateUpdate() {
        if (gameManager != null) {
            // update text on the screen
            // HP bar
            HPBar.maxValue = gameManager.characterClass.maxHP;
            HPBar.value = gameManager.characterClass.HP;
            // text
            HPBar.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "HP: " + gameManager.characterClass.HP + "/" + gameManager.characterClass.maxHP;

            // AP bar
            APBar.maxValue = gameManager.characterClass.maxAP;
            APBar.value = gameManager.characterClass.AP;
            // text
            APBar.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "AP: " + gameManager.characterClass.AP + "/" + gameManager.characterClass.maxAP;

            // misc text
            turnCountText.text = "Turn " + gameManager.turnCount;
            classNameText.text = "Class: " + gameManager.characterClass.className;

            // update toolbar item names for now
            if (gameManager.characterClass.currentItems.Count == 1) {
                toolbarButtons[1].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = gameManager.characterClass.currentItems[0].name;
                toolbarButtons[2].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }

            if (gameManager.characterClass.currentItems.Count == 2) {
                toolbarButtons[1].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = gameManager.characterClass.currentItems[0].name;
                toolbarButtons[2].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = gameManager.characterClass.currentItems[1].name;
            }

            // button controls for the toolbar
            if (Input.GetKeyDown("1")) {
                ToolbarButtonSelected(0);

            } else if (Input.GetKeyDown("2")) {
                ToolbarButtonSelected(1);
            } else if (Input.GetKeyDown("3")) {
                ToolbarButtonSelected(2);
            }

            // pause game
            if (Input.GetKeyDown("escape")) {
                TogglePauseMenu();
            }

            // delete old messages
            if (LogMessageList.Count > maxMessages) {
                // delete last message
                Destroy(LogMessageList[0].messageObject); // delete game object
                LogMessageList.RemoveAt(0); // remove from list
            }

            // control transpareny of old messages
            if (LogMessageList.Count > 3) {
                Transform contentHolder = messageLog.transform.GetChild(0).GetChild(0);
                for (int i = 0; i < LogMessageList.Count - 3; i++) {
                    // log > viewport > content > child[i] is the desired text object
                    int newAlpha = Mathf.RoundToInt(255 - ((maxMessages - 3 - i) * 20));
                    Color32 newColor = new Color32(255, 255, 255, (byte)newAlpha);
                    contentHolder.GetChild(i).GetComponent<TextMeshProUGUI>().color = newColor;
                }
            }

            // update enemy UI objects
            for (int i = 0; i < enemyUIObjects.Count; i++) {
                // update position
                Vector3 enemyPos = gameManager.enemyManager.enemyGameObjects[i].transform.position;
                enemyPos += new Vector3(0, 0.5f, 0); // make them appear above enemy
                Vector2 newPos = Camera.main.WorldToScreenPoint(enemyPos);
                enemyUIObjects[i].transform.position = newPos;

                // update scale
                float dist = Vector3.Distance(Camera.main.transform.position, enemyPos);
                float scale = Mathf.Clamp(1 - dist / (dist + 100) * 3, 0.5f, 1);
                enemyUIObjects[i].transform.localScale = new Vector3(scale, scale, scale);

                // update health bar value
                enemyUIObjects[i].transform.GetChild(1).GetComponent<Slider>().value = gameManager.enemyManager.enemyList[i].HP;
            }
        }
    }

    // enemy UI objects
    public void CreateEnemyUIObjects(Character enemyCharacter) {
        GameObject newUIObjects = Instantiate(enemyUIObjectsPrefab, GameObject.Find("Dynamic UI Objects").transform);
        newUIObjects.name = enemyCharacter.className + " UI objects";
        enemyUIObjects.Add(newUIObjects);

        // set text
        newUIObjects.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = enemyCharacter.className;

        // set max value of heatlh bar
        newUIObjects.transform.GetChild(1).GetComponent<Slider>().maxValue = enemyCharacter.maxHP;
    }

    public void SetVisibilityOfEnemyUIObject(int enemyIndex, bool state) {
        if (state) {
            // make UI objects fully visible
            enemyUIObjects[enemyIndex].transform.GetChild(0).gameObject.SetActive(true);
            enemyUIObjects[enemyIndex].transform.GetChild(1).gameObject.SetActive(true);
        } else {
            // make UI objects fully transparent
            enemyUIObjects[enemyIndex].transform.GetChild(0).gameObject.SetActive(false);
            enemyUIObjects[enemyIndex].transform.GetChild(1).gameObject.SetActive(false);
        }
        
    }

    // delete enemy ui objects when the enemy dies
    public void RemoveEnemyUIObject(int enemyIndex) {
        Destroy(enemyUIObjects[enemyIndex]);
        enemyUIObjects.RemoveAt(enemyIndex);
    }

    // fixed update for consistent time keeping
    // fixed time step is 0.02, so this is run 50 times every second
    private void FixedUpdate() {
        // update message times
        if (LogMessageList.Count != 0) {
            for (int i = 0; i < LogMessageList.Count; i++) {
                if (LogMessageList[i].messageTime >= messageLife * 50 &&
                    LogMessageList[i].messageTurnTime < gameManager.turnCount - 1) { // fixed time step is 0.02, so each timing will be increased by one 50 times a second
                    // message has been alive too long, so destroy it
                    
                    LogMessageList.RemoveAt(i);
                    Destroy(LogMessageList[i].messageObject);
                } else {
                    LogMessageList[i].messageTime += 1;
                }
            }
        }
    }

    // Toggle pause menu
    public void TogglePauseMenu() {
        if (gameManager.pauseMenuEnabled) {
            pauseMenu.SetActive(false);
            controlsMenu.SetActive(false); 
            settingsMenu.SetActive(false);
            gameManager.pauseMenuEnabled = false;
            gameManager.SetControlState(previousCS);
        } else if (!gameManager.pauseMenuEnabled) {
            pauseMenu.SetActive(true);
            gameManager.pauseMenuEnabled = true;
            previousCS = gameManager.cs;
            gameManager.SetControlState(ControlState.Deselected);
        }
    }

    // switch to different panel in the pause menu
    public void SwitchPauseMenuPanel(PauseMenuState newState) {
        // make sure pause menu is enabled before doing anything
        if (gameManager.pauseMenuEnabled) {
            pauseMenu.SetActive(false);
            switch (newState) {
                // open controls panel
                case PauseMenuState.Controls:
                    controlsMenu.SetActive(true);
                    break;
                // open settings panel
                case PauseMenuState.Settings:
                    settingsMenu.SetActive(true);
                    break;
                // return to pause menu
                default:
                    pauseMenu.SetActive(true);
                    controlsMenu.SetActive(false);
                    settingsMenu.SetActive(false);
                    break;
            }
            currentPMState = newState;
        }
    }

    // create new message
    public void SendMessageToLog(string message) {
        string text = "[" + gameManager.turnCount + "] " + message;

        // create the message's object
        GameObject messageObject = Instantiate(messagePrefab, GameObject.Find("Content").transform);
        // set message's text
        messageObject.GetComponent<TextMeshProUGUI>().text = text;

        // create new message
        Message newMessage = new Message(gameManager.turnCount, messageObject);
        
        LogMessageList.Add(newMessage); // add new message to list
    }

    // handle selection of toolbar items
    public void ToolbarButtonSelected(int selectedButtonIndex) {
        // change control state in game manager
        if (selectedButtonIndex == 0) {
            gameManager.SetControlState(ControlState.Move);
        } else if (selectedButtonIndex == 1) {
            if (gameManager.characterClass.currentItems.Count >= 1) {
                gameManager.SetControlState(ControlState.Item1);
            } else {
                gameManager.SetControlState(ControlState.Deselected);
            }
        } else if (selectedButtonIndex == 2) {
            if (gameManager.characterClass.currentItems.Count >= 2) {
                gameManager.SetControlState(ControlState.Item2);
            } else {
                gameManager.SetControlState(ControlState.Deselected);
            }
        }

        Vector3 pos;
        // return all to default position
        for (int i = 0; i < toolbarButtons.Length; i++) {
            pos = toolbarButtons[i].GetComponent<RectTransform>().position;
            toolbarButtons[i].GetComponent<RectTransform>().position = new Vector3(pos.x, 5, pos.z);
        }

        if (toolbarButtons[selectedButtonIndex] != selectedToolbarButton) {
            // new button selected
            pos = toolbarButtons[selectedButtonIndex].GetComponent<RectTransform>().position;
            toolbarButtons[selectedButtonIndex].GetComponent<RectTransform>().position = new Vector3(pos.x, 25, pos.z);
            selectedToolbarButton = toolbarButtons[selectedButtonIndex];
        } else {
            // same button selected (deselect)
            selectedToolbarButton = null;
        }
    }

    public void GameOverMenu(int damageAmount, Character enemyCharacter) {
        gameOverMenu.SetActive(true);
        gameManager.pauseMenuEnabled = true;
        gameManager.SetControlState(ControlState.Deselected);
        string text = "Enemy " + enemyCharacter.className + " killed you with a " + damageAmount +
                      "\ndamage shot. GG.\n";

        if (gameManager.characterClass.killCount == 1) {
            text = text + "\nYou killed 1 enemy.";
        } else {
            text = text + "\nYou killed " + gameManager.characterClass.killCount + " enemies.";
        }
                      
        gameOverMenu.transform.GetChild(1).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void ReturnToMainMenu() {
        gameManager.SetControlState(ControlState.Deselected);
        SceneManager.LoadScene("MainMenu");
    }
}
