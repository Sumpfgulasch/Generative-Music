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

        // Event subscription
        //GameEvents.inst.onFieldChange += OnFieldChange;

        //PlayerControls controls = new PlayerControls();
        //controls.Enable();
        //controls.Gameplay.Move.performed += context => OnMove(context);
        #endregion
    }



    // ----------------------------- public methods ----------------------------




    /// <summary>
    /// Set player width and form, to maintain the same width and correct masks.
    /// </summary>
    public static void UpdatePlayer()
    {
        UpdatePlayerWidth();
        UpdatePlayerFormVertices();
    }




    // ----------------------------- private methods ----------------------------


    


        /// <summary>
        /// Set TunnelData.vertices, fitting into the current outer triangle-collider.
        /// </summary>
    public static void GetTunnelVertices()
    {
        // 1) init
        RaycastHit[] edgeHits = new RaycastHit[3];
        //var vertices = new Vector3[3];
        Vector3 intersection;// = Vector3.zero;

        // 2) Prepare raycast
        for (int i = 0; i < Player.inst.verticesCount; i++)
        {
            Quaternion rot = Quaternion.Euler(0, 0, -i * (360 / Player.inst.verticesCount)+60); // +60 == Hack; i== negativ, damit clockwise (object-z zeigt weg von spieler, ist sozusag. um 180° gedreht)
            Vector3 nextDirection = rot * Vector3.up;

            // 3) Raycasts from player to environment
            if (Physics.Raycast(playerMid, nextDirection, out RaycastHit hit))
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


    /// <summary>
    /// Sämtliche Mesh-Vertices (innerPlayer, outerPlayer, Milch-Fläche, Masken) setzen, abhängig von TunnelData und Spieler-Transform
    /// </summary>
    private static void UpdatePlayerFormVertices()
    {
        // Inner surface
        MeshRef.inst.innerSurface_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(TunnelData.vertices, thisTransform);
        MeshRef.inst.innerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(Player.inst.OuterVertices, thisTransform);
        MeshRef.inst.innerPlayerMesh_mf.transform.localPosition = Vector3.zero;

        // Outer player
        MeshRef.inst.outerPlayerMesh_mf.transform.localScale = Player.inst.transform.localScale;
        MeshRef.inst.outerPlayerMesh_mf.transform.eulerAngles = Player.inst.transform.eulerAngles;

        MeshRef.inst.outerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(TunnelData.vertices, thisTransform);
        MeshRef.inst.innerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(TunnelData.vertices, thisTransform);
    }



    private static void UpdatePlayerWidth()
    {
        if (Player.inst.constantInnerWidth)
        {
            float neededWidthPerc = Mathf.Clamp01(Player.inst.innerWidth / Player.inst.transform.localScale.x);
            Vector3[] newVertices = MeshRef.inst.innerPlayerMesh_mf.mesh.vertices;

            if ((Player.inst.OuterVertices[0] - Player.inst.InnerVertices[0]).magnitude != Player.inst.innerWidth)
            {
                for (int i = 0; i < Player.inst.verticesCount; i++)
                {
                    // change innerVertices only
                    Vector3 newVertex = (Player.inst.outerVertices_mesh[i] - Vector3.zero).normalized * (1 - neededWidthPerc);
                    newVertices[(i * 2) + 1] = newVertex;
                    Player.inst.innerVertices_mesh[i] = newVertex;
                    Player.inst.innerVertices_obj[i].position = playerMid + (Player.inst.OuterVertices[i] - playerMid) * (1 - neededWidthPerc);
                    Player.inst.InnerVertices[i] = Player.inst.innerVertices_obj[i].position;
                }
            }
            MeshRef.inst.innerPlayerMesh_mf.mesh.vertices = newVertices;
            MeshRef.inst.outerPlayerMesh_mf.mesh.vertices = newVertices;

            // TO DO: mesh.recalculatenormals, -bounds, -tangents
        }
    }

    

    /// <summary>
    /// Set a given fields.positions, dependant on the tunnel vertices. Set fieldLength in TunnelData.
    /// </summary>
    public static MusicField[] UpdateFieldsVertices(MusicField[] fields)
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
            for (int fieldInd = 0; fieldInd < VisualController.inst.fieldsPerEdge - 1; fieldInd++)
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
                fields[ID].SetVertices(start, mid, end, positions);
            }
        }

        return fields;
    }


    /// <summary>
    /// Set fieldSurface- and highlightSurface-references to current ID. (+Set lineRenderer positions, disabled). Set highlightSurface opacity.
    /// </summary>
    public static void UpdatePlayerFieldVisibility()
    {
        var curField = Player.inst.curField;

        // 1. 

        // 2. Highlight-surface: disable old, enable new; set opacity to current value
        curField.UpdateSurface();

        // 3. [currently disabled:] Line renderer positions
        if (curField.isCorner)
        {
            var positions = PreventLineRendFromBending(curField.positions);
            var positionCount = positions.Length;
            curField.lineRend.positionCount = positionCount;
            curField.lineRend.SetPositions(positions);
        }
        else
        {
            curField.lineRend.positionCount = curField.positions.Length;
            curField.lineRend.SetPositions(curField.positions);
        }
    }


    /// <summary>
    /// Update positions of beat triangle.
    /// </summary>
    public static void UpdateBeatTriangle()
    {
        var positions_list = TunnelData.vertices.ToList();
        positions_list.Add(positions_list[0]);
        var positions = PreventLineRendFromBending(positions_list.ToArray());

        MeshRef.inst.tunnelEdges_lr.positionCount = positions.Length;
        MeshRef.inst.tunnelEdges_lr.SetPositions(positions);
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



    /// <summary>
    /// Adjust height of fieldSurfaces and position of field lineRenderers, dependant on the chord notes' heights.
    /// </summary>
    /// <param name="fields"></param>
    public static void AdjustFieldHeights(MusicField[] fields)
    {
        var vars = VisualController.inst;

        // 1. get highest & lowest note
        int lowestNote = MusicUtil.LowestFieldNote(fields);
        int highestNote = MusicUtil.HighestFieldNote(fields);

        // 2. set scale of fieldSurface & z-pos of lineRend
        for (int i=0; i<fields.Length; i++)
        {
            int[] curChordNotes = fields[i].chord.notes;
            int curNote = curChordNotes[curChordNotes.Length - 1];

            float targetScale = vars.minFieldSurfaceHeight + ExtensionMethods.Remap(curNote, lowestNote, highestNote, 0, vars.maxFieldSurfaceHeight);

            fields[i].SetLineRendZPos(Player.inst.transform.position.z - targetScale);
            fields[i].fieldSurface.transform.localScale = new Vector3(1, 1, targetScale);

            fields[i].height = targetScale;
        }
    }


    public static Color[] RandomColors(int amount)
    {
        Color[] colors = new Color[amount];
        for (int i = 0; i < amount; i++)
        {
            Color randColor = new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
            colors[i] = randColor;
        }
        return colors;
    }


    /// <summary>
    /// Create color for each field. Sorted like IDs. Corners are similar. Rest is a bit distant and similar, too. Constraints are in VisalController.
    /// </summary>
    /// <returns>RGB colors (Length == FieldsCount). Used for BaseColor and EmissiveColor of lineRend, fieldSurface and highlightSurface.</returns>
    public static Color[] ColorsInRange()
    {
        var vars = VisualController.inst;

        // 1. Corner colors
        float randHue = UnityEngine.Random.Range(0, 1f);
        ColorHSV curColor = new ColorHSV(randHue, vars.fieldsSaturation, vars.fieldsValue);
        ColorHSV[] cornerColors = new ColorHSV[vars.tunnelVertices];

        for (int i=0; i<cornerColors.Length; i++)
        {
            cornerColors[i] = new ColorHSV(curColor.hue, curColor.saturation, curColor.value);

            if (i == cornerColors.Length - 1)
                break;

            curColor.hue = (curColor.hue + vars.fieldsHue_CornerStep) % 1;
            
        }
        // 2. Distance
        curColor.hue = (curColor.hue + vars.fieldsHue_Corner2NoCornerDistance) % 1;

        // 3. NoCorner colors
        List<ColorHSV> noCornerColors = new List<ColorHSV>();

        for (int i=0; i<TunnelData.FieldsCount - vars.tunnelVertices; i++)
        {
            noCornerColors.Add(
                    new ColorHSV(curColor.hue, curColor.saturation, curColor.value)
                    );
            curColor.hue = (curColor.hue + vars.fieldsHue_NoCornerStep) % 1;

            if ((i + 1) % vars.colorCount == 0)
            {
                float startHue = curColor.hue - vars.colorCount * vars.fieldsHue_NoCornerStep;
                curColor.hue = ExtensionMethods.Modulo(startHue, 1);
            }
        }

        // 4. Assign
        Color[] colors = new Color[TunnelData.FieldsCount];
        int cornerCounter = 0;
        for (int i=0; i<colors.Length; i++)
        {
            if (MusicField.IsCorner(i))
            {
                ColorHSV color = cornerColors[cornerCounter];
                colors[i] = Color.HSVToRGB(color.hue, color.saturation, color.value);
                cornerCounter++;
            }
            else
            {
                int randNoCornerIndex = UnityEngine.Random.Range(0, noCornerColors.Count);
                ColorHSV color = noCornerColors[randNoCornerIndex];
                colors[i] = Color.HSVToRGB(color.hue, color.saturation, color.value);
                noCornerColors.Remove(color);
            }
        }

        return colors;
    }



    // --------------------------------- Events --------------------------------

        

    /// <summary>
    /// The mouse collider is for detecting when the mouse is inside or outside the MusicFields.
    /// </summary>
    /// <param name="size">[0 - 1]</param>
    public static void SetMouseColliderSize(float size)
    {
        MeshRef.inst.mouseColllider.transform.localScale = new Vector3(size, size, 1);
    }



}
