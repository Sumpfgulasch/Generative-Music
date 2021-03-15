using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordVisuals : MonoBehaviour
{
    public static RecordVisuals inst;


    void Start()
    {
        inst = this;
    }

    

    void Update()
    {
        
    }



    // ------------------------------ Public functions ------------------------------



    /// <summary>
    /// Instantiate 2 lane surfaces (current chord, looped chord) and keep scaling it by a coroutine upwards from zero until stopped. Writes into recording (!).
    /// </summary>
    /// <param name="recording">Recording data.</param>
    /// <param name="recordObjects">List to add the new chord objects.</param>
    public void CreateRecordObject(Recording recording, List<RecordObject> recordObjects)
    {
        // 0. Refs
        var fields = Player.inst.curFieldSet;
        var ID = recording.fieldID;
        var parent = MeshRef.inst.recordObj_parent;
        var material = MeshRef.inst.recordObj_mat;
        var pos1 = Player.inst.transform.position;
        var pos2 = pos1 + LoopData.distancePerRecLoop * Vector3.forward;
        var layer = 8;

        // 1. Instantiate
        var mesh1 = MeshCreation.CreateLaneSurface(fields, ID, "ChordObject", parent, material, true, 1f, layer).gameObject;
        var mesh2 = MeshCreation.CreateLaneSurface(fields, ID, "ChordObject", parent, material, true, 1f, layer).gameObject;

        var recordObject1 = new RecordObject(mesh1, pos1, ID, recording.notes, recording.sequencer, recording.start, recording.end, recording.loopStart, recording.loopEnd_extended);
        var recordObject2 = new RecordObject(mesh2, pos2, ID, recording.notes, recording.sequencer, recording.start, recording.end, recording.loopStart, recording.loopEnd_extended);

        // 2. Add to list
        recordObjects.Add(recordObject1);
        recordObjects.Add(recordObject2);

        // 3. Edit recording / Start scaling
        recording.obj = recordObject1;              // Noch nötig???
        recording.loopObj = recordObject2;          // Noch nötig???
        recording.scaleRoutine = StartCoroutine(ScaleChordObject(recordObject1.obj.transform, recordObject2.obj.transform));
    }

    public RecordObject DouplicateRecordObject(RecordObject recordObj)
    {
        // 0. Refs
        var fields = Player.inst.curFieldSet;
        var ID = recordObj.fieldID;
        var parent = MeshRef.inst.recordObj_parent;
        var material = MeshRef.inst.recordObj_mat;
        var pos = Recorder.inst.NextLoopPosition(recordObj);            // to rework!!!
        var layer = 8;

        // 1. Instantiate & scale!
        var newObj = MeshCreation.CreateLaneSurface(fields, ID, "ChordObject", parent, material, true, 1f, layer).gameObject;
        newObj.transform.localScale = recordObj.obj.transform.localScale;

        var recordObject = new RecordObject(newObj, pos, ID, recordObj.notes, recordObj.sequencer, recordObj.start, recordObj.end, recordObj.loopStart, recordObj.loopEnd_extended);

        // 2. Set data
        recordObj.isRecording = false;

        // !!! Add to recordObjects-list when invoked (in ObjectManager) !!!

        return recordObj;
    }



    /// <summary>
    /// Stops scaling and sets variables of instantiated recordObjects.
    /// </summary>
    public void StopCreateChordObject(Recording recording)
    {
        StopCoroutine(recording.scaleRoutine);

        recording.obj.isRecording = false;
        recording.loopObj.isRecording = false;

        Debug.Log("stop create 1", gameObject);
        Debug.Log("stop create 2", gameObject);
    }


    public void DestroyRecordObjects()
    {
        var recordObjects = Recorder.inst.recordObjects;

        for (int i=0; i < recordObjects.Count; i++)
        {
            Destroy(recordObjects[i].obj);
        }
        Recorder.inst.recordObjects = new List<RecordObject>();
    }




    // ------------------------------ Private functions ------------------------------




    //private RecordObject InstantiateRecObj(MusicField[] fields, int index, string name, Vector3 position, Transform parent, Material material, bool visible = true, float length = 1f, int layer = 1)
    //{
    //    var laneSurface1 = MeshCreation.CreateLaneSurface(fields, index, name, parent, material, visible, length, layer);
    //    var recordObject = laneSurface1.gameObject.AddComponent<RecordObject>();
    //    recordObject.transform.position = position;

    //    return recordObject;
    //}


    /// <summary>
    /// Increase the scale of the currently recorded object and its already instantiated loop object.
    /// </summary>
    /// <param name="obj1">Currently played chord-object.</param>
    /// <param name="obj2">Loop-chord-object.</param>
    /// <returns></returns>
    private IEnumerator ScaleChordObject(Transform obj1, Transform obj2)
    {
        var playerPos = Player.inst.transform.position.z;
        while (true)
        {
            var scale = playerPos - obj1.position.z;
            obj1.localScale = new Vector3(1, 1, scale);
            obj2.localScale = new Vector3(1, 1, scale);

            yield return null;
        }
    }


    
}
