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
    /// Instantiate 2 lane surfaces (current chord, looped chord) from recording-data and keep scaling it by a coroutine upwards from zero until stopped. Writes into recording (!).
    /// </summary>
    /// <param name="recording">Recording data.</param>
    /// <param name="chordObjects">List to add the new chord objects.</param>
    public void CreateChordObjectTwice(Recording recording, List<ChordObject>[] chordObjects, int trackLayer)
    {
        // 0. Refs
        var fields = Player.inst.curFieldSet;
        var ID = recording.fieldID;
        var parent = MeshRef.inst.recordObj_parent;
        var material = MeshRef.inst.recordObjs_mat[trackLayer];
        var pos1 = Player.inst.transform.position;
        var pos2 = Recorder.inst.NextLoopPosition(recording.sequencer, recording.start, recording.loopStart);
        var layer = 8;
        var collider = true;
        var visible = true;
        var tag = MeshRef.inst.chordObjTag;

        // 1. Instantiate
        var obj1 = MeshCreation.CreateLaneSurface(fields, ID, "ChordObject", parent, material, visible, collider, tag, 1f, layer).gameObject;
        var obj2 = MeshCreation.CreateLaneSurface(fields, ID, "ChordObject", parent, material, visible, collider, tag, 1f, layer).gameObject;

        var recordObject2 = ChordObject.Create(obj2, null, pos2, ID, recording.notes, recording.sequencer, trackLayer, recording.start, recording.end, recording.loopStart, recording.loopEnd_extended);
        var recordObject1 = ChordObject.Create(obj1, recordObject2, pos1, ID, recording.notes, recording.sequencer, trackLayer, recording.start, recording.end, recording.loopStart, recording.loopEnd_extended);

        recordObject1.hasRespawned = true;

        // 2. Add to list
        chordObjects[trackLayer].Add(recordObject1);
        chordObjects[trackLayer].Add(recordObject2);

        // 3. Edit recording / Start scaling
        recording.obj = recordObject1;              // Nur für scaling-coroutine
        recording.loopObj = recordObject2;          // Nur für scaling-coroutine
        recording.scaleRoutine = StartCoroutine(ScaleChordObject(recordObject1, recordObject2));
    }

    public ChordObject DouplicateChordObject(ChordObject chordObj)
    {
        // 0. Refs
        var fields = Player.inst.curFieldSet;
        var ID = chordObj.fieldID;
        var parent = MeshRef.inst.recordObj_parent;
        var material = MeshRef.inst.recordObjs_mat[chordObj.trackLayer];
        var pos = Recorder.inst.NextLoopPosition(chordObj.sequencer, chordObj.start, chordObj.loopStart);
        var layer = 8;
        var visible = true;
        var collider = true;
        var tag = MeshRef.inst.chordObjTag;

        // 1. Instantiate & scale!
        var newObj = MeshCreation.CreateLaneSurface(fields, ID, "ChordObject", parent, material, visible, collider, tag, 1f, layer).gameObject;
        newObj.transform.localScale = chordObj.obj.transform.localScale;

        var douplicateObj = ChordObject.Create(newObj, null, pos, ID, chordObj.notes, chordObj.sequencer, chordObj.trackLayer, chordObj.start, chordObj.end, chordObj.loopStart, chordObj.loopEnd_extended);
        
        // 2. Set data
        chordObj.douplicate = douplicateObj;   // obj that was douplicated

        // !!! in ObjectManager: Add to recordObjects-list when invoked !!!

        return douplicateObj;
    }



    /// <summary>
    /// Stops scaling and sets variables of instantiated chordObjects.
    /// </summary>
    public void StopCreateChordObject(Recording recording)
    {
        StopCoroutine(recording.scaleRoutine);

        recording.obj.end = recording.end;
        recording.loopObj.end = recording.end;

        recording.loopObj.UpdateScale();
    }


    /// <summary>
    /// Destroy all chordObjects in one layer.
    /// </summary>
    /// <param name="layer"></param>
    public void DestroyAllChordObjects(int layer)
    {
        var recordObjects = Recorder.inst.recordObjects[layer];

        for (int i=0; i < recordObjects.Count; i++)
        {
            Destroy(recordObjects[i].obj);
        }
        Recorder.inst.recordObjects[layer] = new List<ChordObject>();
    }

    /// <summary>
    /// Destroy a single chordObject (and its douplicate).
    /// </summary>
    /// <param name="obj"></param>
    public void DestroyChordObject(ChordObject obj)
    {
        Destroy(obj.obj);
        Recorder.inst.recordObjects[obj.trackLayer].Remove(obj);

        if (obj.douplicate != null)
        {
            Destroy(obj.douplicate.obj);
            Recorder.inst.recordObjects[obj.trackLayer].Remove(obj.douplicate);
        }
    }




    // ------------------------------ Private functions ------------------------------





    /// <summary>
    /// Increase the scale of the currently recorded object and its already instantiated loop object.
    /// </summary>
    /// <param name="obj1">Currently played chord-object.</param>
    /// <param name="obj2">Loop-chord-object.</param>
    /// <returns></returns>
    private IEnumerator ScaleChordObject(ChordObject obj1, ChordObject obj2)
    {
        var playerPos = Player.inst.transform.position.z;
        while (true)
        {
            var scale = playerPos - obj1.obj.transform.position.z;
            obj1.obj.transform.localScale = new Vector3(1, 1, scale);
            obj2.obj.transform.localScale = new Vector3(1, 1, scale);

            yield return null;
        }
    }


    
}
