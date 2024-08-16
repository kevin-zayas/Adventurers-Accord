using System.Collections;
using TMPro;
using UnityEngine;

public class FlashingEffect : MonoBehaviour
{
    public TMP_Text text;
    public float startDuration = 1.5f; // Total duration of the flashing effect
    public float speed = 12f; // Speed of the fade-in and fade-out effect

    public void FlashEffect()
    {
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        Color originalColor = text.color;
        originalColor.a = 1; // Ensure the original color is fully opaque

        // Create a transparent version of the original color
        Color transparentColor = new(originalColor.r, originalColor.g, originalColor.b, 0);

        float duration = startDuration;
        while (duration > 0)
        {
            text.color = Lerp(originalColor, transparentColor, speed);
            duration -= Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Reset to the original color after flashing
        text.color = originalColor;
    }

    private Color Lerp(Color start, Color end, float speed)
    {
        // Mathf.Sin(Time.time * speed) creates a smooth oscillation between 0 and 1
        return Color.Lerp(start, end, Mathf.Sin(Time.time * speed));
    }
}
