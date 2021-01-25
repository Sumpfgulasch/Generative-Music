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
        MusicRef.inst.beatSequencer.beatEvent.AddListener(OnBeat);
        GameEvents.inst.onFirstBeat += InstantiateFirstObjects_beat;
    }



    // ------------------------- Public functions -------------------------



    public void InitSpawn()
    {
        
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

    private void InstantiateFirstObjects_start()
    {
        float initalZPos = playerZpos + distancePerBeat;
        movingObjects = new List<GameObject>();

        for (int i = 0; i < maxObjects-1; i++)
        {
            GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
            
            newObj = Instantiate(newObj, new Vector3(0, 0, initalZPos + i * tunnelLength), Quaternion.identity);
            movingObjects.Add(newObj);
        }
    }

    public void InstantiateFirstObjects_beat()
    {
        for (int i = 0; i < maxObjects - 1; i++)
        {
            GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];

            newObj = Instantiate(newObj, new Vector3(0, 0, playerZpos + i * tunnelLength), Quaternion.identity);
            movingObjects.Add(newObj);
        }
    }

    private void SpawnObject()
    {
        GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
        newObj = Instantiate(newObj, new Vector3(0, 0, playerZpos + (maxObjects - 1) * tunnelLength), Quaternion.identity);
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

    /// <summary>
    /// Move each field displaced from back to front. Replace old fields when done.
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="beatsToStart">Mesured in beats [1 bar == beats per bar].</param>
    /// <param name="durationInBeats">Measured in beats.</param>
    /// <returns></returns>
    public IEnumerator SpawnMusicFields(MusicField[] fields, float beatsToStart, float durationInBeats)
    {
        float zSpawn = playerZpos + distancePerBeat * beatsToStart;
        float durationTime = durationInBeats * LoopData.timePerBeat;
        float timeToWait = durationTime / (fields.Length - 1);

        for (int i=0; i<fields.Length; i++)
        {
            StartCoroutine(MoveFieldFromBackToFront(fields[i], zSpawn));
            //Debug.Log("instantiate; time: " + Time.time);
            yield return new WaitForSeconds(timeToWait);
        }
    }


    private IEnumerator MoveFieldFromBackToFront(MusicField field, float zSpawn)
    {
        float beatsToStart = (zSpawn - playerZpos) / distancePerBeat;
        float timeToFinish = beatsToStart * LoopData.timePerBeat;
        float timer = 0;
        float zPos = zSpawn;

        while (timer < timeToFinish)
        {
            zPos = zPos - moveSpeed * deltaTime;
            field.SetZPos(zPos);

            timer += Time.deltaTime;
            yield return null;
        }

        field.SetZPos(playerZpos - VisualController.inst.fieldsBeforeSurface);

        field.isBuildingUp = false;

        // aktiviere variablen
    }
}
