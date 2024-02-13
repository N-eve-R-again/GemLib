using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ScriptCollision;
using static ScriptCollision.CollisionData;

public class ScriptCollision
{

    public struct CollisionData
    {
        public Vector3 adjustedPosition;
        public Vector3 hit_point;
        public Vector3 hit_normal;
        public bool didCollide;
        public enum CollisionType
        {
            Climbable,
            Ground,
            Wall,
            Wall_Fall // plafond et mur non grimpable 
        }

        [SerializeField] public CollisionType currentType;

    }
    public static CollisionData CalculateCollision(Vector3 from, Vector3 to, float radius = 0.1f)
    {
        CollisionData newData = new CollisionData();

        Vector3 direction = to - from;
        float magnitude = direction.magnitude;
        direction.Normalize();

        RaycastHit hit;

        //Debug.DrawRay(from, direction * radius, Color.green);
        Debug.DrawRay(from, direction * radius, Color.green);

        ////// La hit_normal_y change uniquement lorsqu'il touche un obj, mais à une valeur par défaut en fonction du STATE du joueur
        ///// Si il touche un obj en calque 0 alors la hit_normal_y change et le joueur change d'état en fonction de la collision
        //// Obligé de rajouter le sinon pour bloquer la hit_normal_y sinon si il ne touche rien, hit_normal.y reprends la valeur de 0 
        /// Il prends la valeur hit_normal_y_null qui égale en fonction du State du joueur, si le joueur est en move alors sa hit_normal_y 
        // Sera égale à 1 , si en train de grimper la hit_normal_y sera égale à 0 
        if (Physics.Raycast(from, direction, out hit, radius))
        {
            newData.hit_normal.y = hit.normal.y;
            newData.hit_point = hit.point;
            newData.didCollide = true;

            if (hit.transform.gameObject.layer == 7)
            {
                newData.currentType = CollisionType.Climbable;
                return newData;
            }

        }
        else
        {
            newData.didCollide = false;
            return newData;
        }
        ///////////////////////////////// FIN /////////////////////////////////

        switch (newData.hit_normal.y)
        {
            case float n when (n >= 0.2f) && (n <= 1):
                newData.currentType = CollisionType.Ground;
                //newData.hit_normal.y = 1f;
                //Debug.Log("GROUND");
                break;
            case float n when (n < 0.2f):
                newData.currentType = CollisionType.Wall;
                //newData.hit_normal.y = 0f;
                Vector3 collisionVector = newData.hit_point - from;
                newData.adjustedPosition = newData.hit_point - (collisionVector.normalized * 0.1f);
                //Debug.Log("WALL"); 
                break;
            default:
                newData.currentType = ScriptCollision.CollisionData.CollisionType.Ground;
                newData.hit_normal.y = 1;
                //Debug.Log("MISSING ENUM");
                break;
        }
        return newData;
    }
}
