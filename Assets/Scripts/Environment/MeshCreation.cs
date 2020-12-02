using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshCreation : MonoBehaviour
{
    public static MeshCreation instance;
    
    // Private variables
    private Vector3 playerMid;

    private void Start()
    {
        instance = this;
        playerMid = PlayerData.instance.transform.position;
    }




    public void CreateMeshes()
    {
        // = Create all meshes

        InitPlayer();

        // Inner player
        CreatePlayerMesh(ref MeshRef.instance.innerPlayerMesh_mf);

        // Inner surface
        CreateMesh(ref MeshRef.instance.innerSurface_mf, EnvironmentData.instance.curVertices);
        CreateMesh(ref MeshRef.instance.innerMask_mf, PlayerData.instance.outerVertices);
        CreateMesh(ref MeshRef.instance.innerPlayerMask_mf, EnvironmentData.instance.curVertices);

        // Outer player
        CreatePlayerMesh(ref MeshRef.instance.outerPlayerMesh_mf);
        CreateMesh(ref MeshRef.instance.outerPlayerMask_mf, EnvironmentData.instance.curVertices);
    }





    // ----------------------------- private methods ----------------------------
    

    private void InitPlayer()
    {
        // = Create mesh form, create containers & set player variables

        // Create containers
        GameObject vertices = CreateContainer("Vertices", PlayerData.instance.transform);
        GameObject outside = CreateContainer("Outside", vertices.transform);
        GameObject inside = CreateContainer("Inside", vertices.transform);

        for (int i = 0; i < PlayerData.instance.verticesCount; i++)
        {
            // CREATE MESH FORM by calculating vertex positions
            Quaternion rot = Quaternion.Euler(0, 0, i * (360 / PlayerData.instance.verticesCount));
            Vector3 nextDirection = rot * Vector3.up;
            Vector3 nextOuterVertex = nextDirection.normalized;
            Vector3 nextInnerVertex = nextDirection.normalized * (1 - PlayerData.instance.innerWidth);

            // More containers
            GameObject newOuterVert = CreateContainer("Vert" + (i + 1), outside.transform);
            GameObject newInnerVert = CreateContainer("Vert" + (i + 1), inside.transform);
            newOuterVert.transform.position = PlayerData.instance.transform.position + nextOuterVertex;
            newInnerVert.transform.position = PlayerData.instance.transform.position + nextInnerVertex;

            // Assign positions to Player
            PlayerData.instance.outerVertices_obj[i] = newOuterVert.transform;
            PlayerData.instance.innerVertices_obj[i] = newInnerVert.transform;
            PlayerData.instance.outerVertices_mesh[i] = nextOuterVertex;
            PlayerData.instance.innerVertices_mesh[i] = nextInnerVertex;
        }
    }



    private void CreateMesh(ref MeshFilter mf, Vector3[] vertices)
    {
        // = Used for any triangle mesh that is not the player

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = new int[3] { 2, 1, 0 }; // count counter-clockwise!
        newMesh.normals = new Vector3[3] { Vector3.back, Vector3.back, Vector3.back };
        mf.mesh = newMesh;
        // no UVs
    }



    private void CreatePlayerMesh(ref MeshFilter mf)
    {
        // Declarations
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // Calculations
        for (int i = 0; i < PlayerData.instance.verticesCount; i++)
        {
            vertices.AddRange(new Vector3[2] {
                PlayerData.instance.outerVertices_mesh[i],
                PlayerData.instance.innerVertices_mesh[i] });
            triangles.AddRange(new int[6] {
                // outer triangle
                i *2,
                i *2+1,
                (i*2+2) % (PlayerData.instance.verticesCount*2),
                // inner triangle
                i *2+1 ,
                (i*2+3) % (PlayerData.instance.verticesCount*2),
                (i*2+2) % (PlayerData.instance.verticesCount*2) });
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

    private GameObject CreateContainer(string name, Transform parent)
    {
        GameObject newObj = new GameObject(name);
        newObj.transform.parent = parent;
        newObj.transform.localPosition = Vector3.zero;

        return newObj;
    }
}
