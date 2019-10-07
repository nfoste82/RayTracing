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
        private readonly Color _skyDarkColor = new Color(0.1f, 0.2f, 0.25f, 1.0f);

        private Color[] pixelColors;

        private Vector2Int windowSize;

        public void SetPixel(int x, int y, Color color)
        {
            pixelColors[y * windowSize.x + x] = color;
        }

        public Color GetPixel(int x, int y)
        {
            return pixelColors[y * windowSize.x + x];
        }

        public Tracer(Texture2D texture)
        {
            windowSize = new Vector2Int(texture.width, texture.height);
            pixelColors = new Color[texture.width * texture.height];
        }
        
        public void ProcessRenderTexture(Texture2D texture, Scene scene, Camera camera)
        {
            var textureWidth = texture.width;
            var textureHeight = texture.height;
            var cameraOrigin = camera.transform.position;
            var cameraNearPlane = camera.nearClipPlane;

            for (var y = 0; y < textureHeight; ++y)
            {
//                if (interlacingEnabled)
//                {
//                    if (evenLines && y % 2 != 0)
//                    {
//                        for (var x = 0; x < textureWidth; ++x)
//                            pixelColors[y * windowSize.x + x] = Color.black;
//                        
//                        continue;
//                    }
//                    
//                    if (!evenLines && y % 2 == 0)
//                    {
//                        for (var x = 0; x < textureWidth; ++x)
//                            pixelColors[y * windowSize.x + x] = Color.black;
//                        
//                        continue;
//                    }
//                }

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
                        SetPixel(x, y, skyColorAtX);
                    }
                    else
                    {
                        //Profiler.BeginSample("LightColorAffectsHitPoint");
                        var (diffuse, specular) = LightColorAffectsHitPoint(
                            scene.Lights,
                            scene.Spheres,
                            nearestIntersection.intersectionPt,
                            nearestIntersection.normal,
                            rayDirection,
                            nearestIntersection.collider.Material);
                        //Profiler.EndSample();

                        diffuse.r = Math.Max(diffuse.r, _skyDarkColor.r);
                        diffuse.g = Math.Max(diffuse.g, _skyDarkColor.g);
                        diffuse.b = Math.Max(diffuse.b, _skyDarkColor.b);

                        var finalColor = nearestIntersection.collider.Material.Color * diffuse + specular;

                        SetPixel(x, y, finalColor);
                    }
                }
            }

            texture.SetPixels(pixelColors);

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

        private static (Color diffuse, Color specular) LightColorAffectsHitPoint(
            List<Collider> lights, 
            List<Collider> nonLights, 
            Vector3 point, 
            Vector3 ptNormal,
            Vector3 cameraRayDirection,
            RayMaterial materialHit)
        {
            // TODO: If lights have a width then you have to cast to the sides of the light to see if any of them hit

            Color diffuseColor = Color.black;
            Color specColor = Color.black;
            
            foreach (var light in lights)
            {
                var ptToLight = light.Position - point;

                // If the point is facing away from the light then we can skip it
                if (Vector3.Dot(ptToLight, ptNormal) < 0.0f)
                {
                    continue;
                }
                
                var lightHitDistance = Vector3Extensions.NormalizeReturnMag(ref ptToLight);

                var lightHitsThisPoint = true;
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
                    
                    var hitDistance = nonLight.Intersect(point, ptToLight);

                    if (hitDistance >= 0.0f && hitDistance < lightHitDistance)
                    {
                        lightHitsThisPoint = false;
                        break;
                    }
                }

                if (lightHitsThisPoint)
                {
                    var reflect = Vector3.Reflect(ptToLight, ptNormal);
                    
                    // Handle specularity
                    {
                        var viewDot = Vector3.Dot(cameraRayDirection, reflect);

                        if (viewDot > 0.0f)
                        {
                            var pow = (float) Math.Pow(viewDot, Math.Max(64 * (1 - materialHit.Roughness), 1));
                            var specAmount = Math.Max(pow, 0);
                            if (specAmount > 0.01f)
                            {
                                var lightSpecColor = light.Material.Color * specAmount * (1 - materialHit.Roughness);

                                specColor.r = Math.Max(lightSpecColor.r, specColor.r);
                                specColor.g = Math.Max(lightSpecColor.g, specColor.g);
                                specColor.b = Math.Max(lightSpecColor.b, specColor.b);
                            }
                        }
                    }

                    // Diffuse lighting
                    {
                        var rayNormalDot = Vector3.Dot(ptToLight, ptNormal);
                        var lightPercentageThatHits = rayNormalDot;
                        var lightDiffuseColor = light.Material.Color * lightPercentageThatHits;

                        diffuseColor.r = Math.Max(lightDiffuseColor.r, diffuseColor.r);
                        diffuseColor.g = Math.Max(lightDiffuseColor.g, diffuseColor.g);
                        diffuseColor.b = Math.Max(lightDiffuseColor.b, diffuseColor.b);
                    }
                }
            }

            return (diffuseColor, specColor);
        }

        public static Vector3 ColorToVector3(Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }
    }
}