#!/usr/bin/env python3
"""Check scene references without importing the project or requiring a Unity license."""
from pathlib import Path
import re
import sys


def validate(root):
    root = Path(root)
    errors = []
    settings = (root / 'ProjectSettings/EditorBuildSettings.asset').read_text()
    scenes = re.findall(r'- enabled: 1\s+path: (.+)\s+guid: ([a-f0-9]+)', settings)
    enabled = set()
    for path, guid in scenes:
        scene = root / path
        enabled.add(scene.stem)
        if not scene.is_file():
            errors.append(f'Enabled build scene does not exist: {path}')
        elif not Path(str(scene) + '.meta').is_file() or f'guid: {guid}' not in Path(str(scene) + '.meta').read_text():
            errors.append(f'Build scene GUID does not match its metadata: {path}')
    if not scenes or scenes[0][0] != 'Assets/Scenes/MainMenu.unity':
        errors.append('MainMenu must be the first enabled build scene.')
    for script in (root / 'Assets').rglob('*.cs'):
        source = re.sub(r'/\*.*?\*/|//[^\n]*', '', script.read_text(errors='replace'), flags=re.S)
        destinations = re.findall(r'(?:LoadScene(?:Async)?|LoadNextScene)\s*\(\s*"([^"]+)"', source)
        if script.name == 'SceneRoutes.cs':
            destinations += re.findall(r'(?:return|const string \w+ =)\s*"([^"]+)"', source)
        for destination in destinations:
            if destination not in enabled:
                errors.append(f'{script.relative_to(root)} references unavailable scene: {destination}')
    for scene in (root / 'Assets/Scenes').rglob('*.unity'):
        for destination in re.findall(r'^  completedScene: (\S+)', scene.read_text(), re.M):
            if destination not in enabled:
                errors.append(f'{scene.relative_to(root)} has unavailable completion scene: {destination}')
    return errors


if __name__ == '__main__':
    root = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parents[1]
    errors = validate(root)
    for error in errors:
        print(error, file=sys.stderr)
    if errors:
        sys.exit(1)
    print('Project scene references and build metadata are valid.')
