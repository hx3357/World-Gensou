using System.Collections.Generic;

public class MyLODManager
{
    //Default LOD configuration
    private readonly Dictionary<float,int> distanceToChunkResolution = new()
    {
        {200,35},
        {600,31},
        {1200,15},
    };
    
    private int lowestResolution = 7;
    
    public MyLODManager()
    {
        
    }

    public MyLODManager(Dictionary<float, int> m_distanceToChunkResolution, int m_lowestResolution)
    {
        distanceToChunkResolution = m_distanceToChunkResolution;
        lowestResolution = m_lowestResolution;
    }
    
    public int GetChunkResolutionByDistance(float distance)
    {
        foreach (var pair in distanceToChunkResolution)
        {
            if (distance <= pair.Key)
            {
                return pair.Value;
            }
        }

        return lowestResolution;
    }
}