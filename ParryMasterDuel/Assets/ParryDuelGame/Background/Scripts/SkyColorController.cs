using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))] // Ensures we always have a SpriteRenderer
public class SkyColorController : MonoBehaviour
{
    [Header("Gradient Colors")]
    public Color topColor = Color.blue;
    public Color middleColor = Color.cyan;
    public Color bottomColor = Color.white;

    // --- IMPORTANT: Double-check these names in your Shader Graph's "Reference" field ---
    private static readonly int TopColorID = Shader.PropertyToID("_TopColor");
    private static readonly int MiddleColorID = Shader.PropertyToID("_MiddleColor");
    private static readonly int BottomColorID = Shader.PropertyToID("_BottomColor");

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        // Get the components we need
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        // OnEnable is a great place to apply properties, as it runs every time the object is activated.
        ApplyColors();
    }

    void ApplyColors()
    {
        if (spriteRenderer == null || propertyBlock == null)
        {
            // If something went wrong in Awake, re-initialize.
            Awake();
        }

        // 1. Get the current properties from the renderer to not overwrite others.
        spriteRenderer.GetPropertyBlock(propertyBlock);

        // 2. Set the color values on our property block.
        propertyBlock.SetColor(TopColorID, topColor);
        propertyBlock.SetColor(MiddleColorID, middleColor);
        propertyBlock.SetColor(BottomColorID, bottomColor);

        // 3. Apply the modified block back to the renderer.
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    // This is useful for seeing changes live in the Editor without pressing Play.
    void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        // We need a temporary property block for editor updates.
        MaterialPropertyBlock tempBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(tempBlock);
        tempBlock.SetColor(TopColorID, topColor);
        tempBlock.SetColor(MiddleColorID, middleColor);
        tempBlock.SetColor(BottomColorID, bottomColor);
        spriteRenderer.SetPropertyBlock(tempBlock);
    }
}
