using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

/// <summary>
/// THIS WAS LARGELY GENERATED WITH THE HELP OF GOOGLE GEMINI - MANY THANKS.
/// 
/// This manages the input given on a Globe.
/// Currently, this is only Tile movement.
/// 
/// </summary>
public class GlobeController : MonoBehaviour
{
    Globe globe;
    public LayerMask selectionMask;

    GameObject selected;
    GameObject dragged;
    bool dragging = false;
    bool selecting = false;

    public void Initialize(Globe globe)
    {
        this.globe = globe;
        InitWidgets();
    }

    void Awake()
    {
    }

    void InitWidgets()
    {
        float scale = globe.GetFaceScale() * 1.1f;
        selected = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        selected.transform.localScale = scale * Vector3.one;
        selected.GetComponent<Renderer>().material.color = Color.darkBlue;
        // selected.SetActive(false);

        dragged = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dragged.transform.localScale = scale * Vector3.one;
        dragged.GetComponent<Renderer>().material.color = Color.red;
        // dragged.SetActive(false);
    }

    void Update()
    {
        ManageMouseControls();
        FinalizeMouseControls();
    }

    void ManageMouseControls()
    {
        if (globe == null)
        {
            return;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectionMask))
            {
                dragging = true;
                Face hitFace = hit.collider != null ? hit.collider.GetComponentInParent<Face>() : null;
                if (hitFace == null)
                {
                    hitFace = globe.GetClosestFace(hit.point);
                }

                dragged.transform.position = hitFace.Pos;
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectionMask))
            {
                selecting = true;
                Face face = hit.collider != null ? hit.collider.GetComponentInParent<Face>() : null;
                if (face == null)
                {
                    face = globe.GetClosestFace(hit.point);
                }

                selected.transform.position = face.Pos;
                Debug.Log($"Selected {face} at {hit.point}");
            }
            else
            {
                selecting = false;
            }
        }

        if ((selected.transform.position - dragged.transform.position).sqrMagnitude < 0.01f)
        {
            dragging = false;
        }
    }

    void FinalizeMouseControls()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (dragging && selecting)
            {
                Face f = globe.GetClosestFace(selected.transform.position);
                Face g = globe.GetClosestFace(dragged.transform.position);
                globe.SwapTiles(f, g);
            }

            dragging = false;
            selecting = false;
        }
    }
}
