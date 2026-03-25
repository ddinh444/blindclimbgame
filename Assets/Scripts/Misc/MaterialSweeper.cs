using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MaterialSwapper : MonoBehaviour
{
    [Header("Settings")]
    public Material replacementMaterial;
    
    [Header("Exclusions")]
    [Tooltip("Add GameObjects here that should keep their original materials.")]
    public List<GameObject> excludedObjects = new List<GameObject>();

    [ContextMenu("Replace All Materials")]
    public void ReplaceMaterialsInScene()
    {
        if (replacementMaterial == null)
        {
            Debug.LogError("Please assign a Replacement Material first!");
            return;
        }

        // Find every MeshRenderer in the scene (including inactive ones)
        MeshRenderer[] allRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int changedCount = 0;

        foreach (MeshRenderer renderer in allRenderers)
        {
            // Check if this object or any of its parents are in the exclusion list
            if (IsExcluded(renderer.gameObject))
                continue;

            // Register for Undo (so you can Ctrl+Z in the editor)
            #if UNITY_EDITOR
            Undo.RecordObject(renderer, "Replace Material");
            #endif

            // Create an array of the same size filled with the new material
            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = replacementMaterial;
            }

            renderer.sharedMaterials = newMaterials;
            changedCount++;
        }

        Debug.Log($"Successfully replaced materials on {changedCount} objects.");
    }

    private bool IsExcluded(GameObject obj)
    {
        // Checks if the specific object is in the list
        if (excludedObjects.Contains(obj)) return true;

        // Optional: Check if any parent is excluded (to exclude entire hierarchies)
        foreach (GameObject excluded in excludedObjects)
        {
            if (excluded != null && obj.transform.IsChildOf(excluded.transform))
            {
                return true;
            }
        }

        return false;
    }
}