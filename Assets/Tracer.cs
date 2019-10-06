using System;
using System.Collections.Generic;
using UnityEngine;

using Collider = Shapes.Collider;

namespace RayTracer
{
    public class Tracer
    {
        public void ProcessRenderTexture(Texture2D texture, Scene scene, Camera camera)
        {
            var textureWidth = texture.width;
            var textureHeight = texture.height;
            var cameraOrigin = camera.transform.position;
            var cameraNearPlane = camera.nearClipPlane;

            var skyColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

            for (var x = 0; x < textureWidth; ++x)
            {
                for (var y = 0; y < textureHeight; ++y)
                {
                    var pixelPoint = camera.ScreenToWorldPoint(new Vector3(x, y, cameraNearPlane));
                    var rayDirection = (pixelPoint - cameraOrigin).normalized;
                    
                    double nearestPt = float.MaxValue;

                    var nearestIntersection = GetNearestIntersection(scene.Spheres, pixelPoint, rayDirection, ref nearestPt);

                    if (nearestIntersection.Equals(default))
                    {
                        texture.SetPixel(x, y, skyColor);
                    }
                    else
                    {
                        var accumLightColor = LightColorAffectsHitPoint(
                            scene.Lights,
                            scene.Spheres,
                            nearestIntersection.intersectionPt,
                            nearestIntersection.normal);

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
            (Collider collider, Vector3 intersectionPt, Vector3 normal) nearestIntersection = default;
            
            foreach (var collider in colliders)
            {
                var (intersected, hitDistance) = Collider.Intersect(pixelPoint, collider, rayDirection);

                if (intersected && hitDistance < nearestDistance)
                {
                    var intersectionPt = (pixelPoint + (rayDirection * (float) hitDistance));
                    var normal = Collider.GetNormalAtPoint(intersectionPt, collider);
                    
                    nearestIntersection = (collider, (pixelPoint + (rayDirection * (float) hitDistance)), normal);
                    nearestDistance = hitDistance;
                }
            }

            return nearestIntersection;
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
                var ptToLight = (light.Position - point);

                if (Vector3.Dot(ptNormal, ptToLight) < 0.0f)
                {
                    continue;
                }
                
                var lightHitDistance = ptToLight.magnitude;
                var rayDirection = ptToLight.normalized;
                
                var lightWasHit = true;
                foreach (var nonLight in nonLights)
                {
                    var (intersected, hitDistance) = Collider.Intersect(point, nonLight, rayDirection);

                    if (intersected && hitDistance < lightHitDistance)
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

        private static Color MaterialColorAfterLighting(Color surfaceColor, List<Color> lightColors)
        {
            var lightColor = Color.black;

            foreach (var color in lightColors)
            {
                lightColor.r = Math.Max(lightColor.r, color.r);
                lightColor.g = Math.Max(lightColor.g, color.g);
                lightColor.b = Math.Max(lightColor.b, color.b);
            }

            return lightColor * surfaceColor;
        }
    }
}