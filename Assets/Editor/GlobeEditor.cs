using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(Globe))]
public class GlobeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draws the default inspector fields (variables)
        DrawDefaultInspector();

        // Get a reference to the target script
        Globe globe = (Globe)target;

        // Add space before the button
        GUILayout.Space(10);

        if (GUILayout.Button("Make Tiles"))
        {
            globe.MakeGoldbergFaces();
            globe.MakeTiles();
        }

        if (GUILayout.Button("Color Some"))
        {
            globe.ColorSomeTiles(0.2f);
        }

        GUILayout.Space(15);

        if (GUILayout.Button("Unload Unused Assets"))
        {
            Resources.UnloadUnusedAssets();
        }
    }

    
}