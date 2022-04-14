using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using MK.Glow;
using UnityEngine.PostProcessing;
using MorePPEffects;

public class GraphicsQuality : MonoBehaviour {

	private int qualityLevel;
	private int previousQLevel;
	private MKGlowFree glowEffect;
	private Bloom bloom;
	private CreaseShading creaseShading;
	private NoiseAndScratches noiseAndScratches;
	private CameraMotionBlur cameraMotionBlur;
	private PostProcessingBehaviour postProcessingStack;
	private PostProcessingProfile postProcessingProfileRuntime;
	private Colorization colorizationScript;

	private GameObject[] lightsInScene;

	// Use this for initialization
	void Awake(){
		lightsInScene = GameObject.FindGameObjectsWithTag ("light");
	}

	public void Start () {
		qualityLevel = QualitySettings.GetQualityLevel();

		if (GetComponent<MKGlowFree> ())
			glowEffect = GetComponent<MKGlowFree> ();

		if (GetComponent<Bloom> ())
			bloom = GetComponent<Bloom> ();

		if (GetComponent<CreaseShading> ())
			creaseShading = GetComponent<CreaseShading> ();

		if (GetComponent<NoiseAndScratches> ())
			noiseAndScratches = GetComponent<NoiseAndScratches> ();

		if (GetComponent<PostProcessingBehaviour> ()) {
			postProcessingStack = GetComponent<PostProcessingBehaviour> ();
			postProcessingProfileRuntime = Instantiate (postProcessingStack.profile);
			postProcessingStack.profile = postProcessingProfileRuntime;
		}

		if (GetComponent<Colorization> ())
			colorizationScript = GetComponent<Colorization> ();

		if (qualityLevel == 5){
			AntialiasingModel.Settings AASettings = postProcessingProfileRuntime.antialiasing.settings;
			MotionBlurModel.Settings motionBlurSettings = postProcessingProfileRuntime.motionBlur.settings;
			BloomModel.Settings bloomSettings = postProcessingProfileRuntime.bloom.settings;
			AmbientOcclusionModel.Settings ambientOcclusionSettings = postProcessingProfileRuntime.ambientOcclusion.settings;

			switch (PlayerPrefs.GetInt ("RenderMode", 1)) {
			case 0:
				GetComponent<Camera> ().renderingPath = RenderingPath.VertexLit;
				break;
			case 1:
				GetComponent<Camera> ().renderingPath = RenderingPath.Forward;
				break;
			case 2:
				GetComponent<Camera> ().renderingPath = RenderingPath.DeferredShading;
				break;
			}

			switch (PlayerPrefs.GetInt ("AA", 0)) {
			case 0:				
				postProcessingProfileRuntime.antialiasing.enabled = false;
				postProcessingStack.profile.antialiasing.settings = AASettings;
				break;
			case 1:
				postProcessingProfileRuntime.antialiasing.enabled = true;
				AASettings.method = AntialiasingModel.Method.Fxaa;
				AASettings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.ExtremePerformance;
				postProcessingStack.profile.antialiasing.settings = AASettings;
				break;
			case 2:
				postProcessingProfileRuntime.antialiasing.enabled = true;
				AASettings.method = AntialiasingModel.Method.Fxaa;
				AASettings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Performance;
				postProcessingStack.profile.antialiasing.settings = AASettings;
				break;
			case 3:
				postProcessingProfileRuntime.antialiasing.enabled = true;
				AASettings.method = AntialiasingModel.Method.Fxaa;
				AASettings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Default;
				postProcessingStack.profile.antialiasing.settings = AASettings;
				break;
			case 4:
				postProcessingProfileRuntime.antialiasing.enabled = true;
				AASettings.method = AntialiasingModel.Method.Fxaa;
				AASettings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Quality;
				postProcessingStack.profile.antialiasing.settings = AASettings;
				break;
			case 5:
				postProcessingProfileRuntime.antialiasing.enabled = true;
				AASettings.method = AntialiasingModel.Method.Fxaa;
				AASettings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.ExtremeQuality;
				postProcessingStack.profile.antialiasing.settings = AASettings;
				break;
			}

			switch (PlayerPrefs.GetInt ("VSync", 0)) {
			case 0:
				QualitySettings.vSyncCount = 0;
				break;
			case 1:
				QualitySettings.vSyncCount = 1;
				break;
			}

			switch (PlayerPrefs.GetInt ("Textures", 2)) {
			case 0:
				QualitySettings.masterTextureLimit = 3;
				break;
			case 1:
				QualitySettings.masterTextureLimit = 2;
				break;
			case 2:
				QualitySettings.masterTextureLimit = 1;
				break;
			case 3:
				QualitySettings.masterTextureLimit = 0;
				break;
			}

			switch (PlayerPrefs.GetInt ("LOD", 2)) {
			case 0:
				QualitySettings.lodBias = 0.125f;
				break;
			case 1:
				QualitySettings.lodBias = 0.5f;
				break;
			case 2:
				QualitySettings.lodBias = 0.75f;
				break;
			case 3:
				QualitySettings.lodBias = 0.85f;
				break;
			case 4:
				QualitySettings.lodBias = 1f;
				break;
			}

			switch (PlayerPrefs.GetInt ("LightsQ", 2)) {
			case 0:
				QualitySettings.pixelLightCount = 0;
				break;
			case 1:
				QualitySettings.pixelLightCount = 8;
				break;
			case 2:
				QualitySettings.pixelLightCount = 16;
				break;
			case 3:
				QualitySettings.pixelLightCount = 32;
				break;
			case 4:
				QualitySettings.pixelLightCount = 64;
				break;
			}

			switch (PlayerPrefs.GetInt ("Shadows", 2)) {
			case 0:
				QualitySettings.shadows = ShadowQuality.Disable;
				QualitySettings.shadowResolution = ShadowResolution.Low;
				QualitySettings.shadowProjection = ShadowProjection.StableFit;
				QualitySettings.shadowDistance = 0f;
				QualitySettings.shadowCascades = 0;
				break;
			case 1:
				QualitySettings.shadows = ShadowQuality.HardOnly;
				QualitySettings.shadowResolution = ShadowResolution.Medium;
				QualitySettings.shadowProjection = ShadowProjection.StableFit;
				QualitySettings.shadowDistance = 75f;
				QualitySettings.shadowCascades = 0;
				break;
			case 2:
				QualitySettings.shadows = ShadowQuality.All;
				QualitySettings.shadowResolution = ShadowResolution.Medium;
				QualitySettings.shadowProjection = ShadowProjection.StableFit;
				QualitySettings.shadowDistance = 100f;
				QualitySettings.shadowCascades = 0;
				break;
			case 3:
				QualitySettings.shadows = ShadowQuality.All;
				QualitySettings.shadowResolution = ShadowResolution.High;
				QualitySettings.shadowProjection = ShadowProjection.StableFit;
				QualitySettings.shadowDistance = 150f;
				QualitySettings.shadowCascades = 2;
				break;
			case 4:
				QualitySettings.shadows = ShadowQuality.All;
				QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
				QualitySettings.shadowProjection = ShadowProjection.StableFit;
				QualitySettings.shadowDistance = 200f;
				QualitySettings.shadowCascades = 4;
				break;
			}

			switch (PlayerPrefs.GetInt ("MBlur", 2)) {
			case 0:
				postProcessingProfileRuntime.motionBlur.enabled = false;
				postProcessingStack.profile.motionBlur.settings = motionBlurSettings;
				break;
			case 1:
				postProcessingProfileRuntime.motionBlur.enabled = true;
				motionBlurSettings.shutterAngle = 90f;
				motionBlurSettings.sampleCount = 4;
				motionBlurSettings.frameBlending = 0f;
				postProcessingStack.profile.motionBlur.settings = motionBlurSettings;
				break;
			case 2:
				postProcessingProfileRuntime.motionBlur.enabled = true;
				motionBlurSettings.shutterAngle = 120f;
				motionBlurSettings.sampleCount = 8;
				motionBlurSettings.frameBlending = 0f;
				postProcessingStack.profile.motionBlur.settings = motionBlurSettings;
				break;
			case 3:
				postProcessingProfileRuntime.motionBlur.enabled = true;
				motionBlurSettings.shutterAngle = 180f;
				motionBlurSettings.sampleCount = 10;
				motionBlurSettings.frameBlending = 0f;
				postProcessingStack.profile.motionBlur.settings = motionBlurSettings;
				break;
			case 4:
				postProcessingProfileRuntime.motionBlur.enabled = true;
				motionBlurSettings.shutterAngle = 180f;
				motionBlurSettings.sampleCount = 16;
				motionBlurSettings.frameBlending = 0f;
				postProcessingStack.profile.motionBlur.settings = motionBlurSettings;
				break;
			}

			switch (PlayerPrefs.GetInt ("Bloom", 2)) {
			case 0:
				glowEffect.enabled = false;
				bloom.enabled = false;
				postProcessingProfileRuntime.bloom.enabled = false;
				postProcessingStack.profile.bloom.settings = bloomSettings;
				break;
			case 1:
				glowEffect.enabled = true;
				bloom.enabled = false;
				postProcessingProfileRuntime.bloom.enabled = false;
				bloomSettings.bloom.antiFlicker = false;
				postProcessingStack.profile.bloom.settings = bloomSettings;
				break;
			case 2:
				glowEffect.enabled = true;
				bloom.enabled = false;
				postProcessingProfileRuntime.bloom.enabled = true;
				bloomSettings.bloom.antiFlicker = false;
				postProcessingStack.profile.bloom.settings = bloomSettings;
				break;
			case 3:
				glowEffect.enabled = true;
				bloom.enabled = false;
				postProcessingProfileRuntime.bloom.enabled = true;
				bloomSettings.bloom.antiFlicker = true;
				postProcessingStack.profile.bloom.settings = bloomSettings;
				break;
			case 4:
				glowEffect.enabled = true;
				bloom.enabled = true;
				postProcessingProfileRuntime.bloom.enabled = true;
				bloomSettings.bloom.antiFlicker = true;
				postProcessingStack.profile.bloom.settings = bloomSettings;
				break;
			}

			switch (PlayerPrefs.GetInt ("AO", 2)) {
			case 0:
				postProcessingProfileRuntime.ambientOcclusion.enabled = false;
				ambientOcclusionSettings.intensity = 0f;
				ambientOcclusionSettings.radius = 0f;
				ambientOcclusionSettings.sampleCount = AmbientOcclusionModel.SampleCount.Lowest;
				ambientOcclusionSettings.downsampling = true;
				ambientOcclusionSettings.forceForwardCompatibility = false;
				ambientOcclusionSettings.highPrecision = false;
				ambientOcclusionSettings.ambientOnly = false;
				postProcessingStack.profile.ambientOcclusion.settings = ambientOcclusionSettings;
				break;
			case 1:
				postProcessingProfileRuntime.ambientOcclusion.enabled = true;
				ambientOcclusionSettings.intensity = 0.2f;
				ambientOcclusionSettings.radius = 0.1f;
				ambientOcclusionSettings.sampleCount = AmbientOcclusionModel.SampleCount.Lowest;
				ambientOcclusionSettings.downsampling = true;
				ambientOcclusionSettings.forceForwardCompatibility = false;
				ambientOcclusionSettings.highPrecision = false;
				ambientOcclusionSettings.ambientOnly = false;
				postProcessingStack.profile.ambientOcclusion.settings = ambientOcclusionSettings;
				break;
			case 2:
				postProcessingProfileRuntime.ambientOcclusion.enabled = true;
				ambientOcclusionSettings.intensity = 0.4f;
				ambientOcclusionSettings.radius = 0.15f;
				ambientOcclusionSettings.sampleCount = AmbientOcclusionModel.SampleCount.Low;
				ambientOcclusionSettings.downsampling = true;
				ambientOcclusionSettings.forceForwardCompatibility = false;
				ambientOcclusionSettings.highPrecision = false;
				ambientOcclusionSettings.ambientOnly = false;
				postProcessingStack.profile.ambientOcclusion.settings = ambientOcclusionSettings;
				break;
			case 3:
				postProcessingProfileRuntime.ambientOcclusion.enabled = true;
				ambientOcclusionSettings.intensity = 0.6f;
				ambientOcclusionSettings.radius = 0.2f;
				ambientOcclusionSettings.sampleCount = AmbientOcclusionModel.SampleCount.Medium;
				ambientOcclusionSettings.downsampling = true;
				ambientOcclusionSettings.forceForwardCompatibility = false;
				ambientOcclusionSettings.highPrecision = false;
				ambientOcclusionSettings.ambientOnly = false;
				postProcessingStack.profile.ambientOcclusion.settings = ambientOcclusionSettings;
				break;
			case 4:
				postProcessingProfileRuntime.ambientOcclusion.enabled = true;
				ambientOcclusionSettings.intensity = 0.8f;
				ambientOcclusionSettings.radius = 0.25f;
				ambientOcclusionSettings.sampleCount = AmbientOcclusionModel.SampleCount.High;
				ambientOcclusionSettings.downsampling = true;
				ambientOcclusionSettings.forceForwardCompatibility = false;
				ambientOcclusionSettings.highPrecision = false;
				ambientOcclusionSettings.ambientOnly = false;
				postProcessingStack.profile.ambientOcclusion.settings = ambientOcclusionSettings;
				break;
			}

			if (PlayerPrefs.GetInt("RenderMode") == 0) {
				postProcessingStack.enabled = false;
				transform.parent.FindChildIncludingDeactivated("DistortedOutlineFX").gameObject.SetActive(false);
            } else {
				transform.parent.FindChildIncludingDeactivated("DistortedOutlineFX").gameObject.SetActive(true);
			}


		} else { //In case the player is using a preset
			switch (qualityLevel) {
			case 0:
				GetComponent<Camera> ().renderingPath = RenderingPath.VertexLit;
		
				foreach (GameObject light in lightsInScene) {
					light.SetActive (false);
				}

				if (glowEffect)
					glowEffect.enabled = false;

				if (bloom)
					bloom.enabled = false;

				if (creaseShading)
					creaseShading.enabled = false;

				if (noiseAndScratches)
					noiseAndScratches.enabled = false;

				if (cameraMotionBlur)
					cameraMotionBlur.enabled = false;

				if (postProcessingStack)
					postProcessingStack.enabled = false;

				if (colorizationScript)
					colorizationScript.enabled = false;

				break;
			case 1:
			
				foreach (GameObject light in lightsInScene) {
					light.SetActive (false);
				}

				GetComponent<Camera> ().renderingPath = RenderingPath.Forward;

				if (glowEffect)
					glowEffect.enabled = true;

				if (bloom)
					bloom.enabled = false;

				if (creaseShading)
					creaseShading.enabled = false;

				if (noiseAndScratches)
					noiseAndScratches.enabled = true;

				if (cameraMotionBlur)
					cameraMotionBlur.enabled = false;

				if (postProcessingStack) {
					postProcessingStack.enabled = false;
				}

				if (colorizationScript)
					colorizationScript.enabled = true;


				break;
			case 2:
				GetComponent<Camera> ().renderingPath = RenderingPath.Forward;

				foreach (GameObject light in lightsInScene) {
					light.SetActive (true);
				}

				if (glowEffect)
					glowEffect.enabled = true;

				if (bloom)
					bloom.enabled = true;

				if (creaseShading)
					creaseShading.enabled = true;

				if (noiseAndScratches)
					noiseAndScratches.enabled = true;

				if (cameraMotionBlur)
					cameraMotionBlur.enabled = true;

				if (postProcessingStack) {
					postProcessingStack.enabled = true;


					AmbientOcclusionModel.Settings ambientOcclusionSettings = postProcessingProfileRuntime.ambientOcclusion.settings;
					postProcessingProfileRuntime.ambientOcclusion.enabled = true;
					ambientOcclusionSettings.intensity = 0f;
					ambientOcclusionSettings.radius = 0f;
					ambientOcclusionSettings.sampleCount = AmbientOcclusionModel.SampleCount.Lowest;
					ambientOcclusionSettings.downsampling = true;
					ambientOcclusionSettings.forceForwardCompatibility = true;
					ambientOcclusionSettings.highPrecision = false;
					ambientOcclusionSettings.ambientOnly = false;
					postProcessingStack.profile.ambientOcclusion.settings = ambientOcclusionSettings;


					MotionBlurModel.Settings motionBlurSettings = postProcessingProfileRuntime.motionBlur.settings;
					postProcessingProfileRuntime.motionBlur.enabled = false;

					postProcessingProfileRuntime.bloom.enabled = true;
					postProcessingProfileRuntime.vignette.enabled = true;
				}

				if (colorizationScript)
					colorizationScript.enabled = true;


				break;
			case 3:
				GetComponent<Camera> ().renderingPath = RenderingPath.DeferredShading;

				foreach (GameObject light in lightsInScene) {
					light.SetActive (true);
				}

				if (glowEffect)
					glowEffect.enabled = true;
				if (bloom)
					bloom.enabled = true;

				if (creaseShading)
					creaseShading.enabled = true;

				if (noiseAndScratches)
					noiseAndScratches.enabled = true;

				if (cameraMotionBlur)
					cameraMotionBlur.enabled = true;

				if (postProcessingStack) {
					postProcessingStack.enabled = true;


					AmbientOcclusionModel.Settings ambientOcclusionSettings = postProcessingProfileRuntime.ambientOcclusion.settings;
					postProcessingProfileRuntime.ambientOcclusion.enabled = true;
					ambientOcclusionSettings.intensity = 2f;
					ambientOcclusionSettings.radius = 0.3f;
					ambientOcclusionSettings.sampleCount = AmbientOcclusionModel.SampleCount.Lowest;
					ambientOcclusionSettings.downsampling = true;
					ambientOcclusionSettings.forceForwardCompatibility = true;
					ambientOcclusionSettings.highPrecision = false;
					ambientOcclusionSettings.ambientOnly = false;
					postProcessingStack.profile.ambientOcclusion.settings = ambientOcclusionSettings;


					MotionBlurModel.Settings motionBlurSettings = postProcessingProfileRuntime.motionBlur.settings;
					postProcessingProfileRuntime.motionBlur.enabled = true;
					motionBlurSettings.shutterAngle = 120f;
					motionBlurSettings.sampleCount = 16;
					motionBlurSettings.frameBlending = 0f;

					postProcessingProfileRuntime.bloom.enabled = true;
					postProcessingProfileRuntime.vignette.enabled = true;
				}

				if (colorizationScript)
					colorizationScript.enabled = true;

				break;
			case 4:
				GetComponent<Camera> ().renderingPath = RenderingPath.DeferredLighting;

				foreach (GameObject light in lightsInScene) {
					light.SetActive (true);
				}

				if (glowEffect)
					glowEffect.enabled = true;
				if (bloom)
					bloom.enabled = true;

				if (creaseShading)
					creaseShading.enabled = true;

				if (noiseAndScratches)
					noiseAndScratches.enabled = true;
				
				if (cameraMotionBlur)
					cameraMotionBlur.enabled = true;

				if (postProcessingStack) {
					postProcessingStack.enabled = true;


					AmbientOcclusionModel.Settings ambientOcclusionSettings = postProcessingProfileRuntime.ambientOcclusion.settings;
					postProcessingProfileRuntime.ambientOcclusion.enabled = true;
					ambientOcclusionSettings.intensity = 2f;
					ambientOcclusionSettings.radius = 0.3f;
					ambientOcclusionSettings.sampleCount = AmbientOcclusionModel.SampleCount.Medium;
					ambientOcclusionSettings.downsampling = true;
					ambientOcclusionSettings.forceForwardCompatibility = true;
					ambientOcclusionSettings.highPrecision = false;
					ambientOcclusionSettings.ambientOnly = false;
					postProcessingStack.profile.ambientOcclusion.settings = ambientOcclusionSettings;


					MotionBlurModel.Settings motionBlurSettings = postProcessingProfileRuntime.motionBlur.settings;
					postProcessingProfileRuntime.motionBlur.enabled = true;
					motionBlurSettings.shutterAngle = 120f;
					motionBlurSettings.sampleCount = 16;
					motionBlurSettings.frameBlending = 0f;

					postProcessingProfileRuntime.bloom.enabled = true;
					postProcessingProfileRuntime.vignette.enabled = true;
				}

				if (colorizationScript)
					colorizationScript.enabled = true;

				break;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		previousQLevel = qualityLevel;
		qualityLevel = QualitySettings.GetQualityLevel ();

		if (previousQLevel != qualityLevel || PlayerPrefs.GetInt ("UpdateCustom", 0) == 1) {
			PlayerPrefs.SetInt ("UpdateCustom", 0);
			Destroy (postProcessingProfileRuntime);
			Start ();
		}
	}
}
