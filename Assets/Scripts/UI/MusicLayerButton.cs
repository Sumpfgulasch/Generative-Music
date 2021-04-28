using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MusicLayerButton : Button, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Added")]
    public int layer;
    //public Image targetImage;
    public Image unfilled;
    public Image filled;
    public Image glow;
    public TextMeshProUGUI recordText;
    public Image recordImage;

    private Transform filledTransform;
    private Coroutine deleteRoutine;
    private bool isDeleting;


    protected override void Start()
    {
        base.Start();
        filledTransform = transform; // eig nur von filled-image
    }




    // ------------------------------ Public functions ------------------------------









    // ------------------------------ Private functions ------------------------------




    /// <summary>
    /// Scale the button down and clear everything when done.
    /// </summary>
    private void ShrinkAndClear(float waitBeforeStart, float duration, float targetValue)
    {
        deleteRoutine = StartCoroutine(ScaleAndClear(waitBeforeStart, duration, targetValue));
    }


    /// <summary>
    /// Scale the button down, make objects transparent over time and clear everything when done.
    /// </summary>
    private IEnumerator ScaleAndClear(float waitBeforeStart, float duration, float targetValue)
    {
        // Wait
        yield return new WaitForSeconds(waitBeforeStart);

        // Data
        isDeleting = true;
        bool hasExactlyOneLayer = Recorder.inst.HasExactlyOneLayer;
        float t = 0;
        var chordObjects = Recorder.inst.chordObjects[layer];
        var targetChordColor = chordObjects[0].meshRenderer.material.color;
        targetChordColor.a = 0;
        var targetLoopColor = Recorder.inst.loopObjects[0].meshRenderer.material.color;
        targetLoopColor.a = 0;

        while (t < duration)
        {
            // 1. Scale button
            float lerp = t / duration;
            float colorLerp = UIManager.inst.deleteLerp.Evaluate(lerp);
            float scaleLerp = Mathf.Lerp(1, targetValue, lerp);
            filledTransform.localScale = Vector3.one * scaleLerp;

            // 2. Make chordObjects transparent
            foreach(ChordObject chordObject in chordObjects)
            {
                chordObject.meshRenderer.material.color = Color.Lerp(targetChordColor, chordObject.startColor, colorLerp);
            }

            // 3. LoopObjects transparent?
            if (hasExactlyOneLayer)
            {
                foreach(LoopObject loopObject in Recorder.inst.loopObjects)
                {
                    loopObject.meshRenderer.material.color = Color.Lerp(targetLoopColor, loopObject.startColor, colorLerp);
                    //loopObject.meshRenderer.material.SetColor("_EmissionColor", Color.Lerp(targetLoopColor*0, loopObject.startColor, colorLerp));
                }
            }

            t += Time.deltaTime;

            yield return null;
        }

        filledTransform.localScale = Vector3.one;

        // 3. clear
        Recorder.inst.ClearLayer(layer);

        isDeleting = false;

    }






    // ---------------------------------- Events ----------------------------------



    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // 1. Change layer
        UIOps.inst.ChangeLayer(layer);

        // 2. Start delete-routine
        if (MusicManager.inst.curSequencer.GetAllNotes().Count != 0)
        {
            float wait = UIManager.inst.musicLayerButton_waitBeforDelete;
            float duration = UIManager.inst.musicLayerButton_duration;

            ShrinkAndClear(wait, duration, 0);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (isDeleting)
        {
            StopCoroutine(deleteRoutine);
            filledTransform.localScale = Vector3.one;
            isDeleting = false;

            // Reset colors of looping objects
            foreach(ChordObject chordObject in Recorder.inst.chordObjects[layer])
            {
                chordObject.meshRenderer.material.color = chordObject.startColor;
            }
            foreach(LoopObject loopObject in Recorder.inst.loopObjects)
            {
                loopObject.meshRenderer.material.color = loopObject.startColor;
            }
        }
    }


    public void EnableRecordLabel(bool enable)
    {
        recordText.enabled = enable;
        recordImage.enabled = enable;

        if (enable)
        {
            glow.color = MeshRef.inst.recordColor;
        }
        else
        {
            glow.color = Color.white;
        }
    }
}
