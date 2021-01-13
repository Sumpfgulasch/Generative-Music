using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // public
    [Header("Objects")]
    public static ObjectSpawner instance;
    public List<GameObject> availableObjects;
    public int maxObjects;

    [HideInInspector] public float moveSpeed;

    // private
    private List<GameObject> movingObjects;
    private float zSpawn;
    private float tunnelLength;



    

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        tunnelLength = GetTunnelLength();
        moveSpeed = GetMoveSpeed();

        StartCoroutine(SpawnObjects());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private float GetMoveSpeed() 
    {
        int FPS = Screen.currentResolution.refreshRate;
        return (tunnelLength / LoopData.TimePerBar()) / FPS;
    }

    private float GetTunnelLength()
    {
        Vector3 start = availableObjects[0].GetComponent<Move>().start.transform.position;
        Vector3 end = availableObjects[0].GetComponent<Move>().end.transform.position;
        //return 2f;
        return (start - end).magnitude;
    }

    IEnumerator SpawnObjects()
    {
        movingObjects = new List<GameObject>();
        float distancePerBeat = tunnelLength / LoopData.beatsPerBar;
        zSpawn = Player.inst.transform.position.z + distancePerBeat;

        // 1. Initial instatiations
        for (int i = 0; i<maxObjects-1; i++)
        {
            GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
            newObj = Instantiate(newObj, new Vector3(0, 0, zSpawn + i * tunnelLength), Quaternion.identity);
            movingObjects.Add(newObj);
        }

        while (true)
        {
            // 2. REGULAR new objects
            GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
            newObj = Instantiate(newObj, new Vector3(0, 0, zSpawn + (maxObjects - 1) * tunnelLength), Quaternion.identity);
            //newObj.GetComponentInChildren<MeshCollider>().
            movingObjects.Add(newObj);

            // delete when not visible anymore
            List<GameObject> objects2destroy = new List<GameObject>();
            foreach (GameObject obj in movingObjects)
            {
                if (obj.transform.position.z < -20)
                    objects2destroy.Add(obj);
            }
            for (int i = 0; i < objects2destroy.Count; i++)
            {
                movingObjects.Remove(objects2destroy[i]);
                Destroy(objects2destroy[i]);

            }
            yield return new WaitForSeconds(LoopData.TimePerBar());
        }
    }
}
