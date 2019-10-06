using System;
using System.Collections.Generic;
using System.Linq;
using Shapes;
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
                        var lights = GetLightsHittingPoint(
                            scene.Lights,
                            scene.Spheres,
                            nearestIntersection.intersectionPt,
                            nearestIntersection.normal);

                        var totalLightColor = Color.black;

                        foreach (var light in lights)
                        {
                            var lightDir = (nearestIntersection.intersectionPt - light.Position).normalized;
                            var lightPercentageThatHits = Vector3.Dot(lightDir, nearestIntersection.normal) * -1;
                            
                            var lightColor = light.Material.Color * lightPercentageThatHits;
                                
                            totalLightColor.r = Math.Max(lightColor.r, totalLightColor.r);
                            totalLightColor.g = Math.Max(lightColor.g, totalLightColor.g);
                            totalLightColor.b = Math.Max(lightColor.b, totalLightColor.b);
                        }

                        var finalColor = nearestIntersection.collider.Material.Color * totalLightColor;
                        
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

        private static IEnumerable<Collider> GetLightsHittingPoint(List<Collider> lights, List<Collider> nonLights, Vector3 point, Vector3 ptNormal)
        {
            // TODO: If lights have a width then you have to cast to the sides of the light to see if any of them hit
            
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
                    yield return light;
                }
            }
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