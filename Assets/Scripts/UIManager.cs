// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PauseMenuState { Default, Controls, Settings };

public class UIManager : MonoBehaviour {
    [HideInInspector] public GameManager gameManager;

    [HideInInspector] public PauseMenuState currentPMState = PauseMenuState.Default;

    // UI text objects
    public TextMeshProUGUI movementText;
    public TextMeshProUGUI turnCountText;
    public TextMeshProUGUI classNameText;
    public GameObject messagePrefab;

    // toolbarButtons[0] = move state
    // toolbarButtons[1] = item 1
    // toolbarButtons[2] = item 2
    public Button[] toolbarButtons = new Button[3];

    // pauseMenuButtons[0] = return (unpause)
    // pauseMenuButtons[1] = controls
    // pauseMenuButtons[2] = settings
    // pauseMenuButtons[3] = exit to main menu
    public Button[] pauseMenuButtons = new Button[4];

    public Button controlMenuReturnButton;
    public Button settingsMenuReturnButton;

    // menus
    public GameObject pauseMenu;
    public GameObject controlsMenu;
    public GameObject settingsMenu;

    // currently selected item
    [HideInInspector] public Button selectedToolbarButton = null;

    // log variables
    public GameObject messageLog;
    [HideInInspector] public List<Message> LogMessageList = new List<Message>();
    public const int maxMessages = 9;
    public const int messageLife = 30; // in seconds

    public Button buttonNextTurn;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Initiate next turn button
        buttonNextTurn.GetComponent<Button>().onClick.AddListener(delegate { gameManager.FinishTurn(); });

        // Initiate move state button
        toolbarButtons[0].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(0); });

        // Initiate item buttons
        toolbarButtons[1].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(1); });
        toolbarButtons[2].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(2); });

        // Initiate main menu buttons
        pauseMenuButtons[0].GetComponent<Button>().onClick.AddListener(delegate { TogglePauseMenu(); });
        pauseMenuButtons[1].GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Controls); });
        pauseMenuButtons[2].GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Settings); });
        pauseMenuButtons[3].GetComponent<Button>().onClick.AddListener(delegate { CloseGame(); }); // for now, just close the game

        // Initiate control menu button
        controlMenuReturnButton.GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Default); });

        // Initiate settings menu button
        settingsMenuReturnButton.GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Default); });
    }

    void LateUpdate() {
        // update text on the screen
        movementText.text = "AP left: " + gameManager.characterClass.AP;
        turnCountText.text = "Turn " + gameManager.turnCount;
        classNameText.text = "Class: " + gameManager.characterClass.className;

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
        
        // control transparency of background
        if (LogMessageList.Count == 0) {
            // no messages in log, therefore disable the background
            messageLog.GetComponent<Image>().enabled = false;
        } else {
            // there are messages in the log, so enable the background
            messageLog.GetComponent<Image>().enabled = true;
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
    }

    private void FixedUpdate() {
        // update message times
        if (LogMessageList.Count != 0) {
            for (int i = 0; i < LogMessageList.Count; i++) {
                if (LogMessageList[i].messageTime >= messageLife * 50 &&
                    LogMessageList[i].messageTurnTime < gameManager.turnCount - 1) { // fixed time step is 0.02, so each timing will be increased by one 50 times a second
                    // message has been alive too long, so destroy it
                    
                    LogMessageList.RemoveAt(i);
                    Destroy(LogMessageList[i].messageObject);
                    LogMessageList.RemoveAt(i);
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

        } else if (!gameManager.pauseMenuEnabled) {
            pauseMenu.SetActive(true);
            gameManager.pauseMenuEnabled = true;
        }
    }

    // exit the game completely
    public void CloseGame() {
        Application.Quit();
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
            gameManager.SetControlState(ControlState.Item1);
        } else if (selectedButtonIndex == 2) {
            gameManager.SetControlState(ControlState.Item2);
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
}
