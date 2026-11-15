using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

public class SceneRouteTests
{
    [TestCase(1, 'S', "dia1solda")]
    [TestCase(1, 'P', "dia1pc")]
    [TestCase(5, 'M', "dia5mesa")]
    [TestCase(5, 'F', "dia5flip")]
    public void AvailableLocationHasEnabledScene(int day, char location, string expected)
    {
        object actual = GameTypes.Find("SceneRoutes").GetMethod("Location").Invoke(null, new object[] { day, location });
        Assert.That(actual, Is.EqualTo(expected));
        Assert.That(EditorBuildSettings.scenes.Any(s => s.enabled && File.Exists(s.path) &&
            Path.GetFileNameWithoutExtension(s.path) == expected), Is.True);
    }
    [TestCase(1, 'F')]
    [TestCase(1, 'M')]
    [TestCase(5, 'S')]
    [TestCase(5, 'P')]
    public void UnimplementedLocationHasNoRoute(int day, char location)
    {
        Assert.That(GameTypes.Find("SceneRoutes").GetMethod("Location").Invoke(null, new object[] { day, location }), Is.Null);
    }
    [Test] public void EveryEnabledBuildSceneExists()
    {
        foreach (var scene in EditorBuildSettings.scenes.Where(s => s.enabled))
            Assert.That(File.Exists(scene.path), Is.True, scene.path);
    }
}
