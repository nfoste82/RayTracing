﻿using System;
using RayTracer;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] 
    public RayTracer.QualitySettings Settings;
    
    private Texture2D _mainTexture;
    private Tracer _tracer;
    private Scene _scene;
    
    private void Start()
    {
        var cameraHeight = Camera.main.orthographicSize * 2;
        var cameraSize = new Vector2(Camera.main.aspect * cameraHeight, cameraHeight);
        
        var width = Screen.currentResolution.width;
        var height = Screen.currentResolution.height;
        
        _mainTexture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Trilinear,
        };
        
        var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        var sprite = Sprite.Create(
            _mainTexture,
            new Rect(0.0f, 0.0f, _mainTexture.width, _mainTexture.height),
            new Vector2(0.5f, 0.5f), 
            1.0f);
        spriteRenderer.sprite = sprite;

        var spriteSize = spriteRenderer.sprite.bounds.size;
        
        var scale = transform.localScale;
        if (cameraSize.x >= cameraSize.y) { // Landscape (or equal)
            scale *= cameraSize.x / spriteSize.x;
        } else { // Portrait
            scale *= cameraSize.y / spriteSize.y;
        }
        
        transform.localScale = scale;

        spriteRenderer.sprite = sprite;

        _scene = new Scene();
        _scene.CreateTestScene();

        _tracer = new Tracer(_mainTexture, this);
        
        //_shader = Shader.Find("Standard");
    }

    private void Update()
    {
        var colliders = _scene.AllColliders;

        // TEMP: Hack to make colliders move
        for (int i = 0; i < colliders.Count; ++i)
        {
            var collider = colliders[i];

            if (collider.GetBoundingRadius() > 1000f)
            {
                continue;
            }

            collider.Position.y += Mathf.Sin(Time.time + i) * Time.deltaTime;
        }
        
        _tracer.ProcessRenderTexture(_mainTexture, _scene, Camera.main);
    }

//    private void OnRenderImage(RenderTexture src, RenderTexture dest)
//    {
//        if (_material == null)
//        {
//            _material = new Material(_shader);
//        }
//        
//        Graphics.Blit(src, dest, _material);
//    }
//
//    public Shader _shader;
//    private Material _material;
}
