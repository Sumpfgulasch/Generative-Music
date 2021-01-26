using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // public
    [Header("Objects")]
    public static ObjectSpawner inst;
    public List<GameObject> availableObjects;
    public int maxObjects;

    [HideInInspector] public float moveSpeed;

    // private
    private List<GameObject> movingObjects;
    private float playerZpos;
    private float tunnelLength;
    private float distancePerBeat;
    private float FPS;

    // Properties
    private float deltaTime { get { return Time.deltaTime * FPS; }
    }


    private void Awake()
    {
        inst = this;
    }

    private void OnEnable()
    {
        
    }

    void Start()
    {
        FPS = Screen.currentResolution.refreshRate;

        GetData();

        // EVENTS
        //MusicRef.inst.beatSequencer.beatEvent.AddListener(ObjectSpawner.inst.OnBeat);
    }



    // ------------------------- Public functions -------------------------



    public void InitSpawn()
    {
        
    }
    

    // ------------------------------ Events ------------------------------



    public void OnBeat(int beat)
    {
        if (beat == 0)
        {
            SpawnTunnel();
            DeleteFarObjects();
        }
    }



    // ------------------------- Private functions -------------------------



    /// <summary>
    /// Distance-related data to beats.
    /// </summary>
    private void GetData()                                                  // to do: mit loopData-data mischen?
    {
        Vector3 start = availableObjects[0].GetComponent<Move>().start.transform.position;
        Vector3 end = availableObjects[0].GetComponent<Move>().end.transform.position;
        tunnelLength = (start - end).magnitude;

        int FPS = Screen.currentResolution.refreshRate;
        moveSpeed = (tunnelLength / LoopData.timePerBar) / FPS;

        distancePerBeat = tunnelLength / LoopData.beatsPerBar;

        playerZpos = Player.inst.transform.position.z;

        movingObjects = new List<GameObject>();
    }

    //private void InstantiateFirstObjects_start()
    //{
    //    float initalZPos = playerZpos + distancePerBeat;
    //    movingObjects = new List<GameObject>();

    //    for (int i = 0; i < maxObjects-1; i++)
    //    {
    //        GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
            
    //        newObj = Instantiate(newObj, new Vector3(0, 0, initalZPos + i * tunnelLength), Quaternion.identity);
    //        movingObjects.Add(newObj);
    //    }
    //}


    public IEnumerator InstantiateFirstTunnels(float timeToSpawnInBeats, float spawnDistanceInBeats, int amount)
    {
        // 1. wait
        float waitTime = timeToSpawnInBeats * LoopData.timePerBeat;
        yield return new WaitForSeconds(waitTime);

        // 2. Spawn
        for (int i = 0; i < amount; i++)
        {
            GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];

            float zSpawn = playerZpos + distancePerBeat * spawnDistanceInBeats;

            newObj = Instantiate(newObj, new Vector3(0, 0, zSpawn + i * tunnelLength), Quaternion.identity);
            movingObjects.Add(newObj);

            // move milk surface from back to front
            //if (i == 0)
            //{
            //    StartCoroutine(MoveObjectFromBackToFront(MeshRef.inst.innerSurface_mf.gameObject, newObj.transform));
            //}
        }
        yield return null;
    }


    private void SpawnTunnel()
    {
        GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
        newObj = Instantiate(newObj, new Vector3(0, 0, playerZpos + (maxObjects - 1) * tunnelLength), Quaternion.identity);
        movingObjects.Add(newObj);
    }


    private IEnumerator MoveObjectFromBackToFront(GameObject obj, Transform parent)
    {
        obj.GetComponent<MeshRenderer>().enabled = true;

        float zPos = parent.position.z;

        while (zPos > playerZpos)
        {
            obj.transform.position = parent.position;
            zPos = obj.transform.position.z;
            yield return null;
        }
        
    }

    private IEnumerator SetObjectVisible(GameObject obj, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        obj.GetComponent<MeshRenderer>().enabled = true;
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

    /// <summary>
    /// Move each field displaced from back to front. Replace old fields when done.
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="spawnDistanceInBeats">Mesured in beats [1 bar == beats per bar].</param>
    /// <param name="durationInBeats">Measured in beats.</param>
    /// <returns></returns>
    public IEnumerator SpawnMusicFields(MusicField[] fields, float timeToSpawnInBeats, float spawnDistanceInBeats, float durationInBeats)
    {
        // 1. wait
        float waitTime = timeToSpawnInBeats * LoopData.timePerBeat;
        yield return new WaitForSeconds(waitTime);

        // 2. instantiate all, wait between
        //float zSpawn = playerZpos + distancePerBeat * spawnDistanceInBeats;
        float durationTime = durationInBeats * LoopData.timePerBeat;
        float timeToWait = durationTime / (fields.Length - 1);

        for (int i=0; i<fields.Length; i++)
        {
            StartCoroutine(MoveFieldFromBackToFront(fields[i], spawnDistanceInBeats, durationInBeats));

            // milk surface
            if (i == fields.Length-1)
            {
                float time = spawnDistanceInBeats * LoopData.timePerBeat;
                StartCoroutine(SetObjectVisible(MeshRef.inst.innerSurface_mf.gameObject, time));
            }
            yield return new WaitForSeconds(timeToWait);
        }
    }


    private IEnumerator MoveFieldFromBackToFront(MusicField field, float spawnDistanceInBeats, float durationInBeats)
    {
        float zSpawn = playerZpos + distancePerBeat * spawnDistanceInBeats;
        float duration = spawnDistanceInBeats * LoopData.timePerBeat;

        float timer = 0;
        float zPos = zSpawn;

        while (timer < duration)
        {
            zPos -= moveSpeed * deltaTime;
            field.SetZPos(zPos);

            timer += Time.deltaTime;
            yield return null;
        }

        field.SetZPos(playerZpos - VisualController.inst.fieldsBeforeSurface);

        field.isBuildingUp = false;

        // aktiviere variablen
    }
}
