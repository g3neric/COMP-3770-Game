using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetHandler : MonoBehaviour {
    // holds all asset references

    // character prefabs
    [Header("Character prefabs")]
    public GameObject[] classPrefabs = new GameObject[9];

    [Space]
    [Header("Tile outline & line prefabs")]
    public GameObject linePrefab;
    public GameObject tileOutlinePrefab;
    public GameObject tileHoverOutlinePrefab;
    public GameObject[] tilePossibleMovementOutlinePrefabs;
    public GameObject[] rangeOutlinePrefabs;

    [Space]
    [Header("Road prefabs")]
    public GameObject straightRoadPrefab;
    public GameObject crossroadPrefab;

    [Space]
    [Header("Materials")]
    // fog of war material
    public Material fogOfWarOutlineMaterial;

    [Space]
    [Header("Cursors")]
    public Texture2D defaultCursorTexture;
    public Texture2D moveCursorTexture;
    public Texture2D attackCursorTexture;

    [Space]
    [Header("Particle Effects")]
    public GameObject fireEffectsPrefab;

    [Space]
    [Header("Skyboxes and lighting")]
    public Material redSky;
    public Material nightSky;
    public Material clearSky;
}
