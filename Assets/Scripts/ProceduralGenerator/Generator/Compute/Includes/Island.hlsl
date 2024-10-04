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
    if(p_r<0.25)
    {
        return 1/4*r*p_r;
    }
    if(p_r < 0.5)
    {
        return 1/2 * r*p_r;
    }
    return r*p_r;
}

float island_basic_shape_sdf(float3 pos, float3 origin, float baseRadius,float height)
{
    IslandAtom queue[MAX_QUEUE_SIZE];
    uint queueHead = 0,queueRear = 0,queueSize = 0;
    queue[queueRear] = create_island_atom(origin, baseRadius, float3(0,0,0), 0);
    queueRear = (queueRear+1)%MAX_QUEUE_SIZE;
    queueSize++;
    const int initialSpawnCount = 3;
    float sum = SDF_MAX;
    int iterateCount = 0;
    while(queueSize>0 && iterateCount++ < 30)
    {
        IslandAtom atom = queue[queueHead];
        queueHead = (queueHead+1)%MAX_QUEUE_SIZE;
        queueSize --;
        sum = smooth_union(sum,inf_cylinder_sdf(pos, atom.origin, atom.radius), 0.6*atom.radius);
        if(atom.radius < 10)
        {
            continue;
        }
        for(int i = 0;i<3*(atom.radius/10);i++)
        {
            const float angle = iterateCount == 1 ? simple_hash(  atom.origin + 56.9 * float3(i,0,0)) * PI * 2 
                : simple_hash(atom.origin + 56.9 * float3(i,0,0)) * PI - 0.5 * PI + atom.dirAngle.y;
            const float3 dir = float3(cos(angle),0,sin(angle))*atom.radius*(0.8+simple_hash(atom.origin + 23.5 * float3(i,0,0)));
            const float p_r = simple_hash(atom.origin + 23.5 * float3(i,0,0));
            
            if(queueSize < MAX_QUEUE_SIZE)
            {
                queue[queueRear] = create_island_atom(atom.origin + dir,
                    get_radius(atom.radius,p_r),
                    float3(0,angle,0),
                    atom.depth+1);
                queueRear = (queueRear+1)%MAX_QUEUE_SIZE;
                queueSize++;
            }
        }
    }

    //Base shape disformation
    sum += 50*fractalNoise(0.01 * pos* float3(1,0,1),3,2,0.5) ;

    //Bottom face
    sum = max(sum, -sdf_plane(pos, origin.y - 5* height));

    //Surface disformation
    return sum + 5 * fractalNoise(0.03 * pos,5,2,0.5);
}

DotExpl island_basic_shape_sdf(float3 pos, int3 hash,float3 origin, float baseRadius,float height, float3 color)
{
    const float3 islandPos = sdf_pos_islandlize(pos, origin, -0.5*height);
    float w = island_basic_shape_sdf(islandPos, origin, baseRadius, height);
    DotExpl result = create_dot_expl(w, WHITE*0.5);
    result = smooth_colored_intersection(result,
        disform( sdf_plane(islandPos, height, GREEN), 10 * fractalNoise(0.01 * pos,3,2,0.5)),
        0.2*baseRadius);
    return result;
}

DotExpl island_world(float3 pos,float3 size)
{
    // int3 id = round(pos/size);
    // float3 islandPos = pos - id*size;
    // return island_basic_shape_sdf(islandPos, id,(id+0.5)*size, 100, 100, WHITE);
    return island_basic_shape_sdf(pos, int3(0,0,0), float3(0,0,0), 100, 100, WHITE);
}

#endif