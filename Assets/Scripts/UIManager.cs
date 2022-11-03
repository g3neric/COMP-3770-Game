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

    private void Awake() {
        gameManager = gameManagerObject.GetComponent<GameManager>();
    }
    void Update() {
        movementText.text = "AP left: " + gameManager.characterClass.AP;
        turnCountText.text = "Turn " + gameManager.turnCount;
    }
}
