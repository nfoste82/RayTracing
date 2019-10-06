using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Collider = Shapes.Collider;

namespace RayTracer
{
    public class Tracer
    {
        private bool interlacingEnabled = true;
        private bool evenLines = true;
        
        private readonly Color _skyColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
        private readonly Color _skyIndirectColor = new Color(0.2f, 0.4f, 0.5f, 1.0f);
        private readonly Color _skyDarkColor = new Color(0.1f, 0.2f, 0.25f, 1.0f);
        
        public void ProcessRenderTexture(Texture2D texture, Scene scene, Camera camera)
        {
            var textureWidth = texture.width;
            var textureHeight = texture.height;
            var cameraOrigin = camera.transform.position;
            var cameraNearPlane = camera.nearClipPlane;

            evenLines = !evenLines;

            for (var y = 0; y < textureHeight; ++y)
            {
                //                    if (interlacingEnabled)
//                    {
//                        if (evenLines && y % 2 != 0)
//                        {
//                            texture.SetPixel(x, y, Color.black);
//                            continue;
//                        }
//                        
//                        if (!evenLines && y % 2 == 0)
//                        {
//                            texture.SetPixel(x, y, Color.black);
//                            continue;
//                        }
//                    }

                var skyColorAtX = new Color(
                    Mathf.Lerp(_skyColor.r, _skyDarkColor.r, y / (float) textureHeight),
                    Mathf.Lerp(_skyColor.g, _skyDarkColor.g, y / (float) textureHeight),
                    Mathf.Lerp(_skyColor.b, _skyDarkColor.b, y / (float) textureHeight),
                    1f
                );
                
                for (var x = 0; x < textureWidth; ++x)
                {
                    var pixelPoint = camera.ScreenToWorldPoint(new Vector3(x, y, cameraNearPlane));
                    var rayDirection = (pixelPoint - cameraOrigin);
                    rayDirection.Normalize();
                    
                    double nearestPt = float.MaxValue;

                    //Profiler.BeginSample("GetNearestIntersection");
                    var nearestIntersection = GetNearestIntersection(scene.Spheres, pixelPoint, rayDirection, ref nearestPt);
                    //Profiler.EndSample();

                    if (nearestIntersection.collider == null)
                    {
                        texture.SetPixel(x, y, skyColorAtX);
                    }
                    else
                    {
                        //Profiler.BeginSample("LightColorAffectsHitPoint");
                        var accumLightColor = LightColorAffectsHitPoint(
                            scene.Lights,
                            scene.Spheres,
                            nearestIntersection.intersectionPt,
                            nearestIntersection.normal);
                        //Profiler.EndSample();
                        
                        accumLightColor.r = Math.Max(accumLightColor.r, _skyDarkColor.r);
                        accumLightColor.g = Math.Max(accumLightColor.g, _skyDarkColor.g);
                        accumLightColor.b = Math.Max(accumLightColor.b, _skyDarkColor.b);

                        var finalColor = nearestIntersection.collider.Material.Color * accumLightColor;
                        
                        texture.SetPixel(x, y, finalColor);
                    }
                }
            }
            
            texture.Apply();
        }

        private static (Collider collider, Vector3 intersectionPt, Vector3 normal) GetNearestIntersection(
            List<Collider> colliders, 
            Vector3 pixelPoint,
            Vector3 rayDirection, 
            ref double nearestDistance)
        {
            Collider nearestCollider = null;

            foreach (var collider in colliders)
            {
                var hitDistance = collider.Intersect(pixelPoint, rayDirection);

                if (hitDistance >= 0.0f && hitDistance < nearestDistance)
                {
                    nearestCollider = collider;
                    nearestDistance = hitDistance;
                }
            }

            var nearestPt = (nearestCollider == null) ? Vector3.zero : (pixelPoint + (rayDirection * (float)nearestDistance));
            var normal = (nearestCollider == null) ? Vector3.zero : nearestCollider.GetNormalAtPoint(nearestPt);

            return (nearestCollider, nearestPt, normal);
        }

        private static Color LightColorAffectsHitPoint(
            List<Collider> lights, 
            List<Collider> nonLights, 
            Vector3 point, 
            Vector3 ptNormal)
        {
            // TODO: If lights have a width then you have to cast to the sides of the light to see if any of them hit

            Color ptLightColor = Color.black;
            
            foreach (var light in lights)
            {
                var rayDirection = light.Position - point;

                // If the point is facing away from the light then we can skip it
                if (Vector3.Dot(rayDirection, ptNormal) < 0.0f)
                {
                    continue;
                }
                
                var lightHitDistance = Vector3Extensions.NormalizeReturnMag(ref rayDirection);

                var lightWasHit = true;
                foreach (var nonLight in nonLights)
                {
                    // This may help improve performance in a more dense scene. But in the current demo scene it hurts performance
//                    var ptToNonLight = nonLight.Position - point;
//
//                    var minDistanceToCollider = ptToNonLight.magnitude - nonLight.GetBoundingRadius();
//                    if (minDistanceToCollider > lightHitDistance)
//                    {
//                        continue;
//                    }
                    
                    var hitDistance = nonLight.Intersect(point, rayDirection);

                    if (hitDistance >= 0.0f && hitDistance < lightHitDistance)
                    {
                        lightWasHit = false;
                        break;
                    }
                }

                if (lightWasHit)
                {
                    var lightPercentageThatHits = Vector3.Dot(rayDirection, ptNormal);
                            
                    var lightColor = light.Material.Color * lightPercentageThatHits;
                                
                    ptLightColor.r = Math.Max(lightColor.r, ptLightColor.r);
                    ptLightColor.g = Math.Max(lightColor.g, ptLightColor.g);
                    ptLightColor.b = Math.Max(lightColor.b, ptLightColor.b);
                }
            }

            return ptLightColor;
        }
    }
}