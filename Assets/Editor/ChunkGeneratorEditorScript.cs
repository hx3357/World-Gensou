using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChunkFactoryDebugger))]
public class ChunkGeneratorEditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!Application.isPlaying)
            return;
        if (GUILayout.Button("Generate Chunk"))
        {
            ChunkFactoryDebugger.instance.OnRegenerateMeshButtonClicked();
        }
    }
}
