import importlib.util
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('validator', Path(__file__).resolve().parents[1] / 'validate_project.py')
validator = importlib.util.module_from_spec(spec)
spec.loader.exec_module(validator)


class SceneValidationTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        (self.root / 'ProjectSettings').mkdir()
        (self.root / 'Assets/Scenes').mkdir(parents=True)
        (self.root / 'Assets/Scenes/MainMenu.unity').write_text('')
        (self.root / 'Assets/Scenes/MainMenu.unity.meta').write_text('guid: abc')
        (self.root / 'ProjectSettings/EditorBuildSettings.asset').write_text(
            '- enabled: 1\n  path: Assets/Scenes/MainMenu.unity\n  guid: abc\n')

    def test_valid_project(self):
        self.assertEqual(validator.validate(self.root), [])

    def test_missing_scene(self):
        (self.root / 'Assets/Scenes/MainMenu.unity').unlink()
        self.assertTrue(any('does not exist' in e for e in validator.validate(self.root)))

    def test_stale_guid(self):
        (self.root / 'Assets/Scenes/MainMenu.unity.meta').write_text('guid: def')
        self.assertTrue(any('GUID' in e for e in validator.validate(self.root)))

    def test_missing_destination(self):
        (self.root / 'Assets/Route.cs').write_text('SceneManager.LoadScene("missing");')
        self.assertTrue(any('unavailable scene: missing' in e for e in validator.validate(self.root)))

    def test_route_constants_are_checked(self):
        (self.root / 'Assets/SceneRoutes.cs').write_text('public const string FirstDay = "missing";')
        self.assertTrue(any('unavailable scene: missing' in e for e in validator.validate(self.root)))

    def test_comments_are_ignored(self):
        (self.root / 'Assets/Route.cs').write_text('// LoadScene("missing");\n/* LoadScene("also_missing"); */')
        self.assertEqual(validator.validate(self.root), [])

    def test_missing_completion_scene(self):
        (self.root / 'Assets/Scenes/MainMenu.unity').write_text('  completedScene: missing\n')
        self.assertTrue(any('unavailable completion scene' in e for e in validator.validate(self.root)))
