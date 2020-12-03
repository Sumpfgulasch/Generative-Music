using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnvironmentData
{
    // == calc stuff and store data here and in Player
    

    // Public attributes
    [HideInInspector] public static Vector3[] vertices = new Vector3[3];



    // Private variables
    private static Vector3 playerMid;
    private static Player player;



    // Constructor
    static EnvironmentData()
    {
        playerMid = Player.inst.transform.position;
        player = Player.inst;
    } 

    // ----------------------------- Public methods ----------------------------

    public static void HandleData()
    {
        GetEnvironmentTriangle();
    }




    // ----------------------------- Private methods ----------------------------

    private static void GetEnvironmentTriangle()
    {
        // 1) init
        RaycastHit[] edgeHits = new RaycastHit[3];
        vertices = new Vector3[3];
        Vector3 intersection = Vector3.zero;

        // 2) Prepare raycast
        for (int i = 0; i < Player.inst.verticesCount; i++)
        {
            Vector3 playerEdgeMid = Player.inst.outerVertices[i] + ((Player.inst.outerVertices[(i + 1) % 3] - Player.inst.outerVertices[i]) / 2f);
            playerEdgeMid.z = MeshRef.inst.envEdges_lr.transform.position.z;
            Vector3 directionOut = (playerEdgeMid - playerMid).normalized;
            RaycastHit hit;

            // 3) Raycasts from player to environment
            if (Physics.Raycast(playerMid, directionOut, out hit))
            {
                edgeHits[i] = hit;
            }
        }
        // 4) Final: Construct environment triangle by line intersections
        for (int i = 0; i < edgeHits.Length; i++)
        {
            Vector3 point1, point2;
            Vector3 direction1, direction2;
            point1 = edgeHits[i].point;
            point1.z = edgeHits[0].point.z;
            point2 = edgeHits[(i + 1) % 3].point;
            point2.z = edgeHits[0].point.z;
            direction1 = Vector3.Cross(Vector3.forward, edgeHits[i].normal);
            direction1.z = 0;
            direction2 = Vector3.Cross(Vector3.forward, edgeHits[(i + 1) % 3].normal);
            direction2.z = 0;

            if (ExtensionMethods.LineLineIntersection(out intersection, point1, direction1, point2, direction2))
            {
                vertices[i] = intersection;

                // 5) Sort (hack): start with upper vertex & go CLOCKWISE 
                //if (intersection.y > 0.1f)
                //    vertices[0] = intersection;
                //else if (intersection.x < -0.1f)
                //    vertices[2] = intersection;
                //else
                //    vertices[1] = intersection;
            }
        }
    }

    private static void GenerateEdgeParts()
    {

    }

}
