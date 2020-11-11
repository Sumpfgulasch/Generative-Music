using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PosVisualisation : MonoBehaviour
{
    // public
    [Header("General references")]
    public Transform playerMesh;
    [Header("Distance to environment visualisation")]
    public GameObject environmentEdges;
    public MeshFilter innerSurface_mf;
    public MeshFilter innerMask_mf;
    public GameObject outerSurface_obj;
    public MeshFilter outerMask_mf;

    [Header("Führt zu ungenauen States")]
    public float offset = 1f;
    [Header("to be replaced...")]
    public Transform[] playerVertices_hack;

    // private
    private PolygonCollider2D playerCollider;
    private Vector3 playerMid;
    private LineRenderer lineRenderer_envEdges;
    private Vector3[] playerVertices = new Vector3[3];
    private Vector3[] environmentVertices = new Vector3[3];


    void Start()
    {
        playerCollider = GameObject.Find("Player").GetComponent<PolygonCollider2D>();
        playerMid = GameObject.Find("Player").transform.position;
        lineRenderer_envEdges = environmentEdges.GetComponent<LineRenderer>();

        InitMeshes();
    }


    void Update()
    {
        GetPlayerData();
        GetEnvironmentTriangle();

        SetPositionalStates();
        
        DrawEnvironmentEdges();
        UpdateSurfacesTransforms();
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
            triangleEdgeMid.z = environmentEdges.transform.position.z;
            Vector3 directionOut = (triangleEdgeMid - playerMid).normalized;
            RaycastHit hit;

            // 3) Raycasts from player to environment
            if (Physics.Raycast(playerMid, directionOut, out hit))
            {
                edgeHits[i] = hit;
                //Debug.DrawLine(triangleEdgeMid, hit.point, Color.red);
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
                intersection = intersection + (environmentEdges.transform.position - intersection).normalized * offset;
                environmentVertices[i] = intersection;
            }
        }

        for (int i = 0; i < environmentVertices.Length; i++)
            Debug.DrawLine(environmentVertices[i], environmentVertices[(i + 1) % 3], Color.blue);
    }

    void DrawEnvironmentEdges()
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
        lineRenderer_envEdges.positionCount = newPositions.Count;
        lineRenderer_envEdges.SetPositions(newPositions.ToArray());
    }


    // STATES
    void SetPositionalStates()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerMid, playerVertices[0] - playerMid, out hit))
        {
            // calc data
            float playerRadius = (playerVertices[0] - playerMid).magnitude;
            float envDistance = (hit.point - playerMid).magnitude;
            float playerToEnvDistance = playerRadius - envDistance;

            // states
            if (Mathf.Abs(playerToEnvDistance) < Player.instance.edgeTolerance)
                Player.instance.positionState = Player.PositionState.edge;
            else if (playerRadius < envDistance)
                Player.instance.positionState = Player.PositionState.inside;
            else
                Player.instance.positionState = Player.PositionState.outside;
        }
        else
            Player.instance.positionState = Player.PositionState.noTunnel;
    }


    void UpdateSurfacesTransforms()
    {
        // Inner surface
        innerSurface_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(environmentVertices, this.transform);
        innerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(playerVertices, this.transform);

        // Outer surface
        outerSurface_obj.transform.localScale = new Vector3(
            playerMesh.localScale.x * Player.instance.transform.localScale.x,
            playerMesh.localScale.y * Player.instance.transform.localScale.y,
            playerMesh.localScale.z * Player.instance.transform.localScale.z); // TO DO: unnötige scheiße; später nicht mehr nötig wenn playerMesh generiert wird (und dessen scale 1 ist)
        outerSurface_obj.transform.eulerAngles = Player.instance.transform.eulerAngles;
        outerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(environmentVertices, this.transform);
    }

   

    void InitMeshes()
    {
        // Inner surface
        InitMesh(ref innerSurface_mf, environmentVertices);
        InitMesh(ref innerMask_mf, playerVertices);

        // Outside
        InitMesh(ref outerMask_mf, environmentVertices);
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
