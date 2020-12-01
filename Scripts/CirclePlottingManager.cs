using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePlottingManager : MonoBehaviour
{
    [Range(10, 1000)] public int pointCount = 10;
    [Range(2, 500)] public int highlight = 2;
    [Range(0, 100)] public int highlightOffset = 2;
    public float turnFraction, circleRadius = 75, distanceAjuster = 0.5f;

    [Space] [Range(0.00001f, 0.0001f)] public float turnSpeedPerFrame = 0.00002f;
    public bool animateTurn;

    [Space] public Color highlightColor = Color.green;
    public GameObject pointPrefab;

    private List<Transform> points = new List<Transform>();
    private float oldTurnFraction, oldDstAjust;
    private int oldPointCount, oldHighlight, oldHighlightOffset;

    void Start()
    {
        for (int i = 0; i < pointCount; i++) AddPoint2D(i);

        oldTurnFraction = turnFraction;
        oldDstAjust = distanceAjuster;
        oldPointCount = pointCount;
        oldHighlight = highlight;
        oldHighlightOffset = highlightOffset;
    }

    void Update()
    {
        if (animateTurn)
        {
            turnFraction += turnSpeedPerFrame;
            OnValidate();
        }
    }

    private void AddPoint2D(int index)
    {
        Transform newPoint = Instantiate(
                pointPrefab,
                Vector2.zero,
                Quaternion.identity,
                transform).transform;

        points.Add(newPoint);

        newPoint.position = UpdatePointPosition(index);
        UpdateHighlight(index);
    }

    private Vector3 UpdatePointPosition(int index)
    {
        if (index == 0) return transform.position;

        Vector3 position = (float)index / pointCount * Vector2.right;
        position = Quaternion.AngleAxis(turnFraction * 360 * index, Vector3.forward) * position;

        float newMagFraction = Mathf.Pow(position.magnitude, distanceAjuster);
        return transform.position + newMagFraction * circleRadius * position.normalized;
    }

    private void UpdateHighlight(int index)
    {
        if ((index + highlightOffset) % highlight == 0)
            points[index].GetComponent<SpriteRenderer>().color = highlightColor;
        else points[index].GetComponent<SpriteRenderer>().color = Color.gray;
    }

    void OnValidate()
    {
        if (Application.isPlaying && Time.time > 0)
        {
            if (distanceAjuster < -5)
            {
                distanceAjuster = -5;
                return;
            }

            if (turnFraction - oldTurnFraction != 0 || distanceAjuster - oldDstAjust != 0)
                for (int i = 0; i < points.Count; i++)
                    points[i].position = UpdatePointPosition(i);
            oldTurnFraction = turnFraction;
            oldDstAjust = distanceAjuster;

            if (pointCount - oldPointCount > 0) AddPoint2D(points.Count);
            else if (pointCount - oldPointCount < 0)
            {
                Destroy(points[points.Count - 1].gameObject);
                points.RemoveAt(points.Count - 1);
            }
            oldPointCount = pointCount;

            if (highlight - oldHighlight != 0 || highlightOffset - oldHighlightOffset != 0)
                for (int i = 0; i < points.Count; i++) UpdateHighlight(i);
            oldHighlight = highlight;
            oldHighlightOffset = highlightOffset;
        }
    }
}