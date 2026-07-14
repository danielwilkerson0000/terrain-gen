using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Globe : MonoBehaviour
{
    public LayerMask targetables;

    GameObject selected;
    GameObject dragged;
    bool dragging = false;
    bool selecting = false;

    public int height;
    public int width;
    [HideInInspector]
    public Vector3 origin;
    public Vector3 offset;
    List<Vector3> slots;
    List<Face> faces;
    Dictionary<Face, Tile> tiles;
    Globehedron polyhedron;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (tiles is null)
        {
            InitTiles();
        }
        
        // FOR GIZMOS   
        selected = GameObject.CreatePrimitive(PrimitiveType.Cube);
        selected.SetActive(false);

        dragged = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dragged.SetActive(false);
    }

    /// <summary>
    /// Naive search for the closest face to target point.
    /// </summary>
    public Face GetClosestFace(Vector3 target)
    {
        float dist = float.MaxValue;
        Face closest = null;
        foreach (Face face in faces)
        {
            Vector3 p = face.Pos;
            float m = (p - target).sqrMagnitude;
            if (m < dist)
            {
                dist = m;
                closest = face;
            }
        }

        return closest;
    }

    void InitTiles()
    {
        faces = new();
        tiles = new();

        // creates + places faces in the word
        InitGoldbergTiles();


        // creates + places tiles on all faces
        PlaceTiles(0.2f);
    }


    void PlaceTiles(float chance)
    {
        foreach (Face face in faces)
        {
            // Create a new tile and align it to its parent face
            GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tileObject.transform.position = Vector3.zero;
            tileObject.transform.SetParent(face.transform, false);
            tileObject.transform.localScale = Vector3.one * polyhedron.scale;

            Tile tile = tileObject.AddComponent<Tile>();
            tile.SetOwner(face);
            tiles.Add(face, tile);

            // give a few some color!
            if (Random.value < chance)
            {
                Renderer r = tileObject.GetComponent<Renderer>();
                r.material.color = Random.ColorHSV(0, 1, 0.3f, 0.7f, 0.4f, 0.7f);
            }
        }
    }

    // IGNORE THIS
    void InitRandomSphericalTiles()

    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 p = 10 * UnityEngine.Random.onUnitSphere;
                // if the random point is close to a previous one
                if (slots.Exists(s => (s - p).sqrMagnitude < 1))
                {
                    Debug.Log("discared!");
                    continue;
                }

                slots.Add(p);

                if (UnityEngine.Random.value < 0.2)
                {
                    // tiles.Add(p, new Tile());
                }


            }
        }
    }

    // IGNORE THIS
    void InitSphericalTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float phi = Mathf.PI * i / (width - 1);
                float theta = 2 * Mathf.PI * j / height;
                // if (phi == 0 || theta == 0) continue;
                // Vector3 slot = new Vector3(i, 0, j) + offset - origin;
                Vector3 slot = new Vector3(
                    Mathf.Cos(theta) * Mathf.Sin(phi),
                    Mathf.Cos(phi),
                    Mathf.Sin(theta) * Mathf.Sin(phi)
                );

                if (slots.Contains(slot)) continue;
                
                slots.Add(slot);

                if (UnityEngine.Random.value < 0.2)
                {
                    // tiles.Add(slot, new Tile());
                }


            }
        }
    }

    void InitGoldbergTiles()
    {
        polyhedron = transform.GetComponent<Globehedron>();
        polyhedron.Generate(transform);
        faces = polyhedron.faces;

        foreach (Face face in faces)
        {
            face.transform.SetParent(polyhedron.babysitter.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ManageMouseControls();
        FinalizeMouseControls();
    }

    void ManageMouseControls()
    {
        // Try to drag the closest face to hit.
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            // Perform the physics raycast
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetables))
            {
                dragging = true;
                Face hitFace = hit.collider != null ? hit.collider.GetComponentInParent<Face>() : null;
                if (hitFace == null)
                {
                    hitFace = GetClosestFace(hit.point);
                }

                dragged.transform.position = hitFace.Pos;
            }
        }

        // Try to select the closest face to hit
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            // Perform the physics raycast
            // DANGER: make sure to call this with 4 args (or read)!
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetables))
            {
                selecting = true;
                // Access data about the object that was struck
                Debug.Log("Selected " + hit.transform.name + " " + hit.transform.position + " at point " + hit.point);

                Face hitFace = hit.collider != null ? hit.collider.GetComponentInParent<Face>() : null;
                if (hitFace == null)
                {
                    hitFace = GetClosestFace(hit.point);
                }

                selected.transform.position = hitFace.Pos;
            }
            else
            {
                selecting = false;
            }
        }

        // If we are dragging and selecting the same face, assume we are actually NOT dragging.
        // For safety <3
        if ((selected.transform.position - dragged.transform.position).sqrMagnitude < 0.01f)
        {
            dragging = false;
        }
    }

    // Finailize dragging and switching Tiles
    void FinalizeMouseControls()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // switches selected tile and ending tile
            if (dragging && selecting)
            {
                // Find tile S on face A close to position U
                Vector3 u = selected.transform.position;
                Face a = GetClosestFace(u);
                Tile s = tiles[a];

                // Find tile T on face B close to position V
                Vector3 v = dragged.transform.position;
                Face b = GetClosestFace(v);
                Tile t = tiles[b];

                // Take tiles off
                Vector3 p = s.transform.position;
                Vector3 q = t.transform.position;
                tiles.Remove(a);
                tiles.Remove(b);

                // put tile S onto B at position Q
                tiles.Add(a, t);
                s.transform.position = q;
                s.transform.SetParent(b.transform);
                s.transform.localEulerAngles = Vector3.zero;
                s.SetOwner(b);

                // put tile T onto A at postiion P
                tiles.Add(b, s);
                t.transform.position = p;
                t.transform.SetParent(a.transform);
                t.transform.localEulerAngles = Vector3.zero;
                t.SetOwner(a);

            }
            dragging = false;
            selecting = false;
        }
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && !faces.Any())
        {
            InitTiles();
        }

        Vector3 faceScale = Vector3.one * polyhedron.scale;
        foreach (Face face in faces)
        {
            Matrix4x4 orig = Gizmos.matrix;
            Vector3 pos = face.Pos;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, pos - transform.position);
            Matrix4x4 goal = Matrix4x4.TRS(pos - transform.position, rot, faceScale);
            Gizmos.matrix = goal;

            Gizmos.color = Color.white;
            if (tiles.ContainsKey(face))
            {
                Gizmos.color = tiles[face].GetComponent<Renderer>().material.color;
                Gizmos.DrawSphere(Vector3.zero, 1f);
            }

            Gizmos.matrix = orig;
            // Gizmos.DrawRay(pos, rot * Vector3.forward * 0.4f);
            // Gizmos.DrawRay(pos, rot * Vector3.up * 0.4f);
        }

        if (selecting)
        {
            Gizmos.color = Color.darkBlue;
            Gizmos.DrawWireCube(selected.transform.position, faceScale);
        }

        if (dragging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(dragged.transform.position, faceScale);
        }
    }
}