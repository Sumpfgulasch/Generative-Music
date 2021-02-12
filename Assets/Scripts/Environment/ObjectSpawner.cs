using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // public
    [Header("Objects")]
    public static ObjectSpawner inst;
    public List<GameObject> availableObjects;
    //public int maxObjects;

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
    private void GetData()
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeToSpawnInBeats">Time in beats that passes until the first tunnel gets instantiated.</param>
    /// <param name="spawnDistanceInBeats"></param>
    public IEnumerator InstantiateFirstTunnels(float timeToSpawnInBeats, float spawnDistanceInBeats)
    {
        // 1. wait
        float waitTime = timeToSpawnInBeats * LoopData.timePerBeat;
        yield return new WaitForSeconds(waitTime);

        // 2. EVENT SUBSCRIPTION
        GameEvents.inst.onBeat += OnBeat;

        // 3. Spawn
        for (int i = 0; i < GameplayManager.inst.maxTunnelsAtOnce; i++)
        {
            GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];

            float zSpawn = playerZpos + distancePerBeat * spawnDistanceInBeats;

            newObj = Instantiate(newObj, new Vector3(0, 0, zSpawn + i * tunnelLength), Quaternion.identity);
            movingObjects.Add(newObj);
        }
        yield return null;
    }


    private void SpawnTunnel()
    {
        GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
        newObj = Instantiate(newObj, new Vector3(0, 0, playerZpos + (GameplayManager.inst.maxTunnelsAtOnce - 1) * tunnelLength), Quaternion.identity);
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

    /// <summary>
    /// Make the MeshRenderer of a gameObject visible.
    /// </summary>
    /// <param name="obj">GameObject.</param>
    /// <param name="timeToWait">Time to wait in seconds.</param>
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
    /// <param name="fields">The fields to instantiante (make visible). Doesnt have to be a field set.</param>
    /// <param name="spawnDistanceInBeats">The distance from the player in beats, where the first music fields gets instantiated.</param>
    /// <param name="durationInBeats">The time that passes from the first to the last instantiation of a field.</param>
    /// <returns></returns>
    public IEnumerator SpawnMusicFields(MusicField[] fields, float timeToSpawnInBeats, float spawnDistanceInBeats, float durationInBeats)
    {
        // 1. wait
        float waitTime = timeToSpawnInBeats * LoopData.timePerBeat;
        yield return new WaitForSeconds(waitTime);

        // 2. instantiate all, wait between
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
        Player.inst.curFieldSet = fields;
    }

    /// <summary>
    /// Enable line renderer and move from back to front. Set isSpawning = true when done.
    /// </summary>
    private IEnumerator MoveFieldFromBackToFront(MusicField field, float spawnDistanceInBeats, float durationInBeats)
    {
        field.lineRend.enabled = true;

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

        field.isSpawning = false;
        // Player.inst.curField

        // aktiviere variablen
    }
}
