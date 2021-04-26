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
    /// Scale the button down and clear everything when done.
    /// </summary>
    private IEnumerator ScaleAndClear(float waitBeforeStart, float duration, float targetValue)
    {
        isDeleting = true;

        // 1. Wait
        yield return new WaitForSeconds(waitBeforeStart);

        // 2. 
        float t = 0;
        while (t < duration)
        {
            // scale
            float lerp = t / duration;
            float scaleLerp = Mathf.Lerp(1, targetValue, lerp);
            filledTransform.localScale = Vector3.one * scaleLerp;

            // color
            var recordObjects = Recorder.inst.recordObjects[layer];
            var targetColor = recordObjects[0].meshRenderer.material.color;
            targetColor.a = 0;
            foreach(RecordObject recordObject in recordObjects)
            {
                var colorLerp = UIManager.inst.deleteLerp.Evaluate(lerp);
                recordObject.meshRenderer.material.color = Color.Lerp(targetColor, recordObject.startColor, colorLerp);
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
            foreach(RecordObject recordObject in Recorder.inst.recordObjects[layer])
            {
                recordObject.meshRenderer.material.color = recordObject.startColor;
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
