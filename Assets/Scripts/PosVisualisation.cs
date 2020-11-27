using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PosVisualisation : MonoBehaviour
{
    // public
    public static PosVisualisation instance;

    [Header("References")]
    public MeshFilter innerPlayerMesh_mf;
    public MeshFilter innerPlayerMask_mf;
    public MeshFilter innerSurface_mf;
    public MeshFilter innerMask_mf;
    public MeshFilter outerPlayerMesh_mf;
    public MeshFilter outerPlayerMask_mf;
    public GameObject environmentEdges;
    public LineRenderer curEdgePart_lr;
    public List<LineRenderer> curEdgePart2nd_lr;


    [Header("Settings")]
    public int envGridLoops = 6;
    public bool showCursor = true;

    [HideInInspector]
    public Vector3[] environmentVertices = new Vector3[3];

    // private
    private Vector3 playerMid;
    private LineRenderer lineRenderer_envEdges;
    private Vector3[] playerVertices = new Vector3[3];
    

    // get set
    Player player { get { return Player.instance; } }

    void Start()
    {
        instance = this;

        InitPlayerData();
        
        CreateMeshes();

        playerMid = GameObject.Find("Player").transform.position;
        lineRenderer_envEdges = environmentEdges.GetComponent<LineRenderer>();

        if (!showCursor)
            Cursor.visible = false;
    }


    void Update()
    {
        SetPlayerWidth();
        GetEnvironmentTriangle();

        SetPositionalStates();
        
        DrawEnvironmentEdges();
        DrawCurEdgePart();
        UpdateSurfacesTransforms();
    }


    void GetEnvironmentTriangle()
    {
        // 1) init
        RaycastHit[] edgeHits = new RaycastHit[3];
        environmentVertices = new Vector3[3];
        Vector3 intersection = Vector3.zero;

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


    void DrawCurEdgePart()
    {
        if (player.actionState == Player.ActionState.stickToEdge)
        {
            if (player.positionState == Player.PositionState.innerEdge || player.positionState == Player.PositionState.outerEdge)
            {
                // primary
                Vector3 pos1 = player.curEnvEdge.Item1 + (player.curEnvEdge.Item2 - player.curEnvEdge.Item1) / envGridLoops * player.curEnvEdgePart;
                Vector3 pos2 = player.curEnvEdge.Item1 + (player.curEnvEdge.Item2 - player.curEnvEdge.Item1) / envGridLoops * (player.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                curEdgePart_lr.positionCount = 2;
                curEdgePart_lr.SetPosition(0, pos1);
                curEdgePart_lr.SetPosition(1, pos2);

                // secondary
                pos1 = player.curEnvEdge_second.Item1 + (player.curEnvEdge_second.Item2 - player.curEnvEdge_second.Item1) / envGridLoops * player.curEnvEdgePart;
                pos2 = player.curEnvEdge_second.Item1 + (player.curEnvEdge_second.Item2 - player.curEnvEdge_second.Item1) / envGridLoops * (player.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                curEdgePart2nd_lr[0].positionCount = 2;
                curEdgePart2nd_lr[0].SetPosition(0, pos1);
                curEdgePart2nd_lr[0].SetPosition(1, pos2);

                pos1 = player.curEnvEdge_third.Item1 + (player.curEnvEdge_third.Item2 - player.curEnvEdge_third.Item1) / envGridLoops * player.curEnvEdgePart;
                pos2 = player.curEnvEdge_third.Item1 + (player.curEnvEdge_third.Item2 - player.curEnvEdge_third.Item1) / envGridLoops * (player.curEnvEdgePart + 1);
                pos1.z = playerMid.z - 0.001f;
                pos2.z = playerMid.z - 0.001f;
                curEdgePart2nd_lr[1].positionCount = 2;
                curEdgePart2nd_lr[1].SetPosition(0, pos1);
                curEdgePart2nd_lr[1].SetPosition(1, pos2);
            }
            else
            {
                curEdgePart_lr.positionCount = 0;
                curEdgePart2nd_lr[0].positionCount = 0;
                curEdgePart2nd_lr[1].positionCount = 0;
            }
        }
        else
        {
            curEdgePart_lr.positionCount = 0;
            curEdgePart2nd_lr[0].positionCount = 0;
            curEdgePart2nd_lr[1].positionCount = 0;
        }

    }


    // STATES
    void SetPositionalStates()
    {
        player.lastPosState = player.positionState;
        RaycastHit hit;
        if (Physics.Raycast(playerMid, player.outerVertices[0] - playerMid, out hit))
        {
            // Calculations
            float playerRadius = (player.outerVertices[0] - playerMid).magnitude;
            float envDistance = (hit.point - playerMid).magnitude;
            float playerToEnvDistance = Mathf.Abs(playerRadius - envDistance);
            float innerVertexToEnvDistance = (player.innerVertices[0] - (hit.point + (player.innerVertices[0] - hit.point).normalized * player.stickToOuterEdge_holeSize)).magnitude;

            float stickToEdgeTolerance = player.stickToEdgeTolerance;
            if (player.actionState == Player.ActionState.stickToEdge)
                stickToEdgeTolerance *= 3f;

            // States
            if (playerToEnvDistance < stickToEdgeTolerance && !player.startedBounce)
                player.positionState = Player.PositionState.innerEdge;
            else if (innerVertexToEnvDistance < stickToEdgeTolerance && !player.startedBounce)
                player.positionState = Player.PositionState.outerEdge;
            else if (playerRadius < envDistance)
                player.positionState = Player.PositionState.inside;
            else
                player.positionState = Player.PositionState.outside;

            if (player.positionState == Player.PositionState.innerEdge && player.lastPosState != Player.PositionState.innerEdge ||
                player.positionState == Player.PositionState.outerEdge && player.lastPosState != Player.PositionState.outerEdge)
                player.firstEdgeTouch = true;
            else
                player.firstEdgeTouch = false;
            

        // overwrite / hack
            // TO DO: if curState == outside && lastState == inside && mouseSpeed < x ----> StickToInnerEdge
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
        outerPlayerMesh_mf.transform.localScale = player.transform.localScale;
        outerPlayerMesh_mf.transform.eulerAngles = player.transform.eulerAngles;
        
        outerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(environmentVertices, this.transform);
        innerPlayerMask_mf.mesh.vertices = ExtensionMethods.ConvertArrayFromWorldToLocal(environmentVertices, this.transform);
    }

   

    void CreateMeshes()
    {
        // Inner player
        CreatePlayerMesh(ref innerPlayerMesh_mf);

        // Inner surface
        CreateMesh(ref innerSurface_mf, environmentVertices);
        CreateMesh(ref innerMask_mf, player.outerVertices);
        CreateMesh(ref innerPlayerMask_mf, environmentVertices);

        // Outer player
        CreatePlayerMesh(ref outerPlayerMesh_mf);
        CreateMesh(ref outerPlayerMask_mf, environmentVertices);

        
    }

    void CreateMesh(ref MeshFilter mf, Vector3[] vertices)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = new int[3] { 2, 1, 0 };
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
                player.outerVertices_mesh[i],
                player.innerVertices_mesh[i] });
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
        if (player.constantInnerWidth)
        {
            float neededWidthPerc = Mathf.Clamp01(player.innerWidth / player.transform.localScale.x);
            Vector3[] newVertices = innerPlayerMesh_mf.mesh.vertices;

            if ((player.outerVertices[0] - player.innerVertices[0]).magnitude != player.innerWidth)
            {
                //TO DO: if draußen auf wand skalieren ist nicht aktiv
                for (int i = 0; i < player.verticesCount; i++)
                {
                    // change innerVertices only
                    Vector3 newVertex = (player.outerVertices_mesh[i] - Vector3.zero).normalized * (1 - neededWidthPerc);
                    newVertices[(i * 2) + 1] = newVertex;
                    player.innerVertices_mesh[i] = newVertex;
                    player.innerVertices_obj[i].position = playerMid + (player.outerVertices[i] - playerMid) * (1 - neededWidthPerc);
                    player.innerVertices[i] = player.innerVertices_obj[i].position;
                }
            }
            innerPlayerMesh_mf.mesh.vertices = newVertices;
            outerPlayerMesh_mf.mesh.vertices = newVertices;

            // TO DO: mesh.recalculatenormals, -bounds, -tangents
        }
    }



    void InitPlayerData()
    {
        // Create containers
        GameObject vertices = new GameObject("Vertices");
        GameObject outside = new GameObject("Outside");
        GameObject inside = new GameObject("Inside");
        vertices.transform.parent = player.transform;
        outside.transform.parent = vertices.transform;
        inside.transform.parent = vertices.transform;
        vertices.transform.localPosition = Vector3.zero;

        for (int i = 0; i < player.verticesCount; i++)
        {
            // containers
            GameObject newOuterVert = new GameObject("Vert" + (i + 1));
            GameObject newInnerVert = new GameObject("Vert" + (i + 1));
            newOuterVert.transform.parent = outside.transform;
            newInnerVert.transform.parent = inside.transform;

            // Calc vertex positions
            Quaternion rot = Quaternion.Euler(0, 0, i * 120);
            Vector3 nextDirection = rot * Vector3.up;
            Vector3 nextOuterVertex = nextDirection.normalized;
            Vector3 nextInnerVertex = nextDirection.normalized * (1 - player.innerWidth);

            // assign
            newOuterVert.transform.position = player.transform.position + nextOuterVertex;
            newInnerVert.transform.position = player.transform.position + nextInnerVertex;

            player.outerVertices_obj[i] = newOuterVert.transform;
            player.innerVertices_obj[i] = newInnerVert.transform;

            player.outerVertices_mesh[i] = nextOuterVertex;
            player.innerVertices_mesh[i] = nextInnerVertex;
        }
    }
}
