using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(MapManager))]
public class MapUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MapManager generateMap =(MapManager)target;
        if (GUILayout.Button("ReGenerateMap"))
        {
            generateMap.ReGenerateMap();
        }

    }
}
