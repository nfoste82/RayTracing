using System;
using System.Collections.Generic;
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
                        var lights = GetLightsHittingPoint(scene.Lights, scene.Spheres, nearestIntersection.intersectionPt);

                        var finalColor = MaterialColorAfterLighting(nearestIntersection.collider.Material.Color, lights);
                        
                        texture.SetPixel(x, y, finalColor);
                    }
                }
            }
            
            texture.Apply();
        }

        private static (Collider collider, Vector3 intersectionPt) GetNearestIntersection(
            List<Collider> colliders, 
            Vector3 pixelPoint,
            Vector3 rayDirection, 
            ref double nearestPt)
        {
            (Collider collider, Vector3 intersectionPt) nearestIntersection = default;
            
            foreach (var collider in colliders)
            {
                var (intersected, hitDistance) = Collider.Intersect(pixelPoint, collider, rayDirection);

                if (intersected && hitDistance < nearestPt)
                {
                    nearestIntersection = (collider, (pixelPoint + (rayDirection * (float) hitDistance)));
                    nearestPt = hitDistance;
                }
            }

            return nearestIntersection;
        }

        private static IEnumerable<Collider> GetLightsHittingPoint(List<Collider> lights, List<Collider> nonLights, Vector3 point)
        {
            // TODO: If lights have a width then you have to cast to the sides of the light to see if any of them hit
            
            foreach (var light in lights)
            {
                var lightToPt = (light.Position - point);
                var lightHitDistance = lightToPt.magnitude;
                var lightRayDirection = lightToPt.normalized;
                
                var lightWasHit = true;
                foreach (var nonLight in nonLights)
                {
                    var (intersected, hitDistance) = Collider.Intersect(point, nonLight, lightRayDirection);

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

        private static Color MaterialColorAfterLighting(Color surfaceColor, IEnumerable<Collider> lights)
        {
            var lightColor = Color.black;

            foreach (var light in lights)
            {
                var color = light.Material.Color;
                lightColor.r = Math.Max(lightColor.r, color.r);
                lightColor.g = Math.Max(lightColor.g, color.g);
                lightColor.b = Math.Max(lightColor.b, color.b);
            }

            return lightColor * surfaceColor;
        }
    }
}