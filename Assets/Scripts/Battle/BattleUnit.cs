using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Unity.VisualScripting;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }

    [SerializeField] Image spriteImage;
    [SerializeField] Image scratchAnimationImage;
    [SerializeField] Image sendOutEffectImage;
    [SerializeField] Sprite[] scratchAnimationFrames;
    [SerializeField] Sprite[] slashAnimationFrames;
    [SerializeField] Sprite[] cutAnimationFrames;
    [SerializeField] Sprite[] furyswipesAnimationFrames;
    [SerializeField] Sprite[] poisonStingAnimationFrames;
    [SerializeField] Sprite[] stompAnimationFrames;
    [SerializeField] private Sprite[] sendOutFrames; // Assign Poké Ball effect frames
    [SerializeField] private float frameDuration = 0.1f; // Time per frame
    private Sprite originalSprite; // To store Pokémon's original sprite
    private SpriteRenderer spriteRenderer;

    [SerializeField] float blinkDuration = 1.0f;
    [SerializeField] float blinkInterval = 0.1f;
    [SerializeField] private Transform shakeTargetHorizontal;
    [SerializeField] private Transform shakeTargetVertical;
    public float shakeAmount = 0.1f;
    public float shakeDuration = 0.2f;
    public Camera mainCamera;

    [SerializeField] Animator animator;

    Vector3 originalPos;

    private void Awake()
    {
        originalPos = spriteImage.transform.localPosition;
        scratchAnimationImage.gameObject.SetActive(false);
        sendOutEffectImage.gameObject.SetActive(false);
    }

    public void Setup()
    {
        Pokemon = new Pokemon(_base, level);

        if (isPlayerUnit)
            spriteImage.sprite = Pokemon.Base.BackSprite;
        else
            spriteImage.sprite = Pokemon.Base.FrontSprite;

        PlayEnterAnimation();
    }
    public void PlaySendOutAnimation()
    {
        if (!isPlayerUnit) return;

        originalSprite = spriteImage.sprite;
        spriteImage.gameObject.SetActive(true); // Ensure the Image is enabled
        transform.localScale = Vector3.zero;
        StartCoroutine(PlaySendOutEffect());
    }

    private IEnumerator PlaySendOutEffect()
    {
        sendOutEffectImage.gameObject.SetActive(true);
        for (int i = 0; i < sendOutFrames.Length; i++)
        {
            sendOutEffectImage.sprite = sendOutFrames[i];
            if (i == sendOutFrames.Length - 1)
            {
                ApplyPaletteShader(sendOutEffectImage);
            }
            yield return new WaitForSeconds(frameDuration);
        }
        sendOutEffectImage.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f); 
        transform.localScale = Vector3.one;
        spriteImage.gameObject.SetActive(true);

        if (Pokemon.Base.SendOutStages != null && Pokemon.Base.SendOutStages.Length > 0)
        {
            for (int i = 0; i < Pokemon.Base.SendOutStages.Length; i++)
            {
                // Move down instantly before showing the 2nd frame (for all Pokémon)
                if (i == 1)
                {
                    transform.localPosition = new Vector3(originalPos.x, -25f, originalPos.z);
                }

                // Special case for Mewtwo (3rd & 4th frame also at -25)
                if (Pokemon.Base.Name == "MEWTWO" && (i == 2 || i == 3))
                {
                    transform.localPosition = new Vector3(originalPos.x, -25f, originalPos.z);
                }

                else if (Pokemon.Base.Name != "MEWTWO" && i == 2)
                {
                    transform.localPosition = originalPos;
                }

                // Show the current stage
                spriteImage.sprite = Pokemon.Base.SendOutStages[i];
                yield return new WaitForSeconds(0.1f);
            }

            // Reset position **AFTER** the 4th frame for Mewtwo
            if (Pokemon.Base.Name == "MEWTWO")
            {
                yield return new WaitForSeconds(0.01f); // Small delay for smoothness
                transform.localPosition = originalPos;
            }
        }


        spriteImage.sprite = originalSprite;
        spriteImage.gameObject.SetActive(true);
    }

    private void ApplyPaletteShader(Image image)
    {
        Material paletteMat = Resources.Load<Material>("PaletteSwapMat");

        if (paletteMat != null && Pokemon.Base.PaletteTexture != null)
        {
            image.material = Instantiate(paletteMat); // Avoid modifying the original material
            image.material.SetTexture("_PaletteTex", Pokemon.Base.PaletteTexture);
        }
        else
        {
            Debug.LogError("Palette material or Pokémon palette texture is missing!");
        }
    }

    public void PlayEnterAnimation(Action onEnter = null)
    {
        if (isPlayerUnit)
            spriteImage.transform.localPosition = new Vector3(117f, originalPos.y);
        else
            spriteImage.transform.localPosition = new Vector3(-117f, originalPos.y);

        spriteImage.transform.DOLocalMoveX(originalPos.x, 2f)
            .OnComplete(() => onEnter?.Invoke()); // Call the callback when animation finishes
    }


    public void PlayScratchAnimation(Action onComplete)
    {
        StartCoroutine(PlayScratchAnimationCoroutine(onComplete));
    }

    private IEnumerator PlayScratchAnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in scratchAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(BlinkCoroutine());

        onComplete?.Invoke();
    }

    public void PlayScratch2Animation(Action onComplete)
    {
        StartCoroutine(PlayScratch2AnimationCoroutine(onComplete));
    }

    private IEnumerator PlayScratch2AnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in scratchAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreenVertically());

        onComplete?.Invoke();
    }

    public void PlaySlashAnimation(Action onComplete)
    {
        StartCoroutine(PlaySlashAnimationCoroutine(onComplete));
    }

    private IEnumerator PlaySlashAnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in slashAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(BlinkCoroutine());

        onComplete?.Invoke();
    }

    public void PlaySlash2Animation(Action onComplete)
    {
        StartCoroutine(PlaySlash2AnimationCoroutine(onComplete));
    }

    private IEnumerator PlaySlash2AnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in slashAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreenVertically());

        onComplete?.Invoke();
    }

    public void PlayFurySwipesAnimation(Action onComplete)
    {
        StartCoroutine(PlayFurySwipesAnimationCoroutine(onComplete));
    }

    private IEnumerator PlayFurySwipesAnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in furyswipesAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreen(0.2f));

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreen(0.2f));

        onComplete?.Invoke();
    }

    public void PlayFurySwipes2Animation(Action onComplete)
    {
        StartCoroutine(PlayFurySwipes2AnimationCoroutine(onComplete));
    }

    private IEnumerator PlayFurySwipes2AnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in furyswipesAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreenHorizontally());

        onComplete?.Invoke();
    }

    public void PlayPoisonStingAnimation(Action onComplete)
    {
        StartCoroutine(PlayPoisonStingAnimationCoroutine(onComplete));
    }

    private IEnumerator PlayPoisonStingAnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in poisonStingAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreen(0.2f));

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreen(0.2f));

        onComplete?.Invoke();
    }

    public void PlayPoisonSting2Animation(Action onComplete)
    {
        StartCoroutine(PlayPoisonSting2AnimationCoroutine(onComplete));
    }

    private IEnumerator PlayPoisonSting2AnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in poisonStingAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreenHorizontally());

        onComplete?.Invoke();
    }

    public void PlayStompAnimation(Action onComplete)
    {
        StartCoroutine(PlayStompAnimationCoroutine(onComplete));
    }

    private IEnumerator PlayStompAnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in stompAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreen(0.2f));

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreen(0.2f));

        onComplete?.Invoke();
    }

    public void PlayStomp2Animation(Action onComplete)
    {
        StartCoroutine(PlayStomp2AnimationCoroutine(onComplete));
    }

    private IEnumerator PlayStomp2AnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in stompAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreenHorizontally());

        onComplete?.Invoke();
    }

    public void PlayCutAnimation(Action onComplete)
    {
        StartCoroutine(PlayCutAnimationCoroutine(onComplete));
    }

    private IEnumerator PlayCutAnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in cutAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(BlinkCoroutine());

        onComplete?.Invoke();
    }

    public void PlayCut2Animation(Action onComplete)
    {
        StartCoroutine(PlayCut2AnimationCoroutine(onComplete));
    }

    private IEnumerator PlayCut2AnimationCoroutine(Action onComplete)
    {
        scratchAnimationImage.gameObject.SetActive(true);

        foreach (var frame in cutAnimationFrames)
        {
            scratchAnimationImage.sprite = frame;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        scratchAnimationImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShakeScreenVertically());

        onComplete?.Invoke();
    }

    public void PlayTackleAnimation(BattleUnit targetUnit, Action onComplete)
    {
        StartCoroutine(PlayTackleAnimationCoroutine(targetUnit, onComplete));
    }

    public IEnumerator PlayTackleAnimationCoroutine(BattleUnit targetUnit, Action onComplete)
    {
        Vector3 originalPos = spriteImage.transform.localPosition;
        Vector3 targetPos = originalPos + (isPlayerUnit ? new Vector3(5f, 0f, 0f) : new Vector3(-5f, 0f, 0f));

        yield return spriteImage.transform.DOLocalMove(targetPos, 0.15f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return spriteImage.transform.DOLocalMove(originalPos, 0.15f).SetEase(Ease.InQuad).WaitForCompletion();

        // Make the opponent blink instead of the attacker
        yield return targetUnit.StartCoroutine(targetUnit.BlinkCoroutine());

        onComplete?.Invoke();
    }

    public void PlayTackle2Animation(Action onComplete)
    {
        StartCoroutine(PlayTackle2AnimationCoroutine(onComplete));
    }

    public IEnumerator PlayTackle2AnimationCoroutine(Action onComplete)
    {
        Vector3 originalPos = spriteImage.transform.localPosition;
        Vector3 targetPos = originalPos + (isPlayerUnit ? new Vector3(5f, 0f, 0f) : new Vector3(-5f, 0f, 0f));

        yield return spriteImage.transform.DOLocalMove(targetPos, 0.15f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return spriteImage.transform.DOLocalMove(originalPos, 0.15f).SetEase(Ease.InQuad).WaitForCompletion();

        yield return StartCoroutine(ShakeScreenVertically());

        onComplete?.Invoke();
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(spriteImage.transform.DOLocalMoveY(originalPos.y - 64f, 0.5f));
    }

    public void PlayBlinkEffect()
    {
        StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        if (animator != null)
        {
            animator.enabled = false;
        }

        int blinkCount = 11;
        float blinkDuration = 0.1f;

        for (int i = 0; i < blinkCount; i++)
        {
            spriteImage.enabled = !spriteImage.enabled;
            yield return new WaitForSeconds(blinkDuration);
        }

        spriteImage.enabled = true;

        if (animator != null)
        {
            animator.enabled = true;
        }
    }

    private IEnumerator ShakeScreen(float magnitude)
    {
        Vector3 originalPosition = shakeTargetHorizontal.position;
        Vector3 cameraOriginalPosition = mainCamera.transform.position;

        float elapsed = 0f;
        float adjustedShakeAmount = shakeAmount * magnitude;

        while (elapsed < shakeDuration)
        {
            float shakeOffset = (elapsed % 0.1f < 0.05f) ? adjustedShakeAmount : -adjustedShakeAmount;
            shakeTargetHorizontal.position = new Vector3(originalPosition.x + shakeOffset, originalPosition.y, originalPosition.z);
            mainCamera.transform.position = cameraOriginalPosition;

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeTargetHorizontal.position = originalPosition;
    }

    private IEnumerator ShakeScreenVertically()
    {
        Vector3 originalPosition = shakeTargetVertical.position;
        Vector3 cameraOriginalPosition = mainCamera.transform.position;

        float elapsed = 0f;
        float shakeDuration = 1.43f;
        float initialShakeAmount = 0.37f;
        int shakeCount = 9;

        for (int i = 0; i < shakeCount; i++)
        {
            float currentShakeAmount = Mathf.Lerp(initialShakeAmount, 0f, (float)i / (shakeCount - 1));

            // Shake downward
            shakeTargetVertical.position = new Vector3(originalPosition.x, originalPosition.y - currentShakeAmount, originalPosition.z);
            mainCamera.transform.position = cameraOriginalPosition;
            yield return new WaitForSeconds(shakeDuration / (shakeCount * 2));  // Wait before returning

            // Return to original position
            shakeTargetVertical.position = originalPosition;
            yield return new WaitForSeconds(shakeDuration / (shakeCount * 2));  // Wait before next shake
        }
    }

    private IEnumerator ShakeScreenHorizontally()
    {
        Vector3 originalPosition = shakeTargetHorizontal.position; // Store the original position of the target
        Vector3 cameraOriginalPosition = mainCamera.transform.position; // Store the original camera position

        float elapsed = 0f;
        float shakeDuration = 1.43f;  // Total duration for all shakes
        float initialShakeAmount = 0.37f;  // Initial shake amount
        int shakeCount = 9;  // Number of shakes
        float shakeInterval = shakeDuration / shakeCount; // Interval per shake

        for (int i = 0; i < shakeCount; i++)
        {
            float currentShakeAmount = Mathf.Lerp(initialShakeAmount, 0f, (float)i / (shakeCount - 1)); // Decrease shake magnitude each time
            float shakeElapsed = 0f;

            while (shakeElapsed < shakeInterval)
            {
                float currentShake = Mathf.Lerp(0, currentShakeAmount, shakeElapsed / shakeInterval);
                shakeTargetHorizontal.position = new Vector3(originalPosition.x + currentShake, originalPosition.y, originalPosition.z);
                mainCamera.transform.position = cameraOriginalPosition;  // Keep camera stable

                shakeElapsed += Time.deltaTime;
                yield return null; // Wait until the next frame
            }

            // Return to original position
            shakeTargetHorizontal.position = originalPosition;

            // Wait between shakes
            yield return new WaitForSeconds(shakeInterval);
        }
    }
}