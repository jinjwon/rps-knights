# Playable Prototype Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a locally playable Godot prototype covering card choice, RPS penalty, third-person arena combat, skills, gun transformation, rock-only bullet block, round scoring, and bot opposition.

**Architecture:** Pure rule calculations live in focused RefCounted scripts and are exercised by a headless test runner. The playable scene is assembled from reusable fighter and projectile scenes, while one match controller owns phase transitions and UI. The prototype uses primitive meshes so gameplay can be tested before final art.

**Tech Stack:** Godot 4.7.2, typed GDScript, CharacterBody3D, Area3D, Control UI, headless GDScript tests.

**Spec:** `docs/게임_콘셉트와_개발가이드_v0.1.md`

## Global Constraints

- Local player versus bot only; networking is deferred.
- Three-card choice; gun is a 20% scissors transformation costing one skill charge whether it succeeds or fails.
- The RPS loser receives one visible four-second penalty.
- Every fighter starts with 100 health and three skill charges; the fight limit is 45 seconds.
- Rock's timed punch destroys only frontal enemy bullets, never melee attacks.
- A match ends at three wins or after seven rounds.
- No camera or speech recognition in this prototype.

---

### Task 1: Pure Rules and Headless Test Harness

**Files:**
- Create: `scripts/game_rules.gd`
- Create: `tests/test_game_rules.gd`
- Create: `tests/run_tests.gd`
- Create: `tests/test_runner.tscn`

**Interfaces:**
- Produces: `GameRules.rps_result(player_card, opponent_card) -> int`, `calculate_damage(base_damage, attack_modifier, defense, critical, guard_modifier) -> float`, `should_block_bullet(is_bullet, angle_degrees, active, consumed) -> bool`, `round_winner(player_health, bot_health) -> int`.

- [ ] Write table-driven tests for all nine card matchups, damage, ties, and bullet blocking boundaries.
- [ ] Run the headless test scene and verify failure because `GameRules` is missing.
- [ ] Implement the minimal pure rules.
- [ ] Run the headless test scene and verify all assertions pass.
- [ ] Commit the tested rules.

### Task 2: Fighter, Projectile, and Arena Combat

**Files:**
- Create: `scripts/fighter.gd`
- Create: `scripts/projectile.gd`
- Create: `scenes/fighter.tscn`
- Create: `scenes/projectile.tscn`
- Modify: `tests/test_game_rules.gd`

**Interfaces:**
- Consumes: `GameRules.calculate_damage`, `GameRules.should_block_bullet`.
- Produces: fighter signals `health_changed`, `defeated`, `combat_event`; methods `configure`, `reset_for_round`, `set_combat_enabled`, `apply_penalty`, `receive_attack`; projectile ownership and hit delivery.

- [ ] Add failing tests for role stats, penalty modifiers, skill charge consumption, and bullet-block state.
- [ ] Verify the new tests fail because fighter behavior is absent.
- [ ] Implement player movement, bot movement, melee attacks, guard, dodge, role skill, gun shots, hit flash, and bullet block.
- [ ] Verify pure tests and headless scene loading pass.
- [ ] Commit the combat layer.

### Task 3: Match Flow and Player-facing UI

**Files:**
- Create: `scripts/match_controller.gd`
- Replace: `scenes/main.tscn`
- Modify: `README.md`

**Interfaces:**
- Consumes: fighter methods/signals and `GameRules.rps_result`.
- Produces: selection, reveal, combat, result, and match-over phases; card buttons; gun toggle; HUD; restart and next-round actions.

- [ ] Add failing phase-transition and round-scoring tests to `tests/test_game_rules.gd` using pure helper functions.
- [ ] Verify they fail for missing helpers.
- [ ] Implement the arena, third-person camera, card overlay, bot choice, penalty rotation, 20% gun roll, HUD, 45-second timer, three-win/seven-round end conditions, and restart.
- [ ] Verify tests, editor import, interactive startup, and macOS export all succeed without errors.
- [ ] Update README controls and prototype limitations.
- [ ] Commit and push the prototype branch to the personal private repository.
