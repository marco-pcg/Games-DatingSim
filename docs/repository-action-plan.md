# Repository action plan

This Unity 2021.3.23f1 visual novel was reviewed statically. The following work is tracked separately; implementation and Unity verification remain outstanding.

| Priority | Action | Issue | Branch |
| --- | --- | --- | --- |
| High | Fix missing scene routes | [#1](https://github.com/marco-pcg/Games-DatingSim/issues/1) | `fix/scene-routing` |
| High | Stabilize save initialization and manager lifecycle | [#2](https://github.com/marco-pcg/Games-DatingSim/issues/2) | `fix/save-initialization` |
| High | Make dialogue completion and progression explicit | [#3](https://github.com/marco-pcg/Games-DatingSim/issues/3) | `fix/dialogue-progression` |
| Medium | Prevent null reference during scenery selection | [#4](https://github.com/marco-pcg/Games-DatingSim/issues/4) | `fix/scenery-selection` |
| Medium | Repair word-search alphabet inspector | [#5](https://github.com/marco-pcg/Games-DatingSim/issues/5) | `fix/wordsearch-inspector` |
| Medium | Add automated verification and CI | [#6](https://github.com/marco-pcg/Games-DatingSim/issues/6) | `test/unity-verification` |
| Low | Document setup, builds, and gameplay flow | [#7](https://github.com/marco-pcg/Games-DatingSim/issues/7) | `docs/project-setup` |

Recommended order: repair scene routes and save initialization, make dialogue progression explicit, fix scenery and inspector defects, then add regression checks and finish setup documentation.

Validation target: menu → character setup → day 1 → save → restart → resume. Exercise all available day 5 choices and the affected word-search inspector.

The action branches start at the inspected main commit and contain no implementation changes yet. This planning PR does not close the issues.
