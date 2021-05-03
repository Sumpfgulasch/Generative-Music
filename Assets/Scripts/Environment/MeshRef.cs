using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MeshRef : MonoBehaviour
{
    public static MeshRef inst;

    [Header("Masks & meshes")]
    public MeshFilter innerPlayerMesh_mf;
    public MeshFilter innerPlayerMask_mf;
    public MeshFilter innerSurface_mf;
    public MeshFilter innerMask_mf;
    public MeshFilter outerPlayerMesh_mf;
    public MeshFilter outerPlayerMask_mf;
    [Space]
    public Transform milkSurface_parent;
    public LineRenderer tunnelEdges_lr;

    [Header("Fields & lane surfaces")]
    public Transform playerField_parent;
    public Material playerField_mat;
    public Material playerFieldSec_mat;
    public Transform musicFields_parent;
    public Material musicFields_mat;
    [Space]
    public Transform highlightSurfaces_parent;
    public Material highlightSurfaces_mat;
    public string highlightSurfaces_layer = "2nd cam";
    public int highlightSurfaces_renderQueue = 3003;
    public string fieldSurfaces_layer = "2nd cam";
    public int fieldSurfaces_renderQueue = 3001;
    public Transform fieldSurfaces_parent;
    public Material fieldSurfaces_mat;
    [Header("Record")]
    public string chordObjTag = "recordObject";
    public string recordLayer = "Default";
    //public int recordRenderQueue = 
    public GameObject loopObject;
    public TextMeshProUGUI recordText;
    public Image recordImage;
    public Color recordColor;
    public Transform recordObj_parent;
    public Material[] recordObjs_mat;
    public TextMeshProUGUI preRecordCounter;
    [Header("UI")]
    public GameObject additionalSettings;
    public List<MusicLayerButton> layerButtons;
    public TextMeshProUGUI quantizePrecision;
    public Color inactiveColor;
    public AnimationCurve deleteLayerCurve;
    public AnimationCurve deleteChordCurve;
    [Header("Diverse")]
    public PolygonCollider2D mouseColllider;
    public Transform mouseTrail;
    public Texture2D mouse;


    // Start is called before the first frame update
    void Start()
    {
        inst = this;
        //
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
