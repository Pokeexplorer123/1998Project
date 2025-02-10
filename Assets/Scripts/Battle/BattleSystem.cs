using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] Image redTrainer;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] DownCursor downcursor;
    [SerializeField] Image dbox;

    public AudioSource wildintroSource;
    public AudioSource wildloopSource;
    public AudioSource CrySource;
    public AudioClip wildintroClip;
    public AudioClip wildloopClip;
    public AudioClip wildvictoryintroClip;
    public AudioClip wildvictoryloopClip;
    public AudioClip scratchClip;
    public AudioClip SuperEffectiveClip;
    public AudioClip NotVeryEffectiveClip;
    private Camera battleCamera;
    public Material trainerMaterial;
    private Material enemyMaterial;
    private Material playerMaterial;
    public Image battlemask;
    public Image battlemask2;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    private void Start()
    {
        StartCoroutine(SetupBattle());
        PlayBattleMusic();
        Image playerImage = redTrainer.GetComponent<Image>();
        Image enemyImage = enemyUnit.GetComponent<Image>();
        playerMaterial = new Material(trainerMaterial); // Clone the material
        enemyMaterial = new Material(trainerMaterial);
        playerImage.material = playerMaterial;
        enemyImage.material = enemyMaterial;
        playerMaterial.SetFloat("_SwapProgress", 1f);
        enemyMaterial.SetFloat("_SwapProgress", 1f);
        battlemask.gameObject.SetActive(false);
        battlemask2.gameObject.SetActive(false);
    }

    void PlayBattleMusic()
    {
        double startTime = AudioSettings.dspTime; // Get precise audio time

        // Play the intro sound
        wildintroSource.clip = wildintroClip;
        wildintroSource.PlayScheduled(startTime);

        // Schedule the looping sound to start exactly when the intro ends
        wildloopSource.clip = wildloopClip;
        wildloopSource.loop = true;
        wildloopSource.PlayScheduled(startTime + wildintroClip.length);
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        enemyUnit.Setup();

        dialogBox.SetMovesName(playerUnit.Pokemon.Moves);

        redTrainer.gameObject.SetActive(true);
        playerUnit.gameObject.SetActive(false); // Player unit is initially hidden

        PlayTrainerEnterAnimation();

        bool enemyAnimationDone = false;
        enemyUnit.PlayEnterAnimation(() => enemyAnimationDone = true);

        yield return new WaitUntil(() => enemyAnimationDone);
        yield return new WaitForSeconds(0.032f);
        playerMaterial.SetFloat("_SwapProgress", 0f);
        enemyMaterial.SetFloat("_SwapProgress", 0.75f);
        yield return new WaitForSeconds(0.016f);
        enemyMaterial.SetFloat("_SwapProgress", 0f);

        yield return new WaitForSeconds(1.168f);

        CrySource.PlayOneShot(enemyUnit.Pokemon.Base.CryClip);
        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");
        downcursor.gameObject.SetActive(true);
        downcursor.StartBlinking();
        trainerMaterial.SetFloat("_SwapProgress", 0f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.A));
        enemyHud.gameObject.SetActive(true);
        enemyHud.SetData(enemyUnit.Pokemon);
        downcursor.gameObject.SetActive(false);
        downcursor.StopBlinking();
        PlayTrainerLeaveAnimation();
        yield return new WaitForSeconds(0.5f);
        yield return dialogBox.TypeDialog($"Go! {playerUnit.Pokemon.Base.Name}!");
        playerHud.gameObject.SetActive(true);
        playerHud.SetData(playerUnit.Pokemon);
        playerUnit.gameObject.SetActive(true);

        playerUnit.PlaySendOutAnimation(); // Now the coroutine can start

        yield return new WaitForSeconds(1.6f);
        dbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.12f);
        dbox.gameObject.SetActive(false);
        CrySource.PlayOneShot(playerUnit.Pokemon.Base.CryClip);
        PlayerAction();
    }

    void PlayTrainerEnterAnimation()
    {
        // Set the initial position of the trainer and activate it
        redTrainer.transform.localPosition = new Vector3(117f, redTrainer.transform.localPosition.y);
        redTrainer.gameObject.SetActive(true);

        // Animate the trainer's movement
        redTrainer.transform.DOLocalMoveX(-44f, 2f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Activate the first battlemask
                battlemask.gameObject.SetActive(true);
                // Deactivate battlemask after 1 frame (~0.016s)
                DOVirtual.DelayedCall(0.016f, () =>
                {
                    battlemask.gameObject.SetActive(false);

                    // Activate battlemask2 AFTER battlemask disappears
                    battlemask2.gameObject.SetActive(true);

                    // Deactivate battlemask2 after another 1 frame (~0.016s)
                    DOVirtual.DelayedCall(0.016f, () => battlemask2.gameObject.SetActive(false));
                });
            });
    }

    void PlayTrainerLeaveAnimation()
    {
        redTrainer.transform.localPosition = new Vector3(-44f, redTrainer.transform.localPosition.y); // Start off-screen left
        redTrainer.gameObject.SetActive(true);

        redTrainer.transform.DOLocalMoveX(-108.25f, 0.5f).SetEase(Ease.OutQuad); // Move to -117f
    }


    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog(""));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    public IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        if (move.Base.Name == "SCRATCH")
        {
            wildloopSource.PlayOneShot(scratchClip);
            yield return StartCoroutine(PlayAnimationWithCallback(enemyUnit.PlayScratchAnimation, enemyUnit));
        }
        else if (move.Base.Name == "SLASH")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(enemyUnit.PlaySlashAnimation, enemyUnit));
        }
        else if (move.Base.Name == "FURY SWIPES")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(enemyUnit.PlayFurySwipesAnimation, enemyUnit));
        }
        else if (move.Base.Name == "CUT")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(enemyUnit.PlayCutAnimation, enemyUnit));
        }
        else if (move.Base.Name == "TACKLE")
        {
                yield return StartCoroutine(PlayAnimationWithCallback((onComplete) => playerUnit.PlayTackleAnimation(enemyUnit, onComplete), enemyUnit));
        }
        else if (move.Base.Name == "POISON STING")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(enemyUnit.PlayPoisonStingAnimation, enemyUnit));
        }
        else if (move.Base.Name == "STOMP")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(enemyUnit.PlayStompAnimation, enemyUnit));
        }

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            StartCoroutine(PlayVictoryMusic());
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} fainted!");
            enemyUnit.PlayFaintAnimation();
            downcursor.gameObject.SetActive(true);
            downcursor.StartBlinking();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }

        IEnumerator PlayVictoryMusic()
        {
            wildintroSource.clip = wildvictoryintroClip;
            wildintroSource.Play();

            // Wait for the intro to finish
            yield return new WaitForSeconds(wildvictoryintroClip.length);

            // Play the looping victory theme
            wildloopSource.clip = wildvictoryloopClip;
            wildloopSource.loop = true;
            wildloopSource.Play();
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        if (move.Base.Name == "SCRATCH")
        {
            wildloopSource.PlayOneShot(scratchClip);
            yield return StartCoroutine(PlayAnimationWithCallback(playerUnit.PlayScratch2Animation, playerUnit));
        }
        else if (move.Base.Name == "SLASH")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(playerUnit.PlaySlash2Animation, playerUnit));
        }
        else if (move.Base.Name == "FURY SWIPES")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(playerUnit.PlayFurySwipes2Animation, playerUnit));
        }
        else if (move.Base.Name == "CUT")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(playerUnit.PlayCut2Animation, playerUnit));
        }
        else if (move.Base.Name == "TACKLE")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(enemyUnit.PlayTackle2Animation, playerUnit));
        }
        else if (move.Base.Name == "POISON STING")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(playerUnit.PlayPoisonSting2Animation, playerUnit));
        }
        else if (move.Base.Name == "STOMP")
        {
            yield return StartCoroutine(PlayAnimationWithCallback(playerUnit.PlayStomp2Animation, playerUnit));
        }

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} fainted!");
            playerUnit.PlayFaintAnimation();
            downcursor.gameObject.SetActive(true);
            downcursor.StartBlinking();
        }
        else
        {
            PlayerAction();
        }
    }

    private IEnumerator PlayAnimationWithCallback(Action<Action> animationMethod, BattleUnit targetUnit)
    {
        bool animationComplete = false;
        animationMethod(() => animationComplete = true);
        yield return new WaitUntil(() => animationComplete);
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        downcursor.gameObject.SetActive(true);
        downcursor.StartBlinking();

        if (damageDetails.Critical > 1f) 
    {
            yield return dialogBox.TypeDialog("A critical hit!");
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.A));
        }

        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.A));
        }

        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective. . .");
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.A));
        }
        downcursor.gameObject.SetActive(false);
        downcursor.StopBlinking();
    }

    private void Update()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionOnSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentAction == 0)
            {
                // Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                // Run
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.A))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}