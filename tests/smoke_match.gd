extends Node

func _ready() -> void:
	var main_scene: PackedScene = load("res://scenes/main.tscn")
	var match_controller: Node = main_scene.instantiate()
	add_child(match_controller)
	await get_tree().process_frame
	if match_controller.phase != match_controller.Phase.SELECT:
		push_error("Match did not start in card selection")
		get_tree().quit(1)
		return
	match_controller._select_card(GameRules.Card.ROCK)
	await get_tree().create_timer(2.2).timeout
	if match_controller.phase != match_controller.Phase.COMBAT:
		push_error("Card selection did not advance to combat")
		get_tree().quit(1)
		return
	if not is_instance_valid(match_controller.player) or not is_instance_valid(match_controller.bot):
		push_error("Fighters were not created")
		get_tree().quit(1)
		return
	print("SMOKE PASSED: selection -> reveal -> combat")
	get_tree().quit(0)
