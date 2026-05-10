using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SceneCleanup : EditorWindow
{
    [MenuItem("MarbleRace/Clean Up and Rebuild Scene")]
    public static void CleanAndRebuild()
    {
        if (!EditorUtility.DisplayDialog("Clean Up Scene",
            "This will DELETE everything in the scene and rebuild it fresh with one clean copy.\n\nMake sure you are NOT in Play mode!",
            "Clean & Rebuild", "Cancel"))
            return;

        // Delete everything except the default camera, light, and global volume
        var toDelete = new List<GameObject>();
        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (var obj in rootObjects)
        {
            string name = obj.name;
            // Keep only the essentials that came with the scene template
            if (name == "Main Camera" || name == "Directional Light" || name == "Global Volume")
                continue;
            toDelete.Add(obj);
        }

        foreach (var obj in toDelete)
        {
            DestroyImmediate(obj);
        }

        // Also clean up any DontDestroyOnLoad objects from previous plays
        Debug.Log($"Deleted {toDelete.Count} objects. Scene is clean.");

        // Now run the setup wizard
        GameSetupWizard.SetupScene();
    }
}
