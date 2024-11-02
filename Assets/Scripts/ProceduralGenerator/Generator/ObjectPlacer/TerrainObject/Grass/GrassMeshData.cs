using UnityEngine;

public static class GrassMeshData
{
    readonly static float halfWidth = 1;
    readonly static float rowHeight = 3;

    readonly static Vector3[] vertices =
    {
        new Vector3(-halfWidth, 0, 0),
        new Vector3(halfWidth, 0, 0),
        new Vector3(-halfWidth, rowHeight, 0),
        new Vector3(halfWidth, rowHeight, 0),
        new Vector3(-halfWidth * 0.9f, rowHeight * 2, 0),
        new Vector3(halfWidth * 0.9f, rowHeight * 2, 0),
        new Vector3(-halfWidth * 0.8f, rowHeight * 3, 0),
        new Vector3(halfWidth * 0.8f, rowHeight * 3, 0),
        new Vector3(0, rowHeight * 4, 0)
    };

    readonly static Vector3 normal = new Vector3(0, 0, -1);

    readonly static Vector3[] normals =
    {
        normal, normal, normal, normal, normal, normal, normal, normal, normal
    };

    readonly static Vector2[] uvs =
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 0.25f),
        new Vector2(1, 0.25f),
        new Vector2(0, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(0, 0.75f),
        new Vector2(1, 0.75f),
        new Vector2(0.5f, 1)
    };

    readonly static int[] indices =
    {
        0, 1, 2, 1, 3, 2,
        2, 3, 4, 3, 5, 4,
        4, 5, 6, 5, 7, 6,
        6, 7, 8
    };

    readonly public static Mesh GrassMesh;

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