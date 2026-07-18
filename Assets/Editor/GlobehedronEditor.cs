using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Globehedron))]
public class GlobehedronEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draws the default inspector fields (variables)
        DrawDefaultInspector();

        // Get a reference to the target script
        Globehedron globehedron = (Globehedron)target;

        GUILayout.Space(10);
        // Create the button and check if it is clicked
        if (GUILayout.Button("Clear Babysitter Children"))
        {
            int n = globehedron.babysitter.transform.childCount;
            for (int i = n - 1; i >= 0; i--)
            {
                DestroyImmediate(globehedron.babysitter.transform.GetChild(i).gameObject);
            }
        }
        
        // Create the button and check if it is clicked
        if (GUILayout.Button("Generate"))
        {
            globehedron.Generate(null);
        }
    }
}