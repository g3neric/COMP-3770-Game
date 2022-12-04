using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	public AudioClip AssaultRifleFire;
	public AudioClip PistolFire;
	void Start() {
		// Make this game object persistant throughout scenes
		DontDestroyOnLoad(gameObject);
	}
}
