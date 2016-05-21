using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;
using System;


[Serializable]
class CurveData
{
    public bool closed;
    public bool invisible;
    public NodeData[] nodesData;
    public BezierData[] splinesData;
    
}

[ExecuteInEditMode]
public class Curve : TrackElement
{
    // connectors
    public TrackElement nextCurve;
    public string nextCurveNodeSelector = "";
    public string previousCurveNodeSelector = "";
    // ----------

    public bool invisible;

    bool closed = false;
    bool connected = false;
    
    string lastState = "";

    void Awake()
    {
        //if (!(EditorApplication.isPlaying))
        //{            
        //    if(splines.Count == 0) AddSpline();
        //    Save();
        //}
    }

    void LateUpdate()
    {
        // Reload curve when changing between editor and play modes
        string state = "";
        if (EditorApplication.isPlaying)
            state = "PlayMode";
        else
        {
            state = "EditorMode";            
            //Debug.Log("Saved");
        }
        if(state != lastState)
        {            
            //Load();
            Connect();
            //if (splines != null) Extrude();
        }
        lastState = state;

        // On editor: save changes and recreate geometry
        if (state == "EditorMode")
        {
            if (nodes != null) Save();
            //if (splines != null) Extrude();
            Connect();
        }

        
    }

    public override void Connect()
    {
        connected = false;
        // Maintain conection with next curve

        if (nextCurveNodeSelector == "doNothing") return;

        if (nextCurveNodeSelector == "start" || nextCurveNodeSelector == "")
        {
            if (nextCurve != null && nextCurve.nodes.Count > 0 && nextCurve.GetType() != typeof(Bifurcation))
            {
                nodes[nodes.Count - 1].Copy(nextCurve.nodes[0]);
                connected = true;

                nextCurve.previousCurve = this;
            }
            if (nextCurve != null && nextCurve.GetType() == typeof(Bifurcation))
            {// Conection to bifurcation
                nextCurve.nodes[0].Copy(nodes[nodes.Count - 1]);
                connected = true;

                nextCurve.previousCurve = this;
            }
        }
        else if (nextCurveNodeSelector == "end")
        {
            if (nextCurve != null && nextCurve.nodes.Count > 0 && nextCurve.GetType() != typeof(Bifurcation))
            {
                nodes[nodes.Count - 1].Copy(nextCurve.nodes[nextCurve.nodes.Count-1]);
                nextCurve.nodes[nextCurve.nodes.Count - 1].reverse = true;
                nodes[nodes.Count - 1].reverse = true;
                connected = true;

                ((Curve)nextCurve).nextCurve = this;
                ((Curve)nextCurve).nextCurveNodeSelector = "doNothing";
            }
        }

        if (previousCurve != null && previousCurveNodeSelector == "start")
        {
            nodes[0].Copy(previousCurve.nodes[0]);
            previousCurve.previousCurve = this;
            nodes[0].reverse = true;
        }

    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/ProceduralTracks/CurvesSavedData/" + gameObject.name + transform.parent.GetComponent<Track>().id + ".curve");        

        CurveData data = new CurveData();

        List<NodeData> _nodesData = new List<NodeData>();
        if (nodes != null)
        {
            foreach (Node n in nodes)
            {
                _nodesData.Add(n.Serialize());
            }
        }
        data.nodesData = _nodesData.ToArray();

        List<BezierData> _splinesData = new List<BezierData>();
        foreach (BezierSpline b in splines)
        {
            _splinesData.Add(b.GetData());
        }
        data.splinesData = _splinesData.ToArray();

        data.closed = this.closed;
        data.invisible = this.invisible;

        bf.Serialize(file, data);
        file.Close();
    }

    public void Load()
    {
        if(File.Exists(Application.dataPath + "/ProceduralTracks/CurvesSavedData/" + gameObject.name + ".curve"))
        {
            ClearCurve();

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.dataPath + "/ProceduralTracks/CurvesSavedData/" + gameObject.name + ".curve", FileMode.Open);
            CurveData data = (CurveData)bf.Deserialize(file);
            file.Close();

            if (extrudeShape == null) { extrudeShape = new ExtrudeShape(); }

            Node previousNode = null;
            for (int i = 0; i < data.nodesData.Length; ++i)
            {
                // Create Node                
                Node node = CreateNode(transform.position, transform.rotation);
                node.Load(data.nodesData[i]);

                // If not the first, create a spline with the previous one
                if (previousNode != null)
                {
                    BezierSpline spline = CreateSpline(previousNode, node);
                    spline.SetData(data.splinesData[i - 1]);
                    spline.Extrude(meshes[i - 1], extrudeShape);
                }
                previousNode = node;
            }

            this.closed = data.closed;
            if(closed)
            {
                BezierSpline spline = CreateSpline(nodes[nodes.Count-1], nodes[0]);
                spline.Extrude(meshes[meshes.Count-1], extrudeShape);
            }

            this.invisible = data.invisible;
        }
        else
        {
            Debug.Assert(true, "Data file not found");
        }
    }

    public override Node CreateNode(Vector3 position, Quaternion rotation)
    {
        GameObject nodeGO = Instantiate(nodePrefab, position, rotation) as GameObject;
        nodeGO.transform.parent = transform;        
        Node node = nodeGO.GetComponent<Node>();

        node.frontTransform = node.transform.FindChild("front");
        node.backTransform = node.transform.FindChild("back");

        node.position = position;
        node.curve = this;
        nodes.Add(node);        

        return node;
    }

    public override BezierSpline CreateSpline(Node start, Node end)
    {
        GameObject splineGO = Instantiate(splinePrefab, transform.position, transform.rotation) as GameObject;
        splineGO.transform.parent = transform;
        BezierSpline spline = splineGO.GetComponent<BezierSpline>();

        spline.curve = this;
        spline.startNode = start;
        spline.endNode = end;

        spline.transform.position = (start.transform.position + end.transform.position) / 2;

        splines.Add(spline);
        meshes.Add(new Mesh());

        return (spline);
    }

    public void DeleteSpline()
    {
        DestroyImmediate(nodes[nodes.Count - 1].gameObject);
        DestroyImmediate(splines[splines.Count - 1].gameObject);

        nodes.RemoveAt(nodes.Count - 1);
        splines.RemoveAt(splines.Count - 1);
        meshes.RemoveAt(meshes.Count - 1);
    }

    public override void Extrude()
    {     

        if (invisible)
        {
            for (int i = 0; i < splines.Count; ++i)
            {
                splines[i].gameObject.GetComponent<MeshCollider>().sharedMesh = null;
                splines[i].gameObject.GetComponent<MeshFilter>().sharedMesh = null;
            }
            
            return;
        }

        if (extrudeShape == null)
        {
            extrudeShape = new ExtrudeShape();
        }        

        for (int i = 0; i < splines.Count; ++i)
        {
            splines[i].Extrude(meshes[i], extrudeShape);
        }
    }    

    public void AddSpline ()
    {
        if (closed || connected) return;

        if (splines.Count == 0)
        {
            // Create the first segment           
            Node firstNode = CreateNode(transform.position, transform.rotation);
            Node secondNode = CreateNode(transform.position + transform.forward * newNodeDistance, transform.rotation);
            CreateSpline(firstNode, secondNode);
        }
        else
        {
            Node lastNode = nodes[nodes.Count - 1];          
            Node newNode = CreateNode(lastNode.position + lastNode.transform.forward * newNodeDistance, lastNode.transform.rotation);
            CreateSpline(lastNode, newNode);
        }
    }	

    public void CloseCurve()
    {
        if (closed || connected) return;

        Node lastNode = nodes[nodes.Count - 1];
        Node firstNode = nodes[0];
        CreateSpline(lastNode, firstNode);

        closed = true;       
    }

    public void ClearCurve()
    {
        // Clear node references
        for (int i = 0; i < nodes.Count; ++i)
        {
            if(nodes[i] != null)
                DestroyImmediate(nodes[i].gameObject);
        }
        nodes.Clear();

        // Clear spline references
        for (int i = 0; i < splines.Count; ++i)
        {
            if (splines[i] != null)
                DestroyImmediate(splines[i].gameObject);
        }
        splines.Clear();

        // Clear mesh references
        for (int i = 0; i < meshes.Count; ++i)
        {
            if (meshes[i] != null)
                DestroyImmediate(meshes[i]);
        }
        meshes.Clear();

        // destroy other unreferenced elements        
        while(transform.childCount != 0)
        {
            if(transform.GetChild(0) != null)
                DestroyImmediate(transform.GetChild(0).gameObject);
        }

        closed = false;

    }


    public void Split(BezierSpline splitSpline)
    {
        Debug.Log("Splitting curve");

        // splines split
        List<BezierSpline> firstSplines, secondSplines;
        firstSplines = new List<BezierSpline>();
        secondSplines = new List<BezierSpline>();
        // meshes split
        List<Mesh> firstMeshes, secondMeshes;
        firstMeshes = new List<Mesh>();
        secondMeshes = new List<Mesh>();

        int splitPoint = splines.IndexOf(splitSpline);
        for (int i = 0; i < splines.Count; ++i)
        {
            if (i < splitPoint)
            {
                firstSplines.Add(splines[i]);
                firstMeshes.Add(meshes[i]);
            }
            else
            {
                secondSplines.Add(splines[i]);
                secondMeshes.Add(meshes[i]);
            }
        }

        // nodes split
        List<Node> firstNodes, secondNodes;
        firstNodes = new List<Node>();
        secondNodes = new List<Node>();

        for (int i = 0; i < nodes.Count; ++i)
        {
            if (i < splitPoint+1) firstNodes.Add(nodes[i]);
            else secondNodes.Add(nodes[i]);
        }
        // Create new node in the splitting point
        Node newNode = CreateNode(Vector3.zero, Quaternion.identity);
        newNode.Copy(firstNodes[firstNodes.Count - 1]);
        secondNodes.Insert(0, newNode);
        // update first spline of the second group with the new node as start
        secondSplines[0].startNode = newNode; 


        // Create new curve
        Curve newCurve = GetComponentInParent<Track>().AddCurve();
        newCurve.ClearCurve();
        newCurve.transform.position = firstNodes[firstNodes.Count - 1].transform.position;
        newCurve.transform.rotation = firstNodes[firstNodes.Count - 1].transform.rotation;

        // reassignment
        newCurve.nodes = secondNodes;
        newCurve.splines = secondSplines;
        newCurve.meshes = secondMeshes;

        nodes = firstNodes;
        splines = firstSplines;
        meshes = firstMeshes;

        // reparent
        for (int i = 0; i < newCurve.splines.Count; ++i)
        {
            newCurve.splines[i].transform.parent = newCurve.transform;
        }
        for (int i = 0; i < newCurve.nodes.Count; ++i)
        {
            newCurve.nodes[i].transform.parent = newCurve.transform;
        }

        

    }

        
}
