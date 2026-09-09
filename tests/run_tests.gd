extends Node

func _ready() -> void:
	var suite: RefCounted = load("res://tests/test_game_rules.gd").new()
	var failures: Array[String] = suite.run_all()
	if failures.is_empty():
		print("TESTS PASSED: %d assertions" % suite.assertion_count)
		get_tree().quit(0)
	else:
		for failure in failures:
			push_error(failure)
		print("TESTS FAILED: %d" % failures.size())
		get_tree().quit(1)
