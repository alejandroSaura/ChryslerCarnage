using UnityEngine;
using System.Collections.Generic;

public abstract class TrackElement : MonoBehaviour
{
    // for making paths
    public bool north_south = false;
    public bool north_east = false;
    public bool north_west = false;

    public bool south_east = false;
    public bool south_west = false;

    public bool east_west = false;
    //-----------------

    public TrackElement previousCurve;

    public float trackWidth;
    public int horizontalDivisions;
    public int divisionsPerCurve = 5;

    public float newNodeDistance = 10;

    public List<Node> nodes;
    public List<BezierSpline> splines;

    public List<Mesh> meshes;

    public GameObject nodePrefab;
    public GameObject splinePrefab;

    public ExtrudeShape extrudeShape;

    public abstract BezierSpline CreateSpline(Node start, Node end);
    public abstract Node CreateNode(Vector3 position, Quaternion rotation);
    public abstract void Connect();
    public abstract void Extrude();

}
