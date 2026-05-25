using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldGenerator gen = (WorldGenerator)target;

        if (GUILayout.Button("Regenerate World"))
        {
            gen.GenerateWorld();
        }
    }
}