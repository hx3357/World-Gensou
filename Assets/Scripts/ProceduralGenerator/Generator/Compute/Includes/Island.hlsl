#ifndef ISLAND_HLSL
#define ISLAND_HLSL

#define MAX_QUEUE_SIZE 50

#include "SDFOperations.hlsl"
#include "SDFBasicShapes.hlsl"
#include "SDFEssential.hlsl"
#include "Hash.hlsl"
#include "../Noise/fractalNoise.hlsl"

typedef struct
{
    float3 origin;
    float radius;
    float3 dirAngle;
    int depth;
} IslandAtom;

IslandAtom create_island_atom(float3 origin, float radius, float3 dirAngle, int depth)
{
    IslandAtom atom;
    atom.origin = origin;
    atom.radius = radius;
    atom.dirAngle = dirAngle;
    atom.depth = depth;
    return atom;
}

float get_radius(float r,float p_r)
{
    if(p_r<0.2)
    {
        return 1/3*r*p_r;
    }
    if(p_r < 0.6)
    {
        return 2/3 * r*p_r;
    }
    return r*p_r;
}

#define USE_BFS 0

float island_basic_shape_sdf(float3 pos, float3 origin, float baseRadius,float height,float maxRadius)
{
    #if USE_BFS == 0
    
    float sum = inf_cylinder_sdf(pos, origin, baseRadius);
    
    #else
    IslandAtom queue[MAX_QUEUE_SIZE];
    uint queueHead = 0,queueRear = 0,queueSize = 0;
    queue[queueRear] = create_island_atom(origin, baseRadius, float3(0,0,0), 0);
    queueRear = (queueRear+1)%MAX_QUEUE_SIZE;
    queueSize++;
    float sum = SDF_MAX;
    int iterateCount = 0;

    //BFS
    while(queueSize>0 && iterateCount++ < 1)
    {
        IslandAtom atom = queue[queueHead];
        queueHead = (queueHead+1)%MAX_QUEUE_SIZE;
        queueSize --;
        sum = smooth_union(sum,inf_cylinder_sdf(pos, atom.origin, atom.radius), 0.6*atom.radius);
        if(atom.radius < 70)
        {
            continue;
        }
        for(int i = 0;i<3;i++)
        {
            const float angle = iterateCount == 1 ? simple_hash(  atom.origin + 56.9 * float3(i,0,0)) * PI * 2 
                : simple_hash(atom.origin + 56.9 * float3(i,0,-i)) * PI - 0.5 * PI + atom.dirAngle.y;
            const float3 dir = float3(cos(angle),0,sin(angle))*atom.radius*(0.8+simple_hash(atom.origin + 23.5 * float3(i,0,0)));
            const float p_r = simple_hash(atom.origin + 23.5 * float3(i,0,0));
            const float3 newOrigin = atom.origin + dir;
            float newRadius = get_radius(atom.radius,p_r);

            if(queueSize >= MAX_QUEUE_SIZE || length(newOrigin-origin) + newRadius >= maxRadius)
                continue;

            if(length(newOrigin-origin) + newRadius >= maxRadius)
            {
                if(2*length(newOrigin-origin) < maxRadius)
                    newRadius = maxRadius - length(newOrigin-origin);
                else
                    continue;
            }
            
            queue[queueRear] = create_island_atom(newOrigin,
                newRadius,
                float3(0,angle,0),
                atom.depth+1);
            queueRear = (queueRear+1)%MAX_QUEUE_SIZE;
            queueSize++;
            
        }
    }

    #endif

    //Bottom face
    sum = max(sum, -sdf_plane(pos, origin.y - height/2));
    
    return sum;
}

DotExpl island_basic_shape_sdf(float3 pos, int3 hash,float3 origin, float baseRadius,float height, float3 color,float maxRadius)
{
    const float3 islandPos = sdf_pos_islandlize(pos, origin, origin.y+0.5*(simple_hash(origin*2.11f)*2-1)*height);
    float w = island_basic_shape_sdf(islandPos, origin, baseRadius, height, maxRadius);
    
    //Base shape disformation

    const float frac_noise =0.5+ 3* fractalNoise(0.16/baseRadius * (origin + pos* float3(1,0,1)),5,2,0.6);
    
    w -= (maxRadius - baseRadius)*clamp(frac_noise,0,1);
   
    //Surface disformation
    w += 8 * fractalNoise(0.04 * pos,3,2,0.5);
    
    DotExpl result = create_dot_expl(w, color);
    // Top face
    result = smooth_colored_intersection(result,
        disform( sdf_plane(islandPos, origin.y+height/2, GREEN*(1-0.3*f_snoise(0.001*pos))),
            100 * fractalNoise(0.001 * pos,4,2.4,0.4)),
        0.2*baseRadius);
    return result;
}

// DotExpl island_world(float3 pos,float3 size)
// {
//     // int3 id = round(pos/size);
//     // float3 islandPos = pos - id*size;
//     // return island_basic_shape_sdf(islandPos, id,(id+0.5)*size, 100, 100, WHITE);
//     return island_basic_shape_sdf(pos, int3(0,0,0), float3(0,0,0), 100, 100, WHITE);
// }

#endif