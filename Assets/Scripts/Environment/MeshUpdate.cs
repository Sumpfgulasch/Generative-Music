using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MeshUpdate
{
    // Private variables
    private static Vector3 playerMid;
    private static Transform thisTransform;
    
    // Constructor
    static MeshUpdate()
    {
        playerMid = Player.inst.transform.position;
        thisTransform = VisualController.inst.transform;
    }

    

    // public method

    public static void UpdateMeshes()
    {
        SetPlayerWidth();
        DrawEnvironmentEdges();
        UpdateEdgeParts();
        UpdateSurfacesTransforms();
    }






    // ----------------------------- private methods ----------------------------

    private static void UpdateSurfacesTransforms()
    {
        // Inner surface
        MeshRef.inst.innerSurface_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.vertices, thisTransform);
        MeshRef.inst.innerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(Player.inst.outerVertices, thisTransform);
        MeshRef.inst.innerPlayerMesh_mf.transform.localPosition = Vector3.zero;

        // Outer player
        MeshRef.inst.outerPlayerMesh_mf.transform.localScale = Player.inst.transform.localScale;
        MeshRef.inst.outerPlayerMesh_mf.transform.eulerAngles = Player.inst.transform.eulerAngles;

        MeshRef.inst.outerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.vertices, thisTransform);
        MeshRef.inst.innerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.vertices, thisTransform);
    }



    private static void SetPlayerWidth()
    {
        if (Player.inst.constantInnerWidth)
        {
            float neededWidthPerc = Mathf.Clamp01(Player.inst.innerWidth / Player.inst.transform.localScale.x);
            Vector3[] newVertices = MeshRef.inst.innerPlayerMesh_mf.mesh.vertices;

            if ((Player.inst.outerVertices[0] - Player.inst.innerVertices[0]).magnitude != Player.inst.innerWidth)
            {
                //TO DO: if draußen auf wand skalieren ist nicht aktiv
                for (int i = 0; i < Player.inst.verticesCount; i++)
                {
                    // change innerVertices only
                    Vector3 newVertex = (Player.inst.outerVertices_mesh[i] - Vector3.zero).normalized * (1 - neededWidthPerc);
                    newVertices[(i * 2) + 1] = newVertex;
                    Player.inst.innerVertices_mesh[i] = newVertex;
                    Player.inst.innerVertices_obj[i].position = playerMid + (Player.inst.outerVertices[i] - playerMid) * (1 - neededWidthPerc);
                    Player.inst.innerVertices[i] = Player.inst.innerVertices_obj[i].position;
                }
            }
            MeshRef.inst.innerPlayerMesh_mf.mesh.vertices = newVertices;
            MeshRef.inst.outerPlayerMesh_mf.mesh.vertices = newVertices;

            // TO DO: mesh.recalculatenormals, -bounds, -tangents
        }
    }



    private static void DrawEnvironmentEdges()
    {
        // = Draw lineRenderer lines for each edge (3)

        // 1) Add extra points for LineRenderer
        List<Vector3> newPositions = EnvironmentData.vertices.ToList();
        int insertCounter = 0;
        for (int i = 1; i < EnvironmentData.vertices.Length; i++)
        {
            // insert before
            newPositions.Insert(i + insertCounter, EnvironmentData.vertices[i]);
            insertCounter++;
            // insert after
            newPositions.Insert(i + 1 + insertCounter, EnvironmentData.vertices[i]);
            insertCounter++;
        }
        newPositions.Add(EnvironmentData.vertices[0]);


        // 2) Add to LineRenderer
        MeshRef.inst.envEdges_lr.positionCount = newPositions.Count;
        MeshRef.inst.envEdges_lr.SetPositions(newPositions.ToArray());
    }


    private static void UpdateEdgeParts()
    {
        // Environment
        for (int i = 0; i < EnvironmentData.vertices.Length; i++)
        {
            for (int j = 0; j < VisualController.inst.envGridLoops; j++)
            {
                Vector3 start = EnvironmentData.vertices[i] + (((EnvironmentData.vertices[(i + 1) % EnvironmentData.vertices.Length] - EnvironmentData.vertices[i]) / VisualController.inst.envGridLoops) * j);
                Vector3 end = EnvironmentData.vertices[i] + (((EnvironmentData.vertices[(i + 1) % EnvironmentData.vertices.Length] - EnvironmentData.vertices[i]) / VisualController.inst.envGridLoops) * (j+1));
                
                EnvironmentData.edgeParts[i * VisualController.inst.envGridLoops + j].Set(start, end);
            }
        }

        // Player
        if (Player.inst.curEdge.firstTouch)
        {
            Player.inst.curEdgePart.Visible = true;
        }
        else if (Player.inst.curEdge.leave)
        {
            Player.inst.curEdgePart.Visible = false;
        }
    }
}
