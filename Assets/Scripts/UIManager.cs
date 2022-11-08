// Desgined and created by Tyler R. Renaud
// All rights belong to creator

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {
    public GameObject gameManagerObject;
    [HideInInspector] public GameManager gameManager;

    // UI objects
    public TextMeshProUGUI movementText;
    public TextMeshProUGUI turnCountText;

    // toolbarButtons[0] = move state
    // toolbarButtons[1] = item 1
    // toolbarButtons[2] = item 2
    public Button[] toolbarButtons = new Button[3];

    public Button selectedToolbarButton = null;

    public Button buttonNextTurn;


    private void Awake() {
        gameManager = gameManagerObject.GetComponent<GameManager>();

        // Initiate next turn button
        buttonNextTurn.GetComponent<Button>().onClick.AddListener(delegate { gameManager.FinishTurn(); });

        // Initiate move state button
        toolbarButtons[0].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(0); });

        // Initiate item buttons
        toolbarButtons[1].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(1); });
        toolbarButtons[2].GetComponent<Button>().onClick.AddListener(delegate { ToolbarButtonSelected(2); });
    }
    void Update() {
        // update counts on the screen
        movementText.text = "AP left: " + gameManager.characterClass.AP;
        turnCountText.text = "Turn " + gameManager.turnCount;

        // button controls for the toolbar
        if (Input.GetKeyDown("1")) {
            ToolbarButtonSelected(0);
        } else if (Input.GetKeyDown("2")) {
            ToolbarButtonSelected(1);
        } else if (Input.GetKeyDown("3")) {
            ToolbarButtonSelected(2);
        }
    }

    // handle selection of toolbar item
    public void ToolbarButtonSelected(int selectedButtonIndex) {
        // change control state in game manager
        if (selectedButtonIndex == 0) {
            gameManager.SetItemSelected(ItemSelected.Move);
        } else if (selectedButtonIndex == 1) {
            gameManager.SetItemSelected(ItemSelected.Item1);
        } else if (selectedButtonIndex == 2) {
            gameManager.SetItemSelected(ItemSelected.Item2);
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
