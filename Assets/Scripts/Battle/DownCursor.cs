using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DownCursor : MonoBehaviour
{
    [SerializeField] private float blinkInterval = 0.5f; // Time between blinks
    private Image image;
    private Coroutine blinkCoroutine;

    void Awake()
    {
        image = GetComponent<Image>(); // Get Image component
    }

    public void StartBlinking()
    {
        if (blinkCoroutine == null)
            blinkCoroutine = StartCoroutine(BlinkCursor());
    }

    public void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            image.enabled = true; // Ensure it's visible when stopping
        }
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            image.enabled = !image.enabled; // Toggle visibility
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
