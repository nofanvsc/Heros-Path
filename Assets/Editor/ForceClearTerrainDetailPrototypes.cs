using UnityEngine;
using UnityEditor;

public class ForceClearTerrainDetailPrototypes
{
    [MenuItem("Tools/Force Clear All Terrain Detail References")]
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

            // Clear painted details (grass/bushes)
            int detailLayerCount = data.detailPrototypes.Length;
            for (int i = 0; i < detailLayerCount; i++)
            {
                int[,] emptyLayer = new int[data.detailWidth, data.detailHeight];
                data.SetDetailLayer(0, 0, i, emptyLayer);
            }

            // Clear the detail prototypes (the references themselves)
            data.detailPrototypes = new DetailPrototype[0];

            EditorUtility.SetDirty(data);
            Debug.Log($"✅ Fully cleared details from TerrainData: {data.name}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("🌿 All terrain detail references and painted data fully wiped and saved.");
    }
}
