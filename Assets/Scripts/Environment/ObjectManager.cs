using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    // public
    [Header("Objects")]
    public static ObjectManager inst;
    public List<GameObject> availableObjects;

    [HideInInspector] public float moveSpeed;

    // private
    private List<GameObject> movingObjects;
    private float playerZpos;
    [HideInInspector] public float tunnelLength;
    [HideInInspector] public float distancePerQuarter;
    private float FPS;

    // Properties
    private float DeltaTime { get { return Time.deltaTime * FPS; }
    }


    private void Awake()
    {
        inst = this;
    }

    void Start()
    {
        FPS = Screen.currentResolution.refreshRate;

        GetData();
    }




    private void Update()
    {
        ManageRecordedChords();
    }




    // ------------------------- Public functions -------------------------




    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeToSpawnInBeats">Time in beats that passes until the first tunnel gets instantiated.</param>
    /// <param name="spawnDistanceInBeats"></param>
    public IEnumerator SpawnFirstTunnels(int tunnels, float timeToSpawnInBeats, float spawnDistanceInBeats)
    {
        // 1. wait
        float waitTime = timeToSpawnInBeats * LoopData.timePerBeat;
        yield return new WaitForSeconds(waitTime);

        // 2. EVENT SUBSCRIPTION
        GameEvents.inst.onFirstBeat += OnFirstBeat;

        // 3. Spawn
        for (int i = 0; i < tunnels; i++)
        {
            var newObj = availableObjects[Random.Range(0, availableObjects.Count)];
            var zSpawn = playerZpos + distancePerQuarter * spawnDistanceInBeats;
            var pos = new Vector3(0, 0, zSpawn + i * tunnelLength);

            newObj = Instantiate(newObj, pos, Quaternion.identity);

            movingObjects.Add(newObj);
        }
        yield return null;
    }


    /// <summary>
    /// Spawn each field displaced and move from back to front (lineRend and fieldSurface). Replace old fields when done.
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
        float duration = durationInBeats * LoopData.timePerBeat;
        float timeToWait = duration / (fields.Length - 1);

        for (int i = 0; i < fields.Length; i++)
        {
            StartCoroutine(MoveFieldFromBackToFront(fields[i], spawnDistanceInBeats, durationInBeats));

            // milk surface
            if (i == fields.Length - 1)
            {
                var time = spawnDistanceInBeats * LoopData.timePerBeat;
                var obj = MeshRef.inst.innerSurface_mf.gameObject;

                StartCoroutine(SetObjectVisible(obj, true, time));
            }
            yield return new WaitForSeconds(timeToWait);
        }
        Player.inst.curFieldSet = fields;
    }


    // ------------------------------ Events ------------------------------



    public void OnFirstBeat()
    {
        SpawnTunnel();
        DeleteFarObjects();
    }



    // ------------------------- Private functions -------------------------


    private void SpawnTunnel()
    {
        GameObject newObj = availableObjects[Random.Range(0, availableObjects.Count)];
        newObj = Instantiate(newObj, new Vector3(0, 0, playerZpos + (GameplayManager.inst.maxTunnelsAtOnce - 1) * tunnelLength), Quaternion.identity);
        movingObjects.Add(newObj);
    }


    /// <summary>
    /// Instantiate and delete objects, for each recorded chord.
    /// </summary>
    private void ManageRecordedChords()
    {
        var removeList = new List<RecordObject>();
        var addList = new List<RecordObject>();
        var chordRecData = Recorder.inst.recording;

        foreach(RecordObject obj in Recorder.inst.recordObjects)
        {
            // Don't do if recording
            if (!obj.isRecording)
            {
                // Destroy?
                if (obj.EndZPos < -2f)
                {
                    removeList.Add(obj);
                }

                // Instantiate?
                if (obj.StartZPos <= 0 && !obj.hasRespawned)
                {
                    obj.hasRespawned = true;
                    var newObj = RecordVisuals.inst.DouplicateRecordObject(obj);

                    addList.Add(newObj);
                }
            }
        }

        // remove
        foreach(RecordObject obj in removeList)
        {
            Recorder.inst.recordObjects.Remove(obj);
            Destroy(obj.gameObject);
        }

        // add
        foreach(RecordObject obj in addList)
        {
            Recorder.inst.recordObjects.Add(obj);
        }


    }



    /// <summary>
    /// Distance-related data to beats.
    /// </summary>
    private void GetData()
    {
        Vector3 start = availableObjects[0].GetComponent<MoveTunnel>().startPos.transform.position;
        Vector3 end = availableObjects[0].GetComponent<MoveTunnel>().endPos.transform.position;
        tunnelLength = (start - end).magnitude;

        int FPS = Screen.currentResolution.refreshRate;
        moveSpeed = (tunnelLength / LoopData.timePerBar) / FPS;

        distancePerQuarter = tunnelLength / LoopData.quartersPerBar;

        playerZpos = Player.inst.transform.position.z;

        movingObjects = new List<GameObject>();
    }

    


    /// <summary>
    /// Make the MeshRenderer of a gameObject visible.
    /// </summary>
    /// <param name="obj">GameObject.</param>
    /// <param name="timeToWait">Time to wait in seconds.</param>
    private IEnumerator SetObjectVisible(GameObject obj, bool value, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        obj.GetComponent<MeshRenderer>().enabled = value;
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
    /// Move lineRenderer from back to front. Fold out fieldSurface when done. Set musicField-variables (e.g. isSpawning = false).
    /// </summary>
    private IEnumerator MoveFieldFromBackToFront(MusicField field, float spawnDistanceInBeats, float durationInBeats)
    {
        // 0. Start
        field.lineRend.enabled = true;
        
        float duration = spawnDistanceInBeats * LoopData.timePerBeat;

        float timer = 0;
        float zPos = playerZpos + distancePerQuarter * spawnDistanceInBeats;

        // 1. Move lineRend to front
        while (timer < duration)
        {
            zPos -= moveSpeed * DeltaTime;
            field.SetLineRendZPos(zPos);

            timer += Time.deltaTime;
            yield return null;
        }

        // 2. "Fold" out fieldSurface
        field.fieldSurface.enabled = true;

        timer = 0;
        duration = VisualController.inst.fieldFoldOutTime;

        while (timer < duration)
        {
            // scale up fieldSurface & move lineRend to front
            float remappedTimer = timer / duration; // remapped to 0-1
            float curveValue = VisualController.inst.fieldFoldOutCurve.Evaluate(remappedTimer); // between 0-1
            float curScale = curveValue.Remap(0, 1f, 0, field.height); // remapped to final

            field.fieldSurface.transform.localScale = new Vector3(1, 1, curScale);
            field.SetLineRendZPos(Player.inst.transform.position.z - curScale);
            timer += Time.deltaTime;
            yield return null;
        }

        field.fieldSurface.transform.localScale = new Vector3(1, 1, field.height);
        field.SetLineRendZPos(Player.inst.transform.position.z - field.height);
        field.isSpawning = false;

        // aktiviere variablen
    }
}
