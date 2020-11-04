using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PosVisualisation : MonoBehaviour
{
    // public
    [Header("Distance to environment visualisation")]
    public float offset = 1f;

    // private
    private PolygonCollider2D playerCollider;
    private Vector3 playerMid;
    private LineRenderer lineRenderer;
    private Vector2[] playerVertices;
    private Vector3[] environmentVertices = new Vector3[3];

    void Start()
    {
        playerCollider = GameObject.Find("Player").GetComponent<PolygonCollider2D>();
        playerMid = GameObject.Find("Player").transform.position;
        lineRenderer = this.GetComponent<LineRenderer>();
    }


    void Update()
    {
        GetPlayerData();
        VisualizeCurrentPlane();
        CalcStates();
    }




    void VisualizeCurrentPlane()
    {
        // 1) init
        RaycastHit[] edgeHits = new RaycastHit[3];
        environmentVertices = new Vector3[3];
        Vector3 intersection;

        // 2) Prepare raycast
        for (int i = 0; i < playerVertices.Length; i++)
        {
            Vector3 triangleEdgeMid = playerVertices[i] + ((playerVertices[(i + 1) % 3] - playerVertices[i]) / 2f);
            triangleEdgeMid.z = this.transform.position.z;
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
                intersection = intersection + (this.transform.position - intersection).normalized * offset;
                environmentVertices[i] = intersection;
            }
        }

        for (int i = 0; i < environmentVertices.Length; i++)
            Debug.DrawLine(environmentVertices[i], environmentVertices[(i + 1) % 3], Color.blue);


        // 6) Add extra points for LineRenderer
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

        
        // 7) Add to LineRenderer
        lineRenderer.positionCount = newPositions.Count;
        lineRenderer.SetPositions(newPositions.ToArray());
    }

    void CalcStates()
    {
        Vector3[,] outerTriangles = new Vector3[3,3];
        Vector3[,] innerTriangles = new Vector3[3,3];
        int counter = 0;

        // 1) Inner triangles
        for (int i=0; i<environmentVertices.Length; i++)
        {
            // 1.1. Checke ob die erste environmentEdge zwei der drei playerEdges schneidet
            counter = 0;
            Vector2 intersection;
            Vector3 environmentPoint1 = environmentVertices[i];
            Vector3 environmentPoint2 = environmentVertices[(i + 1) % 3];
            
            for (int j = 0; j<playerCollider.points.Length; j++)
            {
                Vector3 playerPoint1 = playerCollider.points[j];
                Vector3 playerPoint2 = playerCollider.points[(j+1)%3];

                if (ExtensionMethods.LineSegmentsIntersection(out intersection, environmentPoint1, environmentPoint2, playerPoint1, playerPoint2))
                {
                    // STATE: OUTSIDE
                    Player.instance.state = Player.State.outside;

                    // create outer triangle
                    outerTriangles[i,j] = new Vector3(intersection.x, intersection.y, Player.instance.transform.position.z);
                    counter++;

                    // set missing vertex of outer triangle
                    if (counter == 2)
                    {
                        if (outerTriangles[i,j-1] == null)
                           outerTriangles[i,j-1] = playerPoint2;
                        else
                            outerTriangles[i,(j+1)%3] = playerPoint1;
                    }
                }

                // create inner triangle
            }
        }
        if (counter == 0)
        {
            // STATE: INSIDE
            Player.instance.state = Player.State.outside;
        }
        // TO DO: States und intersections debuggen

        //print("State: " + Player.instance.state);
    }



    void GetPlayerData()
    {
        playerVertices = playerCollider.points;
        for (int i = 0; i < playerVertices.Length; i++)
            playerVertices[i] = this.transform.TransformPoint(playerVertices[i]);
    }
}
