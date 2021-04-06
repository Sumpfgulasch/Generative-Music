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




    private void ScaleAndDelete(float waitBeforeStart, float duration, float targetValue)
    {
        deleteRoutine = StartCoroutine(ScaleAndClear(waitBeforeStart, duration, targetValue));
    }


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

        // 1. Glow-image
        EnableGlow();

        // 2. MusicManager
        MusicManager.inst.ChangeLayer(layer);

        // 3. Delete?
        if (MusicManager.inst.curSequencer.GetAllNotes().Count != 0)
        {
            float wait = UIManager.inst.musicLayerButton_waitBeforDelete;
            float duration = UIManager.inst.musicLayerButton_duration;

            ScaleAndDelete(wait, duration, 0);
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

    

    /// <summary>
    /// Disable old and enable new glow-image.
    /// </summary>
    private void EnableGlow()
    {
        // 1. Disable old & enable new graphics
        UIManager.inst.activeLayerButton.glow.enabled = false;
        glow.enabled = true;

        // 2. Set active-variable
        UIManager.inst.activeLayerButton = this;
    }


    private void EnableImage(Image image, bool value)
    {
        image.enabled = value;
    }
}
