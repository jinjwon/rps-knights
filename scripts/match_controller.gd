extends Node3D

enum Phase { SELECT, REVEAL, COMBAT, RESULT, MATCH_OVER }
const FIGHTER_SCENE := preload("res://scenes/fighter.tscn")
const PROJECTILE_SCRIPT := preload("res://scripts/projectile.gd")
const CARD_NAMES := ["가위 · 검 기사", "바위 · 해머 기사", "보 · 방패 기사"]
const PENALTY_NAMES := ["이동속도 -10%", "공격력 -15%", "방어력 20→10", "치명타 확률 15→5%"]

var phase := Phase.SELECT
var player: Fighter
var bot: Fighter
var player_card := -1
var bot_card := -1
var player_wins := 0
var bot_wins := 0
var rounds_played := 0
var fight_time := 45.0
var current_penalty := 0
var round_resolved := false
var selection_panel: PanelContainer
var gun_toggle: CheckButton
var status_label: Label
var score_label: Label
var timer_label: Label
var player_hud: Label
var bot_hud: Label
var action_button: Button
var hand_reveal: ColorRect
var left_hand: Label
var right_hand: Label
var chant_label: Label
var reveal_result: Label

func _ready() -> void:
	randomize()
	_configure_environment()
	_build_ui()
	_start_selection()

func _process(delta: float) -> void:
	if phase == Phase.COMBAT:
		fight_time = maxf(0.0, fight_time - delta)
		if fight_time <= 0.0:
			_resolve_round(GameRules.round_winner(player.health, bot.health))
	_update_hud()

func _unhandled_input(event: InputEvent) -> void:
	if phase != Phase.SELECT or not event.is_pressed():
		return
	if event.is_action("card_scissors"): _select_card(GameRules.Card.SCISSORS)
	elif event.is_action("card_rock"): _select_card(GameRules.Card.ROCK)
	elif event.is_action("card_paper"): _select_card(GameRules.Card.PAPER)

func _configure_environment() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color("111827")
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color("9ab2d4")
	environment.ambient_light_energy = 0.55
	$WorldEnvironment.environment = environment

func _build_ui() -> void:
	var root := Control.new()
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	$UI.add_child(root)
	score_label = _label(root, Vector2(24, 18), Vector2(470, 52), 24)
	timer_label = _label(root, Vector2(570, 18), Vector2(710, 62), 32)
	timer_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	player_hud = _label(root, Vector2(24, 650), Vector2(610, 710), 19)
	bot_hud = _label(root, Vector2(670, 650), Vector2(1256, 710), 19)
	bot_hud.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	status_label = _label(root, Vector2(170, 76), Vector2(1110, 132), 23)
	status_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	var help := _label(root, Vector2(150, 595), Vector2(1130, 635), 16)
	help.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	help.text = "WASD 이동 · 좌클릭 공격 · 우클릭 가드 · Space 회피 · Q 스킬 · E 바위의 총알 막기"
	selection_panel = PanelContainer.new()
	selection_panel.position = Vector2(330, 145)
	selection_panel.size = Vector2(620, 410)
	root.add_child(selection_panel)
	var box := VBoxContainer.new()
	box.name = "Choices"
	box.add_theme_constant_override("separation", 14)
	selection_panel.add_child(box)
	var title := Label.new()
	title.text = "무엇을 낼까요?"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 30)
	box.add_child(title)
	var penalty := Label.new()
	penalty.name = "Penalty"
	penalty.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	penalty.add_theme_font_size_override("font_size", 18)
	box.add_child(penalty)
	for card in range(3):
		var button := Button.new()
		button.text = "%d  %s" % [card + 1, ["✌  가위", "✊  바위", "✋  보"][card]]
		button.custom_minimum_size.y = 62
		button.add_theme_font_size_override("font_size", 21)
		button.pressed.connect(_select_card.bind(card))
		box.add_child(button)
	gun_toggle = CheckButton.new()
	gun_toggle.text = "가위 선택 시 총 변형 시도 (20% · 성공/실패 모두 스킬 1회 소비)"
	box.add_child(gun_toggle)
	var hint := Label.new()
	hint.text = "방패는 근접 방어 · 바위의 E 주먹은 정면 총알 한 발만 막습니다"
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	box.add_child(hint)
	action_button = Button.new()
	action_button.position = Vector2(515, 520)
	action_button.size = Vector2(250, 58)
	action_button.add_theme_font_size_override("font_size", 21)
	action_button.hide()
	action_button.pressed.connect(_action_pressed)
	root.add_child(action_button)
	_build_hand_reveal(root)

func _build_hand_reveal(root: Control) -> void:
	hand_reveal = ColorRect.new()
	hand_reveal.position = Vector2.ZERO
	hand_reveal.size = Vector2(1280, 720)
	hand_reveal.color = Color("101827")
	hand_reveal.mouse_filter = Control.MOUSE_FILTER_STOP
	hand_reveal.hide()
	root.add_child(hand_reveal)
	chant_label = _label(hand_reveal, Vector2(340, 75), Vector2(940, 145), 36)
	chant_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	left_hand = _label(hand_reveal, Vector2(180, 180), Vector2(560, 430), 150)
	left_hand.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	right_hand = _label(hand_reveal, Vector2(720, 180), Vector2(1100, 430), 150)
	right_hand.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	var versus := _label(hand_reveal, Vector2(570, 255), Vector2(710, 330), 36)
	versus.text = "VS"
	versus.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	reveal_result = _label(hand_reveal, Vector2(220, 465), Vector2(1060, 575), 25)
	reveal_result.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER

func _label(parent: Control, from: Vector2, to: Vector2, font_size: int) -> Label:
	var label := Label.new()
	label.position = from
	label.size = to - from
	label.add_theme_font_size_override("font_size", font_size)
	parent.add_child(label)
	return label

func _start_selection() -> void:
	_clear_fighters()
	phase = Phase.SELECT
	round_resolved = false
	current_penalty = rounds_played % PENALTY_NAMES.size()
	selection_panel.get_node("Choices/Penalty").text = "이번 패배 패널티: %s (4초)" % PENALTY_NAMES[current_penalty]
	selection_panel.show()
	hand_reveal.hide()
	action_button.hide()
	status_label.text = "상대의 선택은 공개 전까지 숨겨집니다"

func _select_card(card: int) -> void:
	if phase != Phase.SELECT:
		return
	player_card = card
	bot_card = randi_range(0, 2)
	var player_role := card
	var bot_role := bot_card
	var player_attempt := card == GameRules.Card.SCISSORS and gun_toggle.button_pressed
	var bot_attempt := bot_card == GameRules.Card.SCISSORS and randf() < 0.35
	var player_gun := player_attempt and randf() < 0.20
	var bot_gun := bot_attempt and randf() < 0.20
	if player_gun: player_role = GameRules.Role.GUN
	if bot_gun: bot_role = GameRules.Role.GUN
	var result := GameRules.rps_result(player_card, bot_card)
	phase = Phase.REVEAL
	selection_panel.hide()
	var outcome := "무승부 — 패널티 없음"
	if result == GameRules.Outcome.WIN: outcome = "카드 승리 — 상대에게 패널티"
	if result == GameRules.Outcome.LOSS: outcome = "카드 패배 — 플레이어에게 패널티"
	var gun_result := ""
	if player_attempt: gun_result += " · 내 총 %s" % ("성공" if player_gun else "실패")
	if bot_attempt: gun_result += " · 상대 총 %s" % ("성공" if bot_gun else "실패")
	await _play_hand_reveal(outcome, gun_result)
	_spawn_fighters(player_role, bot_role)
	if player_attempt: player.skill_charges -= 1
	if bot_attempt: bot.skill_charges -= 1
	if result == GameRules.Outcome.LOSS: player.apply_penalty(current_penalty)
	if result == GameRules.Outcome.WIN: bot.apply_penalty(current_penalty)
	status_label.text = "%s  VS  %s\n%s%s" % [CARD_NAMES[player_card], CARD_NAMES[bot_card], outcome, gun_result]
	await get_tree().create_timer(1.0).timeout
	hand_reveal.hide()
	if phase == Phase.REVEAL: _begin_combat()

func _play_hand_reveal(outcome: String, gun_result: String) -> void:
	hand_reveal.show()
	left_hand.text = "✊"
	right_hand.text = "✊"
	reveal_result.text = ""
	for chant in ["가위…", "바위…", "보!"]:
		chant_label.text = chant
		left_hand.position.y = 195.0
		right_hand.position.y = 195.0
		var tween := create_tween().set_parallel(true)
		tween.tween_property(left_hand, "position:y", 145.0, 0.16).set_trans(Tween.TRANS_QUAD)
		tween.tween_property(right_hand, "position:y", 145.0, 0.16).set_trans(Tween.TRANS_QUAD)
		await tween.finished
		var down := create_tween().set_parallel(true)
		down.tween_property(left_hand, "position:y", 195.0, 0.16).set_trans(Tween.TRANS_QUAD)
		down.tween_property(right_hand, "position:y", 195.0, 0.16).set_trans(Tween.TRANS_QUAD)
		await down.finished
	left_hand.text = ["✌", "✊", "✋"][player_card]
	right_hand.text = ["✌", "✊", "✋"][bot_card]
	chant_label.text = "결과 공개!"
	reveal_result.text = "나: %s    상대: %s\n%s%s" % [CARD_NAMES[player_card], CARD_NAMES[bot_card], outcome, gun_result]
	await get_tree().create_timer(0.75).timeout

func _spawn_fighters(player_role: int, bot_role: int) -> void:
	player = FIGHTER_SCENE.instantiate()
	bot = FIGHTER_SCENE.instantiate()
	add_child(player)
	add_child(bot)
	player.configure(player_role, true, bot)
	bot.configure(bot_role, false, player)
	player.reset_for_round(Vector3(-2.8, 0.0, 0.0))
	bot.reset_for_round(Vector3(2.8, 0.0, 0.0))
	player.defeated.connect(_fighter_defeated)
	bot.defeated.connect(_fighter_defeated)
	player.projectile_requested.connect(_spawn_projectile)
	bot.projectile_requested.connect(_spawn_projectile)
	player.combat_event.connect(_show_event)
	bot.combat_event.connect(_show_event)

func _begin_combat() -> void:
	phase = Phase.COMBAT
	fight_time = 45.0
	player.set_combat_enabled(true)
	bot.set_combat_enabled(true)
	status_label.text = "전투 시작!"

func _fighter_defeated(defeated_fighter: Fighter) -> void:
	_resolve_round(GameRules.Outcome.LOSS if defeated_fighter == player else GameRules.Outcome.WIN)

func _resolve_round(result: int) -> void:
	if round_resolved: return
	round_resolved = true
	player.set_combat_enabled(false)
	bot.set_combat_enabled(false)
	rounds_played += 1
	var result_text := "라운드 무승부"
	if result == GameRules.Outcome.WIN: player_wins += 1; result_text = "라운드 승리!"
	if result == GameRules.Outcome.LOSS: bot_wins += 1; result_text = "라운드 패배"
	if GameRules.is_match_over(player_wins, bot_wins, rounds_played):
		phase = Phase.MATCH_OVER
		var match_text := "경기 무승부"
		if player_wins > bot_wins: match_text = "경기 승리!"
		if player_wins < bot_wins: match_text = "경기 패배"
		status_label.text = "%s · %s" % [result_text, match_text]
		action_button.text = "새 경기"
	else:
		phase = Phase.RESULT
		status_label.text = result_text
		action_button.text = "다음 라운드"
	action_button.show()

func _action_pressed() -> void:
	if phase == Phase.MATCH_OVER:
		player_wins = 0; bot_wins = 0; rounds_played = 0
	_start_selection()

func _spawn_projectile(source: Fighter, target_fighter: Fighter, damage: float) -> void:
	var projectile := Node3D.new()
	projectile.set_script(PROJECTILE_SCRIPT)
	add_child(projectile)
	projectile.setup(source, target_fighter, damage)

func _show_event(message: String) -> void:
	if phase == Phase.COMBAT: status_label.text = message

func _update_hud() -> void:
	score_label.text = "플레이어 %d : %d 상대 · 라운드 %d/7" % [player_wins, bot_wins, rounds_played + 1]
	timer_label.text = "%02d" % ceili(fight_time) if phase == Phase.COMBAT else "--"
	if is_instance_valid(player): player_hud.text = "%s  HP %d · 스킬 %d · 탄약 %d%s" % [player.role_name(), ceili(player.health), player.skill_charges, player.ammo, _penalty_text(player)]
	if is_instance_valid(bot): bot_hud.text = "%s  HP %d · 스킬 %d · 탄약 %d%s" % [bot.role_name(), ceili(bot.health), bot.skill_charges, bot.ammo, _penalty_text(bot)]

func _penalty_text(fighter: Fighter) -> String:
	return " · 패널티 %.1f초" % fighter.penalty_remaining() if fighter.penalty_remaining() > 0.0 else ""

func _clear_fighters() -> void:
	if is_instance_valid(player): player.queue_free()
	if is_instance_valid(bot): bot.queue_free()
	for child in get_children():
		if child is BattleProjectile: child.queue_free()
