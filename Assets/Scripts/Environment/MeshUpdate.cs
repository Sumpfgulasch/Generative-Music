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
        playerMid = Player.inst.transform.position;
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
        MeshRef.inst.innerSurface_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.vertices, this.transform);
        MeshRef.inst.innerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(Player.inst.outerVertices, this.transform);
        MeshRef.inst.innerPlayerMesh_mf.transform.localPosition = Vector3.zero;

        // Outer player
        MeshRef.inst.outerPlayerMesh_mf.transform.localScale = Player.inst.transform.localScale;
        MeshRef.inst.outerPlayerMesh_mf.transform.eulerAngles = Player.inst.transform.eulerAngles;

        MeshRef.inst.outerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.vertices, this.transform);
        MeshRef.inst.innerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.vertices, this.transform);
    }



    void SetPlayerWidth()
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



    void DrawEnvironmentEdges()
    {
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



    void DrawCurEdgePart()
    {
        if (Player.inst.actionState == Player.ActionState.stickToEdge)
        {
            if (Player.inst.positionState == Player.PositionState.innerEdge || Player.inst.positionState == Player.PositionState.outerEdge)
            {
                // primary
                Vector3 pos1 = Player.inst.curEnvEdge.Item1 + (Player.inst.curEnvEdge.Item2 - Player.inst.curEnvEdge.Item1) / VisualsManager.instance.envGridLoops * Player.inst.curEnvEdgePart;
                Vector3 pos2 = Player.inst.curEnvEdge.Item1 + (Player.inst.curEnvEdge.Item2 - Player.inst.curEnvEdge.Item1) / VisualsManager.instance.envGridLoops * (Player.inst.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                MeshRef.inst.curEdgePart_lr.positionCount = 2;
                MeshRef.inst.curEdgePart_lr.SetPosition(0, pos1);
                MeshRef.inst.curEdgePart_lr.SetPosition(1, pos2);

                // secondary
                pos1 = Player.inst.curEnvEdge_2nd.Item1 + (Player.inst.curEnvEdge_2nd.Item2 - Player.inst.curEnvEdge_2nd.Item1) / VisualsManager.instance.envGridLoops * Player.inst.curEnvEdgePart;
                pos2 = Player.inst.curEnvEdge_2nd.Item1 + (Player.inst.curEnvEdge_2nd.Item2 - Player.inst.curEnvEdge_2nd.Item1) / VisualsManager.instance.envGridLoops * (Player.inst.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                MeshRef.inst.curEdgePart2nd_lr[0].positionCount = 2;
                MeshRef.inst.curEdgePart2nd_lr[0].SetPosition(0, pos1);
                MeshRef.inst.curEdgePart2nd_lr[0].SetPosition(1, pos2);

                pos1 = Player.inst.curEnvEdge_3rd.Item1 + (Player.inst.curEnvEdge_3rd.Item2 - Player.inst.curEnvEdge_3rd.Item1) / VisualsManager.instance.envGridLoops * Player.inst.curEnvEdgePart;
                pos2 = Player.inst.curEnvEdge_3rd.Item1 + (Player.inst.curEnvEdge_3rd.Item2 - Player.inst.curEnvEdge_3rd.Item1) / VisualsManager.instance.envGridLoops * (Player.inst.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                MeshRef.inst.curEdgePart2nd_lr[1].positionCount = 2;
                MeshRef.inst.curEdgePart2nd_lr[1].SetPosition(0, pos1);
                MeshRef.inst.curEdgePart2nd_lr[1].SetPosition(1, pos2);
            }
            else
            {
                MeshRef.inst.curEdgePart_lr.positionCount = 0;
                MeshRef.inst.curEdgePart2nd_lr[0].positionCount = 0;
                MeshRef.inst.curEdgePart2nd_lr[1].positionCount = 0;
            }
        }
        else
        {
            MeshRef.inst.curEdgePart_lr.positionCount = 0;
            MeshRef.inst.curEdgePart2nd_lr[0].positionCount = 0;
            MeshRef.inst.curEdgePart2nd_lr[1].positionCount = 0;
        }
    }
}
