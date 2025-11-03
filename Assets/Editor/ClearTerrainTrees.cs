using UnityEditor;
using UnityEngine;

public class ClearTerrainTrees : EditorWindow
{
    [MenuItem("Tools/Clear Terrain Tree Prefabs")]
    static void ClearTrees()
    {
        if (Selection.activeObject is Terrain terrain)
        {
            var data = terrain.terrainData;
            if (data != null)
            {
                data.treePrototypes = new TreePrototype[0];
                data.RefreshPrototypes();
                EditorUtility.SetDirty(data);
                Debug.Log($"✅ Cleared tree prototypes from terrain data: {data.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ No TerrainData found on selected terrain.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Select a Terrain object first in the Hierarchy.");
        }
    }
}
