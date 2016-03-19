using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Track))]
public class TrackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Track myTrack = (Track)target;
        if (GUILayout.Button("AddBifurcation"))
        {
            myTrack.AddBifurcation();
        }
        if (GUILayout.Button("AddCurve"))
        {
            myTrack.AddCurve();
        }

    }

}
