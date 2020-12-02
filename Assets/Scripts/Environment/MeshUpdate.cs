using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshUpdate: MonoBehaviour
{
    // = Draw stuff und update positions


    public static MeshUpdate instance;

    private Vector3 playerMid;

    void Start()
    {
        playerMid = PlayerData.instance.transform.position;
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
        MeshRef.instance.innerSurface_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.instance.curVertices, this.transform);
        MeshRef.instance.innerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(PlayerData.instance.outerVertices, this.transform);
        MeshRef.instance.innerPlayerMesh_mf.transform.localPosition = Vector3.zero;

        // Outer player
        MeshRef.instance.outerPlayerMesh_mf.transform.localScale = PlayerData.instance.transform.localScale;
        MeshRef.instance.outerPlayerMesh_mf.transform.eulerAngles = PlayerData.instance.transform.eulerAngles;

        MeshRef.instance.outerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.instance.curVertices, this.transform);
        MeshRef.instance.innerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(EnvironmentData.instance.curVertices, this.transform);
    }



    void SetPlayerWidth()
    {
        if (PlayerData.instance.constantInnerWidth)
        {
            float neededWidthPerc = Mathf.Clamp01(PlayerData.instance.innerWidth / PlayerData.instance.transform.localScale.x);
            Vector3[] newVertices = MeshRef.instance.innerPlayerMesh_mf.mesh.vertices;

            if ((PlayerData.instance.outerVertices[0] - PlayerData.instance.innerVertices[0]).magnitude != PlayerData.instance.innerWidth)
            {
                //TO DO: if draußen auf wand skalieren ist nicht aktiv
                for (int i = 0; i < PlayerData.instance.verticesCount; i++)
                {
                    // change innerVertices only
                    Vector3 newVertex = (PlayerData.instance.outerVertices_mesh[i] - Vector3.zero).normalized * (1 - neededWidthPerc);
                    newVertices[(i * 2) + 1] = newVertex;
                    PlayerData.instance.innerVertices_mesh[i] = newVertex;
                    PlayerData.instance.innerVertices_obj[i].position = playerMid + (PlayerData.instance.outerVertices[i] - playerMid) * (1 - neededWidthPerc);
                    PlayerData.instance.innerVertices[i] = PlayerData.instance.innerVertices_obj[i].position;
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
        List<Vector3> newPositions = EnvironmentData.instance.curVertices.ToList();
        int insertCounter = 0;
        for (int i = 1; i < EnvironmentData.instance.curVertices.Length; i++)
        {
            // insert before
            newPositions.Insert(i + insertCounter, EnvironmentData.instance.curVertices[i]);
            insertCounter++;
            // insert after
            newPositions.Insert(i + 1 + insertCounter, EnvironmentData.instance.curVertices[i]);
            insertCounter++;
        }
        newPositions.Add(EnvironmentData.instance.curVertices[0]);


        // 2) Add to LineRenderer
        MeshRef.instance.envEdges_lr.positionCount = newPositions.Count;
        MeshRef.instance.envEdges_lr.SetPositions(newPositions.ToArray());
    }



    void DrawCurEdgePart()
    {
        if (PlayerData.instance.actionState == PlayerData.ActionState.stickToEdge)
        {
            if (PlayerData.instance.positionState == PlayerData.PositionState.innerEdge || PlayerData.instance.positionState == PlayerData.PositionState.outerEdge)
            {
                // primary
                Vector3 pos1 = PlayerData.instance.curEnvEdge.Item1 + (PlayerData.instance.curEnvEdge.Item2 - PlayerData.instance.curEnvEdge.Item1) / VisualController.instance.envGridLoops * PlayerData.instance.curEnvEdgePart;
                Vector3 pos2 = PlayerData.instance.curEnvEdge.Item1 + (PlayerData.instance.curEnvEdge.Item2 - PlayerData.instance.curEnvEdge.Item1) / VisualController.instance.envGridLoops * (PlayerData.instance.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                MeshRef.instance.curEdgePart_lr.positionCount = 2;
                MeshRef.instance.curEdgePart_lr.SetPosition(0, pos1);
                MeshRef.instance.curEdgePart_lr.SetPosition(1, pos2);

                // secondary
                pos1 = PlayerData.instance.curEnvEdge_2nd.Item1 + (PlayerData.instance.curEnvEdge_2nd.Item2 - PlayerData.instance.curEnvEdge_2nd.Item1) / VisualController.instance.envGridLoops * PlayerData.instance.curEnvEdgePart;
                pos2 = PlayerData.instance.curEnvEdge_2nd.Item1 + (PlayerData.instance.curEnvEdge_2nd.Item2 - PlayerData.instance.curEnvEdge_2nd.Item1) / VisualController.instance.envGridLoops * (PlayerData.instance.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                MeshRef.instance.curEdgePart2nd_lr[0].positionCount = 2;
                MeshRef.instance.curEdgePart2nd_lr[0].SetPosition(0, pos1);
                MeshRef.instance.curEdgePart2nd_lr[0].SetPosition(1, pos2);

                pos1 = PlayerData.instance.curEnvEdge_3rd.Item1 + (PlayerData.instance.curEnvEdge_3rd.Item2 - PlayerData.instance.curEnvEdge_3rd.Item1) / VisualController.instance.envGridLoops * PlayerData.instance.curEnvEdgePart;
                pos2 = PlayerData.instance.curEnvEdge_3rd.Item1 + (PlayerData.instance.curEnvEdge_3rd.Item2 - PlayerData.instance.curEnvEdge_3rd.Item1) / VisualController.instance.envGridLoops * (PlayerData.instance.curEnvEdgePart + 1);
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
