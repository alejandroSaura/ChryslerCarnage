using UnityEditor;
using UnityEngine;
using System.IO;

public class MenuExtension : MonoBehaviour
{   
    static GameObject GetTrack()
    {
        GameObject track = GameObject.Find("ProceduralTrack");
        if (track == null)
        {
            track = new GameObject("ProceduralTrack");
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(track, "Create " + track.name);
            Selection.activeObject = track;
            track.AddComponent<Track>();
            track.AddComponent<MeshRenderer>();
            track.AddComponent<MeshCollider>();

            if (!AssetDatabase.IsValidFolder(Track.savedDataPath.Remove(Track.savedDataPath.Length - 1)))
                Directory.CreateDirectory(Track.savedDataPath.Remove(Track.savedDataPath.Length - 1));

        }
        return track;
    }


    [MenuItem("Track/ReCreateGeometry _e")]
    static void FirstCommand()
    {
        Debug.Log("Recreating geometry");

        Curve[] curves = GameObject.FindObjectsOfType<Curve>();
        foreach (Curve c in curves)
        {
            c.Extrude();
        }

        Bifurcation[] bifurcations = GameObject.FindObjectsOfType<Bifurcation>();
        foreach (Bifurcation b in bifurcations)
        {
            b.Extrude();
        }
    }


    [MenuItem("Track/AddTrack")]
    static void AddTrack()
    {
        int newId = Track.trackIdGenerator++;
        GameObject track = new GameObject("ProceduralTrack" + newId);
        Selection.activeObject = track;
        track.AddComponent<Track>();
        track.AddComponent<MeshRenderer>();
        track.AddComponent<MeshCollider>();
        track.GetComponent<Track>().id = newId;

        Undo.RegisterCreatedObjectUndo(track, "Create " + track.name);
        Selection.activeObject = track;
    }

    //[MenuItem("Track/AddCurve")]
    //static void AddCurve()
    //{        
    //    Track trackScript = GetTrack().GetComponent<Track>();


    //    if (File.Exists(Track.savedDataPath + "curve" + trackScript.curveIdGenerator + ".curve"))
    //    {
    //        File.Delete(Track.savedDataPath + "curve" + trackScript.curveIdGenerator + ".curve");
    //    }
    //    GameObject curvePrefab = (GameObject)Resources.Load("CurvePrefab");
    //    GameObject go = Instantiate(curvePrefab, Vector3.zero, Quaternion.identity) as GameObject;
    //    go.name = "curve" + trackScript.curveIdGenerator;
    //    ++trackScript.curveIdGenerator;
    //    trackScript.Save();
    //    go.transform.parent = trackScript.transform;   


    //    Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
    //    Selection.activeObject = go;
    //}

    //[MenuItem("Track/AddBifurcation")]
    //static void AddBifurcation()
    //{
    //    Track trackScript = GetTrack().GetComponent<Track>();

    //    if (File.Exists(Track.savedDataPath + "bifurcation" + trackScript.bifIdGenerator + ".curve"))
    //    {
    //        File.Delete(Track.savedDataPath + "bifurcation" + trackScript.bifIdGenerator + ".curve");
    //    }
    //    GameObject bifurcationPrefab = (GameObject)Resources.Load("BifurcationPrefab");
    //    GameObject go = Instantiate(bifurcationPrefab, Vector3.zero, Quaternion.identity) as GameObject;
    //    go.name = "bifurcation" + trackScript.bifIdGenerator;
    //    ++trackScript.bifIdGenerator;
    //    trackScript.Save();
    //    go.transform.parent = trackScript.transform;

    //    Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
    //    Selection.activeObject = go;
    //}
}
