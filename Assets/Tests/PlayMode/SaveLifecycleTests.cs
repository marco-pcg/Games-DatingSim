using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SaveLifecycleTests
{
    private string directory;
    private Component manager;
    private Component game;

    [UnitySetUp] public IEnumerator SetUp()
    {
        // Isolate both the singleton lifecycle and disk writes from the player's save.
        foreach (string name in new[] { "SaveManager", "GameManager" })
            foreach (var component in UnityEngine.Object.FindObjectsOfType(GameTypes.Find(name)))
                UnityEngine.Object.Destroy(((Component)component).gameObject);
        yield return null;
        game = new GameObject("Test GameManager").AddComponent(GameTypes.Find("GameManager"));
        manager = new GameObject("Test SaveManager").AddComponent(GameTypes.Find("SaveManager"));
        directory = Path.Combine(Path.GetTempPath(), "DatingSimLifecycle-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        GameTypes.Set(manager, "saveHandler", Activator.CreateInstance(GameTypes.Find("SaveHandler"), directory, "save.json"));
    }
    [UnityTearDown] public IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(manager.gameObject);
        UnityEngine.Object.Destroy(game.gameObject);
        yield return null;
        Directory.Delete(directory, true);
    }
    [UnityTest] public IEnumerator DuplicateManagerDoesNotReplaceSingleton()
    {
        var duplicate = new GameObject("Duplicate SaveManager").AddComponent(GameTypes.Find("SaveManager"));
        yield return null;
        Assert.That(duplicate == null, Is.True);
        Assert.That(GameTypes.Find("SaveManager").GetProperty("Instance").GetValue(null), Is.SameAs(manager));
    }
    [Test] public void SaveWorksBeforeOnPlayAndLoadRestoresCharacter()
    {
        GameTypes.Set(game, "playerName", "Alex");
        GameTypes.Set(game, "selectedGender", "Male");
        GameTypes.Call(manager, "SavePlayerInfo");
        GameTypes.Call(manager, "SaveGame");
        GameTypes.Set(game, "playerName", "Changed");
        GameTypes.Call(manager, "LoadGame");
        Assert.That(GameTypes.Get(game, "playerName"), Is.EqualTo("Alex"));
        Assert.That(GameTypes.Get(game, "selectedGender"), Is.EqualTo("Male"));
    }
    [Test] public void DayEndStoresNextImplementedDayAndNewGameResetsState()
    {
        GameTypes.Call(game, "OnDayEnd");
        GameTypes.Call(manager, "LoadGame");
        Assert.That(GameTypes.Get(game, "currentDay"), Is.EqualTo(5));
        GameTypes.Call(manager, "NewGame");
        Assert.That(GameTypes.Get(game, "currentDay"), Is.EqualTo(1));
        Assert.That(GameTypes.Get(game, "playerName"), Is.Null.Or.Empty);
    }
}
