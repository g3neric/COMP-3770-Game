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

    // UI objects
    public TextMeshProUGUI movementText;
    public TextMeshProUGUI turnCountText;
    public TextMeshProUGUI classNameText;

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

    public Button buttonNextTurn;

    private void Awake() {
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

        // Initiate control menu button
        controlMenuReturnButton.GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Default); });

        // Initiate settings menu button
        settingsMenuReturnButton.GetComponent<Button>().onClick.AddListener(delegate { SwitchPauseMenuPanel(PauseMenuState.Default); });
    }
    void Update() {
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
