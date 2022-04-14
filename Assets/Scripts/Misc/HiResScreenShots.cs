using UnityEngine;
using System.Collections;
using System.IO;

public class HiResScreenShots : MonoBehaviour {
	public GameObject cameraGO;
	public bool takePicture = false;

	public int resWidth = 4096; 
	public int resHeight = 2304;
	public bool includeHalfResolution = true;

	private bool takeHiResShot = false;
	private float currentTimescale;

	public static string ScreenShotName(int width, int height) {

		if(!Directory.Exists(Application.persistentDataPath + "/screenshots/")){ 
			Directory.CreateDirectory(Application.persistentDataPath + "/screenshots/");
		}

		return string.Format("{0}/screenshots/screen_{1}x{2}_{3}_" + ".png", 
			Application.persistentDataPath, 
			width, height, 
			System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}

	public void TakeHiResShot() {
		takeHiResShot = true;
	}

	void LateUpdate() {
		takeHiResShot = Input.GetKeyDown(KeyCode.SysReq);
		if (takeHiResShot || takePicture) {
			StartCoroutine (TakeScreenShot ());
		}
	}

	IEnumerator TakeScreenShot(){
		currentTimescale = Time.timeScale;
		Time.timeScale = 0f;
		takePicture = false;
		takeHiResShot = false;
		RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
		Camera.main.targetTexture = rt;
		Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
		if (cameraGO == null)
			//GameObject.Find("PlayerSpawn").GetComponent<PlayerSpawn>().Player.GetComponent<References>().Camera.GetComponent<Camera>().Render();
			Camera.main.Render();
		else
			cameraGO.GetComponent<Camera>().Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
		Camera.main.targetTexture = null;
		RenderTexture.active = null; // JC: added to avoid errors
		Destroy(rt);
		byte[] bytes = screenShot.EncodeToPNG();
		string filename = ScreenShotName(resWidth, resHeight);
		System.IO.File.WriteAllBytes(filename, bytes);
		Debug.Log(string.Format("Took screenshot to: {0}", filename));
		takeHiResShot = false;

		if (includeHalfResolution) {
			rt = new RenderTexture (resWidth / 2, resHeight / 2, 24);
			Camera.main.targetTexture = rt;
			screenShot = new Texture2D (resWidth / 2, resHeight / 2, TextureFormat.RGB24, false);
			Camera.main.Render ();
			RenderTexture.active = rt;
			screenShot.ReadPixels (new Rect (0, 0, resWidth / 2, resHeight / 2), 0, 0);
			Camera.main.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			Destroy (rt);
			bytes = screenShot.EncodeToPNG ();
			filename = ScreenShotName (resWidth / 2, resHeight / 2);
			System.IO.File.WriteAllBytes (filename, bytes);
			Debug.Log (string.Format ("Took screenshot to: {0}", filename));
			takeHiResShot = false;
		}
		yield return null;
		Time.timeScale = currentTimescale;
	}
}