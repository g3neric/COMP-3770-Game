using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {

    [HideInInspector] public float speedX = 0.1f;
    [HideInInspector] public float speedY = 0.1f;
    private float curX;
    private float curY;

    // Use this for initialization
    void Start() {
        curX = GetComponent<Renderer>().material.mainTextureOffset.x;
        curY = GetComponent<Renderer>().material.mainTextureOffset.y;
    }

    // Update is called once per frame
    void FixedUpdate() {
        curX += Time.deltaTime * speedX;
        curY += Time.deltaTime * speedY;
        GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(curX, curY));
    }
}
