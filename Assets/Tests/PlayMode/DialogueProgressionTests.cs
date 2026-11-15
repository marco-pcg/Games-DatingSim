using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

public class DialogueProgressionTests
{
    private GameObject host;
    private Component manager;
    private ScriptableObject plain;
    private ScriptableObject choices;
    private int completions;

    [UnitySetUp] public IEnumerator SetUp()
    {
        host = new GameObject("Dialogue test");
        manager = host.AddComponent(GameTypes.Find("DialogueManager"));
        GameTypes.Set(manager, "dialogueBox", new GameObject("Dialogue box"));
        GameTypes.Set(manager, "dialogueOptionUI", new GameObject("Options"));
        GameTypes.Set(manager, "characterPortraits", Array.CreateInstance(Type.GetType("UnityEngine.UI.Image, UnityEngine.UI", true), 0));
        plain = ScriptableObject.CreateInstance(GameTypes.Find("DialogueBase"));
        GameTypes.Set(plain, "dialogueInfo", Array.CreateInstance(GameTypes.Find("DialogueBase+Info"), 0));
        choices = ScriptableObject.CreateInstance(GameTypes.Find("DialogueOptions"));
        GameTypes.Set(choices, "dialogueInfo", Array.CreateInstance(GameTypes.Find("DialogueBase+Info"), 0));
        Type optionType = GameTypes.Find("DialogueOptions+Options");
        Array options = Array.CreateInstance(optionType, 1);
        object option = Activator.CreateInstance(optionType);
        GameTypes.Set(option, "myEvent", new UnityEvent());
        options.SetValue(option, 0);
        GameTypes.Set(choices, "optionsInfo", options);
        // A real option component makes one available button without requiring scene assets.
        var button = new GameObject("Choice");
        button.transform.SetParent(((GameObject)GameTypes.Get(manager, "dialogueOptionUI")).transform);
        button.AddComponent(GameTypes.Find("UnityEventHandler"));
        completions = 0;
        ((UnityEvent)GameTypes.Get(manager, "onDialogueCompleted")).AddListener(() => completions++);
        yield return null;
    }
    [UnityTearDown] public IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy((GameObject)GameTypes.Get(manager, "dialogueBox"));
        UnityEngine.Object.Destroy((GameObject)GameTypes.Get(manager, "dialogueOptionUI"));
        UnityEngine.Object.Destroy(host);
        UnityEngine.Object.Destroy(plain);
        UnityEngine.Object.Destroy(choices);
        yield return null;
    }
    [Test] public void OrdinaryDialogueCompletesOnceWithoutChangingScene()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameTypes.Call(manager, "EnqueueDialogue", plain);
        GameTypes.Call(manager, "DequeueDialogue");
        Assert.That(completions, Is.EqualTo(1));
        Assert.That(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, Is.EqualTo(scene));
    }
    [Test] public void TerminalChoiceCompletesOnlyOnce()
    {
        GameTypes.Call(manager, "EnqueueDialogue", choices);
        Assert.That(completions, Is.Zero);
        Assert.That(((GameObject)GameTypes.Get(manager, "dialogueOptionUI")).activeSelf, Is.True);
        GameTypes.Call(manager, "SelectOption", 0);
        GameTypes.Call(manager, "SelectOption", 0);
        Assert.That(completions, Is.EqualTo(1));
    }
    [Test] public void OrdinaryDialogueAfterOptionsClearsChoiceState()
    {
        GameTypes.Call(manager, "EnqueueDialogue", choices);
        GameTypes.Call(manager, "EnqueueDialogue", plain);
        Assert.That(GameTypes.Get(manager, "optionsAmount"), Is.EqualTo(0));
        Assert.That(((GameObject)GameTypes.Get(manager, "dialogueOptionUI")).activeSelf, Is.False);
        Assert.That(completions, Is.EqualTo(1));
    }
}
