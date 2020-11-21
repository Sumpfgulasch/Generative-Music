using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PosVisualisation : MonoBehaviour
{
    // public

    [Header("References")]
    public MeshFilter innerPlayerMesh_mf;
    public MeshFilter innerPlayerMask_mf;
    public MeshFilter innerSurface_mf;
    public MeshFilter innerMask_mf;
    public GameObject outerPlayerSurface_obj;
    public MeshFilter outerPlayerMask_mf;
    public GameObject environmentEdges;

    [Header("Führt zu ungenauen States")]
    public float offset = 1f;
    [Header("to be replaced...")]
    public Transform[] playerVertices_hack;

    // private
    private Vector3 playerMid;
    private LineRenderer lineRenderer_envEdges;
    private Vector3[] playerVertices = new Vector3[3];
    private Vector3[] environmentVertices = new Vector3[3];

    // get set
    Player player { get { return Player.instance; } }

    void Start()
    {
        SetPlayerData();
        
        CreateMeshes();

        playerMid = GameObject.Find("Player").transform.position;
        lineRenderer_envEdges = environmentEdges.GetComponent<LineRenderer>();
    }


    void Update()
    {
        SetPlayerWidth();
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
        for (int i = 0; i < player.verticesCount; i++)
        {
            Vector3 playerEdgeMid = player.outerVertices[i] + ((player.outerVertices[(i + 1) % 3] - player.outerVertices[i]) / 2f);
            playerEdgeMid.z = environmentEdges.transform.position.z;
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
                // 5) Offset
                intersection = intersection + (environmentEdges.transform.position - intersection).normalized * offset;
                environmentVertices[i] = intersection;
            }
        }
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
        player.lastPosState = player.positionState;
        RaycastHit hit;
        if (Physics.Raycast(playerMid, player.outerVertices[0] - playerMid, out hit))
        {
            float playerRadius = (player.outerVertices[0] - playerMid).magnitude;
            float envDistance = (hit.point - playerMid).magnitude;
            float playerToEnvDistance = playerRadius - envDistance;

            // states
            if (Mathf.Abs(playerToEnvDistance) < player.edgeTolerance && !player.startedBounce)
                player.positionState = Player.PositionState.edge;
            else if (playerRadius < envDistance)
                player.positionState = Player.PositionState.inside;
            else
                player.positionState = Player.PositionState.outside;
        }
        else
            player.positionState = Player.PositionState.noTunnel;
    }


    void UpdateSurfacesTransforms()
    {
        // Inner surface
        innerSurface_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(environmentVertices, this.transform);
        innerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(player.outerVertices, this.transform);
        innerPlayerMesh_mf.transform.localPosition = Vector3.zero;

        // Outer player
        outerPlayerSurface_obj.transform.localScale = new Vector3(
            innerPlayerMesh_mf.transform.localScale.x * player.transform.localScale.x,
            innerPlayerMesh_mf.transform.localScale.y * player.transform.localScale.y,
            innerPlayerMesh_mf.transform.localScale.z * player.transform.localScale.z); // TO DO: unnötige scheiße; später nicht mehr nötig wenn playerMesh generiert wird (und dessen scale 1 ist)
        outerPlayerSurface_obj.transform.eulerAngles = player.transform.eulerAngles;
        outerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(environmentVertices, this.transform);
        innerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(environmentVertices, this.transform);
    }

   

    void CreateMeshes()
    {
        // Data & inner player
        CreatePlayerMesh(ref innerPlayerMesh_mf);

        // Inner surface
        CreateMesh(ref innerSurface_mf, environmentVertices);
        CreateMesh(ref innerMask_mf, player.outerVertices);
        CreateMesh(ref innerPlayerMask_mf, environmentVertices);

        // Outer player
        CreateMesh(ref outerPlayerMask_mf, environmentVertices);

        
    }

    void CreateMesh(ref MeshFilter mf, Vector3[] vertices)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = new int[3] { 0, 1, 2 };
        newMesh.normals = new Vector3[3] { Vector3.back, Vector3.back, Vector3.back };
        mf.mesh = newMesh;
        // no UVs
    }

    void CreatePlayerMesh(ref MeshFilter mf)
    {
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        
        for (int i=0; i < player.verticesCount; i++)
        {
            vertices.AddRange(new Vector3[2] {
                player.outerMeshVertices[i],
                player.innerMeshVertices[i] });
            triangles.AddRange(new int[6] {
                // outer triangle
                i *2,                                   
                i *2+1,
                (i*2+2) % (player.verticesCount*2),
                // inner triangle
                i *2+1 ,     
                (i*2+3) % (player.verticesCount*2),
                (i*2+2) % (player.verticesCount*2) });
            normals.AddRange(new Vector3[2] {
                Vector3.back,
                Vector3.back });
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.MarkDynamic();
        mf.mesh = newMesh;
        // no UVs
    }

    void SetPlayerWidth()
    {
        Vector3 convertedVert = innerPlayerMesh_mf.transform.TransformPoint(player.innerMeshVertices[0]);
        //this.transform.trans
        float neededWidthPerc = (1 / player.transform.localScale.x) * player.innerWidth;
        //player.curInnerWidth
        Vector3[] newVertices = innerPlayerMesh_mf.mesh.vertices;

        Debug.DrawLine(convertedVert, player.transform.position, Color.magenta);
        if ((player.outerVertices[0] - player.innerVertices[0]).magnitude != player.innerWidth)
        {
            //TO DO: if draußen auf wand skalieren ist nicht aktiv
            for (int i=0; i<player.verticesCount; i++)
            {
                // change innerVertices only
                newVertices[(i * 2) + 1] = (player.outerMeshVertices[i * 1] - Vector3.zero).normalized * (1-neededWidthPerc);
                // TO DO: testen
            }

        }

        innerPlayerMesh_mf.mesh.vertices = newVertices;


        // TO DO: mesh.recalculatenormals, -bounds, -tangents
    }



    void SetPlayerData()
    {
        // Create Container
        GameObject vertices = new GameObject("Vertices");
        GameObject outside = new GameObject("Outside");
        GameObject inside = new GameObject("Inside");
        vertices.transform.parent = player.transform;
        outside.transform.parent = vertices.transform;
        inside.transform.parent = vertices.transform;
        vertices.transform.localPosition = Vector3.zero;

        for (int i = 0; i < player.verticesCount; i++)
        {
            // Calc vertex positions
            Quaternion rot = Quaternion.Euler(0, 0, i * 120);
            Vector3 nextDirection = rot * Vector3.up;
            Vector3 nextOuterVertex = nextDirection.normalized;
            Vector3 nextInnerVertex = nextDirection.normalized * (1 - player.innerWidth);

            // assign
            player.outerMeshVertices[i] = nextOuterVertex;
            player.innerMeshVertices[i] = nextInnerVertex;

            GameObject newOuterVert = new GameObject("Vert" + (i + 1));
            GameObject newInnerVert = new GameObject("Vert" + (i + 1));
            newOuterVert.transform.parent = outside.transform;
            newInnerVert.transform.parent = inside.transform;
            newOuterVert.transform.position = player.transform.position + nextOuterVertex;
            newInnerVert.transform.position = player.transform.position + nextInnerVertex;

            player.outerVertices_obj[i] = newOuterVert.transform;
            player.innerVertices_obj[i] = newInnerVert.transform;
        }
    }
}
