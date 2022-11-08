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

    private ControlState csAttack = ControlState.Attack;
    private ControlState csMove = ControlState.Move;

    // UI objects
    public TextMeshProUGUI movementText;
    public TextMeshProUGUI turnCountText;

    public Button buttonNextTurn;
    public Button buttonMoveState;
    public Button buttonAttackState;

    private void Awake() {
        gameManager = gameManagerObject.GetComponent<GameManager>();

        // Initiate next turn button
        buttonNextTurn.GetComponent<Button>().onClick.AddListener(delegate { gameManager.FinishTurn(); });

        // Initiate move state button
        buttonMoveState.GetComponent<Button>().onClick.AddListener(delegate { gameManager.SetControlState(csMove); });

        // Initiate attack state button
        buttonAttackState.GetComponent<Button>().onClick.AddListener(delegate { gameManager.SetControlState(csAttack); });
    }
    void Update() {
        movementText.text = "AP left: " + gameManager.characterClass.AP;
        turnCountText.text = "Turn " + gameManager.turnCount;

        RectTransform buttonAttackStateTransform = buttonAttackState.GetComponent<RectTransform>();
        RectTransform buttonMoveStateTransform = buttonMoveState.GetComponent<RectTransform>();

        // set both to default
        Vector3 pos = buttonAttackStateTransform.position;
        buttonAttackStateTransform.position = new Vector3(pos.x, 5, pos.z);
        pos = buttonMoveStateTransform.position;
        buttonMoveStateTransform.position = new Vector3(pos.x, 5, pos.z);
        if (gameManager.cs == ControlState.Attack) {
            // raise attack button a bit
            pos = buttonAttackStateTransform.position;
            buttonAttackStateTransform.position = new Vector3(pos.x, 25, pos.z);
        } else if (gameManager.cs == ControlState.Move) {
            // raise move button a bit
            pos = buttonMoveStateTransform.position;
            buttonMoveStateTransform.position = new Vector3(pos.x, 25, pos.z);
        }
    }
}
