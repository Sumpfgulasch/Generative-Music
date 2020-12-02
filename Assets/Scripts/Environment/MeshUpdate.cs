using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshUpdate: MonoBehaviour
{
    public static MeshUpdate instance;

    private Vector3 playerMid;

    void Start()
    {
        playerMid = Player.instance.transform.position;
        instance = this;
    }


    void Update()
    {
        
    }

    // public method

    public void UpdateMeshes()
    {
        SetPlayerWidth();
        DrawEnvironmentEdges();
        DrawCurEdgePart();
        UpdateSurfacesTransforms();
    }






    // ----------------------------- private methods ----------------------------

    void UpdateSurfacesTransforms()
    {
        // Inner surface
        MeshRef.instance.innerSurface_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(MeshRef.instance.envVertices, this.transform);
        MeshRef.instance.innerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(Player.instance.outerVertices, this.transform);
        MeshRef.instance.innerPlayerMesh_mf.transform.localPosition = Vector3.zero;

        // Outer player
        MeshRef.instance.outerPlayerMesh_mf.transform.localScale = Player.instance.transform.localScale;
        MeshRef.instance.outerPlayerMesh_mf.transform.eulerAngles = Player.instance.transform.eulerAngles;

        MeshRef.instance.outerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(MeshRef.instance.envVertices, this.transform);
        MeshRef.instance.innerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(MeshRef.instance.envVertices, this.transform);
    }



    void SetPlayerWidth()
    {
        if (Player.instance.constantInnerWidth)
        {
            float neededWidthPerc = Mathf.Clamp01(Player.instance.innerWidth / Player.instance.transform.localScale.x);
            Vector3[] newVertices = MeshRef.instance.innerPlayerMesh_mf.mesh.vertices;

            if ((Player.instance.outerVertices[0] - Player.instance.innerVertices[0]).magnitude != Player.instance.innerWidth)
            {
                //TO DO: if draußen auf wand skalieren ist nicht aktiv
                for (int i = 0; i < Player.instance.verticesCount; i++)
                {
                    // change innerVertices only
                    Vector3 newVertex = (Player.instance.outerVertices_mesh[i] - Vector3.zero).normalized * (1 - neededWidthPerc);
                    newVertices[(i * 2) + 1] = newVertex;
                    Player.instance.innerVertices_mesh[i] = newVertex;
                    Player.instance.innerVertices_obj[i].position = playerMid + (Player.instance.outerVertices[i] - playerMid) * (1 - neededWidthPerc);
                    Player.instance.innerVertices[i] = Player.instance.innerVertices_obj[i].position;
                }
            }
            MeshRef.instance.innerPlayerMesh_mf.mesh.vertices = newVertices;
            MeshRef.instance.outerPlayerMesh_mf.mesh.vertices = newVertices;

            // TO DO: mesh.recalculatenormals, -bounds, -tangents
        }
    }



    void DrawEnvironmentEdges()
    {
        // 1) Add extra points for LineRenderer
        List<Vector3> newPositions = MeshRef.instance.envVertices.ToList();
        int insertCounter = 0;
        for (int i = 1; i < MeshRef.instance.envVertices.Length; i++)
        {
            // insert before
            newPositions.Insert(i + insertCounter, MeshRef.instance.envVertices[i]);
            insertCounter++;
            // insert after
            newPositions.Insert(i + 1 + insertCounter, MeshRef.instance.envVertices[i]);
            insertCounter++;
        }
        newPositions.Add(MeshRef.instance.envVertices[0]);


        // 2) Add to LineRenderer
        MeshRef.instance.envEdges_lr.positionCount = newPositions.Count;
        MeshRef.instance.envEdges_lr.SetPositions(newPositions.ToArray());
    }



    void DrawCurEdgePart()
    {
        if (Player.instance.actionState == Player.ActionState.stickToEdge)
        {
            if (Player.instance.positionState == Player.PositionState.innerEdge || Player.instance.positionState == Player.PositionState.outerEdge)
            {
                // primary
                Vector3 pos1 = Player.instance.curEnvEdge.Item1 + (Player.instance.curEnvEdge.Item2 - Player.instance.curEnvEdge.Item1) / VisualsManager.instance.envGridLoops * Player.instance.curEnvEdgePart;
                Vector3 pos2 = Player.instance.curEnvEdge.Item1 + (Player.instance.curEnvEdge.Item2 - Player.instance.curEnvEdge.Item1) / VisualsManager.instance.envGridLoops * (Player.instance.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                MeshRef.instance.curEdgePart_lr.positionCount = 2;
                MeshRef.instance.curEdgePart_lr.SetPosition(0, pos1);
                MeshRef.instance.curEdgePart_lr.SetPosition(1, pos2);

                // secondary
                pos1 = Player.instance.curEnvEdge_2nd.Item1 + (Player.instance.curEnvEdge_2nd.Item2 - Player.instance.curEnvEdge_2nd.Item1) / VisualsManager.instance.envGridLoops * Player.instance.curEnvEdgePart;
                pos2 = Player.instance.curEnvEdge_2nd.Item1 + (Player.instance.curEnvEdge_2nd.Item2 - Player.instance.curEnvEdge_2nd.Item1) / VisualsManager.instance.envGridLoops * (Player.instance.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                MeshRef.instance.curEdgePart2nd_lr[0].positionCount = 2;
                MeshRef.instance.curEdgePart2nd_lr[0].SetPosition(0, pos1);
                MeshRef.instance.curEdgePart2nd_lr[0].SetPosition(1, pos2);

                pos1 = Player.instance.curEnvEdge_3rd.Item1 + (Player.instance.curEnvEdge_3rd.Item2 - Player.instance.curEnvEdge_3rd.Item1) / VisualsManager.instance.envGridLoops * Player.instance.curEnvEdgePart;
                pos2 = Player.instance.curEnvEdge_3rd.Item1 + (Player.instance.curEnvEdge_3rd.Item2 - Player.instance.curEnvEdge_3rd.Item1) / VisualsManager.instance.envGridLoops * (Player.instance.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                MeshRef.instance.curEdgePart2nd_lr[1].positionCount = 2;
                MeshRef.instance.curEdgePart2nd_lr[1].SetPosition(0, pos1);
                MeshRef.instance.curEdgePart2nd_lr[1].SetPosition(1, pos2);
            }
            else
            {
                MeshRef.instance.curEdgePart_lr.positionCount = 0;
                MeshRef.instance.curEdgePart2nd_lr[0].positionCount = 0;
                MeshRef.instance.curEdgePart2nd_lr[1].positionCount = 0;
            }
        }
        else
        {
            MeshRef.instance.curEdgePart_lr.positionCount = 0;
            MeshRef.instance.curEdgePart2nd_lr[0].positionCount = 0;
            MeshRef.instance.curEdgePart2nd_lr[1].positionCount = 0;
        }
    }
}
