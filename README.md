# RayTracing
3D CPU-Raycasting demo using Unity and C#. Single-threaded. Runs in real time but is slow at any decent resolution. 
If you want a good framerate you'll need to be around 150x150 resolution or lower.

Currently scene data is just in a file called Scene.cs. It might be nice in the future to have the spheres and lights come
from the Unity scene so they could be moved around while the application is running.

This was my first raytracer written just for fun, and because it's meant to run somewhat in real time instead of 
just rendering a single high quality image, it's missing a lot of features that you'd find in higher-end renderers
like:
* Soft shadows
* Indirect lighting
* Anti-aliasing
* Caustics
* Glossy specularity

![SampleScene](https://imgur.com/eypzVFr.png)
