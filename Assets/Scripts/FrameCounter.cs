
using System.Collections;
using UnityEngine;

public class FrameCounter : MonoSingleton<FrameCounter>
{
    public int frameCount { get; private set; } = 0;
    
}
