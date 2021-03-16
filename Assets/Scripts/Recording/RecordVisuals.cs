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
    public void CreateRecordObjectTwice(Recording recording, List<RecordObject>[] recordObjects, int trackLayer)
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

        var recordObject1 = new RecordObject(mesh1, pos1, ID, recording.notes, recording.sequencer, trackLayer, recording.start, recording.end, recording.loopStart, recording.loopEnd_extended);
        var recordObject2 = new RecordObject(mesh2, pos2, ID, recording.notes, recording.sequencer, trackLayer, recording.start, recording.end, recording.loopStart, recording.loopEnd_extended);

        recordObject1.hasRespawned = true;

        // 2. Add to list
        recordObjects[trackLayer].Add(recordObject1);
        recordObjects[trackLayer].Add(recordObject2);

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
        var pos = Recorder.inst.NextLoopPosition(recordObj);
        var layer = 8;

        // 1. Instantiate & scale!
        var newObj = MeshCreation.CreateLaneSurface(fields, ID, "ChordObject", parent, material, true, 1f, layer).gameObject;
        newObj.transform.localScale = recordObj.obj.transform.localScale;

        var douplicateObj = new RecordObject(newObj, pos, ID, recordObj.notes, recordObj.sequencer, recordObj.layer, recordObj.start, recordObj.end, recordObj.loopStart, recordObj.loopEnd_extended);

        // 2. Set data
        douplicateObj.isRecording = false;

        // !!! Add to recordObjects-list when invoked (in ObjectManager) !!!

        return douplicateObj;
    }



    /// <summary>
    /// Stops scaling and sets variables of instantiated recordObjects.
    /// </summary>
    public void StopCreateChordObject(Recording recording)
    {
        StopCoroutine(recording.scaleRoutine);

        recording.obj.isRecording = false;
        recording.loopObj.isRecording = false;
    }


    /// <summary>
    /// Destroy all recordObjects in one layer.
    /// </summary>
    /// <param name="layer"></param>
    public void DestroyRecordObjects(int layer)
    {
        var recordObjects = Recorder.inst.recordObjects[layer];

        for (int i=0; i < recordObjects.Count; i++)
        {
            Destroy(recordObjects[i].obj);
        }
        Recorder.inst.recordObjects[layer] = new List<RecordObject>();
    }




    // ------------------------------ Private functions ------------------------------





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
