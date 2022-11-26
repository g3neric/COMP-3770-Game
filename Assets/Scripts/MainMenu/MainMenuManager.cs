using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public enum MainMenuState { Default, NewGame, LoadGame, Controls, Settings };

public class MainMenuManager : MonoBehaviour {
    public GameManager gameManager;

    [HideInInspector] public MainMenuState currentMMState = MainMenuState.Default;

    // mainMenuButtons[0] = new game
    // mainMenuButtons[1] = load game
    // mainMenuButtons[2] = controls
    // mainMenuButtons[3] = settings
    // mainMenuButtons[4] = exit game
    public Button[] mainMenuButtons = new Button[5];

    public Button[] returnToMainMenuButtons = new Button[4];

    public Button startNewGameButton;

    public GameObject mainMenu;
    public GameObject newGameMenu;
    public GameObject loadGameMenu;
    public GameObject controlsMenu;
    public GameObject settingsMenu;


    private void Start() {
        // initiate main menu
        mainMenu.SetActive(true);
        newGameMenu.SetActive(false);
        loadGameMenu.SetActive(false);
        controlsMenu.SetActive(false);
        settingsMenu.SetActive(false);

        // initiate main menu button functions
        mainMenuButtons[0].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.NewGame); });
        mainMenuButtons[1].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.LoadGame); });
        mainMenuButtons[2].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Controls); });
        mainMenuButtons[3].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Settings); });
        mainMenuButtons[4].GetComponent<Button>().onClick.AddListener(delegate { CloseGame(); });

        returnToMainMenuButtons[0].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Default); });
        returnToMainMenuButtons[1].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Default); });
        returnToMainMenuButtons[2].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Default); });
        returnToMainMenuButtons[3].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Default); });

        startNewGameButton.GetComponent<Button>().onClick.AddListener(delegate { StartNewGame(); ; });
    }

    public void SwitchMainMenuPanel(MainMenuState newState) {
        // make sure pause menu is enabled before doing anything
        mainMenu.SetActive(false);
        switch (newState) {
            // open controls panel
            case MainMenuState.NewGame:
                newGameMenu.SetActive(true);
                break;
            case MainMenuState.LoadGame:
                loadGameMenu.SetActive(true);
                break;
            case MainMenuState.Controls:
                controlsMenu.SetActive(true);
                break;
            case MainMenuState.Settings:
                settingsMenu.SetActive(true);
                break;
            default:
                mainMenu.SetActive(true);
                newGameMenu.SetActive(false);
                loadGameMenu.SetActive(false);
                controlsMenu.SetActive(false);
                settingsMenu.SetActive(false);
                break;
        }
        currentMMState = newState;
    }

    // coroutine because we have to wait a little bit before calling InitiateGameSession()
    private void StartNewGame() {
        SceneManager.LoadScene("Game");
        gameManager.StartNewGame();
    }

    // exit the game completely
    public void CloseGame() {
        Application.Quit();
    }
}
