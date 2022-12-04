using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetHandler : MonoBehaviour {
    // holds all asset references

    // character prefabs
    [Header("Character prefabs")]
    public GameObject EngineerPrefab;
    public GameObject GruntPrefab;
    public GameObject JokerPrefab;
    public GameObject SaboteurPrefab;
    public GameObject ScoutPrefab;
    public GameObject SharpshooterPrefab;
    public GameObject SurgeonPrefab;
    public GameObject TankPrefab;

    [Space]
    [Header("Tile outline & line prefabs")]
    public GameObject linePrefab;
    public GameObject tileOutlinePrefab;
    public GameObject tileHoverOutlinePrefab;
    public GameObject[] tilePossibleMovementOutlinePrefabs;
    public GameObject[] rangeOutlinePrefabs;


    [Space]
    [Header("Materials")]
    // fog of war material
    public Material fogOfWarOutlineMaterial;

    [Space]
    [Header("Cursors")]
    public Texture2D defaultCursorTexture;
    public Texture2D moveCursorTexture;
    public Texture2D attackCursorTexture;

    void Start() {
        DontDestroyOnLoad(gameObject);
    }
}