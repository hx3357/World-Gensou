using UnityEngine;


public static class HashUtility
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>A vector whose each component is between 0 and 1</returns>
    public static Vector3 Get3DHash(Vector3 pos)
    {
        float x = (uint)((pos + Vector3.one * 23.7f).GetHashCode()) % 10000 / 10000f;
        float y = (uint)((pos + Vector3.one * 45.3f).GetHashCode()) % 10000 / 10000f;
        float z = (uint)((pos + Vector3.one * 56.8f).GetHashCode()) % 10000 / 10000f;
        return new Vector3(x,y,z);
    }
}
