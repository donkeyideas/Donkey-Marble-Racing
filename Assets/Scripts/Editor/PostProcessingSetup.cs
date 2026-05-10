using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static class PostProcessingSetup
{
    public static void Setup(UnityEngine.Camera mainCam)
    {
        // Enable post-processing on the camera via UniversalAdditionalCameraData
        // Use reflection to avoid hard compile-time dependency on URP assembly
        var urpCamDataType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (urpCamDataType != null)
        {
            var camData = mainCam.GetComponent(urpCamDataType);
            if (camData != null)
            {
                var renderPostProp = urpCamDataType.GetProperty("renderPostProcessing");
                if (renderPostProp != null) renderPostProp.SetValue(camData, true);
            }
        }

        // Create Global Volume
        var volumeObj = new GameObject("PostProcessVolume");
        var volume = volumeObj.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1;

        // Create Volume Profile asset
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Use reflection to add URP-specific overrides (Bloom, Vignette, etc.)
        AddBloom(profile);
        AddVignette(profile);
        AddColorAdjustments(profile);
        AddFilmGrain(profile);

        // Save profile as asset
        string profilePath = "Assets/Data/PostProcessProfile.asset";
        if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath) != null)
            AssetDatabase.DeleteAsset(profilePath);
        AssetDatabase.CreateAsset(profile, profilePath);
        volume.profile = profile;

        Debug.Log("[MarbleRace] Post-processing configured: Bloom, Vignette, Color Grading, Film Grain.");
    }

    static void AddBloom(VolumeProfile profile)
    {
        var bloomType = System.Type.GetType("UnityEngine.Rendering.Universal.Bloom, Unity.RenderPipelines.Universal.Runtime");
        if (bloomType == null) return;
        var bloom = profile.Add(bloomType, true);
        SetVolumeParam(bloom, "threshold", 0.9f);
        SetVolumeParam(bloom, "intensity", 1.5f);
        SetVolumeParam(bloom, "scatter", 0.7f);
    }

    static void AddVignette(VolumeProfile profile)
    {
        var vignetteType = System.Type.GetType("UnityEngine.Rendering.Universal.Vignette, Unity.RenderPipelines.Universal.Runtime");
        if (vignetteType == null) return;
        var vignette = profile.Add(vignetteType, true);
        SetVolumeParam(vignette, "intensity", 0.3f);
        SetVolumeParam(vignette, "smoothness", 0.4f);
    }

    static void AddColorAdjustments(VolumeProfile profile)
    {
        var colorType = System.Type.GetType("UnityEngine.Rendering.Universal.ColorAdjustments, Unity.RenderPipelines.Universal.Runtime");
        if (colorType == null) return;
        var color = profile.Add(colorType, true);
        SetVolumeParam(color, "postExposure", 0.3f);
        SetVolumeParam(color, "contrast", 15f);
        SetVolumeParam(color, "saturation", 10f);
    }

    static void AddFilmGrain(VolumeProfile profile)
    {
        var grainType = System.Type.GetType("UnityEngine.Rendering.Universal.FilmGrain, Unity.RenderPipelines.Universal.Runtime");
        if (grainType == null) return;
        var grain = profile.Add(grainType, true);
        SetVolumeParam(grain, "intensity", 0.15f);
    }

    static void SetVolumeParam(VolumeComponent component, string fieldName, float value)
    {
        var field = component.GetType().GetField(fieldName);
        if (field == null) return;
        var param = field.GetValue(component);
        if (param == null) return;

        // Set overrideState = true
        var overrideProp = param.GetType().GetProperty("overrideState");
        if (overrideProp != null) overrideProp.SetValue(param, true);

        // Set value
        var valueProp = param.GetType().GetProperty("value");
        if (valueProp != null) valueProp.SetValue(param, value);
    }
}
