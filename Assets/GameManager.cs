using RayTracer;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Texture2D _mainTexture;
    private Tracer _tracer;
    private Scene _scene;
    
    private void Start()
    {
        var width = 250;
        var height = 250;
        _mainTexture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Point,
        };
        
        var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        var sprite = Sprite.Create(
            _mainTexture,
            new Rect(0.0f, 0.0f, _mainTexture.width, _mainTexture.height),
            new Vector2(0.5f, 0.5f), 1.0f);

        spriteRenderer.sprite = sprite;

        _scene = new Scene();
        _scene.CreateTestScene();

        _tracer = new Tracer();
    }

    private void Update()
    {
        _tracer.ProcessRenderTexture(_mainTexture, _scene, Camera.main);
    }
}
