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
                    var origin = camera.ScreenToWorldPoint(new Vector3(x, y, cameraNearPlane));
                    var rayDirection = (origin - cameraOrigin);
                    rayDirection.Normalize();
                    
                    CastRayFromScreen(origin, rayDirection, scene, x, y, skyColorAtX);
                }
            }

            texture.SetPixels(pixelColors);

            texture.Apply();
        }

        private void CastRayFromScreen(Vector3 origin, Vector3 rayDirection, Scene scene, int x, int y, Color skyColorAtX)
        {
            // (currently only a max depth of 1 supported)
            const int maxDepth = 1;
            
            var depth = 0;
            Collider previousCollider = null;

            while (true)
            {
                double nearestPt = float.MaxValue;

                var nearestIntersection = GetNearestIntersection(scene.Spheres, previousCollider, origin, rayDirection, ref nearestPt);

                if (nearestIntersection.collider == null)
                {
                    if (depth > 0)
                    {
                        var pixelColor = pixelColors[y * windowSize.x + x];

                        var previousOpacity = previousCollider.Material.Opacity;
                        var inverseOpacity = 1 - previousOpacity;
                        
                        pixelColor.r = Math.Max(pixelColor.r, skyColorAtX.r * inverseOpacity);
                        pixelColor.g = Math.Max(pixelColor.g, skyColorAtX.g * inverseOpacity);
                        pixelColor.b = Math.Max(pixelColor.b, skyColorAtX.b * inverseOpacity);

                        pixelColors[y * windowSize.x + x] = pixelColor;
                    }
                    else
                    {
                        pixelColors[y * windowSize.x + x] = skyColorAtX;
                    }
                }
                else
                {
                    var (diffuse, specular) = LightColorAffectsHitPoint(scene.Lights, scene.Spheres, nearestIntersection.intersectionPt, nearestIntersection.normal, rayDirection, nearestIntersection.collider.Material);

                    // Ambient lighting
                    diffuse.r = Math.Max(diffuse.r, _skyDarkColor.r);
                    diffuse.g = Math.Max(diffuse.g, _skyDarkColor.g);
                    diffuse.b = Math.Max(diffuse.b, _skyDarkColor.b);

                    var finalColor = (nearestIntersection.collider.Material.Color * diffuse + specular);
                    finalColor.a = 1.0f;

                    if (depth == maxDepth)
                    {
                        // Add colors from each depth together (currently only depth of 1 supported)
                        
                        var pixelColor = pixelColors[y * windowSize.x + x];

                        var previousOpacity = previousCollider.Material.Opacity;
                        var inverseOpacity = 1 - previousOpacity;
                        
                        pixelColor.r = Math.Max(pixelColor.r, finalColor.r * inverseOpacity);
                        pixelColor.g = Math.Max(pixelColor.g, finalColor.g * inverseOpacity);
                        pixelColor.b = Math.Max(pixelColor.b, finalColor.b * inverseOpacity);

                        pixelColors[y * windowSize.x + x] = pixelColor;
                    }
                    else
                    {
                        pixelColors[y * windowSize.x + x] = finalColor;
                    }

                    var opacity = nearestIntersection.collider.Material.Opacity;
                    if (opacity < 1.0f && depth < maxDepth)
                    {
                        previousCollider = nearestIntersection.collider;
                        origin = nearestIntersection.intersectionPt;
                        
                        // Refract when we hit the transparent object
                        var colliderRefIndex = nearestIntersection.collider.Material.RefractionIndex;
                        rayDirection = rayDirection.Refract(1.0f, colliderRefIndex, nearestIntersection.normal);
                        
                        // Calculate exit point of sphere
                        var colliderPos = nearestIntersection.collider.Position;
                        var colliderRadius = nearestIntersection.collider.GetBoundingRadius(); 
                        var halfway = Vector3Extensions.GetClosestPointOnLineSegment(origin, origin + 2 * colliderRadius * rayDirection,
                            colliderPos);
                        var exitPoint = ((halfway - origin) * 2f) + origin;
                        var exitPointNormal = (exitPoint - colliderPos).normalized;

                        // Now refract again since we've left the sphere
                        rayDirection = rayDirection.Refract(colliderRefIndex, 1.0f, exitPointNormal);
                        origin = exitPoint;
                        
                        
                        ++depth;
                        
                        continue;
                    }
                }

                break;
            }
        }

        private static (Collider collider, Vector3 intersectionPt, Vector3 normal) GetNearestIntersection(
            List<Collider> colliders,
            Collider colliderToIgnore,
            Vector3 origin,
            Vector3 rayDirection, 
            ref double nearestDistance)
        {
            Collider nearestCollider = null;

            foreach (var collider in colliders)
            {
                if (collider == colliderToIgnore)
                {
                    continue;
                }
                
                var hitDistance = collider.Intersect(origin, rayDirection);

                if (hitDistance >= 0.0f && hitDistance < nearestDistance)
                {
                    nearestCollider = collider;
                    nearestDistance = hitDistance;
                }
            }

            var nearestPt = (nearestCollider == null) ? Vector3.zero : (origin + (rayDirection * (float)nearestDistance));
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

                var transparentColor = Color.black;

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
                        var opacity = nonLight.Material.Opacity;
                        if (opacity < 1.0f && transparentColor.a >= 0f)
                        {
                            var nonLightColor = nonLight.Material.Color;
                            transparentColor.r = Math.Max(transparentColor.r, nonLightColor.r * opacity);
                            transparentColor.g = Math.Max(transparentColor.g, nonLightColor.g * opacity);
                            transparentColor.b = Math.Max(transparentColor.b, nonLightColor.b * opacity);
                            transparentColor.a = Math.Max(transparentColor.a - opacity, 0f);
                        }
                        else
                        {
                            lightHitsThisPoint = false;
                            break;
                        }
                    }
                }

                if (lightHitsThisPoint)
                {
                    var lightColor = light.Material.Color;
                    
                    if (transparentColor.a < 1f && transparentColor.a > 0f)
                    {
                        var lightPercentageThatHits = transparentColor.a;
                        
                        lightColor.r = Math.Max(lightColor.r * lightPercentageThatHits, transparentColor.r);
                        lightColor.g = Math.Max(lightColor.g * lightPercentageThatHits, transparentColor.g);
                        lightColor.b = Math.Max(lightColor.b * lightPercentageThatHits, transparentColor.b);
                    }
                    
                    var opacity = materialHit.Opacity;
                    
                    // Handle specularity
                    {
                        var reflect = Vector3.Reflect(ptToLight, ptNormal);
                        var viewDot = Vector3.Dot(cameraRayDirection, reflect);

                        if (viewDot > 0.0f)
                        {
                            var roughness = materialHit.Roughness;
                            var pow = (float) Math.Pow(viewDot, Math.Max(64 * (1 - roughness), 1));
                            var specAmount = Math.Max(pow, 0);
                            if (specAmount > 0.005f)
                            {
                                var specMods = specAmount * (1 - roughness) * Math.Min(opacity * 2, 1f);
                                if (specMods > 0.005f)
                                {
                                    var lightSpecColor = lightColor * specMods;

                                    specColor.r = Math.Max(lightSpecColor.r, specColor.r);
                                    specColor.g = Math.Max(lightSpecColor.g, specColor.g);
                                    specColor.b = Math.Max(lightSpecColor.b, specColor.b);
                                }
                            }
                        }
                    }

                    // Diffuse lighting
                    {
                        var rayNormalDot = Vector3.Dot(ptToLight, ptNormal);
                        var lightPercentageThatHits = rayNormalDot * opacity;
                        if (lightPercentageThatHits > 0.005f)
                        {
                            var lightDiffuseColor = lightColor * lightPercentageThatHits;

                            diffuseColor.r = Math.Max(lightDiffuseColor.r, diffuseColor.r);
                            diffuseColor.g = Math.Max(lightDiffuseColor.g, diffuseColor.g);
                            diffuseColor.b = Math.Max(lightDiffuseColor.b, diffuseColor.b);
                        }
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