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

        // Event subscription
        GameEvents.inst.onFieldChange += OnFieldChange;
    }



    // ----------------------------- public methods ----------------------------
    

    /// <summary>
    /// Get vertices from tunnel, create form and update vertices variables of music fields. Set Z to player.z.
    /// </summary>
    public static void UpdateFieldsPositions()
    {
        // 1. Tunnel
        GetTunnelVertices();                // einmalig
        UpdateFieldsVertices();             // einmalig
    }


    /// <summary>
    /// Set vertices and visibility of focused/played player field, set player width, set all mask-mesh-vertices (to do: sort out).
    /// </summary>
    public static void UpdatePlayer()
    {
        // 1. Player
        SetPlayerFieldVisibility();         // regelmäßig       to do: auf input verschieben
        SetPlayerWidth();                   // regelmäßig       to do: auf input verschieben

        // 2. Mischmasch
        UpdateSurfacesTransforms();         // to do: siehe Funktion
    }




    // ----------------------------- private methods ----------------------------


    



    private static void GetTunnelVertices()
    {
        // 1) init
        RaycastHit[] edgeHits = new RaycastHit[3];
        var vertices = new Vector3[3];
        Vector3 intersection = Vector3.zero;

        // 2) Prepare raycast
        for (int i = 0; i < Player.inst.verticesCount; i++)
        {
            Vector3 playerEdgeMid = Player.inst.outerVertices[i] + ((Player.inst.outerVertices[(i + 1) % 3] - Player.inst.outerVertices[i]) / 2f);
            playerEdgeMid.z = MeshRef.inst.tunnelEdges_lr.transform.position.z;
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
                // 5) Sort: Start with lower-left vertex & go CLOCKWISE (like every mesh-creation here)
                if (intersection.x < -0.1f)
                    TunnelData.vertices[0] = intersection;
                else if (intersection.y > 0.1f)
                    TunnelData.vertices[1] = intersection;
                else
                    TunnelData.vertices[2] = intersection;
            }
        }
    }


    private static void UpdateSurfacesTransforms()
    {
        // == Sämtliche Mesh-Vertices (innerPlayer, outerPlayer, Milch-Fläche, Masken) setzen, abhängig von TunnelData und Spieler-Transform

        // To do: ausklamüsern, was nur bei Player-Input und Spielstart geupdated werden müsste

        // Inner surface
        MeshRef.inst.innerSurface_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(TunnelData.vertices, thisTransform);
        MeshRef.inst.innerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(Player.inst.outerVertices, thisTransform);
        MeshRef.inst.innerPlayerMesh_mf.transform.localPosition = Vector3.zero;

        // Outer player
        MeshRef.inst.outerPlayerMesh_mf.transform.localScale = Player.inst.transform.localScale;
        MeshRef.inst.outerPlayerMesh_mf.transform.eulerAngles = Player.inst.transform.eulerAngles;

        MeshRef.inst.outerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(TunnelData.vertices, thisTransform);
        MeshRef.inst.innerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(TunnelData.vertices, thisTransform);
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


    #region DrawTunnelEdges
    //private static void DrawEnvironmentEdges()
    //{
    //    // = Draw lineRenderer lines for each edge (3)

    //    // 1) Add extra points for LineRenderer
    //    List<Vector3> newPositions = EnvironmentData.vertices.ToList();
    //    int insertCounter = 0;
    //    for (int i = 1; i < EnvironmentData.vertices.Length; i++)
    //    {
    //        // insert before
    //        newPositions.Insert(i + insertCounter, EnvironmentData.vertices[i]);
    //        insertCounter++;
    //        // insert after
    //        newPositions.Insert(i + 1 + insertCounter, EnvironmentData.vertices[i]);
    //        insertCounter++;
    //    }
    //    newPositions.Add(EnvironmentData.vertices[0]);


    //    // 2) Add to LineRenderer
    //    MeshRef.inst.envEdges_lr.positionCount = newPositions.Count;
    //    MeshRef.inst.envEdges_lr.SetPositions(newPositions.ToArray());
    //}
    #endregion


    /// <summary>
    /// Abhängig von sich veränderndem Tunnel setze die Punkte für LineRenderer aller MusicFields
    /// </summary>
    private static void UpdateFieldsVertices()
    {
        // Tunnel

        if (TunnelData.vertices[0] == Vector3.zero)
            Debug.LogError("Tried to get tunnel vertices too early, no collider yet.");
        for (int i = 0; i < TunnelData.vertices.Length; i++)
        {
            for (int j = 0; j < VisualController.inst.fieldsPerEdge; j++)
            {
                // calc
                Vector3 start = TunnelData.vertices[i] + (((TunnelData.vertices[(i + 1) % TunnelData.vertices.Length] - TunnelData.vertices[i]) / VisualController.inst.fieldsPerEdge) * j);
                Vector3 end = TunnelData.vertices[i] + (((TunnelData.vertices[(i + 1) % TunnelData.vertices.Length] - TunnelData.vertices[i]) / VisualController.inst.fieldsPerEdge) * (j+1));

                start.z -= VisualController.inst.fieldsBeforeSurface;
                end.z -= VisualController.inst.fieldsBeforeSurface;

                int ID = i * VisualController.inst.fieldsPerEdge + j;

                // assign
                TunnelData.fields[ID].UpdateVertices(start, end);
            }
        }
    }

    private static void SetPlayerFieldVisibility()
    {
        // Player
        if (Player.inst.curEdge.firstTouch)
        {
            Player.inst.curField.SetToPlay();
            foreach(PlayerField secField in Player.inst.curSecondaryFields)
                secField.SetVisible(true);
        }
        else if (Player.inst.curEdge.leave)
        {
            Player.inst.curField.SetToFocus();
            foreach (PlayerField secField in Player.inst.curSecondaryFields)
                secField.SetVisible(false);
        }

        if (Player.inst.curField.changed)
        {
            //Player.inst.curField.UpdateLineRenderer();
            //foreach (PlayerField secField in Player.inst.curSecondaryFields)
            //    secField.UpdateLineRenderer();
        }
    }


    /// <summary>
    /// Set line renderer positions from data. In corners add identical positions to prevent bending lines.
    /// </summary>
    public static void UpdatePlayerLineRenderer(PlayerField data)
    {
        var curField = Player.inst.curField;
        if (!data.isCorner)
        {
            curField.lineRend.positionCount = data.positions.Length;
            curField.lineRend.SetPositions(data.positions);
        }
        else
        {
            //corners: add empty line renderer positions, to prevent bending
            List<Vector3> newPositions = data.positions.ToList();
            Vector3 cornerPos = newPositions[1];
            newPositions.Insert(1, cornerPos);
            newPositions.Insert(1, cornerPos);


            curField.lineRend.positionCount = newPositions.Count;
            curField.lineRend.SetPositions(newPositions.ToArray());
        }
    }

    // --------------------------------- Events --------------------------------
    private static void OnFieldChange(PlayerField data)
    {
        UpdatePlayerLineRenderer(data);
        //foreach (PlayerField secField in Player.inst.curSecondaryFields)          // TO DO
        //    secField.UpdatePlayerLineRenderer(data);
    }


    /// <summary>
    /// Defines when fieldChanges are allowed.
    /// </summary>
    /// <param name="size">[0 - 1]</param>
    public static void SetMouseColliderSize(float size)
    {
        MeshRef.inst.mouseColllider.transform.localScale = new Vector3(size, size, 1);
    }



}
