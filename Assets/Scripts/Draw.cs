using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour {

    // steam vr controller shenanigans
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device Controller
    {
        get
        {
            return SteamVR_Controller.Input((int)trackedObject.index);
        }
    }
    private Vector3 controllerPosition;

    private GameObject UIOverlayObject;
    private SpriteRenderer UIOverlay;

    private GameObject Cursor;

    private Color currentColor = Color.blue;
    private float currentStroke = (float)0.02;
    private bool StrokePicked = false;

    // keep a list of line renderers for each time you pull the trigger
    private List<GameObject> lineSegments;
    private LineRenderer currentLineRenderer;
    private int lineRendererIndex = 0;

    private bool drawing = false;

    void CreateNewLineSegment()
    {
        lineSegments.Add(new GameObject());
        currentLineRenderer = lineSegments[lineSegments.Count - 1].AddComponent<LineRenderer>();
        currentLineRenderer.positionCount = 0;
        currentLineRenderer.startWidth = currentStroke;
        currentLineRenderer.endWidth = currentStroke;
        currentLineRenderer.startColor = currentColor;
        currentLineRenderer.endColor = currentColor;
        currentLineRenderer.material = new Material(Shader.Find("Particles/Alpha Blended"));
        lineRendererIndex = 0;
    }

    void HandleUndo()
    {
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            if (lineSegments.Count > 0)
            {
                Destroy(lineSegments[lineSegments.Count - 1].GetComponent<LineRenderer>());
                Destroy(lineSegments[lineSegments.Count - 1]);
                lineSegments.RemoveAt(lineSegments.Count - 1);
            }
        }
    }

    void HandleReset()
    {
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            Debug.Log("pushed the destroy button...");
            while (lineSegments.Count != 0)
            {
                Destroy(lineSegments[0].GetComponent<LineRenderer>());
                Destroy(lineSegments[0]);
                lineSegments.RemoveAt(0);
            }

        }
    }

    void HandleDrawing()
    {
        if (Controller.GetHairTriggerDown() && drawing == false)
        {
            CreateNewLineSegment();
            drawing = true;
        }
        else if (Controller.GetHairTriggerUp())
        {
            currentLineRenderer.positionCount = lineRendererIndex - 1;
            drawing = false;
        }

        if (drawing)
        {
            currentLineRenderer.positionCount = lineRendererIndex + 1;
            currentLineRenderer.SetPosition(lineRendererIndex, controllerPosition);
            lineRendererIndex++;
        }
    }

    void DrawUIOverlay()
    {
        UIOverlay.transform.position = controllerPosition;
        UIOverlay.transform.rotation = Controller.transform.rot;
        UIOverlay.transform.Rotate(90, 0, 0);
        UIOverlay.transform.Translate(0, (float)-0.055, (float)-0.01);

    }

    void HandleColorPicker()
    {
        if (Controller.GetAxis() != Vector2.zero)
        {
            if (Mathf.Abs(Controller.GetAxis().y) < 0.33) {
                currentColor = Color.HSVToRGB(Controller.GetAxis().x, 1, 1);
            }
        }
    }

    void HandleStrokePicker()
    {
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && StrokePicked == false)
        {
            if (Controller.GetAxis().y > 0.6)
            {
                currentStroke += (float)0.01;
            }
            else if (Controller.GetAxis().y < 0.4)
            {
                currentStroke -= (float)0.01;
            }
        }
    }

    void UpdateCursor()
    {
        Cursor.transform.position = controllerPosition;
        //Cursor.transform.Translate(0, (float)0.0, (float)0.0);
        Cursor.transform.localScale = new Vector3(currentStroke, currentStroke, currentStroke);
        Cursor.GetComponent<Renderer>().material.color = currentColor;
    }

    void Awake () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        lineSegments = new List<GameObject>();

        UIOverlayObject = new GameObject();
        UIOverlay = UIOverlayObject.AddComponent<SpriteRenderer>();
        UIOverlay.sprite = Resources.Load<Sprite>("Sprites/ControllerMap");

        Cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
	}

    void Update()
    {
        controllerPosition = trackedObject.transform.position;

        UpdateCursor();
        DrawUIOverlay();
        HandleDrawing();
        if (!drawing)
        {
            HandleUndo();
            HandleReset();
            HandleColorPicker();
            HandleStrokePicker();
        }
    }
}
