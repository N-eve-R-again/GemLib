using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDataRevemped
{
    public Vector3 adjustedPosition;
    public float dot;
    public bool did_collide;
    public CollisionTypeRevemped collisionType;
    public enum CollisionTypeRevemped
    {
        Climbable,
        Ground,
        Wall,
        Ceiling // plafond et mur non grimpable 
    }
    
}
