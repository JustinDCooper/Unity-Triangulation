using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TIN))]
public class TINEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TIN tin = (TIN)target;

        Rect lastRect = GUILayoutUtility.GetLastRect();
        Rect ButtonRect = new Rect(lastRect.x, lastRect.y, lastRect.xMax-lastRect.x, 20);

        if (GUI.Button(ButtonRect,"Generate New Vertices"))
        {
            tin.GenerateNewVertices();
        }

        //lastRect = GUILayoutUtility.GetLastRect();
        //ButtonRect = new Rect(lastRect.x, lastRect.y + 5 * EditorGUIUtility.singleLineHeight, lastRect.xMax - lastRect.x, 20);

        //if (GUI.Button(ButtonRect, "Triangulate"))
        //{
        //    tin.GenerateNewVertices();
        //}

        if (GUILayout.Button("Triangulate"))
        {
            tin.InitTriangulation();
        }
    }
}
