using UnityEngine;
using UnityEditor;

public static class PostProcessingSetup
{
    public static void Setup(UnityEngine.Camera mainCam)
    {
        // Enable post-processing on the camera
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

        // Create Global Volume using reflection (avoids compile-time dependency on render pipeline core)
        var volumeType = System.Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime");
        var profileType = System.Type.GetType("UnityEngine.Rendering.VolumeProfile, Unity.RenderPipelines.Core.Runtime");

        if (volumeType == null || profileType == null)
        {
            Debug.LogWarning("[MarbleRace] Could not find Volume types. Enable Post-Processing manually.");
            return;
        }

        var volumeObj = new GameObject("PostProcessVolume");
        var volume = volumeObj.AddComponent(volumeType);

        // Set isGlobal = true, priority = 1
        var isGlobalProp = volumeType.GetProperty("isGlobal");
        if (isGlobalProp != null) isGlobalProp.SetValue(volume, true);
        var priorityProp = volumeType.GetProperty("priority");
        if (priorityProp != null) priorityProp.SetValue(volume, 1f);

        // Create VolumeProfile
        var profile = ScriptableObject.CreateInstance(profileType);

        // Add volume overrides via reflection
        var addMethod = profileType.GetMethod("Add", new System.Type[] { typeof(System.Type), typeof(bool) });
        if (addMethod != null)
        {
            AddOverride(addMethod, profile, "UnityEngine.Rendering.Universal.Bloom, Unity.RenderPipelines.Universal.Runtime",
                new (string, float)[] { ("threshold", 0.9f), ("intensity", 1.5f), ("scatter", 0.7f) });

            AddOverride(addMethod, profile, "UnityEngine.Rendering.Universal.Vignette, Unity.RenderPipelines.Universal.Runtime",
                new (string, float)[] { ("intensity", 0.3f), ("smoothness", 0.4f) });

            AddOverride(addMethod, profile, "UnityEngine.Rendering.Universal.ColorAdjustments, Unity.RenderPipelines.Universal.Runtime",
                new (string, float)[] { ("postExposure", 0.3f), ("contrast", 15f), ("saturation", 10f) });

            AddOverride(addMethod, profile, "UnityEngine.Rendering.Universal.FilmGrain, Unity.RenderPipelines.Universal.Runtime",
                new (string, float)[] { ("intensity", 0.15f) });
        }

        // Save profile and assign to volume
        string profilePath = "Assets/Data/PostProcessProfile.asset";
        var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(profilePath);
        if (existing != null) AssetDatabase.DeleteAsset(profilePath);
        AssetDatabase.CreateAsset(profile, profilePath);

        var profileProp = volumeType.GetProperty("profile");
        if (profileProp != null) profileProp.SetValue(volume, profile);

        Debug.Log("[MarbleRace] Post-processing configured: Bloom, Vignette, Color Grading, Film Grain.");
    }

    static void AddOverride(System.Reflection.MethodInfo addMethod, Object profile, string typeName, (string field, float value)[] parameters)
    {
        var overrideType = System.Type.GetType(typeName);
        if (overrideType == null) return;

        var component = addMethod.Invoke(profile, new object[] { overrideType, true });
        if (component == null) return;

        foreach (var (field, value) in parameters)
        {
            var fieldInfo = overrideType.GetField(field);
            if (fieldInfo == null) continue;
            var param = fieldInfo.GetValue(component);
            if (param == null) continue;

            var overrideProp = param.GetType().GetProperty("overrideState");
            if (overrideProp != null) overrideProp.SetValue(param, true);

            var valueProp = param.GetType().GetProperty("value");
            if (valueProp != null) valueProp.SetValue(param, value);
        }
    }
}
