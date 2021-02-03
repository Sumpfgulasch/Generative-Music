using System.Collections;
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

    /// <summary>
    /// Create all player meshes
    /// </summary>
    public static void CreatePlayerMeshes()
    {
        InitPlayer();

        // Inner player
        CreatePlayerMesh(ref MeshRef.inst.innerPlayerMesh_mf);

        // Milk surface
        CreateTriangleMesh(ref MeshRef.inst.innerSurface_mf, TunnelData.vertices);
        CreateTriangleMesh(ref MeshRef.inst.innerMask_mf, Player.inst.outerVertices);
        CreateTriangleMesh(ref MeshRef.inst.innerPlayerMask_mf, TunnelData.vertices);

        // Outer player
        CreatePlayerMesh(ref MeshRef.inst.outerPlayerMesh_mf);
        CreateTriangleMesh(ref MeshRef.inst.outerPlayerMask_mf, TunnelData.vertices);
    }


    /// <summary>
    /// Instantiates music fields and player fields, with line renderers disabled. Stores them in TunnelData.fields.
    /// </summary>
    public static void InitFields()
    {
        // Music fields
        TunnelData.fields = InstantiateFieldSet();

        // Player fields
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



    public static void CreatePlayerMesh(ref MeshFilter mf)
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
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.MarkDynamic();                      // for better performance
        mf.mesh = newMesh;
        // no UVs
    }


    // ENVIRONMENT

    /// <summary>
    /// Create game objects for a complete field set, with line renderers being unvisible (empty positions). Set data: ID, isCorner, isEdgeMid, lineRend.
    /// </summary>
    public static MusicField[] InstantiateFieldSet()
    {
        int fieldsCount = TunnelData.FieldsCount;
        MusicField[] newFields = new MusicField[fieldsCount];

        for (int i = 0; i < fieldsCount; i++)
        {
            // Get data
            int ID = i;
            bool isCorner = MusicField.IsCorner(ID);
            bool isEdgeMid = MusicField.IsEdgeMid(ID);
            GameObject newObj = CreateContainer("Field" + ID, MeshRef.inst.musicFields_parent);
            LineRenderer lineRend = newObj.AddLineRenderer(2, MeshRef.inst.musicFields_mat, VisualController.inst.fieldThickness, false);      // TO DO: init mit zwei empty lineRend positions?

            // Assign
            newFields[ID] = new MusicField(ID, lineRend, isCorner, isEdgeMid);
        }

        return newFields;
    }

    // PLAYER
    /// <summary>
    /// Instantiate 1 gameObject with lineRenderer with empty positions and disabled.
    /// </summary>
    private static void CreatePlayerFields()
    {
        // FIELDS
        // Primary
        GameObject newObj = CreateContainer("Primary", MeshRef.inst.playerField_parent);
        LineRenderer lineRend = newObj.AddLineRenderer(2, MeshRef.inst.playerField_mat, VisualController.inst.playerFieldPlayThickness);
        Player.curField = new PlayerField(PlayerField.Type.Main, lineRend);
        Player.curField.SetToFocus();

        // Seoncdary
        Player.curSecondaryFields = new PlayerField[Player.verticesCount - 1];
        for (int i = 0; i < Player.curSecondaryFields.Length; i++)
        {
            GameObject newObj2 = CreateContainer("Secondary", MeshRef.inst.playerField_parent);
            LineRenderer lineRend2 = newObj2.AddLineRenderer(2, MeshRef.inst.playerFieldSec_mat, VisualController.inst.playerSecFieldThickness);
            Player.curSecondaryFields[i] = new PlayerField(PlayerField.Type.Second, lineRend2);
            lineRend2.enabled = false;
        }

        // EDGES
        Player.curEdge = new Edge();
        Player.curSecEdges = new Edge[Player.verticesCount - 1];
        for (int i = 0; i < Player.curSecEdges.Length; i++)
            Player.curSecEdges[i] = new Edge();
    }

    /// <summary>
    /// Create Meshes.
    /// </summary>
    public static void CreateOuterFields()
    {
        for (int i=0; i<TunnelData.FieldsCount; i++)
        {
            var outerfield = CreateOuterField(i);
            TunnelData.fields[i].outerSurface = outerfield;
        }
    }

    private static MeshRenderer CreateOuterField(int index)
    {
        // 0. Container & components
        GameObject outerField = CreateContainer("OuterField" + index, MeshRef.inst.outerFields_parent);
        var meshRenderer = outerField.AddComponent<MeshRenderer>();
        var meshFilter = outerField.AddComponent<MeshFilter>();
        
        // 1. Positions
        var fieldPositions = TunnelData.fields[index].positions;
        var vertices = fieldPositions.ToList();
        for (int j = 0; j < fieldPositions.Length; j++)
        {
            var pos = fieldPositions[j];
            pos.z = -1;
            vertices.Add(pos);
        }

        // 2. Triangles & normals
        int[] triangles;
        Vector3[] normals;

        if (TunnelData.fields[index].isCorner)
        {
            // Corner?
            triangles = new int[]
            {
                0, 3, 4,
                4, 1, 0,
                1, 4, 5,
                5, 2, 1
            };

            normals = new Vector3[]
            {
                Vector3.Cross(vertices[0] - vertices[1], vertices[4] - vertices[1]),
                Vector3.Cross(vertices[0] - vertices[1], vertices[4] - vertices[1]),
                Vector3.Cross(vertices[1] - vertices[2], vertices[5] - vertices[2]),
                Vector3.Cross(vertices[1] - vertices[2], vertices[5] - vertices[2])
            };
        }
        else
        {
            // Regular?
            triangles = new int[]
            {
                0, 2, 3,
                3, 1, 0
            };

            normals = new Vector3[]
            {
                Vector3.Cross(vertices[0] - vertices[1], vertices[3] - vertices[1]),
                Vector3.Cross(vertices[0] - vertices[1], vertices[3] - vertices[1])
            };
        }

        // Assign
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.normals = normals;
        meshFilter.mesh = mesh;

        meshRenderer.enabled = false;

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
