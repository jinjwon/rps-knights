extends RefCounted

var assertion_count := 0
var failures: Array[String] = []

func check(actual: Variant, expected: Variant, label: String) -> void:
	assertion_count += 1
	if actual != expected:
		failures.append("%s: expected %s, got %s" % [label, expected, actual])

func check_close(actual: float, expected: float, label: String) -> void:
	assertion_count += 1
	if not is_equal_approx(actual, expected):
		failures.append("%s: expected %.4f, got %.4f" % [label, expected, actual])

func run_all() -> Array[String]:
	var rules = load("res://scripts/game_rules.gd")
	var cases := [
		[0, 0, 0], [0, 1, -1], [0, 2, 1],
		[1, 0, 1], [1, 1, 0], [1, 2, -1],
		[2, 0, -1], [2, 1, 1], [2, 2, 0],
	]
	for case in cases:
		check(rules.rps_result(case[0], case[1]), case[2], "RPS %s vs %s" % [case[0], case[1]])
	check_close(rules.calculate_damage(20.0, 1.0, 20.0, false, 1.0), 16.6666667, "normal damage")
	check_close(rules.calculate_damage(20.0, 0.85, 10.0, true, 0.3), 6.9545455, "modified critical guarded damage")
	check(rules.should_block_bullet(true, 60.0, true, false), true, "frontal bullet at boundary")
	check(rules.should_block_bullet(true, 60.1, true, false), false, "bullet outside front arc")
	check(rules.should_block_bullet(false, 0.0, true, false), false, "melee is never blocked")
	check(rules.should_block_bullet(true, 0.0, false, false), false, "inactive block")
	check(rules.should_block_bullet(true, 0.0, true, true), false, "consumed block")
	check(rules.round_winner(20.0, 20.0), 0, "equal health tie")
	check(rules.round_winner(50.0, 20.0), 1, "player higher health")
	check(rules.round_winner(0.0, 10.0), -1, "player defeated")
	check(rules.role_profile(0).weapon, "검", "scissors weapon")
	check(rules.role_profile(1).weapon, "해머", "rock weapon")
	check(rules.role_profile(2).guard_multiplier, 0.4, "paper stronger guard")
	check(rules.role_profile(3).ammo, 3, "gun starts with three bullets")
	check(rules.penalty_modifiers(0).move_speed, 0.9, "movement penalty")
	check(rules.penalty_modifiers(1).attack, 0.85, "attack penalty")
	check(rules.penalty_modifiers(2).defense, 10.0, "defense penalty")
	check(rules.penalty_modifiers(3).critical_chance, 0.05, "critical penalty")
	check(rules.is_match_over(3, 0, 4), true, "three player wins ends match")
	check(rules.is_match_over(2, 2, 7), true, "seven rounds ends match")
	check(rules.is_match_over(2, 2, 6), false, "match continues before limit")
	return failures
