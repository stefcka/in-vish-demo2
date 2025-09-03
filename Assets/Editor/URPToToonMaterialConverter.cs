using UnityEngine;
using UnityEditor;

public class URPToToonMaterialConverter
{
    [MenuItem("Tools/Convert Selected URP Lit Materials to Toon")]
    public static void ConvertSelectedMaterials()
    {
        UnityEngine.Object[] selectedObjects = Selection.GetFiltered(typeof(Material), SelectionMode.Assets);
        int convertedCount = 0;

        foreach (UnityEngine.Object obj in selectedObjects)
        {
            Material mat = obj as Material;
            if (mat == null) continue;

            if (mat.shader.name != "Universal Render Pipeline/Lit")
                continue;

            // Cache original values
            Texture baseMap = mat.GetTexture("_BaseMap");
            Color baseColor = mat.GetColor("_BaseColor");
            Texture normalMap = mat.GetTexture("_BumpMap");

            // Find Toon Shader
            Shader toonShader = Shader.Find("Universal Render Pipeline/Toon/Lit");
            if (toonShader == null)
            {
                UnityEngine.Debug.LogError("Toon shader not found. Make sure URP Toon Shader Graph is installed.");
                return;
            }

            // Switch shader and reassign properties
            mat.shader = toonShader;
            if (baseMap) mat.SetTexture("_BaseMap", baseMap);
            mat.SetColor("_BaseColor", baseColor);
            if (normalMap) mat.SetTexture("_BumpMap", normalMap);

            EditorUtility.SetDirty(mat);
            convertedCount++;
        }

        AssetDatabase.SaveAssets();
        UnityEngine.Debug.Log($"Converted {convertedCount} selected material(s) to Toon shader.");
    }
}
