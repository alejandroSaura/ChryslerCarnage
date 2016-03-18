using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;
using System;


[Serializable]
class BifurcationData
{
    public NodeData[] nodesData;
    public BezierData[] splinesData;
    public float planeX;    
}

[ExecuteInEditMode]
public class Bifurcation : TrackElement
{
    // connectors
    public TrackElement nextCurveRight;
    public string nextCurveRightNodeSelector;
    public TrackElement nextCurveLeft;
    public string nextCurveLeftNodeSelector;
    // ----------

    public float planeX;

    string lastState = "";

    void Awake()
    {
        if (!(EditorApplication.isPlaying))
        {            
            Load();
            Save();
        }
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
            
        }
        if (state != lastState)
        {
            Load();
        }
        lastState = state;

        // On editor: save changes and recreate geometry
        if (state == "EditorMode")
        {
            if (nodes != null) Save();
            if (splines != null) Extrude();
        }

        // Maintain conection with next curves        
        Connect();
    }

    public override void Connect()
    {
        if (nextCurveRight != null)
        {
            if (nextCurveRightNodeSelector == "start")
                //nodes[1].Copy(nextCurveRight.nodes[0]);
                nextCurveRight.nodes[0].Copy(nodes[1]);
            else if (nextCurveRightNodeSelector == "right")
            {
                //nodes[1].Copy(nextCurveRight.nodes[1]);
                nextCurveRight.nodes[1].Copy(nodes[1]);
                //nodes[1].reverse = true;
                nextCurveRight.nodes[1].reverse = true;
            }
            else if (nextCurveRightNodeSelector == "end")
            {
                //nodes[1].Copy(nextCurveRight.nodes[1]);
                nextCurveRight.nodes[nextCurveRight.nodes.Count-1].Copy(nodes[1]);
                //nodes[1].reverse = true;
                nextCurveRight.nodes[nextCurveRight.nodes.Count - 1].reverse = true;
            }
            else if (nextCurveRightNodeSelector == "left")
            {
                //nodes[1].Copy(nextCurveRight.nodes[2]);
                nextCurveRight.nodes[2].Copy(nodes[1]);
                //nodes[1].reverse = true;
                nextCurveRight.nodes[2].reverse = true;
            }
            //nodes[1].Copy(nextCurveRight);
        }

        if (nextCurveLeft != null)
        {
            if (nextCurveLeftNodeSelector == "start")
                //nodes[2].Copy(nextCurveLeft.nodes[0]);
                nextCurveLeft.nodes[0].Copy(nodes[2]);
            else if (nextCurveLeftNodeSelector == "right")
            {
                //nodes[2].Copy(nextCurveLeft.nodes[1]);
                nextCurveLeft.nodes[1].Copy(nodes[2]);
                //nodes[2].reverse = true;
                nextCurveLeft.nodes[1].reverse = true;
            }
            else if (nextCurveLeftNodeSelector == "end")
            {
                //nodes[2].Copy(nextCurveLeft.nodes[1]);
                nextCurveLeft.nodes[nextCurveLeft.nodes.Count - 1].Copy(nodes[2]);
                //nodes[2].reverse = true;
                nextCurveLeft.nodes[nextCurveLeft.nodes.Count - 1].reverse = true;
            }
            else if (nextCurveLeftNodeSelector == "left")
            {
                //nodes[2].Copy(nextCurveLeft.nodes[2]);
                nextCurveLeft.nodes[2].Copy(nodes[2]);
                //nodes[2].reverse = true;
                nextCurveLeft.nodes[2].reverse = true;
            }
            //nodes[2].Copy(nextCurveLeft);
        }
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/ProceduralTracks/CurvesSavedData/" + gameObject.name + ".curve");

        BifurcationData data = new BifurcationData();

        List<NodeData> _nodesData = new List<NodeData>();
        if (nodes != null)
        {
            foreach (Node n in nodes)
            {
                _nodesData.Add(n.Serialize());
            }
            data.nodesData = _nodesData.ToArray();
        }

        List<BezierData> _splinesData = new List<BezierData>();
        foreach (BezierSpline b in splines)
        {
            _splinesData.Add(b.GetData());
        }
        data.splinesData = _splinesData.ToArray();

        data.planeX = planeX;


        bf.Serialize(file, data);
        file.Close();
    }

    public void Load()
    {
        if (File.Exists(Application.dataPath + "/ProceduralTracks/CurvesSavedData/" + gameObject.name + ".curve"))
        {
            ClearCurve();

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.dataPath + "/ProceduralTracks/CurvesSavedData/" + gameObject.name + ".curve", FileMode.Open);
            BifurcationData data = (BifurcationData)bf.Deserialize(file);
            file.Close();

            if (extrudeShape == null) { extrudeShape = new ExtrudeShape(); }

            for (int i = 0; i < data.nodesData.Length; ++i)
            {
                // Create Nodes                
                Node node = CreateNode(transform.position, transform.rotation);
                node.Load(data.nodesData[i]);                
            }

            planeX = data.planeX;            

            // Create Splines           
            CreateSpline(nodes[0], nodes[1]).ExtrudeSide(meshes[0], extrudeShape, "right", planeX);
            CreateSpline(nodes[0], nodes[2]).ExtrudeSide(meshes[1], extrudeShape, "left", planeX);

            splines[0].SetData(data.splinesData[0]);
            splines[1].SetData(data.splinesData[1]);
        }
        else
        {
            //Debug.Assert(true, "Data file not found");
            //Create();
        }
    }

    public void Create()
    {
        //ClearCurve();        

        //// Create Nodes                        
        //Node node1 = CreateNode(transform.position * newNodeDistance, 
        //   transform.rotation);
        //Node node2 = CreateNode(node1.position + node1.transform.forward * newNodeDistance + 0.5f * node1.transform.right * newNodeDistance, 
        //    transform.rotation);
        //Node node3 = CreateNode(node1.position + node1.transform.forward * newNodeDistance - 0.5f * node1.transform.right * newNodeDistance,
        //    transform.rotation);

        //// Create Splines        
        //CreateSpline(node1, node2).ExtrudeSide(meshes[0], extrudeShape, "right"); ;
        //CreateSpline(node1, node3).ExtrudeSide(meshes[1], extrudeShape, "left"); ;
        //planeX = 0;
    }

    public override Node CreateNode(Vector3 position, Quaternion rotation)
    {
        GameObject nodeGO = Instantiate(nodePrefab, position, transform.rotation) as GameObject;
        nodeGO.transform.parent = transform;
        Node node = nodeGO.GetComponent<Node>();

        node.frontTransform = node.transform.FindChild("front");
        node.backTransform = node.transform.FindChild("back");

        //node.position = position;
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

    public override void Extrude()
    {
        if (extrudeShape == null)
        {
            extrudeShape = new ExtrudeShape();
        }        
        splines[0].ExtrudeSide(meshes[0], extrudeShape, "right", planeX);
        splines[1].ExtrudeSide(meshes[1], extrudeShape, "left", planeX);       
    }

    public void AddSpline()
    {
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

    public void ClearCurve()
    {
        // Clear node references
        for (int i = 0; i < nodes.Count; ++i)
        {
            if (nodes[i] != null)
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
        while (transform.childCount != 0)
        {
            if (transform.GetChild(0) != null)
                DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }


}

