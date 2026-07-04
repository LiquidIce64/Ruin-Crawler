using System;
using UnityEngine;

[Serializable]
public class TutorialStep
{
    [Tooltip("Уникальный идентификатор шага")]
    public string stepId;
    [TextArea(3, 5)]
    public string message;
    public Sprite keyIcon;

    public bool isCompleted;

    [System.NonSerialized]
    public Func<bool> completionCondition;
}