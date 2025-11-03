using UnityEngine;
using UnityEditor;

public class ForceClearTerrainPrototypes
{
    [MenuItem("Tools/Force Clear All Terrain Tree References")]
    static void ForceClearAll()
    {
        foreach (var terrain in GameObject.FindObjectsOfType<Terrain>())
        {
            var data = terrain.terrainData;
            if (data == null)
            {
                Debug.LogWarning($"⚠️ {terrain.name} has no TerrainData assigned.");
                continue;
            }

            // Clear tree prototypes and instances
            data.treePrototypes = new TreePrototype[0];
            data.RefreshPrototypes();

            data.treeInstances = new TreeInstance[0];
            EditorUtility.SetDirty(data);

            Debug.Log($"✅ Fully cleared trees from TerrainData: {data.name}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("🌲 All terrain tree references fully wiped and saved.");
    }
}
