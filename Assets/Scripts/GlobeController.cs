using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// THIS WAS LARGELY GENERATED WITH THE HELP OF GOOGLE GEMINI - MANY THANKS.
/// 
/// This manages the input given on a Globe.
/// Currently, this is only Tile movement.
/// 
/// </summary>
public class GlobeController : MonoBehaviour
{
    public Globe globe;
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

    void InitWidgets()
    {
        float scale = globe.GetFaceScale() * 1.3f;

        selected = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        selected.name = "selected";
        selected.transform.localScale = scale * Vector3.one;
        selected.GetComponent<Renderer>().material.color = Color.darkBlue;
        // selected.SetActive(false);

        dragged = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dragged.name = "dragged";
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
        if (globe == null) return;

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 p = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(p);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectionMask))
            {
                if (hit.collider == null) return;

                selecting = Mouse.current.leftButton.wasPressedThisFrame;
                dragging = true;
                selected.SetActive(selecting || dragging);
                dragged.SetActive(dragging);

                Face face = globe.GetClosestFace(hit.point);
                dragged.transform.position = face.Pos;
                if (selecting)
                    selected.transform.position = face.Pos;
            }
        }
    }

    void FinalizeMouseControls()
    {
        if ((selected.transform.position - dragged.transform.position).sqrMagnitude < 0.01f)
        {
            dragging = false;
        }

        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Face f = globe.GetClosestFace(selected.transform.position);
            Face g = globe.GetClosestFace(dragged.transform.position);
            globe.SwapTiles(f, g);
        }

        if (!Mouse.current.leftButton.isPressed)
        {
            selecting = false;
            dragging = false;
            selected.SetActive(false);
            dragged.SetActive(false);
        }
    }
}
