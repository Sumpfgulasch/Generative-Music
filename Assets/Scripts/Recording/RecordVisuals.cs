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
    ///     /// Instantiate 2 lane surfaces (current chord, looped chord) and keep scaling it by a coroutine upwards from zero until stopped. Writes into chordRecord (!).
    /// </summary>
    /// <param name="chordRecData">Recording data.</param>
    /// <param name="recordObjects">List to add the new chord objects.</param>
    public void CreateRecordObject(RecordData chordRecData, List<RecordObject> recordObjects)
    {
        // 0. Refs
        var fields = Player.inst.curFieldSet;
        var index = chordRecData.fieldID;
        var parent = MeshRef.inst.recordObj_parent;
        var material = MeshRef.inst.recordObj_mat;
        var pos1 = Player.inst.transform.position;
        var pos2 = pos1 + LoopData.distancePerRecLoop * Vector3.forward;

        // 1. Instantiate
        var recordObject1 = InstantiateRecObj(fields, index, "ChordObject", pos1, parent, material, true, 1f, 8);
        var recordObject2 = InstantiateRecObj(fields, index, "ChordObject", pos2, parent, material, true, 1f, 8);

        // 2. Set data
        recordObjects.Add(recordObject1);
        recordObjects.Add(recordObject2);
        chordRecData.obj = recordObject1;
        chordRecData.loopObj = recordObject2;

        recordObject1.data = new RecordData();
        recordObject2.data = new RecordData();
        recordObject1.data.fieldID = index; // HACK
        recordObject2.data.fieldID = index; // HACK

        // 3. Start scaling
        chordRecData.scaleRoutine = StartCoroutine(ScaleChordObject(recordObject1.transform, recordObject2.transform));
    }

    public RecordObject DouplicateRecordObject(RecordObject recordObj)
    {
        // 0. Refs
        var fields = Player.inst.curFieldSet;
        var index = recordObj.data.fieldID;
        var parent = MeshRef.inst.recordObj_parent;
        var material = MeshRef.inst.recordObj_mat;
        var pos = Recorder.inst.NextLoopPosition();

        // 1. Instantiate
        var newObj = InstantiateRecObj(fields, index, "ChordObject", pos, parent, material, true, 1f, 8);
        // 1.5. Scale!
        newObj.transform.localScale = recordObj.transform.localScale;

        // 2. Set data
        //recordObjects.Add(recordObject1);
        //chordRecData.loopObj = recordObject;
        newObj.data = new RecordData();
        newObj.data.fieldID = index;
        newObj.isRecording = false;

        return newObj;
    }



    /// <summary>
    /// Stops scaling and sets variables of instantiated recordObjects.
    /// </summary>
    public void StopCreateChordObject(RecordData chordRecordData)
    {
        StopCoroutine(chordRecordData.scaleRoutine);

        chordRecordData.obj.isRecording = false;
        chordRecordData.loopObj.isRecording = false;
    }




    // ------------------------------ Private functions ------------------------------




    private RecordObject InstantiateRecObj(MusicField[] fields, int index, string name, Vector3 position, Transform parent, Material material, bool visible = true, float length = 1f, int layer = 1)
    {
        var laneSurface1 = MeshCreation.CreateLaneSurface(fields, index, name, parent, material, visible, length, layer);
        var recordObject = laneSurface1.gameObject.AddComponent<RecordObject>();
        recordObject.transform.position = position;

        return recordObject;
    }


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
