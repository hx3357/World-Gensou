using System.Collections.Generic;

public class ChunkLODManager : MonoSingleton<ChunkLODManager>
{
    // Default LOD configuration
    private Dictionary<float, int> distanceToChunkResolution = new()
    {
        { 600, 35 },
        { 800, 31 },
        { 1200, 15 }
    };

    private int fallbackResolution = 7;

    public LODConfig config;

    protected override void Awake()
    {
        base.Awake();
        if (config == null) return;
        distanceToChunkResolution = config.GetLODConfigTable();
        fallbackResolution = config.fallbackResolution;
    }

    public int GetChunkResolutionByDistance(float distance)
    {
        foreach (var pair in distanceToChunkResolution)
            if (distance <= pair.Key)
                return pair.Value;

        return fallbackResolution;
    }
    
}