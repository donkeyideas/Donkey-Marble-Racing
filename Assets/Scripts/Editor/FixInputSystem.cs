using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class FixInputSystem
{
    [MenuItem("MarbleRace/Fix Input System (Run This!)")]
    public static void Fix()
    {
        // Find EventSystem
        var eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var esObj = new GameObject("EventSystem");
            eventSystem = esObj.AddComponent<EventSystem>();
        }

        // Remove old StandaloneInputModule if present
        var oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            Object.DestroyImmediate(oldModule);
            Debug.Log("Removed old StandaloneInputModule");
        }

        // Add new InputSystemUIInputModule if not present
        var newModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (newModule == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            Debug.Log("Added InputSystemUIInputModule");
        }

        EditorUtility.SetDirty(eventSystem.gameObject);
        Debug.Log("Input system fixed! Press Play to test.");
    }
}
