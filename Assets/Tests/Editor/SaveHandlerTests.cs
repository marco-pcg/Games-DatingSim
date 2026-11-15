using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SaveHandlerTests
{
    private string directory;
    private object handler;
    [SetUp] public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "DatingSimTests-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        handler = Activator.CreateInstance(GameTypes.Find("SaveHandler"), directory, "save.json");
    }
    [TearDown] public void TearDown() { Directory.Delete(directory, true); }

    [Test] public void MissingSaveReturnsNull()
    {
        Assert.That(GameTypes.Call(handler, "Load"), Is.Null);
    }
    [Test] public void RoundTripRestoresCheckpointAndCharacterWithoutSceneObjects()
    {
        object data = Activator.CreateInstance(GameTypes.Find("SaveData"));
        GameTypes.Set(data, "day", 5);
        GameTypes.Set(data, "sceneName", "dia5mesa");
        GameTypes.Set(data, "playerName", "Alex");
        GameTypes.Set(data, "selectedGender", "Female");
        GameTypes.Set(data, "affectionPoints", 42);
        Assert.That(GameTypes.Call(handler, "Save", data), Is.EqualTo(true));
        object loaded = GameTypes.Call(handler, "Load");
        Assert.That(GameTypes.Get(loaded, "sceneName"), Is.EqualTo("dia5mesa"));
        Assert.That(GameTypes.Get(loaded, "day"), Is.EqualTo(5));
        Assert.That(GameTypes.Get(loaded, "playerName"), Is.EqualTo("Alex"));
        Assert.That(GameTypes.Get(loaded, "selectedGender"), Is.EqualTo("Female"));
        Assert.That(GameTypes.Get(loaded, "affectionPoints"), Is.EqualTo(42));
        foreach (var field in data.GetType().GetFields())
            Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType), Is.False, field.Name);
        Assert.That(File.Exists(Path.Combine(directory, "save.json.tmp")), Is.False);
    }
    [Test] public void RepeatedSavesReplaceTheCheckpoint()
    {
        object data = Activator.CreateInstance(GameTypes.Find("SaveData"));
        GameTypes.Call(handler, "Save", data);
        GameTypes.Set(data, "affectionPoints", 10);
        Assert.That(GameTypes.Call(handler, "Save", data), Is.EqualTo(true));
        Assert.That(GameTypes.Get(GameTypes.Call(handler, "Load"), "affectionPoints"), Is.EqualTo(10));
    }
    [TestCase("not json")]
    [TestCase("{}")]
    [TestCase("null")]
    [TestCase("{\"day\":99}")]
    [TestCase("{\"day\":1,\"version\":99}")]
    public void CorruptOrUnsupportedSaveReturnsNull(string json)
    {
        File.WriteAllText(Path.Combine(directory, "save.json"), json);
        LogAssert.Expect(LogType.Warning, new Regex("Unable to load save"));
        Assert.That(GameTypes.Call(handler, "Load"), Is.Null);
    }
    [Test] public void LegacySaveUsesItsDayAsCheckpoint()
    {
        File.WriteAllText(Path.Combine(directory, "save.json"), "{\"day\":5,\"playerName\":\"Alex\",\"selectedGender\":\"Male\"}");
        object data = GameTypes.Call(handler, "Load");
        Assert.That(GameTypes.Get(data, "sceneName"), Is.EqualTo("dia5"));
    }
}
