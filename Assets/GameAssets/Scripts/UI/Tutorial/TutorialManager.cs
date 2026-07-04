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
    [SerializeField] private float fadeDuration = 2f;

    [SerializeField] private TutorialStep[] steps;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private VehicleController vehicleController;

    private TutorialStep currentStep;
    private Queue<string> stepQueue = new Queue<string>();

    private bool jumpDone = false;
    private bool winchPulled = false;

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
        messageText.text = step.message;
        if (keyImage != null) keyImage.sprite = step.keyIcon;

        StopAllCoroutines();
        tutorialPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if (currentStep != null && currentStep.completionCondition != null)
        {
            if (currentStep.completionCondition.Invoke())
            {
                currentStep.isCompleted = true;
                HidePanel();
                currentStep = null;
                TryShowNextStep();
            }
        }
        else if (currentStep == null && stepQueue.Count > 0)
        {
            TryShowNextStep();
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float t = 0f;
        canvasGroup.alpha = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private void HidePanel()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float t = 0f;
        float startAlpha = canvasGroup.alpha;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }
        tutorialPanel.SetActive(false);
    }

    private void HidePanelInstant()
    {
        tutorialPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }
}