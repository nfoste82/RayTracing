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

            for (var x = 0; x < textureWidth; ++x)
            {
                for (var y = 0; y < textureHeight; ++y)
                {
                    var pixelPoint = camera.ScreenToWorldPoint(new Vector3(x, y, camera.nearClipPlane));
                    var rayDirection = pixelPoint - camera.transform.position;
                    
                    float nearestPt = float.MaxValue;
                    foreach (var sphere in scene.Spheres)
                    {
                        var intersect = sphere.Intersect(pixelPoint, rayDirection, null);
                        if (intersect.intersected)
                        {
                            texture.SetPixel(x, y, Color.white);
                        }
                        else
                        {
                            texture.SetPixel(x, y, Color.black);
                        }
                    }
                }
            }
            
            texture.Apply();
        }
    }
}