using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public enum MainMenuState { Default, NewGame, Controls, Stats };

public class MainMenuManager : MonoBehaviour {
    public GameManager gameManager;

    [HideInInspector] public MainMenuState currentMMState = MainMenuState.Default;

    // mainMenuButtons[0] = new game
    // mainMenuButtons[1] = controls
    // mainMenuButtons[2] = stats
    // mainMenuButtons[3] = exit game
    public Button[] mainMenuButtons = new Button[4];

    public Button[] returnToMainMenuButtons = new Button[3];

    public Button startNewGameButton;

    // all the tabs - these are canvases
    public GameObject mainMenu;
    public GameObject newGameMenu;
    public GameObject controlsMenu;
    public GameObject statsMenu;

    // dropDowns[0] = class dropdown
    // dropDowns[1] = difficulty dropdown
    // dropDowns[2] = biomes dropdown
    // dropDowns[3] = map size dropdown
    public GameObject[] dropDowns = new GameObject[4];

    public Sprite[] classImages = new Sprite[7];
    public GameObject classImageObject;

    public TextMeshProUGUI classStatsText;

    private void Start() {
        // initiate main menu
        mainMenu.SetActive(true);
        newGameMenu.SetActive(false);
        controlsMenu.SetActive(false);
        statsMenu.SetActive(false);

        // initiate main menu button functions
        mainMenuButtons[0].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.NewGame); });
        mainMenuButtons[0].GetComponent<Button>().onClick.AddListener(delegate { gameManager.soundManager.PlayButtonClick(); });
        mainMenuButtons[1].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Controls); });
        mainMenuButtons[1].GetComponent<Button>().onClick.AddListener(delegate { gameManager.soundManager.PlayButtonClick(); });
        mainMenuButtons[2].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Stats); });
        mainMenuButtons[2].GetComponent<Button>().onClick.AddListener(delegate { gameManager.soundManager.PlayButtonClick(); });

        returnToMainMenuButtons[0].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Default); });
        returnToMainMenuButtons[0].GetComponent<Button>().onClick.AddListener(delegate { gameManager.soundManager.PlayButtonClick(); });
        returnToMainMenuButtons[1].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Default); });
        returnToMainMenuButtons[1].GetComponent<Button>().onClick.AddListener(delegate { gameManager.soundManager.PlayButtonClick(); });
        returnToMainMenuButtons[2].GetComponent<Button>().onClick.AddListener(delegate { SwitchMainMenuPanel(MainMenuState.Default); });
        returnToMainMenuButtons[2].GetComponent<Button>().onClick.AddListener(delegate { gameManager.soundManager.PlayButtonClick(); });
    }


    public void SwitchMainMenuPanel(MainMenuState newState) {
        // make sure pause menu is enabled before doing anything
        mainMenu.SetActive(false);
        switch (newState) {
            // open controls panel
            case MainMenuState.NewGame:
                newGameMenu.SetActive(true);
                // initiate character stats text
                UpdateCharacterStats();
                break;
            case MainMenuState.Controls:
                controlsMenu.SetActive(true);
                break;
            case MainMenuState.Stats:
                statsMenu.SetActive(true);
                break;
            default:
                mainMenu.SetActive(true);
                newGameMenu.SetActive(false);
                controlsMenu.SetActive(false);
                statsMenu.SetActive(false);
                break;
        }
        currentMMState = newState;
    }

    // coroutine because we have to wait a little bit before calling InitiateGameSession()
    public void StartNewGame() {
        // create new player character with the value from the dropdown
        gameManager.characterClassInt = dropDowns[0].GetComponent<TMP_Dropdown>().value;

        // difficulty dropdown
        switch (dropDowns[1].GetComponent<TMP_Dropdown>().value) {
            case 0:
                gameManager.difficulty = DifficultyState.Ez;
                break;
            case 1:
                gameManager.difficulty = DifficultyState.Mid;
                break;
            case 2:
                gameManager.difficulty = DifficultyState.Impossible;
                break;
        }

        // biome setting dropdown
        switch(dropDowns[2].GetComponent<TMP_Dropdown>().value) {
            case 0:
                gameManager.biomeSetting = BiomeSetting.Default;
                break;
            case 1:
                gameManager.biomeSetting = BiomeSetting.Hilly;
                break;
            case 2:
                gameManager.biomeSetting = BiomeSetting.Superflat;
                break;
            case 3:
                gameManager.biomeSetting = BiomeSetting.Mountaineous;
                break;
            case 4:
                gameManager.biomeSetting = BiomeSetting.Swampland;
                break;
        }

        // map size dropdown
        switch (dropDowns[3].GetComponent<TMP_Dropdown>().value) {
            case 0:
                gameManager.mapSize = 100;
                break;
            case 1:
                gameManager.mapSize = 125;
                break;
            case 2:
                gameManager.mapSize = 150;
                break;
            case 3:
                gameManager.mapSize = 175;
                break;
        }
        gameManager.StartNewGame();
        SceneManager.LoadScene("Game");
    }

    public void UpdateCharacterStats() {
        Character newChar = GameManager.ClassIntToClass(dropDowns[0].GetComponent<TMP_Dropdown>().value);
        classStatsText.text = newChar.GetStringCharacterStats();

        // update images
        classImageObject.GetComponent<Image>().sprite = classImages[dropDowns[0].GetComponent<TMP_Dropdown>().value];
    }
    // grunt, engie, joker, scout, sharpshooter, surgeon, tank
    // exit the game completely
    public void CloseGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
