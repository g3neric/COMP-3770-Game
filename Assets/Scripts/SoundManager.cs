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

	public bool soundFXMuted = false;
	public bool musicMuted = false;

	[HideInInspector] private AudioSource aS;


	void Awake() {
		// Make this game object persistant throughout scenes
		aS = GetComponent<AudioSource>();

		DontDestroyOnLoad(gameObject);

	}

    public void Update() {
		aS.mute = musicMuted;
    }

	public void ToggleMuteMusic() {
		if (musicMuted) {
			musicMuted = false;
			PlayGameMusic();
		} else if (!musicMuted) {
			PauseMusic();
			musicMuted = true;
			aS.clip = null;
		}
    }

    public void PlayMainMenuMusic() {
		PauseMusic();
		if (!musicMuted) {
			PauseMusic();
			aS.clip = messiSong;
			aS.Play();
		}
    }

	public void PlayGameMusic() {	
		if (!musicMuted) {
			int ran = Random.Range(1, 101);
			if (ran <= 35) {
				aS.clip = sometimesMusic;
			} else {
				aS.clip = battleMusic;
			}
			aS.Play();
		}
	}

	public void PlayButtonClick() {
		aS.PlayOneShot(buttonClick);
    }

	public void PauseMusic() {
		aS.Stop();
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
