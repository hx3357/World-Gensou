using UnityEngine;

public static class GrassMeshData
{
    private static readonly float halfWidth = 0.5f;
    private static readonly float rowHeight = 1;

    private static readonly Vector3[] vertices =
    {
        new(-halfWidth, 0, 0),
        new(halfWidth, 0, 0),
        new(-halfWidth, rowHeight, 0),
        new(halfWidth, rowHeight, 0),
        new(-halfWidth * 0.9f, rowHeight * 2, 0),
        new(halfWidth * 0.9f, rowHeight * 2, 0),
        new(-halfWidth * 0.8f, rowHeight * 3, 0),
        new(halfWidth * 0.8f, rowHeight * 3, 0),
        new(0, rowHeight * 4, 0)
    };

    private static readonly Vector3 normal = new(0, 0, -1);

    private static readonly Vector3[] normals =
    {
        normal, normal, normal, normal, normal, normal, normal, normal, normal
    };

    private static readonly Vector2[] uvs =
    {
        new(0, 0),
        new(1, 0),
        new(0, 0.25f),
        new(1, 0.25f),
        new(0, 0.5f),
        new(1, 0.5f),
        new(0, 0.75f),
        new(1, 0.75f),
        new(0.5f, 1)
    };

    private static readonly int[] indices =
    {
        0, 1, 2, 1, 3, 2,
        2, 3, 4, 3, 5, 4,
        4, 5, 6, 5, 7, 6,
        6, 7, 8
    };

    public static readonly Mesh GrassMesh;
    public static readonly Vector3 GrassMeshSize = new(halfWidth * 2, rowHeight * 4, 0);

    static GrassMeshData()
    {
        GrassMesh = new Mesh
        {
            vertices = vertices,
            normals = normals,
            uv = uvs,
            triangles = indices
        };
    }
}