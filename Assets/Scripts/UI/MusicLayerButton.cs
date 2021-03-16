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


    private void Start()
    {
        base.Start();
        filledTransform = transform;
    }




    // ------------------------------ Public functions ------------------------------









    // ------------------------------ Private functions ------------------------------




    private void ScaleImage(float waitBeforeStart, float duration, float targetValue)
    {
        deleteRoutine = StartCoroutine(Scale(waitBeforeStart, duration, targetValue));
    }


    private IEnumerator Scale(float waitBeforeStart, float duration, float targetValue)
    {
        isDeleting = true;

        // 1. Wait
        yield return new WaitForSeconds(waitBeforeStart);

        // 2. 
        float t = 0;
        while (t < duration)
        {
            float lerp = Mathf.Lerp(1, targetValue, t / duration);
            filledTransform.localScale = Vector3.one * lerp;

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
            print("all notes: " + MusicManager.inst.curSequencer.allNotes.Length);
            float wait = UIManager.inst.musicLayerButton_waitBeforDelete;
            float duration = UIManager.inst.musicLayerButton_duration;

            ScaleImage(wait, duration, 0);
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
