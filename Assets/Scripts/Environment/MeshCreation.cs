using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MeshCreation
{
    // = Create meshes

        
    
    // Private variables
    private static Vector3 playerMid;



    // Constructor
    static MeshCreation()
    {
        playerMid = Player.inst.transform.position;
    }


    // ----------------------------- Public methods ----------------------------

    public static void CreateMeshes()
    {
        // = Create all meshes

        InitPlayer();

        // Inner player
        CreatePlayerMesh(ref MeshRef.inst.innerPlayerMesh_mf);

        // Inner surface
        CreateMesh(ref MeshRef.inst.innerSurface_mf, EnvironmentData.vertices);
        CreateMesh(ref MeshRef.inst.innerMask_mf, Player.inst.outerVertices);
        CreateMesh(ref MeshRef.inst.innerPlayerMask_mf, EnvironmentData.vertices);

        // Outer player
        CreatePlayerMesh(ref MeshRef.inst.outerPlayerMesh_mf);
        CreateMesh(ref MeshRef.inst.outerPlayerMask_mf, EnvironmentData.vertices);
    }





    // ----------------------------- Private methods ----------------------------
    

    private static void InitPlayer()
    {
        // = Create mesh form, create containers & set player variables

        // Create containers
        GameObject vertices = CreateContainer("Vertices", Player.inst.transform);
        GameObject outside = CreateContainer("Outside", vertices.transform);
        GameObject inside = CreateContainer("Inside", vertices.transform);

        for (int i = 0; i < Player.inst.verticesCount; i++)
        {
            // CREATE MESH FORM by calculating vertex positions
            Quaternion rot = Quaternion.Euler(0, 0, i * (360 / Player.inst.verticesCount));
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
        newMesh.triangles = new int[3] { 2, 1, 0 }; // count counter-clockwise!
        newMesh.normals = new Vector3[3] { Vector3.back, Vector3.back, Vector3.back };
        mf.mesh = newMesh;
        // no UVs
    }



    private static void CreatePlayerMesh(ref MeshFilter mf)
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
            triangles.AddRange(new int[6] {
                // outer triangle
                i *2,
                i *2+1,
                (i*2+2) % (Player.inst.verticesCount*2),
                // inner triangle
                i *2+1 ,
                (i*2+3) % (Player.inst.verticesCount*2),
                (i*2+2) % (Player.inst.verticesCount*2) });
            normals.AddRange(new Vector3[2] {
                Vector3.back,
                Vector3.back });
        }

        // Assign
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.MarkDynamic();
        mf.mesh = newMesh;
        // no UVs
    }




    // Helper methods

    private static GameObject CreateContainer(string name, Transform parent)
    {
        GameObject newObj = new GameObject(name);
        newObj.transform.parent = parent;
        newObj.transform.localPosition = Vector3.zero;

        return newObj;
    }
}
