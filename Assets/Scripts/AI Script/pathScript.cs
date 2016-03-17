using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class pathScript : MonoBehaviour {
    private List<Transform> nodes;
    private Color rayColour = Color.red;
	// Use this for initialization

    void OnDrawGizmos()
    {
        Gizmos.color = rayColour;
        Transform[] pathNode = GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        //foreach (Transform pathNodes in pathNode)
        //{
        //    if (pathNodes != transform)
        //    {
        //        nodes.Add(pathNodes);
        //    }
        //}

        for (int i = 0; i < pathNode.Length; i++)
        {
            if (pathNode[i] != transform)
            {
                nodes.Add(pathNode[i]);
            }
        }
            for(int i = 0;i<nodes.Count;i++)
            {
                Vector3 currentNode = nodes[i].position;
                Vector3 prevNode = Vector3.zero;
                if (i > 0)
                {
                    prevNode = nodes[i - 1].position;
                }
                else if(i ==0 && nodes.Count >1)
                {
                    prevNode = nodes[nodes.Count - 1].position;
                }
            Gizmos.DrawLine(prevNode, currentNode);
            Gizmos.DrawWireSphere(currentNode, 5.0f);
        }
        
        
    }

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
