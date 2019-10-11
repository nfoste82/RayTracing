using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Collider = Shapes.Collider;

namespace RayTracer
{
    public class Tracer
    {
        private GameManager _gameManager;
        
        public Tracer(Texture2D texture, GameManager gameManager)
        {
            _gameManager = gameManager;
            _windowSize = new Vector2Int(texture.width, texture.height);
            _pixelColors = new Color[texture.width * texture.height];
            _rays = new Ray[texture.width,texture.height];

            for (var y = 0; y < texture.height; ++y)
            {
                for (var x = 0; x < texture.width; ++x)
                {
                    _rays[x, y] = new Ray();
                }
            }
        }
        
        public void ProcessRenderTexture(Texture2D texture, Scene scene, Camera camera)
        {
            var textureWidth = texture.width;
            var textureHeight = texture.height;
            var cameraOrigin = camera.transform.position;
            var cameraNearPlane = camera.nearClipPlane;

            for (var y = 0; y < textureHeight; ++y)
            {
                for (var x = 0; x < textureWidth; ++x)
                {
                    var ray = _rays[x, y];
                    ray.Origin = camera.ScreenToWorldPoint(new Vector3(x, y, cameraNearPlane));
                    ray.Direction = ray.Origin - cameraOrigin;
                    ray.Direction.Normalize();
                    ray.Color = Color.black;
                    ray.Energy = 1.0f;
                    
                    CastRayFromScreen(ray, scene, x, y, textureHeight);

                    _pixelColors[y * _windowSize.x + x] = ray.Color;
                }
            }

            texture.SetPixels(_pixelColors);
            texture.Apply();
        }

        private void CastRayFromScreen(Ray ray, Scene scene, int x, int y, int maxY)
        {
            var depth = 0;
            Collider previousCollider = null;

            while (true)
            {
                double nearestPt = float.MaxValue;

                var intersection = GetNearestIntersection(ray, scene.Spheres, previousCollider, ref nearestPt);

                if (intersection.Collider == null)
                {
                    var skyColorAtX = new Color(
                        Mathf.Lerp(_skyColor.r, _skyDarkColor.r, y / (float)maxY),
                        Mathf.Lerp(_skyColor.g, _skyDarkColor.g, y / (float)maxY),
                        Mathf.Lerp(_skyColor.b, _skyDarkColor.b, y / (float)maxY),
                        1f
                    );
                    
                    ray.Color = ColorExtensions.Combine(ray.Color, skyColorAtX * ray.Energy);
                    break;
                }
                
                var (diffuse, specular) = LightColorAffectsHitPoint(scene.Lights, scene.Spheres, intersection, ray);

                // Add in ambient lighting
                diffuse = ColorExtensions.Combine(diffuse, _skyDarkColor);

                Color matColor = intersection.Collider.Material.Color;
                if (intersection.Collider.Material.CheckeredTexture)
                {
                    var xEven = (int)intersection.Position.x % 6 == 0;
                    var zEven = (int)intersection.Position.z % 6 == 0;
                    if (xEven == zEven)
                    {
                        matColor = intersection.Collider.Material.Color * 0.5f;
                        matColor.a = 1.0f;
                    }
                }
                
                var diffuseAndSpec = matColor * diffuse + specular;
                diffuseAndSpec.a = 1.0f;

                ray.Color = ColorExtensions.Combine(ray.Color, diffuseAndSpec * ray.Energy);

                // Stop if we've bounced the max number of times
                if (depth >= _gameManager.Settings.numRayBounces)
                {
                    break;
                }
                
                // TODO: We should be able to spawn a new ray here and combine the results
                //       Because part of the ray could go through the transparent surface and
                //       part would reflect off of it.

                var opacity = intersection.Collider.Material.Opacity;
                if (opacity < 1.0f)
                {
                    // Refract through the transparent surface
                    intersection.Collider.Refract(ray, intersection);
                }
                else
                {
                    // Bounce of the opaque surface
                    ray.Origin = intersection.Position;
                    ray.Direction = Vector3.Reflect(ray.Direction, intersection.Normal);
                    ray.Energy -= 0.65f * intersection.Collider.Material.Roughness;
                }

                if (ray.Energy <= 0.005f)
                {
                    break;
                }
                
                ++depth;
            }
        }

        private static Intersection GetNearestIntersection(
            Ray ray,
            List<Collider> colliders,
            Collider colliderToIgnore,
            ref double nearestDistance)
        {
            Collider nearestCollider = null;

            foreach (var collider in colliders)
            {
                if (collider == colliderToIgnore)
                {
                    continue;
                }
                
                var hitDistance = collider.Intersect(ray.Origin, ray.Direction);

                if (hitDistance >= 0.0f && hitDistance < nearestDistance)
                {
                    nearestCollider = collider;
                    nearestDistance = hitDistance;
                }
            }

            var nearestPt = (nearestCollider == null) ? Vector3.zero : ray.GetPoint(nearestDistance);
            var normal = (nearestCollider == null) ? Vector3.zero : nearestCollider.GetNormalAtPoint(nearestPt);

            return new Intersection
            {
                Position = nearestPt,
                Normal = normal,
                Collider = nearestCollider
            };
        }

        private static (Color diffuse, Color specular) LightColorAffectsHitPoint(
            List<Collider> lights, 
            List<Collider> nonLights, 
            Intersection intersection,
            Ray ray)
        {
            // TODO: If lights have a width then you have to cast to the sides of the light to see if any of them hit

            Color diffuseColor = Color.black;
            Color specColor = Color.black;
            
            foreach (var light in lights)
            {
                var ptToLight = light.Position - intersection.Position;

                // If the point is facing away from the light then we can skip it
                if (Vector3.Dot(ptToLight, intersection.Normal) < 0.0f)
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
                    
                    var hitDistance = nonLight.Intersect(intersection.Position, ptToLight);

                    if (hitDistance >= 0.0f && hitDistance < lightHitDistance)
                    {
                        var opacity = nonLight.Material.Opacity;
                        if (opacity < 1.0f && transparentColor.a >= 0f)
                        {
                            var nonLightColor = nonLight.Material.Color;

                            transparentColor = ColorExtensions.Combine(transparentColor, nonLightColor);
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

                        lightColor = ColorExtensions.Combine(lightColor * lightPercentageThatHits, transparentColor);
                    }
                    
                    var opacity = intersection.Collider.Material.Opacity;
                    
                    // Handle specularity
                    {
                        var reflect = Vector3.Reflect(ptToLight, intersection.Normal);
                        var viewDot = Vector3.Dot(ray.Direction, reflect);

                        if (viewDot > 0.0f)
                        {
                            var roughness = intersection.Collider.Material.Roughness;
                            var pow = (float) Math.Pow(viewDot, Math.Max(64 * (1 - roughness), 1));
                            var specAmount = Math.Max(pow, 0);
                            if (specAmount > 0.005f)
                            {
                                var specMods = specAmount * (1 - roughness) * Math.Min(opacity * 2, 1f);
                                if (specMods > 0.005f)
                                {
                                    var lightSpecColor = lightColor * specMods;
                                    
                                    specColor = ColorExtensions.Combine(lightSpecColor, specColor);
                                }
                            }
                        }
                    }

                    // Diffuse lighting
                    {
                        var rayNormalDot = Vector3.Dot(ptToLight, intersection.Normal);
                        var lightPercentageThatHits = rayNormalDot * opacity;
                        if (lightPercentageThatHits > 0.005f)
                        {
                            var lightDiffuseColor = lightColor * lightPercentageThatHits;

                            diffuseColor = ColorExtensions.Combine(lightDiffuseColor, diffuseColor);
                        }
                    }
                }
            }

            return (diffuseColor, specColor);
        }

        private readonly Color _skyColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
        private readonly Color _skyDarkColor = new Color(0.1f, 0.2f, 0.25f, 1.0f);

        private readonly Color[] _pixelColors;
        private readonly Ray[,] _rays;

        private Vector2Int _windowSize;
    }
}