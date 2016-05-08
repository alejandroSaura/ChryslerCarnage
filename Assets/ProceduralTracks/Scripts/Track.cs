using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections;

[System.Serializable]
public class TrackData
{
    public int trackIdGenerator = Track.trackIdGenerator;

    public int curveId;
    public int bifId;

    public float trackWidth;
    public int horizontalDivisions;
    public int divisionsPerCurve;
}

[ExecuteInEditMode]
public class Track : MonoBehaviour
{
    public static int trackIdGenerator = 0;

    public int id;

    public static string savedDataPath = Application.dataPath + "/ProceduralTracks/CurvesSavedData/";

    public float trackWidth = 8.09f;
    public int horizontalDivisions = 6;
    public int divisionsPerCurve = 120;

    public int curveIdGenerator = 0;
    public int bifIdGenerator = 0;

    public Mesh mesh;    

    string lastState = "";
    void Update()
    {
        // Reload when changing between editor and play modes
        string state = "";
        if (EditorApplication.isPlaying)
            state = "PlayMode";
        else
        {
            state = "EditorMode";            
        }
        if (state != lastState)
        {
            Load();
            if (state == "PlayMode") //StartCoroutine(CombineMeshes()); 
                gameObject.GetComponent<MeshCollider>().enabled = false;
            if (state == "EditorMode")
            {
                ReactivateMeshes();
                RemovePath();
            }
        }
        lastState = state;

        // On editor: save changes and recreate geometry
        if (state == "EditorMode")
        {
            //Save();
        }
    }


    public Curve AddCurve()
    {
        Track trackScript = this;

        if (File.Exists(Track.savedDataPath + "curve" + trackScript.id + trackScript.curveIdGenerator + ".curve"))
        {
            File.Delete(Track.savedDataPath + "curve" + trackScript.id + trackScript.curveIdGenerator + ".curve");
        }
        GameObject curvePrefab = (GameObject)Resources.Load("CurvePrefab");
        GameObject go = Instantiate(curvePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        go.name = "curve" + trackScript.id + trackScript.curveIdGenerator;
        ++trackScript.curveIdGenerator;
        trackScript.Save();
        go.transform.parent = trackScript.transform;


        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;

        return go.GetComponent<Curve>();
    }


    public void AddBifurcation()
    {
        Track trackScript = this;

        if (File.Exists(Track.savedDataPath + "bifurcation" + trackScript.id + trackScript.bifIdGenerator + ".curve"))
        {
            File.Delete(Track.savedDataPath + "bifurcation" + trackScript.id + trackScript.bifIdGenerator + ".curve");
        }
        GameObject bifurcationPrefab = (GameObject)Resources.Load("BifurcationPrefab");
        GameObject go = Instantiate(bifurcationPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        go.name = "bifurcation" + trackScript.id + trackScript.bifIdGenerator;
        ++trackScript.bifIdGenerator;
        trackScript.Save();
        go.transform.parent = trackScript.transform;

        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }


    void Start()
    {
        //SetPath("east_west");
    }

    public void RemovePath()
    {
        BezierSpline[] splines = gameObject.GetComponentsInChildren<BezierSpline>();

        foreach (BezierSpline s in splines)
        {
            s.isPath = false;
        }
    }

    public void SetPath(string path)
    {
        BezierSpline[] splines = gameObject.GetComponentsInChildren<BezierSpline>();

        if (splines.Length == 0 || path == "" || path == null)
        {
            foreach (BezierSpline s in splines)
            {
                s.isPath = false;
            }
            return;
        }
        
        foreach (BezierSpline s in splines)
        {
            bool isPath = (bool)typeof(BezierSpline).GetField(path).GetValue(s);
            if (isPath)
            {
                s.isPath = true;
            }
            else s.isPath = false;
        }
    }


    public void ReactivateMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();        
        int i = 0;
        while (i < meshFilters.Length)
        {
            if (meshFilters[i].gameObject != gameObject)
            {
                //meshFilters[i].gameObject.SetActive(true);
                meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = true;
                meshFilters[i].gameObject.GetComponent<MeshCollider>().enabled = true;
            }
            i++;
        }
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    IEnumerator CombineMeshes()
    {
        TrackElement[] elements = GetComponentsInChildren<TrackElement>();
        int j = 0;
        while (j < elements.Length)
        {
            elements[j].Connect();
            elements[j].Extrude();
            j++;
        }

        yield return null; // Wait for the gameobjects to connect

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            if (meshFilters[i].gameObject != gameObject)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = transform.worldToLocalMatrix * (meshFilters[i].transform.localToWorldMatrix);
                //meshFilters[i].gameObject.SetActive(false);   

                meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
                meshFilters[i].gameObject.GetComponent<MeshCollider>().enabled = false;
            }
            i++;
        }
        if (gameObject.GetComponent<MeshFilter>() == null)
                    gameObject.AddComponent<MeshFilter>();
        if (gameObject.GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        if (gameObject.GetComponent<MeshCollider>() == null)
            gameObject.AddComponent<MeshCollider>();

         
        transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        transform.GetComponent<MeshCollider>().sharedMesh = transform.GetComponent<MeshFilter>().sharedMesh;        
    }


    public void InstantCombineMeshes()
    {
        TrackElement[] elements = GetComponentsInChildren<TrackElement>();
        int j = 0;
        while (j < elements.Length)
        {
            elements[j].Connect();
            elements[j].Extrude();
            j++;
        }

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            if (meshFilters[i].gameObject != gameObject)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = transform.worldToLocalMatrix * (meshFilters[i].transform.localToWorldMatrix);
                //meshFilters[i].gameObject.SetActive(false);   

                //meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
                //meshFilters[i].gameObject.GetComponent<MeshCollider>().enabled = false;
            }
            i++;
        }
        if (gameObject.GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (gameObject.GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        if (gameObject.GetComponent<MeshCollider>() == null)
            gameObject.AddComponent<MeshCollider>();


        transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        transform.GetComponent<MeshCollider>().sharedMesh = transform.GetComponent<MeshFilter>().sharedMesh;
    }

    public void Save()
    {
        //Debug.Log("Track ids saved");
        if (!AssetDatabase.IsValidFolder(Track.savedDataPath.Remove(Track.savedDataPath.Length - 1)))
            Directory.CreateDirectory(Track.savedDataPath.Remove(Track.savedDataPath.Length - 1));

        TrackElement[] children = transform.GetComponentsInChildren<TrackElement>();
        foreach (TrackElement e in children)
        {
            e.trackWidth = trackWidth;
            e.horizontalDivisions = horizontalDivisions;
            e.divisionsPerCurve = divisionsPerCurve;
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Track.savedDataPath + gameObject.name + ".curve");

        TrackData data = new TrackData();
        data.bifId = bifIdGenerator;
        data.curveId = curveIdGenerator;

        data.trackWidth = trackWidth;
        data.horizontalDivisions = horizontalDivisions;
        data.divisionsPerCurve = divisionsPerCurve;

       data.trackIdGenerator = Track.trackIdGenerator;

        bf.Serialize(file, data);
        file.Close();
    }

    public void Load()
    {
        if (File.Exists(Track.savedDataPath + gameObject.name + ".curve"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Track.savedDataPath + gameObject.name + ".curve", FileMode.Open);
            TrackData data = (TrackData)bf.Deserialize(file);
            file.Close();

            curveIdGenerator = data.curveId;
            bifIdGenerator = data.bifId;

            trackWidth = data.trackWidth;
            horizontalDivisions = data.horizontalDivisions;
            divisionsPerCurve = data.divisionsPerCurve;

            Track.trackIdGenerator = data.trackIdGenerator;
        }
    }

    public void OnDestroy()
    {
        FileUtil.DeleteFileOrDirectory(Track.savedDataPath.Remove(Track.savedDataPath.Length - 1));
        Directory.CreateDirectory(Track.savedDataPath.Remove(Track.savedDataPath.Length - 1));
    }

}


