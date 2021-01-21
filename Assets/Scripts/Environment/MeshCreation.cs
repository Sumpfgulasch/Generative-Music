using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MeshCreation
{
    // = Create meshes
    // Alle Vertices werden IM Uhrzeigersinn gezählt


    // Get/set
    static Player player { get { return Player.inst; } }



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
        CreateMesh(ref MeshRef.inst.innerSurface_mf, TunnelData.vertices);
        CreateMesh(ref MeshRef.inst.innerMask_mf, Player.inst.outerVertices);
        CreateMesh(ref MeshRef.inst.innerPlayerMask_mf, TunnelData.vertices);

        // Outer player
        CreatePlayerMesh(ref MeshRef.inst.outerPlayerMesh_mf);
        CreateMesh(ref MeshRef.inst.outerPlayerMask_mf, TunnelData.vertices);
    }

    /// <summary>
    /// Instantiate music fields and player fields, with line renderers but empty positions (-> not visible)
    /// </summary>
    public static void CreateFields()
    {
        InstantiateFields();
        CreatePlayerFields();
    }





    // ----------------------------- Private methods ----------------------------
    

    private static void InitPlayer()
    {
        // = Create mesh form, create containers & set player variables

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



    private static void CreateMesh(ref MeshFilter mf, Vector3[] vertices)
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
    private static void InstantiateFields()
    {
        // == Create gameObjects with lineRenderers with empty positions

        int fieldsCount = VisualController.inst.FieldsCount;
        TunnelData.fields = new MusicField[fieldsCount];

        for (int i = 0; i < VisualController.inst.tunnelVertices; i++)
        {
            for (int j = 0; j < VisualController.inst.fieldsPerEdge; j++)
            {
                // Get data
                int ID = i * VisualController.inst.fieldsPerEdge + j;
                bool isCorner = MusicField.IsCorner(ID);
                bool isEdgeMid = MusicField.IsEdgeMid(ID);
                GameObject newObj = CreateContainer("Field" + ID, MeshRef.inst.musicFields_parent);
                LineRenderer lineRend = newObj.AddLineRenderer(2, MeshRef.inst.musicFields_mat, VisualController.inst.edgePartThickness);

                // Assign
                TunnelData.fields[ID] = new MusicField(ID, lineRend, isCorner, isEdgeMid);
            }
        }
    }

    // PLAYER
    private static void CreatePlayerFields()
    {
        // == Create 1 gameObject with lineRenderer with empty positions

        // EDGE PARTS
        // Primary
        GameObject newObj = CreateContainer("Primary", MeshRef.inst.playerField_parent);
        LineRenderer lineRend = newObj.AddLineRenderer(2, MeshRef.inst.playerField_mat, VisualController.inst.playerEdgePartThickness);
        player.curEdgePart = new PlayerEdgePart(PlayerEdgePart.Type.Main, lineRend);

        // Seoncdary
        player.curSecEdgeParts = new PlayerEdgePart[player.verticesCount - 1];
        for (int i = 0; i < player.curSecEdgeParts.Length; i++)
        {
            GameObject newObj2 = CreateContainer("Secondary", MeshRef.inst.playerField_parent);
            LineRenderer lineRend2 = newObj2.AddLineRenderer(2, MeshRef.inst.playerFieldSec_mat, VisualController.inst.playerEdgePartThickness);
            player.curSecEdgeParts[i] = new PlayerEdgePart(PlayerEdgePart.Type.Second, lineRend2);
            lineRend2.enabled = false;
        }

        // EDGES
        player.curEdge = new Edge();
        player.curSecEdges = new Edge[player.verticesCount - 1];
        for (int i = 0; i < player.curSecEdges.Length; i++)
            player.curSecEdges[i] = new Edge();
    }




    // Helper methods

    private static GameObject CreateContainer(string name, Transform parent)
    {
        GameObject newObj = new GameObject(name);
        newObj.transform.parent = parent;
        newObj.transform.localPosition = Vector3.zero;
        
        return newObj;
    }

    private static LineRenderer AddLineRenderer(this GameObject obj, int positionCount, Material material, float width)
    {
        LineRenderer lineRend = obj.AddComponent<LineRenderer>();
        lineRend.positionCount = positionCount;
        lineRend.startWidth = width;
        lineRend.endWidth = width;
        lineRend.material = material;

        return lineRend;
    }
}
