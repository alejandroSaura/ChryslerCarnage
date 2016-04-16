using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
[CanEditMultipleObjects]
public class BezierSplineEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BezierSpline spline = (BezierSpline)target;
        if (GUILayout.Button("SplitSpline"))
        {
            spline.Split();
        }
        if (GUILayout.Button("SplitCurve"))
        {
            spline.SplitCurve();
        }

    }

}