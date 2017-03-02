﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapScript))]
// Displays Unity Edtior modifications
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        MapScript mapGen = (MapScript)target;

        if (DrawDefaultInspector())
        {
            mapGen.GenerateMap();
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
}
