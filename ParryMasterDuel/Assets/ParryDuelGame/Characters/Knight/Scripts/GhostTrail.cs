using System.Collections;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [Header("Sprite Renderers")]
    public SpriteRenderer[] spriteRenderers;

    [Header("Trail Settings")]
    public float spawnInterval = 0.05f;
    public float ghostLifetime = 0.3f;

    [Header("Color")]
    public Color ghostColor = new Color(1f, 1f, 1f, 0.5f);
    public Color fadeToColor = new Color(1f, 1f, 1f, 0f);
    public bool fadeOut = true;

    [Header("Scale")]
    public bool matchScale = true;

    private Coroutine trailCoroutine;

    public void StartTrail()
    {
        if (trailCoroutine != null)
            StopCoroutine(trailCoroutine);
        trailCoroutine = StartCoroutine(SpawnGhosts());
    }

    public void StopTrail()
    {
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }
    }

    IEnumerator SpawnGhosts()
    {
        while (true)
        {
            SpawnGhost();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnGhost()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) return;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr == null || sr.sprite == null) continue;

            GameObject ghost = new GameObject("GhostPart");
            ghost.transform.position = sr.transform.position;
            ghost.transform.rotation = sr.transform.rotation;

            if (matchScale)
                ghost.transform.localScale = sr.transform.lossyScale;

            SpriteRenderer ghostSR = ghost.AddComponent<SpriteRenderer>();
            ghostSR.sprite = sr.sprite;
            ghostSR.sortingLayerName = sr.sortingLayerName;
            ghostSR.sortingOrder = sr.sortingOrder - 1;
            ghostSR.color = ghostColor;
            ghostSR.flipX = sr.flipX;
            ghostSR.flipY = sr.flipY;

            StartCoroutine(FadeGhost(ghost, ghostSR));
        }
    }

    IEnumerator FadeGhost(GameObject ghost, SpriteRenderer ghostSR)
    {
        float elapsed = 0f;

        while (elapsed < ghostLifetime)
        {
            elapsed += Time.deltaTime;
            if (fadeOut && ghostSR != null)
                ghostSR.color = Color.Lerp(ghostColor, fadeToColor, elapsed / ghostLifetime);
            yield return null;
        }

        Destroy(ghost);
    }
}