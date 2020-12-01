using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpherePlottingManager))]
public class SphereVisualizer : MonoBehaviour
{
    [Tooltip("Color every n point with the highlight material")] [Range(0, 500)] public int highlight;
    [Tooltip("Offsets which points are highlighted")] [Range(0, 100)] public int highlightOffset;

    [Space]
    [Tooltip("Adds to turnFraction every frame if animateTurn is true")]
    [Range(0.000002f, 0.0002f)] public float turnSpeedPerFrame = 0.00002f;
    public bool animateTurn, animateAddingPoints;

    [Space]
    [Tooltip("Leave null for basic spheres")] [Space] public GameObject pointPrefab;
    public Color mainColor = Color.white, highlightColor = Color.yellow;

    private SpherePlottingManager manager;
    private List<Transform> points = new List<Transform>();
    private Material mainMat, highlightMat;
    private float oldTurnFraction, oldDstBias; // Used for live updates
    private int oldPointCount, oldHighlight, oldHighlightOffset;

    private float lastUpdateTime;

    void Start()
    {
        manager = GetComponent<SpherePlottingManager>();

        mainMat = new Material(Shader.Find("Unlit/Color"));
        mainMat.color = mainColor;
        highlightMat = new Material(Shader.Find("Unlit/Color"));
        highlightMat.color = highlightColor;

        for (int i = 0; i < manager.pointCount; i++) AddPoint3D(i);
        UpdateAllPositions();
        UpdateHighlights();

        foreach (Transform t in points.ToList())
            if (Vector3.Angle(transform.forward, t.position) > manager.viewAngle)
            {
                Destroy(t.gameObject);
                points.Remove(t);
            }

        oldTurnFraction = manager.turnFraction;
        oldDstBias = manager.distanceBias;
        oldPointCount = manager.pointCount;
        oldHighlight = highlight;
        oldHighlightOffset = highlightOffset;
    }

    private void AddPoint3D(int index)
    {
        Transform newPoint;

        if (pointPrefab == null)
        {
            newPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            newPoint.localScale = 0.1f * manager.viewRadius * Vector3.one;
            newPoint.parent = transform;
            Destroy(newPoint.GetComponent<SphereCollider>());
            newPoint.GetComponent<MeshRenderer>().material = mainMat;
        }
        else
        {
            newPoint = Instantiate(
                pointPrefab,
                Vector2.zero,
                Quaternion.identity,
                transform).transform;
        }

        points.Add(newPoint);
    }

    void Update()
    {
        if (animateTurn)
        {
            manager.turnFraction += turnSpeedPerFrame;
            oldTurnFraction = manager.turnFraction;
            UpdateAllPositions();
        }
    }

    void FixedUpdate() // Animates adding points
    {
        if (animateAddingPoints && Time.time - lastUpdateTime >= 1f / 3)
        {
            AddPoint3D(points.Count);
            UpdateAllPositions();
            UpdateHighlights();
            lastUpdateTime = Time.time;
            manager.pointCount++;
        }
    }

    private void UpdateAllPositions()
    {
        for (int i = 0; i < points.Count; i++)
            points[i].position = manager.UpdatePointPosition(i);
    }

    private void UpdateHighlights()
    {
        if (highlight == 0) foreach (Transform t in points)
                t.GetComponent<MeshRenderer>().material = mainMat;
        else
            for (int i = 0; i < points.Count; i++)
            {
                if ((i + highlightOffset) % highlight == 0)
                    points[i].GetComponent<MeshRenderer>().material = highlightMat;
                else
                    points[i].GetComponent<MeshRenderer>().material = mainMat;
            }
    }

    void OnValidate() // Live Updates
    {
        if (Application.isPlaying && Time.time > 0)
        {
            if (manager.turnFraction - oldTurnFraction != 0)
            {
                UpdateAllPositions();
                oldTurnFraction = manager.turnFraction;
            }

            if (manager.distanceBias - oldDstBias != 0)
            {
                UpdateAllPositions();
                oldDstBias = manager.distanceBias;
            }

            if (manager.pointCount - oldPointCount != 0)
            {
                if (manager.pointCount - oldPointCount > 0)
                {
                    for (int i = 0; i < manager.pointCount - oldPointCount; i++)
                        AddPoint3D(points.Count);

                    UpdateAllPositions();
                }
                else if (manager.pointCount - oldPointCount < 0)
                {
                    for (int i = 0; i < oldPointCount - manager.pointCount; i++)
                    {
                        Destroy(points[points.Count - 1].gameObject);
                        points.RemoveAt(points.Count - 1);
                    }

                    UpdateAllPositions();
                }
                oldPointCount = manager.pointCount;
            }

            if (highlight - oldHighlight != 0 || highlightOffset - oldHighlightOffset != 0)
            {
                UpdateHighlights();
                oldHighlight = highlight;
                oldHighlightOffset = highlightOffset;
            }
        }
    }
}