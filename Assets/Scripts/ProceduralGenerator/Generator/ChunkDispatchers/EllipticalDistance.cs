using UnityEngine;

public static class EllipticalDistance
{
    public static float GetDistance(Vector3 dir, float xzRadius,float yRadius)
    {
        dir = dir.normalized;
        float yFactor = dir.y;
        return Mathf.Lerp(xzRadius, yRadius, yFactor);
    }
}
