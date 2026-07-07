using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private UnityEngine.UI.Image keyImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 2f;

    [SerializeField] private TutorialStep[] steps;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private VehicleController vehicleController;

    private TutorialStep currentStep;
    private Queue<string> stepQueue = new Queue<string>();

    private bool jumpDone = false;
    private bool winchPulled = false;
    private bool winchExtended = false;
    private bool stepCompletedFlag = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        HidePanelInstant();

        foreach (var step in steps)
        {
            switch (step.stepId)
            {
                case "move_forward":
                    step.completionCondition = () => vehicleController.driverInput.y > 0.1f;
                    break;
                case "steer":
                    step.completionCondition = () => Mathf.Abs(vehicleController.driverInput.x) > 0.3f;
                    break;
                case "turbo":
                    step.completionCondition = () => vehicleController.turbo;
                    break;
                case "jump":
                    vehicleController.onJumpPerformed += () => jumpDone = true;
                    step.completionCondition = () => jumpDone;
                    break;
                case "attach_winch":
                    step.completionCondition = () =>
                        vehicleController.frontWinch.IsAttached || vehicleController.backWinch.IsAttached;
                    break;
                case "pull_winch":
                    playerController.onAnyWinchPull += () => winchPulled = true;
                    step.completionCondition = () => winchPulled;
                    break;
                case "swing_winch":
                    step.completionCondition = () =>
                        (vehicleController.frontWinch.IsAttached || vehicleController.backWinch.IsAttached) &&
                        !vehicleController.IsAnyWheelGrounded() &&
                        (Mathf.Abs(vehicleController.driverInput.x) > 0.01f || Mathf.Abs(vehicleController.driverInput.y) > 0.01f);
                    break;
                case "loose_winch":
                    playerController.onAnyWinchExtend += () => winchExtended = true;
                    step.completionCondition = () => winchExtended;
                    break;
                case "attach_movable":
                    step.completionCondition = () =>
                        (vehicleController.frontWinch.IsAttached &&
                         vehicleController.frontWinch.ConnectedBody != null &&
                         vehicleController.frontWinch.ConnectedBody.GetComponent<MovableObject>() != null)
                        ||
                        (vehicleController.backWinch.IsAttached &&
                         vehicleController.backWinch.ConnectedBody != null &&
                         vehicleController.backWinch.ConnectedBody.GetComponent<MovableObject>() != null);
                    break;
            }
        }
    }

    public void RequestStep(string stepId)
    {
        if (!stepQueue.Contains(stepId))
        {
            stepQueue.Enqueue(stepId);
            TryShowNextStep();
        }
    }

    private void TryShowNextStep()
    {
        if (currentStep != null) return;

        while (stepQueue.Count > 0)
        {
            string nextId = stepQueue.Peek();
            TutorialStep step = GetStepById(nextId);
            if (step == null || step.isCompleted)
            {
                stepQueue.Dequeue();
                continue;
            }
            ShowStep(step);
            break;
        }
    }

    private TutorialStep GetStepById(string id)
    {
        foreach (var s in steps)
            if (s.stepId == id) return s;
        return null;
    }

    private void ShowStep(TutorialStep step)
    {
        currentStep = step;
        stepCompletedFlag = false;
        messageText.text = step.message;
        if (keyImage != null) keyImage.sprite = step.keyIcon;

        StopAllCoroutines();
        tutorialPanel.SetActive(true);
        StartCoroutine(ShowRoutine());
    }

    private System.Collections.IEnumerator ShowRoutine()
    {
        float t = 0f;
        canvasGroup.alpha = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        while (!stepCompletedFlag)
        {
            yield return null;
        }

        t = 0f;
        float startAlpha = canvasGroup.alpha;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        tutorialPanel.SetActive(false);

        currentStep.isCompleted = true;
        currentStep = null;
        TryShowNextStep();
    }

    private void Update()
    {
        if (currentStep != null && currentStep.completionCondition != null)
        {
            if (currentStep.completionCondition.Invoke())
            {
                stepCompletedFlag = true;
            }
        }
        else if (currentStep == null && stepQueue.Count > 0)
        {
            TryShowNextStep();
        }
    }

    private void HidePanelInstant()
    {
        tutorialPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }
}