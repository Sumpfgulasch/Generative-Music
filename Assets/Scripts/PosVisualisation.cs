using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PosVisualisation : MonoBehaviour
{
    // public
    [Header("Distance to environment visualisation")]
    public GameObject perfectTriangle;
    public MeshFilter surfaceInside_mf;
    public MeshFilter maskInside_mf;
    public MeshFilter surfaceOutside_mf;
    public MeshFilter maskOutside_mf;

    [Header("Führt zu ungenauen States")]
    public float offset = 1f;
    [Header("to be replaced...")]
    public Transform[] playerVertices_hack;

    // private
    private PolygonCollider2D playerCollider;
    private Vector3 playerMid;
    private LineRenderer lineRenderer_perfectTriangle;
    private Vector3[] playerVertices = new Vector3[3];
    private Vector3[] environmentVertices = new Vector3[3];
    //private Mesh insideSurface_mesh;
    //private Mesh insideSurface_mask;


    void Start()
    {
        playerCollider = GameObject.Find("Player").GetComponent<PolygonCollider2D>();
        playerMid = GameObject.Find("Player").transform.position;
        lineRenderer_perfectTriangle = perfectTriangle.GetComponent<LineRenderer>();

        InitMeshes();
    }


    void Update()
    {
        GetPlayerData();
        GetEnvironmentTriangle();

        CalcStates();
        
        DrawPerfectTriangle();
        UpdateSurfacePositions();
    }


    void GetEnvironmentTriangle()
    {
        // 1) init
        RaycastHit[] edgeHits = new RaycastHit[3];
        environmentVertices = new Vector3[3];
        Vector3 intersection;

        // 2) Prepare raycast
        for (int i = 0; i < playerVertices.Length; i++)
        {
            Vector3 triangleEdgeMid = playerVertices[i] + ((playerVertices[(i + 1) % 3] - playerVertices[i]) / 2f);
            triangleEdgeMid.z = perfectTriangle.transform.position.z;
            Vector3 directionOut = (triangleEdgeMid - playerMid).normalized;
            RaycastHit hit;

            // 3) Raycasts from player to environment
            if (Physics.Raycast(playerMid, directionOut, out hit))
            {
                edgeHits[i] = hit;
                Debug.DrawLine(triangleEdgeMid, hit.point, Color.red);
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
                // 5) Offset
                intersection = intersection + (perfectTriangle.transform.position - intersection).normalized * offset;
                environmentVertices[i] = intersection;
            }
        }

        for (int i = 0; i < environmentVertices.Length; i++)
            Debug.DrawLine(environmentVertices[i], environmentVertices[(i + 1) % 3], Color.blue);
    }

    void DrawPerfectTriangle()
    {
        // 1) Add extra points for LineRenderer
        List<Vector3> newPositions = environmentVertices.ToList();
        int insertCounter = 0;
        for (int i = 1; i < environmentVertices.Length; i++)
        {
            // insert before
            newPositions.Insert(i + insertCounter, environmentVertices[i]);
            insertCounter++;
            // insert after
            newPositions.Insert(i + 1 + insertCounter, environmentVertices[i]);
            insertCounter++;
        }
        newPositions.Add(environmentVertices[0]);

        
        // 2) Add to LineRenderer
        lineRenderer_perfectTriangle.positionCount = newPositions.Count;
        lineRenderer_perfectTriangle.SetPositions(newPositions.ToArray());
    }



    // Check for intersections of environmentVertices and playerVertices
    void CalcStates()
    {
        if (environmentVertices[0] == Vector3.zero)
        {
            // STATE: NO TUNNEL
            Player.instance.state = Player.State.noTunnel;
        }
        else
        {
            int counter = 0;
            for (int i = 0; i < environmentVertices.Length; i++)
            {
                // 1.1. Checke ob die erste environmentEdge zwei der drei playerEdges schneidet
                counter = 0;
                Vector2 intersection;
                Vector3 environmentPoint1 = environmentVertices[i];
                Vector3 environmentPoint2 = environmentVertices[(i + 1) % 3];

                for (int j = 0; j < playerVertices.Length; j++)
                {
                    Vector3 playerPoint1 = playerVertices[j];
                    Vector3 playerPoint2 = playerVertices[(j + 1) % 3];

                    if (ExtensionMethods.LineSegmentsIntersection(out intersection, environmentPoint1, environmentPoint2, playerPoint1, playerPoint2))
                    {
                        // STATE: OUTSIDE
                        Player.instance.state = Player.State.outside;
                        counter++;

                        #region OuterTringle vertices setzen
                        // create outer triangle
                        //outerTriangles[i,j] = new Vector3(intersection.x, intersection.y, Player.instance.transform.position.z);

                        // set missing vertex of outer triangle
                        //if (counter == 2)
                        //{
                        //    if (outerTriangles[i,j-1] == null)
                        //       outerTriangles[i,j-1] = playerPoint2;
                        //    else
                        //        outerTriangles[i,(j+1)%3] = playerPoint1;
                        //}
                        #endregion
                    }
                }
            }

            // TO DO: perfect state
            //if (playerToEnvironment_distance <= x)
            //    {
            //    Player.instance.state = Player.State.perfect;
            //}

            if (counter == 0) // else hinzu
            {
                // STATE: INSIDE
                Player.instance.state = Player.State.inside;
            }
        }
    }


    void UpdateSurfacePositions()
    {
        // INSIDE
        // Surface
        surfaceInside_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(environmentVertices, this.transform);

        // Mask
        maskInside_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(playerVertices, this.transform);

        // OUTSIDE
        // Surface

        // Mask
    }

   

    void InitMeshes()
    {
        // inside mesh
        InitMesh(ref surfaceInside_mf, environmentVertices);

        // inside mask
        InitMesh(ref maskInside_mf, playerVertices);

    }

    void InitMesh(ref MeshFilter mf, Vector3[] vertices)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = new int[3] { 0, 1, 2 };
        newMesh.normals = new Vector3[3] { Vector3.back, Vector3.back, Vector3.back };
        mf.mesh = newMesh;
        // no UVs
    }



    void GetPlayerData()
    {
        for (int i = 0; i < playerVertices.Length; i++)
        {
            // hack
            playerVertices[i] = playerVertices_hack[i].position;
        }
    }
}
