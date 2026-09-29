using BeatTiming;
using UnityEngine;

// Small visual cues on the player: one eye on the side it's facing,
// and a gold tint while a dash is charged (first press landed, waiting for the next beat)
[RequireComponent(typeof(PlayerMovement))]
public class PlayerLook : MonoBehaviour
{
    public Color eyeColor = new Color(0.1f, 0.1f, 0.12f);
    public Vector2 eyeSize = new Vector2(0.16f, 0.26f);
    // Eye position from the player's center; x flips with the facing direction
    public Vector2 eyeOffset = new Vector2(0.22f, 0.5f);
    public Color chargedColor = new Color(1f, 0.8f, 0.25f);

    private PlayerMovement movement;
    private PlayerDash dash;
    private SpriteRenderer body;
    private Transform eye;
    private Color normalColor;


    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        dash = GetComponent<PlayerDash>();
        body = GetComponent<SpriteRenderer>();
        if (body != null) normalColor = body.color;

        var eyeObject = new GameObject("Eye");
        eye = eyeObject.transform;
        eye.SetParent(transform, false);
        eye.localScale = new Vector3(eyeSize.x, eyeSize.y, 1f);
        SpriteRenderer eyeRenderer = eyeObject.AddComponent<SpriteRenderer>();
        eyeRenderer.sprite = ProceduralSprites.Square;
        eyeRenderer.color = eyeColor;
        eyeRenderer.sortingOrder = body != null ? body.sortingOrder + 1 : 1;
    }

    void LateUpdate()
    {
        eye.localPosition = new Vector3(eyeOffset.x * movement.FacingDirection, eyeOffset.y, 0f);

        if (body != null) body.color = dash != null && dash.IsCharged ? chargedColor : normalColor;
    }
}
