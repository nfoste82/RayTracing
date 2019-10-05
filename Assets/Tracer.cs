using Shapes;
using UnityEngine;

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

                    (Sphere sphere, Vector3 intersectionPt) nearestIntersection = default;
                    
                    foreach (var sphere in scene.Spheres)
                    {
                        var (intersected, hitDistance) = Sphere.Intersect(pixelPoint, sphere, rayDirection, null);

                        if (intersected && hitDistance < nearestPt)
                        {
                            nearestIntersection = (sphere, (pixelPoint + (rayDirection * (float)hitDistance)));
                            nearestPt = hitDistance;
                        }
                    }

                    if (!nearestIntersection.Equals(default))
                    {
                        texture.SetPixel(x, y, nearestIntersection.sphere.Material.Color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, skyColor);
                    }
                }
            }
            
            texture.Apply();
        }
    }
}