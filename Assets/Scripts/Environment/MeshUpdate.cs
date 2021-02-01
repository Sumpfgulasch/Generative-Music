using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public static class MeshUpdate
{
    // Private variables
    private static Vector3 playerMid;
    private static Transform thisTransform;

    //Action<InputAction.CallbackContext> MoveSubscriptionHandler;
    
    // Constructor
    static MeshUpdate()
    {
        playerMid = Player.inst.transform.position;
        thisTransform = VisualController.inst.transform;

        #region Malte Tipps event subscription
        //Action<InputAction.CallbackContext> GeileFunktionSubscriptionHandler = ctx => GEILEFUNKTION();  //Erstellt den Handler mit dem man Subscribed/UnSubscribed
        //controls.EditMode.AddVertex.performed += GeileFunktionSubscriptionHandler //subscribed den handler(der die GEILEFUNKTION beinhaltet) zur EditMode map, an den AddVertex command, wenn er performed wurde
        //controls.EditMode.AddVertex.performed -= GeileFunktionSubscriptionHandler //Durch den Handler kann nun auch unsubscribed werden
        #endregion

        // Event subscription
        GameEvents.inst.onFieldChange += OnFieldChange;

        
        PlayerControls controls = new PlayerControls();
        controls.Enable();
        controls.Gameplay.Move.performed += context => OnMove(context);
        Debug.Log("MeshUpdate Constructor");
    }



    // ----------------------------- public methods ----------------------------
    

    /// <summary>
    /// Get vertices from tunnel, create tunnel form and update vertices variables of music fields. Set Z to player.z.
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
    /// Set fields positions (dependant on the tunnel vertices) and store it in TunnelData.fields. Set fieldLength in TunnelData.
    /// </summary>
    private static void UpdateFieldsVertices()
    {
        if (TunnelData.vertices[0] == Vector3.zero)
            Debug.LogError("Tried to get tunnel vertices too early, no collider yet.");

        // Set field length
        float fieldLength = (TunnelData.vertices[1] - TunnelData.vertices[0]).magnitude / VisualController.inst.fieldsPerEdge;
        TunnelData.fieldLength = fieldLength;


        // 1. Iterate over edges
        for (int edgeInd = 0; edgeInd < TunnelData.vertices.Length; edgeInd++)
        {
            Vector3 edgeVec = TunnelData.vertices[(edgeInd + 1) % TunnelData.vertices.Length] - TunnelData.vertices[edgeInd];
            Vector3 fieldVec = edgeVec.normalized * fieldLength;

            // 2. Interate over fields per edge
            for (int fieldInd = 0; fieldInd < VisualController.inst.fieldsPerEdge; fieldInd++)
            {
                int ID = edgeInd * (VisualController.inst.fieldsPerEdge - 1) + fieldInd;

                Vector3 start, end, mid;
                Vector3[] positions;

                // Regular field
                bool isCorner = MusicField.IsCorner(ID);
                if (!isCorner)
                {
                    start = TunnelData.vertices[edgeInd] + fieldVec * fieldInd;
                    end = TunnelData.vertices[edgeInd] + fieldVec * (fieldInd + 1);
                    mid = (start + end) / 2f;
                    positions = new Vector3[] { start, end };
                }
                // Corner
                else
                {
                    int oppositeIndex = ExtensionMethods.Modulo(edgeInd - 1, TunnelData.vertices.Length);
                    Vector3 oppositeEdgeVec = TunnelData.vertices[oppositeIndex] - TunnelData.vertices[edgeInd];
                    Vector3 oppositeFieldFec = oppositeEdgeVec.normalized * fieldLength;

                    start = TunnelData.vertices[edgeInd] + oppositeFieldFec;
                    mid = TunnelData.vertices[edgeInd];
                    end = TunnelData.vertices[edgeInd] + fieldVec;
                    positions = new Vector3[] { start, mid, end };
                }

                // Assign
                TunnelData.fields[ID].UpdateVertices(start, mid, end, positions);

                // TO DO: vertices dem line renderer zuweisen, line renderer über andere funktion für start unvisible setzen
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
    }


    /// <summary>
    /// Set line renderer positions from data. In corners add identical positions to prevent bending lines.
    /// </summary>
    public static void UpdatePlayerLineRenderer(PlayerField data)
    {
        // TO DO: mischung aus data & curField ist weird und unnötig

        var curField = Player.inst.curField;
        Vector3[] positions;
        int positionCount;

        if (!data.isCorner)
        {
            // Regular field
            positions = data.positions;
            positionCount = data.positions.Length;
        }
        else
        {
            // Corner: add empty line renderer positions, to prevent bending
            positions = PreventLineRendFromBending(data.positions);
            positionCount = positions.Length;
        }

        // Set
        curField.lineRend.positionCount = positionCount;
        curField.lineRend.SetPositions(positions);
    }

    /// <summary>
    /// Douplicate most of the line renderer vertices to prevent it from unwanted bending.
    /// </summary>
    public static Vector3[] PreventLineRendFromBending(Vector3[] positions)
    {
        List<Vector3> newPositions = positions.ToList();
        int counter = 0;

        // skip first and last position
        for (int i=1; i<positions.Length-1; i++)
        {
            Vector3 curPos = positions[i];
            newPositions.Insert(i + counter, curPos);
            newPositions.Insert(i + counter, curPos);
            counter += 2;
        }

        return newPositions.ToArray();
    }



    // --------------------------------- Events --------------------------------


    
    private static void OnFieldChange(PlayerField data)
    {
        UpdatePlayerLineRenderer(data);
        //foreach (PlayerField secField in Player.inst.curSecondaryFields)          // TO DO
        //    secField.UpdatePlayerLineRenderer(data);
    }

    public static void OnMove(InputAction.CallbackContext context)
    {
        // TO DO: doppelt berechnet, in player

        var input = context.ReadValue<Vector2>();

        var mousePos = Camera.main.ScreenToWorldPoint(new Vector3(input.x, input.y, playerMid.z));
        mousePos.z = playerMid.z - 1;

        var ray = new Ray(mousePos, Vector3.forward);
        var hit = Physics2D.GetRayIntersection(ray, 3);

        if (Player.inst.actionState == Player.ActionState.None) 
            {
            if (hit && hit.collider.tag.Equals("MouseCollider"))
            {
                Player.inst.curField.SetColor(Color.white);
                Player.inst.curField.SetOpacity(0);
            }
            else
            {
                Player.inst.curField.SetColor(Color.white);
                Player.inst.curField.SetOpacity(1f);
            }
        }
        else
        {
            if (hit && hit.collider.tag.Equals("MouseCollider"))
            {
                Player.inst.curField.SetColor(Color.white);
                Player.inst.curField.SetOpacity(1f);
            }
            else
            {
                Player.inst.curField.SetColor(Color.white);
                Player.inst.curField.SetOpacity(1f);
            }
        }
        
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
