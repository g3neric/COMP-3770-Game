using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	// sound fx
	public AudioClip AssaultRifleFire;
	public AudioClip PistolFire;
	public AudioClip SniperFire;
	public AudioClip buttonClick;

	// music
	public AudioClip messiSong;
	public AudioClip battleMusic;
	public AudioClip sometimesMusic;

	[HideInInspector] public bool soundFXMuted = false;
	[HideInInspector] public bool musicMuted = false;

	[HideInInspector] private AudioSource aS;

	void Start() {
		// Make this game object persistant throughout scenes
		aS = GetComponent<AudioSource>();
		DontDestroyOnLoad(gameObject);
	}

    public void Update() {
		aS.mute = musicMuted;
    }

    public void PlayMainMenuMusic() {
		aS.clip = messiSong;
		aS.Play();
    }

	public void PlayButtonClick() {
		aS.PlayOneShot(buttonClick);
    }

	public void PlayGameMusic() {
		int ran = Random.Range(1, 101);
		if (ran <= 10) {
			aS.clip = sometimesMusic;
        } else {
			aS.clip = battleMusic;
        }
		aS.Play();
    }

	public void PauseMusic() {
		aS.Pause();
    }

	public void PlayGunshot(string name) {
		// play sound
		if (!soundFXMuted) {
			if (name == "Assault Rifle") {
				aS.PlayOneShot(AssaultRifleFire);
			} else if (name == "Pistol") {
				aS.PlayOneShot(PistolFire);
			} else if (name == "Sniper Rifle") {
				aS.PlayOneShot(SniperFire);
			}
		}
	}
}
