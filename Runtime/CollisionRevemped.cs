using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace geminitaurus.gemlib.runtime
{
    public static class CollisionRevemped
    {
        public static float wallthreshold = 0.55f;
        public static LayerMask mask = 1 << 7 | 1 << 0;

        public static Vector3 Unstuck(Vector3 pos, SphereCollider mainCollider, float radius = 0.50f)
        {
            if (Physics.CheckSphere(pos, radius))//ajust si jamais la position est dans un collider
            {
                //Debug.Log("Unstuck");

                Collider[] temp = new Collider[10];
                temp = Physics.OverlapSphere(pos, radius, mask);
                Vector3 unstuck = Vector3.zero;
                int i = 0;
                foreach (Collider collider in temp)
                {
                    if (collider == mainCollider)
                        continue; // skip ourself
                                  //Vector3 hitpoint = Physics.ClosestPoint(pos, collider, collider.transform.position, collider.transform.rotation);
                    Vector3 hitpoint = Vector3.zero;
                    Vector3 hitnormal = Vector3.zero;
                    float penetrationDepth = 0f;
                    if (CheckSphereExtra(collider, mainCollider, out hitpoint, out hitnormal, out penetrationDepth))
                    {
                        unstuck = hitnormal * penetrationDepth;
                        i++;
                    }
                    //Debug.Log(hitpoint);


                }

                unstuck = unstuck * 1.1f;
                //Debug.Log(unstuck*1.1f);
                if (Physics.CheckSphere(pos - unstuck, radius))
                {
                    unstuck = unstuck * 1.1f - (Vector3.up * Time.deltaTime * 0.5f);
                    Debug.Log("StillStuck");
                }

                return pos - unstuck;
            }
            else
            {
                Debug.Log("notstuck");
            }
            return pos;
        }
        public static bool CheckSphereExtra(Collider target_collider, SphereCollider sphere_collider, out Vector3 closest_point, out Vector3 surface_normal, out float surface_penetration_depth)
        {
            closest_point = Vector3.zero;
            surface_normal = Vector3.zero;
            surface_penetration_depth = 0;

            Vector3 sphere_pos = sphere_collider.transform.position;
            if (Physics.ComputePenetration(target_collider, target_collider.transform.position, target_collider.transform.rotation, sphere_collider, sphere_pos, Quaternion.identity, out surface_normal, out surface_penetration_depth))
            {
                closest_point = sphere_pos + (surface_normal * (sphere_collider.radius - surface_penetration_depth));

                //surface_normal = -surface_normal;
                surface_penetration_depth = surface_penetration_depth * 1.1f;
                return true;
            }

            return false;
        }

        public static CollisionDataRevemped CalculateCollision(CollisionDataRevemped collisionDataResult, Vector3 from, Vector3 to, float radius = 0.4f)
        {

            collisionDataResult.did_collide = false;

            float overlap = 1.8f;
            //Unstuck(from);
            Vector3 dir = (to - from).normalized;
            Vector3 result = from;
            Vector3 resultminus1 = from;
            Vector3 hitnormal = Vector3.zero;

            int step = Mathf.FloorToInt(Vector3.Distance(from, to) / ((2 - overlap) * radius)) + 2;

            for (int i = 0; i < step + 1; i++)
            {
                Vector3 currentpos = Vector3.Lerp(from, to, (float)(i) / (float)step);
                Vector3 currentposminus1 = Vector3.Lerp(from, to, Mathf.Clamp01((float)(i - 1) / (float)step));

                if (Physics.CheckSphere(currentpos, radius, mask))
                {
                    result = currentpos;
                    resultminus1 = currentposminus1;


                    collisionDataResult.did_collide = true;
                    break;

                }
            }
            if (!collisionDataResult.did_collide) return collisionDataResult;// si j'ai rien touché, ça sert à rien de calculer le reste


            collisionDataResult.adjustedPosition = resultminus1;//position ajustée


            //maintenant faut trouver la hit normal pour definir le type de collision



            float dist = Vector3.Distance(resultminus1, result);
            RaycastHit[] hits = Physics.SphereCastAll(resultminus1, radius, dir, dist, mask);
            RaycastHit shortestHit = new RaycastHit();
            float disthighscore = 9999f;
            foreach (RaycastHit hit in hits)
            {

                if (Vector3.Distance(resultminus1, hit.point) < disthighscore)
                {
                    disthighscore = Vector3.Distance(resultminus1, hit.point);
                    shortestHit = hit;
                }
            }

            int layer = 0;
            if (hits.Length > 0)
            {
                hitnormal = shortestHit.normal;
                layer = shortestHit.transform.gameObject.layer;
            }


            Vector3 side1 = from - to;
            Vector3 side2 = (to + hitnormal) - to;
            float c = Vector3.Distance(resultminus1, to);
            //Debug.Log(c);

            float B = Vector3.Angle(side1.normalized, side2.normalized);

            float resultdist = Mathf.Sin((90 - B) * Mathf.Deg2Rad) * (c / Mathf.Sin(90 * Mathf.Deg2Rad));
            collisionDataResult.adjustedPosition = to + (hitnormal * resultdist);
            float dot = Vector3.Dot(hitnormal, Vector3.up);
            collisionDataResult.dot = dot;
            if (layer == 7)
            {
                collisionDataResult.collisionType = CollisionDataRevemped.CollisionTypeRevemped.Climbable;
                //Debug.Log("collided with climb");
            }
            else
            {


                if (dot <= wallthreshold && dot >= -wallthreshold)
                {
                    collisionDataResult.collisionType = CollisionDataRevemped.CollisionTypeRevemped.Wall;
                    //Debug.Log("collided with wall");
                    return collisionDataResult;
                }
                if (dot < -wallthreshold)
                {
                    collisionDataResult.collisionType = CollisionDataRevemped.CollisionTypeRevemped.Ceiling;
                    //Debug.Log("collided with cieling");
                    return collisionDataResult;
                }
                if (dot > wallthreshold)
                {
                    collisionDataResult.collisionType = CollisionDataRevemped.CollisionTypeRevemped.Ground;
                    //Debug.Log("collided with ground");
                    return collisionDataResult;
                }

            }

            return collisionDataResult;








        }
    }

}

