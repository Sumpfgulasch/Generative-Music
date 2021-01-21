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
    private float distancePerBeat;






    void Start()
    {
        instance = this;

        // Init
        GetData();
        InitiallySpawnObjects();

        // EVENTS
        MusicRef.inst.beatSequencer.beatEvent.AddListener(OnBeat);
    }
    

    // ------------------------------ Events ------------------------------



    private void OnBeat(int beat)
    {
        if (beat == 0)
        {
            SpawnObject();
            DeleteFarObjects();
        }
    }



    // ------------------------- Private functions -------------------------




    private void GetData()
    {
        Vector3 start = availableObjects[0].GetComponent<Move>().start.transform.position;
        Vector3 end = availableObjects[0].GetComponent<Move>().end.transform.position;
        tunnelLength = (start - end).magnitude;

        int FPS = Screen.currentResolution.refreshRate;
        moveSpeed = (tunnelLength / LoopData.TimePerBar()) / FPS;

        distancePerBeat = tunnelLength / LoopData.beatsPerBar;


    }

    private void InitiallySpawnObjects()
    {
        float initalZPos = Player.inst.transform.position.z + distancePerBeat;
        movingObjects = new List<GameObject>();

        for (int i = 0; i < maxObjects-1; i++)
        {
            GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
            
            newObj = Instantiate(newObj, new Vector3(0, 0, initalZPos + i * tunnelLength), Quaternion.identity);
            movingObjects.Add(newObj);
        }
    }

    private void SpawnObject()
    {
        zSpawn = Player.inst.transform.position.z;
        GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
        newObj = Instantiate(newObj, new Vector3(0, 0, zSpawn + (maxObjects - 1) * tunnelLength), Quaternion.identity);
        movingObjects.Add(newObj);
    }

    private void DeleteFarObjects()
    {
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
    }
}
