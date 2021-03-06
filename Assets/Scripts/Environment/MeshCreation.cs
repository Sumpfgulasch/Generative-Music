﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MeshCreation
{
    // = Create meshes
    // Alle Vertices werden IM Uhrzeigersinn gezählt


    // Get/set
    static Player Player { get { return Player.inst; } }



    // Private variables
    private static Vector3 playerMid;



    // Constructor
    static MeshCreation()
    {
        
    }


    // ----------------------------- Public methods ----------------------------



    public static void CreateAll()
    {
        MeshUpdate.GetTunnelVertices();

        CreateFields();
        CreatePlayer();

        InitMouseCollider();
        MeshUpdate.UpdateBeatTriangle();
    }


    /// <summary>
    /// Create all player meshes that define its form.
    /// </summary>
    public static void CreatePlayerForm()
    {
        // FORM
        InitPlayer();

        // Inner player
        CreatePlayerMesh(ref MeshRef.inst.innerPlayerMesh_mf, VisualController.inst.playerAlpha);

        // Milk surface
        CreateTriangleMesh(ref MeshRef.inst.innerSurface_mf, TunnelData.vertices);
        CreateTriangleMesh(ref MeshRef.inst.innerMask_mf, Player.inst.OuterVertices);
        CreateTriangleMesh(ref MeshRef.inst.innerPlayerMask_mf, TunnelData.vertices);
        MeshRef.inst.milkSurface_parent.gameObject.SetActive(VisualController.inst.showMilkSurface);

        // Outer player
        CreatePlayerMesh(ref MeshRef.inst.outerPlayerMesh_mf, VisualController.inst.playerAlpha);
        CreateTriangleMesh(ref MeshRef.inst.outerPlayerMask_mf, TunnelData.vertices);
    }


    /// <summary>
    /// Create all MusicField-meshes (line renderers and outer surfaces), initialize with data (positions, isCorner, ...) and assign to TunnelData.fields
    /// </summary>
    public static void CreateFields()
    {
        TunnelData.fields = CreateFieldSet();
        
    }

    public static void CreatePlayer()
    {
        CreatePlayerForm();

        MeshUpdate.UpdatePlayer();

        CreatePlayerFields();
    }





    // ----------------------------- Private methods ----------------------------

    /// <summary>
    /// Create mesh form, create containers and set player variables.
    /// </summary>
    private static void InitPlayer()
    {
        playerMid = Player.inst.transform.position;

        // Create containers
        GameObject vertices = CreateContainer("Vertices", Player.inst.transform);
        GameObject outside = CreateContainer("Outside", vertices.transform);
        GameObject inside = CreateContainer("Inside", vertices.transform);

        for (int i = 0; i < Player.inst.verticesCount; i++)
        {
            // CREATE MESH FORM by calculating vertex positions
            Quaternion rot = Quaternion.Euler(0, 0, -i * (360 / Player.inst.verticesCount)); // negativ damit clockwise (object-z zeigt weg von spieler, ist sozusag. um 180° gedreht)
            Vector3 nextDirection = rot * Vector3.up;
            Vector3 nextOuterVertex = nextDirection.normalized;
            Vector3 nextInnerVertex = nextDirection.normalized * (1 - Player.inst.innerWidth);

            // More containers
            GameObject newOuterVert = CreateContainer("Vert" + (i + 1), outside.transform);
            GameObject newInnerVert = CreateContainer("Vert" + (i + 1), inside.transform);
            newOuterVert.transform.position = Player.inst.transform.position + nextOuterVertex;
            newInnerVert.transform.position = Player.inst.transform.position + nextInnerVertex;

            // Assign positions to Player
            Player.inst.outerVertices_obj[i] = newOuterVert.transform;
            Player.inst.innerVertices_obj[i] = newInnerVert.transform;
            Player.inst.outerVertices_mesh[i] = nextOuterVertex;
            Player.inst.innerVertices_mesh[i] = nextInnerVertex;
        }
    }



    private static void CreateTriangleMesh(ref MeshFilter mf, Vector3[] vertices)
    {
        // = Used for any triangle mesh that is not the player

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = new int[3] { 0, 1, 2 }; // count clockwise!
        newMesh.normals = new Vector3[3] { -Vector3.forward, -Vector3.forward, -Vector3.forward };
        mf.mesh = newMesh;
        // no UVs
    }



    public static void CreatePlayerMesh(ref MeshFilter mf, float alpha)
    {
        // Declarations
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // Calculations
        for (int i = 0; i < Player.inst.verticesCount; i++)
        {
            vertices.AddRange(new Vector3[2] {
                Player.inst.outerVertices_mesh[i],
                Player.inst.innerVertices_mesh[i] });
            triangles.AddRange(new int[6] {         // count clockwise!
                // outer triangle
                i *2,
                (i*2+2) % (Player.inst.verticesCount*2),
                i *2+1,
                // inner triangle
                i *2+1 ,
                (i*2+2) % (Player.inst.verticesCount*2),
                (i*2+3) % (Player.inst.verticesCount*2) });
            normals.AddRange(new Vector3[2] {
                -Vector3.forward,
                -Vector3.forward });
        }

        // Assign
        Mesh newMesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals.ToArray()
        };
        newMesh.MarkDynamic();                      // for better performance
        mf.mesh = newMesh;
        // no UVs

        // Set Alpha
        Material mat = mf.GetComponent<MeshRenderer>().material;
        Color color = mat.color;
        color.a = alpha;
        mat.SetColor("_BaseColor", color);
    }


    // ENVIRONMENT

    /// <summary>
    /// Create a complete field set, with parented gameObjects, line renderers and outer surfaces. Initialize with data (ID, positions, isCorner, isEdgeMid, lineRend) and return the field set.
    /// </summary>
    public static MusicField[] CreateFieldSet()
    {
        int fieldsCount = TunnelData.FieldsCount;
        MusicField[] fields = new MusicField[fieldsCount];

        // 1. Create gameObjects, lineRenderers and initialize with data (no positions)
        for (int i = 0; i < fieldsCount; i++)
        {
            // Get data
            int ID = i;
            bool isCorner = MusicField.IsCorner(ID);
            bool isEdgeMid = MusicField.IsEdgeMid(ID);
            GameObject newObj = CreateContainer("Field" + ID, MeshRef.inst.musicFields_parent);
            LineRenderer lineRend = newObj.AddLineRenderer(0, MeshRef.inst.musicFields_mat, VisualController.inst.fieldThickness, false);      // TO DO: init mit zwei empty lineRend positions?

            // Assign
            fields[ID] = new MusicField(ID, lineRend, isCorner, isEdgeMid);
        }

        // 2. Assign positions
        fields = MeshUpdate.UpdateFieldsVertices(fields);

        // 3. Surfaces
        CreateFieldsSurfaces(fields);

        return fields;
    }

    // PLAYER
    /// <summary>
    /// Create LineRenderers for current and secondary fields (disabled). Assign to Player.curField.lineRend. Initialize curField.outerSurface (index = 0, disabled).
    /// </summary>
    private static void CreatePlayerFields()
    {
        // FIELDS
        // Primary
        GameObject newObj = CreateContainer("Primary", MeshRef.inst.playerField_parent);
        LineRenderer lineRend = newObj.AddLineRenderer(2, MeshRef.inst.playerField_mat, VisualController.inst.playerFieldPlayThickness);
        lineRend.enabled = VisualController.inst.showPlayerLinerend;
        Player.curField = new PlayerField(lineRend, VisualController.inst.fieldsPerEdge - 1);
        
        // Seoncdary
        Player.curSecondaryFields = new PlayerField[Player.verticesCount - 1];
        for (int i = 0; i < Player.curSecondaryFields.Length; i++)
        {
            GameObject newObj2 = CreateContainer("Secondary", MeshRef.inst.playerField_parent);
            LineRenderer lineRend2 = newObj2.AddLineRenderer(2, MeshRef.inst.playerFieldSec_mat, VisualController.inst.playerSecFieldThickness);
            Player.curSecondaryFields[i] = new PlayerField(lineRend2, 0);
            lineRend2.enabled = false;
        }

        // EDGES
        Player.curEdge = new Edge();
        Player.curSecEdges = new Edge[Player.verticesCount - 1];
        for (int i = 0; i < Player.curSecEdges.Length; i++)
            Player.curSecEdges[i] = new Edge();

        // Init field & outerSurface
        Player.curField.InitSurface();
        Player.curField.SetToFocus();
    }


    /// <summary>
    /// Create highlightSurface and fieldSurface for each field. Assign to given fields. Partially disabled MeshRenderers (!).
    /// </summary>
    public static void CreateFieldsSurfaces(MusicField[] fields)
    {
        for (int i=0; i<fields.Length; i++)
        {
            int ID = fields[i].ID;
            Transform parent_field = MeshRef.inst.fieldSurfaces_parent;
            Material material_field = MeshRef.inst.fieldSurfaces_mat;
            int fieldLayer = LayerMask.NameToLayer(MeshRef.inst.fieldSurfaces_layer);
            int fieldRenderQueue = MeshRef.inst.fieldSurfaces_renderQueue;
            Transform parent_high = MeshRef.inst.highlightSurfaces_parent;
            Material material_high = MeshRef.inst.highlightSurfaces_mat;
   

            MeshRenderer fieldSurface = CreateLaneSurface(fields, ID, "FieldSurface", parent_field, material_field, false, -1f, fieldLayer, fieldRenderQueue);
            MeshRenderer highlightSurface = CreateLaneSurface(fields, ID, "HighlightSurface", parent_high, material_high, false, -2f);

            fields[ID].fieldSurface = fieldSurface;
            fields[ID].highlightSurface = highlightSurface;

            // TO DO: nicht gut, dass argument direkt bearbeitet wird(?); sollte lieber neuen array erstellen und returnen
        }
    }

    /// <summary>
    /// Create a lane surface (gameObj, MeshRenderer, MeshFilter) with data (vertices, ...) for a given ID. Disable MeshRenderer (!).
    /// </summary>
    /// <param name="index">[0, fields.Length]</param>
    public static MeshRenderer CreateLaneSurface(MusicField[] fields, int index, string name, Transform parent, Material material, bool visible = false, float length = -1f, int layer = 0, int renderQueue = -1)
    {
        // 0. Container & components
        GameObject laneSurface = CreateContainer(name + index, parent);
        var meshRenderer = laneSurface.AddComponent<MeshRenderer>();
        var meshFilter = laneSurface.AddComponent<MeshFilter>();

        // 1. MESH CREATION
        // 1.1. Vertices
        var fieldPositions = fields[index].positions;
        var vertices = fieldPositions.ToList();
        for (int j = 0; j < fieldPositions.Length; j++)
        {
            var pos = fieldPositions[j];
            pos.z += length;
            vertices.Add(pos);
        }

        // 1.2. Triangles & normals
        int[] triangles;
        Vector3[] normals;

        if (fields[index].isCorner)
        {
            // Corner
            if (length < 0)
            {
                // Shape ragt nach vorne
                triangles = new int[]
                {
                    0, 3, 4,
                    4, 1, 0,
                    1, 4, 5,
                    5, 2, 1
                };
            }
            else
            {
                // Shape ragt nach hinten
                triangles = new int[]
                {
                    0, 4, 3,
                    4, 0, 1,
                    1, 5, 4,
                    5, 1, 2
                };
            }

            #region normals
            //normals = new Vector3[]                                                                     // not used
            //{
            //    Vector3.Cross(vertices[1] - vertices[0], vertices[3] - vertices[0]).normalized,
            //    Vector3.Cross(vertices[5] - vertices[2], vertices[1] - vertices[2]).normalized,
            //    (((vertices[0] - vertices[1]) + (vertices[2] - vertices[1])) / 2f).normalized,
            //    Vector3.Cross(vertices[1] - vertices[0], vertices[3] - vertices[0]).normalized,         // twice
            //    Vector3.Cross(vertices[5] - vertices[2], vertices[1] - vertices[2]).normalized,
            //    (((vertices[0] - vertices[1]) + (vertices[2] - vertices[1])) / 2f).normalized
            //};
            #endregion
        }
        else
        {
            // Regular
            if (length < 0)
            {
                // Shape ragt nach vorne
                triangles = new int[]
                {
                    0, 2, 3,
                    3, 1, 0
                };
            }
            else
            {
                // Shape ragt nach hinten
                triangles = new int[]
                {
                    0, 3, 2,
                    3, 0, 1
                };
            }

            #region normals
            //normals = new Vector3[]                                                                 // not used
            //{
            //    Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]),
            //    Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]),
            //    Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]),                // twice
            //    Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0])
            //};
            #endregion
        }

        // 1.3. Assign
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        //mesh.normals = normals;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        // 2. Rendering & material
        laneSurface.layer = layer;
        meshRenderer.material = material;
        meshRenderer.enabled = visible;
        if (renderQueue != -1)
            meshRenderer.material.renderQueue = renderQueue;
        

        return meshRenderer;
    }


    /// <summary>
    /// Collider for selection with the mouse. Fill with vertices from TunnelData-form.
    /// </summary>
    public static void InitMouseCollider()
    {
        MeshRef.inst.mouseColllider.points = ExtensionMethods.Vector3ToVector2(TunnelData.vertices);
        MeshUpdate.SetMouseColliderSize(VisualController.inst.mouseColliderSize_play);
    }




    // Helper methods

    private static GameObject CreateContainer(string name, Transform parent)
    {
        GameObject newObj = new GameObject(name);
        newObj.transform.parent = parent;
        newObj.transform.localPosition = Vector3.zero;
        
        return newObj;
    }

    private static LineRenderer AddLineRenderer(this GameObject obj, int positionCount, Material material, float width, bool enabled = true)
    {
        LineRenderer lineRend = obj.AddComponent<LineRenderer>();
        lineRend.positionCount = positionCount;
        lineRend.startWidth = width;
        lineRend.endWidth = width;
        lineRend.material = material;
        lineRend.enabled = enabled;

        Vector3[] positions = new Vector3[] { Vector3.zero, Vector3.zero };
        lineRend.SetPositions(positions);

        return lineRend;
    }
}
