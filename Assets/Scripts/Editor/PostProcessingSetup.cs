using UnityEngine;
using UnityEditor;

public static class PostProcessingSetup
{
    public static void Setup(UnityEngine.Camera mainCam)
    {
        // Post-processing is best enabled manually in Unity:
        // 1. Select the camera -> check "Post Processing" in the inspector
        // 2. Add a Global Volume object -> add Bloom, Vignette overrides
        // 3. In URP Asset settings, enable MSAA or SMAA for anti-aliasing
        Debug.Log("[MarbleRace] Enable Post-Processing manually: Select Camera > check 'Post Processing'. Add a Global Volume for Bloom/Vignette.");
    }
}
