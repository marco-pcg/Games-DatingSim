# Verification

The regression suite covers the scene-routing, save-initialization, and dialogue-progression fixes. Merge those fixes before this test PR; running the suite against the original main branch intentionally exposes those defects.

## Without Unity

Run from the repository root with Python 3:

```sh
python3 -m unittest discover -s tools/tests -v
python3 tools/validate_project.py
```

The validator checks enabled scene files and GUIDs, literal scene destinations, the location-route constants, and serialized dialogue completion scenes. It does not execute C# or validate dynamically constructed routes.

## Unity tests

Use Unity **2021.3.23f1**. Open **Window → General → Test Runner**, then run both EditMode and PlayMode suites. Test assemblies use reflection to reach the existing predefined `Assembly-CSharp` assembly, avoiding a production assembly migration and changes to serialized UnityEvent type names.

- EditMode: save round trips, repeated replacement, missing/corrupt/unsupported saves, legacy checkpoints, location routes, and enabled build scenes.
- PlayMode: save before `OnPlay`, duplicate singleton cleanup, reload, new-game reset, day-end checkpoints, option completion, repeated clicks, and option-to-ordinary dialogue state.

For batch execution, set `UNITY_EDITOR` to the editor executable and choose an absolute results path:

```sh
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode -testResults /tmp/datingsim-editmode.xml -logFile /tmp/datingsim-editmode.log
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults /tmp/datingsim-playmode.xml -logFile /tmp/datingsim-playmode.log
```

Do not add `-quit`: the test runner exits after its asynchronous test run. Inspect the XML and logs as well as the process status. The save tests use temporary directories and remove them afterward.

## GitHub Actions

`.github/workflows/unity-tests.yml` runs static validation on pull requests, main pushes, and manual dispatch. It runs both Unity test modes when all three repository secrets are configured: `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD`. Obtain an appropriate activated license using the [GameCI activation instructions](https://game.ci/docs/github/activation/), then follow the [official test runner configuration](https://game.ci/docs/github/test-runner/). Never commit license files or account credentials.

Without secrets (including fork PRs), Unity jobs are explicitly skipped with a notice; a green static check does **not** mean Unity tests passed. The repository had no configured secrets when this workflow was added. License configuration remains an owner setup task. Each Unity job uploads test artifacts and logs even on failure. Third-party actions are pinned to commits.

## Manual gameplay check

Start from `Assets/Scenes/MainMenu.unity`. Select **INICIAR**, choose a gender, enter a name, and confirm. Visit both available day 1 locations; complete dialogue and each option path. Confirm day 5 opens and only Mesa/Flip are available. Complete those dialogues and confirm they return to day 5 selection. Restart the application and use **CONTINUAR**; verify the most recently entered story scene and character information are restored. Saves resume at scene entry rather than at an individual dialogue line.
