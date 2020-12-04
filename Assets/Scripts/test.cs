using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public MeshFilter mesh;
    Vector3[] outerVertices, innerVertices;

    // Start is called before the first frame update
    void Start()
    {
        outerVertices = new Vector3[3] { Vector3.up, Vector3.right, Vector3.down };
        innerVertices = new Vector3[3] { 0.7f * Vector3.up, 0.7f * Vector3.right, 0.7f * Vector3.down };
        Vector3[] vertices = new Vector3[3] { Vector3.up, Vector3.right, Vector3.down};
        //CreateMesh(ref mesh, vertices);
        CreatePlayerMesh(ref mesh);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void CreateMesh(ref MeshFilter mf, Vector3[] vertices)
    {
        // = Used for any triangle mesh that is not the player

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = new int[3] { 0, 1, 2 }; // count clockwise!
        newMesh.normals = new Vector3[3] { Vector3.forward, Vector3.forward, Vector3.forward };
        newMesh.RecalculateNormals();
        mf.mesh = newMesh;
        // no UVs
    }


    public void CreatePlayerMesh(ref MeshFilter mf)
    {
        // Declarations
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // Calculations
        for (int i = 0; i < 3; i++)
        {
            vertices.AddRange(new Vector3[2] {
                outerVertices[i],
                innerVertices[i] });
            triangles.AddRange(new int[6] {
                // outer triangle
                i *2,
                (i*2+2) % (3*2),
                i *2+1,
                // inner triangle
                i *2+1 ,
                (i*2+2) % (3*2),
                (i*2+3) % (3*2) });
            normals.AddRange(new Vector3[2] {
                -Vector3.forward,
                -Vector3.forward });
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


}
