using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Luminosity.IO;
using UnityEngine.PostProcessing;

public class EnemyWavesManager : MonoBehaviour {

	public bool displayWavesGui = true;
	public bool lastIsVictory = true;
	public AudioClip WinSound;

	private playerGUI playerGUI;

	public int wavesLimit;
	public float timeToNextWave = 50f;
	public float timeToNextTimer;
	public int currentWave;
	public List<GameObject> CurrentWave = new List<GameObject>();
	public List<GameObject> CurrentWaveTemp = new List<GameObject>();

	public List<GameObject> SpawnSpots;
	private List<GameObject> TemporalSpawnSpots;
	private int deathsAmount;
	private bool youWin = false;

	public List<GameObject> Wave1 = new List<GameObject>();
	public List<GameObject> Wave2 = new List<GameObject>();
	public List<GameObject> Wave3 = new List<GameObject>();
	public List<GameObject> Wave4 = new List<GameObject>();
	public List<GameObject> Wave5 = new List<GameObject>();
	public List<GameObject> Wave6 = new List<GameObject>();
	public List<GameObject> Wave7 = new List<GameObject>();
	public List<GameObject> Wave8 = new List<GameObject>();
	public List<GameObject> Wave9 = new List<GameObject>();
	public List<GameObject> Wave10 = new List<GameObject>();
	public List<GameObject> Wave11 = new List<GameObject>();
	public List<GameObject> Wave12 = new List<GameObject>();
	public List<GameObject> Wave13 = new List<GameObject>();
	public List<GameObject> Wave14 = new List<GameObject>();
	public List<GameObject> Wave15 = new List<GameObject>();
	public List<GameObject> Wave16 = new List<GameObject>();
	public List<GameObject> Wave17 = new List<GameObject>();
	public List<GameObject> Wave18 = new List<GameObject>();
	public List<GameObject> Wave19 = new List<GameObject>();
	public List<GameObject> Wave20 = new List<GameObject>();


	private List<List<GameObject>> listOfWaves = new List<List<GameObject>>();
	private List<List<GameObject>> temporalListofWaves = new List<List<GameObject>>();

	public float targetTime = 0.1f;
	public float timeBetweenSpawns = 0f;

	private float checkTimer;
	private bool coroRuning;
	private bool initializedWave;
	private bool initialized;
	private bool begin;
	private bool finishedSpawning;
	private Transform spawnSpot;
	private GameObject newEnemy;

	private GameObject Player;

	// Use this for initialization
	void Start () {
		listOfWaves = new List<List<GameObject>>	{Wave1, Wave2, Wave3, Wave4, Wave5, Wave6, Wave7, Wave8, Wave9, Wave10, 
													Wave11, Wave12, Wave13, Wave14, Wave15, Wave16, Wave17, Wave18, Wave19, Wave20};
		foreach (List<GameObject> wave in listOfWaves) {
			if (wave.Count == 0)
				temporalListofWaves.Remove (wave);
		}

		foreach (List<GameObject> wave in temporalListofWaves) {
			listOfWaves.Remove (wave);
		}
		temporalListofWaves = new List<List<GameObject>>();
	}

	IEnumerator DelayedStart(){
		yield return null;
		initialized = true;
		Player = GameObject.Find ("Player");
		playerGUI = Player.GetComponent<playerGUI> ();
		playerGUI.waveNumber = currentWave + 1;
		if (displayWavesGui)
			GameObject.Find ("PlayerSpawn").GetComponent<PlayerSpawn> ().Player.GetComponent<playerGUI> ().wavesMode = true;
		yield return new WaitForSeconds (3.0f);
		begin = true;
		yield break;

/*
		if (Wave1.Count > 0)
			listOfWaves.Add (Wave1);
		if (Wave2.Count > 0)
			listOfWaves.Add (Wave2);
		if (Wave3.Count > 0)
			listOfWaves.Add (Wave3);
		if (Wave4.Count > 0)
			listOfWaves.Add (Wave4);
		if (Wave5.Count > 0)
			listOfWaves.Add (Wave5);
		if (Wave6.Count > 0)
			listOfWaves.Add (Wave6);
		if (Wave7.Count > 0)
			listOfWaves.Add (Wave7);
		if (Wave8.Count > 0)
			listOfWaves.Add (Wave8);
		if (Wave9.Count > 0)
			listOfWaves.Add (Wave9);
		if (Wave10.Count > 0)
			listOfWaves.Add (Wave10);
*/	

	}

	void OnEnable(){
		Start ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!initialized)
			StartCoroutine (DelayedStart ());

		checkTimer += Time.deltaTime;
		if (begin && !coroRuning && checkTimer >= targetTime) {
			checkTimer = 0f;
			targetTime = Random.Range (0.01f, 0.25f);
			StartCoroutine (UpdateValues ());
		}
	}

	IEnumerator UpdateValues(){
		coroRuning = true;

		yield return new WaitForFixedUpdate ();
		if (!initializedWave) {
			temporalListofWaves = new List<List<GameObject>>();

			TemporalSpawnSpots = new List<GameObject> ();
			foreach (GameObject SpawnSpot in SpawnSpots) {
				TemporalSpawnSpots.Add (SpawnSpot);
			}

		//	TemporalSpawnSpots = SpawnSpots;
			foreach (GameObject enemy in listOfWaves[currentWave]) {
				if (TemporalSpawnSpots.Count > 0) {
					spawnSpot = TemporalSpawnSpots [Random.Range (0, TemporalSpawnSpots.Count - 1)].transform;
					newEnemy = Instantiate (enemy, spawnSpot.position, Quaternion.Euler (0f, Random.Range (0f, 359f), 0f));
					if (newEnemy.GetComponent<AI> ()) {
						newEnemy.GetComponent<AI> ().target = Player;
					}
					spawnSpot.GetComponent<SpawnSpot> ().ParticlesEffect.GetComponent<ParticleSystem>().Play();
					spawnSpot.GetComponent<SpawnSpot> ().ParticlesEffect.GetComponent<AudioSource>().Play();
					CurrentWave.Add (newEnemy);
					yield return new WaitForSeconds ((timeBetweenSpawns + Random.Range(0.01f, 0.5f)));
					TemporalSpawnSpots.Remove (spawnSpot.gameObject);
				} else {
					TemporalSpawnSpots = new List<GameObject> ();
					foreach (GameObject SpawnSpot in SpawnSpots) {
						TemporalSpawnSpots.Add (SpawnSpot);
					}
				}
				yield return null;
			}
			initializedWave = true;
		} else if (initializedWave){
			deathsAmount = 0;
			CurrentWaveTemp = new List<GameObject>(CurrentWave);
			foreach (GameObject enemy in CurrentWaveTemp) {
				if (enemy == null || enemy.GetComponent<HealthBar> ().isDead) {
					CurrentWave.Remove (enemy);
					//deathsAmount += 1;
				}
			}

			playerGUI.enemiesLeft = CurrentWave.Count;

			if (CurrentWave.Count == 0){
				timeToNextTimer = timeToNextWave;
				playerGUI.showTimer = true;

				if (lastIsVictory && currentWave >= wavesLimit || currentWave > listOfWaves.Count) {
					StartCoroutine (Congrats ());
					Player.GetComponent<playerGUI> ().youWin = true;
					yield break;
				}

				for (timeToNextTimer = timeToNextWave; timeToNextTimer >= 0f; timeToNextTimer -= Time.deltaTime) {
					playerGUI.timer = timeToNextTimer;
					yield return new WaitForEndOfFrame ();
				}

/*				do {
					timeToNextTimer = -Time.deltaTime;
					print ("doing");
					yield return new WaitForEndOfFrame ();
				} while (timeToNextTimer <= 0f);
*/					
				currentWave += 1;
				playerGUI.waveNumber = currentWave + 1;
				TemporalSpawnSpots = SpawnSpots;
				CurrentWave = new List<GameObject> ();
				initializedWave = false;
				playerGUI.showTimer = false;
			}
		}
		coroRuning = false;
		yield return null;
	}


	IEnumerator Congrats(){
		float currentTimeScale = Time.timeScale;
		float currentFixedDeltaTime = Time.fixedDeltaTime;
		float currentMaximumDelta = Time.maximumDeltaTime;

		StartCoroutine (Bloom ());
		StartCoroutine (MusicChange ());

		float t = 3f;
		while (t > 0) {
			t -= Time.deltaTime / Time.timeScale * 2f;
			Time.timeScale = 0.1f;
			Time.fixedDeltaTime = 0.0025f;
			yield return null;
		}
	}

	IEnumerator MusicChange(){

		yield return new WaitForSecondsRealtime (0.5f);
		AudioSource oldMusic = GameObject.Find ("SoundManager").GetComponent<AudioSource>();
		while (oldMusic.volume > 0.1f) {
			yield return new WaitForFixedUpdate ();
			oldMusic.volume -= 0.01f;
			Player.GetComponent<Hero_Movement> ().doNotMove = true;
			Player.GetComponent<Hero_Movement> ().cantStab = true;
			Player.GetComponent<Hero_Movement> ().cantShoot = true;
		}

		oldMusic.gameObject.SetActive (false);
		AudioSource audio = CustomMethods.PlayClipAt (WinSound, Camera.main.transform.position);
		audio.spatialBlend = 0f;
		audio.spatialize = false;
		audio.dopplerLevel = 0f;
		audio.pitch = 1f;
		audio.reverbZoneMix = 0f;
		//StartCoroutine (Bloom ());

		GameObject.Find ("Player").GetComponent<playerGUI>().youWin = true;

		while (audio.isPlaying) {
			//Time.timeScale = Mathf.Lerp (Time.timeScale, 0f, 0.01f);
			yield return new WaitForFixedUpdate();
		}

		yield return new WaitForSecondsRealtime (1f);

		PlayerPrefs.SetString ("NextScene", "NewMainMenuGeneric");
		SceneManager.LoadScene ("LoadingScene");
	}

	IEnumerator Bloom(){
		PostProcessingBehaviour postProcessingBehavior = Player.GetComponent<References> ().Camera.GetComponent<PostProcessingBehaviour> ();
		PostProcessingProfile oldProfile = postProcessingBehavior.profile;
		PostProcessingProfile newProfile = Instantiate(postProcessingBehavior.profile);
		BloomModel.Settings bloomSettings = newProfile.bloom.settings;
		float startingIntensity = newProfile.bloom.settings.bloom.intensity;
		float startingThreshold = newProfile.bloom.settings.bloom.threshold;
		float finalIntensity = startingIntensity;
		float finalThreshold;

		startingIntensity = newProfile.bloom.settings.bloom.intensity;
		startingThreshold = newProfile.bloom.settings.bloom.threshold;
		finalIntensity = oldProfile.bloom.settings.bloom.intensity + 0.25f;
		finalThreshold = oldProfile.bloom.settings.bloom.threshold / 4f;

		float timer = 0f;
		while (timer < 1f) {
			timer += Time.deltaTime / (0.8f * Time.timeScale);
			bloomSettings.bloom.intensity = Mathf.Lerp (startingIntensity, finalIntensity, timer);
			bloomSettings.bloom.threshold = Mathf.Lerp (startingThreshold, finalThreshold, timer);
			newProfile.bloom.settings = bloomSettings;
			postProcessingBehavior.profile = newProfile;
			yield return null;
		}
	}
}
