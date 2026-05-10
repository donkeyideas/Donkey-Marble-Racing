using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PostProcessingSetup
{
    public static void Setup(UnityEngine.Camera mainCam)
    {
        // Enable post-processing on camera
        var camData = mainCam.GetUniversalAdditionalCameraData();
        if (camData != null)
        {
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
        }

        // Create Global Volume
        var volumeObj = new GameObject("PostProcessVolume");
        var volume = volumeObj.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1;

        // Create Volume Profile asset
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Bloom
        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.value = 0.9f;
        bloom.threshold.overrideState = true;
        bloom.intensity.value = 1.5f;
        bloom.intensity.overrideState = true;
        bloom.scatter.value = 0.7f;
        bloom.scatter.overrideState = true;

        // Vignette
        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.value = 0.3f;
        vignette.intensity.overrideState = true;
        vignette.smoothness.value = 0.4f;
        vignette.smoothness.overrideState = true;
        vignette.color.value = new Color(0.05f, 0.0f, 0.1f);
        vignette.color.overrideState = true;

        // Color Adjustments
        var colorAdj = profile.Add<ColorAdjustments>(true);
        colorAdj.postExposure.value = 0.3f;
        colorAdj.postExposure.overrideState = true;
        colorAdj.contrast.value = 15f;
        colorAdj.contrast.overrideState = true;
        colorAdj.saturation.value = 10f;
        colorAdj.saturation.overrideState = true;

        // Film Grain (subtle)
        var grain = profile.Add<FilmGrain>(true);
        grain.intensity.value = 0.15f;
        grain.intensity.overrideState = true;

        // Save profile as asset
        string profilePath = "Assets/Data/PostProcessProfile.asset";
        AssetDatabase.CreateAsset(profile, profilePath);
        volume.profile = profile;

        Debug.Log("[MarbleRace] Post-processing configured: Bloom, Vignette, Color Grading, Film Grain.");
    }
}
