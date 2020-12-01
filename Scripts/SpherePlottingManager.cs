using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpherePlottingManager : MonoBehaviour
{
    [Tooltip("Number of points")] [Range(10, 1000)] public int pointCount = 100;
    [Tooltip("Shifts points towards center")] [Range(0, 5)] public float distanceBias = 1.4f;
    [Tooltip("Max angle from transform.forward in degrees")] [Range(0, 180)] public float viewAngle = 180;
    public float viewRadius = 75;
    [Tooltip("Add this fraction of 360 degrees to each new point")] public float turnFraction = 1.61803f;

    private List<Vector3> directions = new List<Vector3>();

    void Awake()
    {
        for (int i = 0; i < pointCount; i++) directions.Add(Vector3.zero);
        UpdateAllVectorPositions();

        foreach (Vector3 v in directions.ToList())
            if (Vector3.Angle(transform.forward, v) > viewAngle) directions.Remove(v);
    }

    public Vector3[] GetVectors()
    {
        UpdateAllVectorPositions();
        return directions.ToArray();
    }

    private void UpdateAllVectorPositions()
    {
        for (int i = 0; i < directions.Count; i++)
            directions[i] = UpdatePointPosition(i);
    }

    public Vector3 UpdatePointPosition(int index)
    {
        Vector3 position = viewRadius * transform.forward;

        if (index == 0) return position;

        position = Quaternion.AngleAxis(
            PositionBias((float)index / (pointCount - 1)) * 180, // Forms semi-circle
            transform.right) * position;

        position = Quaternion.AngleAxis(
            turnFraction * 360 * index, // Forms sphere based on turnFraction
            transform.forward) * position;
        return position;
    }

    private float PositionBias(float turnFraction)
    {
        if (distanceBias == 0) return turnFraction;

        return Mathf.Pow(turnFraction, Mathf.Lerp(
            1 / distanceBias,
            distanceBias,
            turnFraction));
    }
}