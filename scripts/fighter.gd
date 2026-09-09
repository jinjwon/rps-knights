class_name Fighter
extends CharacterBody3D

signal health_changed(current: float)
signal defeated(fighter: Fighter)
signal combat_event(message: String)
signal projectile_requested(source: Fighter, target: Fighter, damage: float)

var role := GameRules.Role.SWORD
var is_player := false
var target: Fighter
var health := 100.0
var skill_charges := 3
var ammo := 0
var combat_enabled := false
var guarding := false
var block_active := false
var block_consumed := false

var attack_cooldown := 0.0
var skill_cooldown := 0.0
var dodge_cooldown := 0.0
var block_cooldown := 0.0
var block_time := 0.0
var penalty_time := 0.0
var penalty_kind := -1
var bot_think_time := 0.0
var attack_modifier := 1.0
var move_modifier := 1.0
var defense := 20.0
var critical_chance := 0.15
var profile: Dictionary = {}
var body_mesh: MeshInstance3D
var weapon_mesh: MeshInstance3D

const MOVE_SPEED := 5.0
const ARENA_LIMIT := 5.2

func _ready() -> void:
	_build_visuals()

func configure(new_role: int, player_controlled: bool, opponent: Fighter) -> void:
	role = new_role
	is_player = player_controlled
	target = opponent
	profile = GameRules.role_profile(role)
	ammo = profile.ammo
	if is_node_ready():
		_update_visuals()

func reset_for_round(start_position: Vector3) -> void:
	global_position = start_position
	health = 100.0
	skill_charges = 3
	ammo = GameRules.role_profile(role).ammo
	attack_cooldown = 0.0
	skill_cooldown = 0.0
	dodge_cooldown = 0.0
	block_cooldown = 0.0
	block_time = 0.0
	penalty_time = 0.0
	penalty_kind = -1
	guarding = false
	block_active = false
	block_consumed = false
	_reset_modifiers()
	health_changed.emit(health)

func set_combat_enabled(enabled: bool) -> void:
	combat_enabled = enabled
	guarding = false
	velocity = Vector3.ZERO

func apply_penalty(kind: int, duration: float = 4.0) -> void:
	penalty_kind = kind
	penalty_time = duration
	var modifiers := GameRules.penalty_modifiers(kind)
	move_modifier = modifiers.move_speed
	attack_modifier = modifiers.attack
	defense = modifiers.defense
	critical_chance = modifiers.critical_chance

func penalty_remaining() -> float:
	return penalty_time

func _process(delta: float) -> void:
	attack_cooldown = maxf(0.0, attack_cooldown - delta)
	skill_cooldown = maxf(0.0, skill_cooldown - delta)
	dodge_cooldown = maxf(0.0, dodge_cooldown - delta)
	block_cooldown = maxf(0.0, block_cooldown - delta)
	if block_time > 0.0:
		block_time -= delta
		if block_time <= 0.0:
			block_active = false
	if penalty_time > 0.0 and combat_enabled:
		penalty_time -= delta
		if penalty_time <= 0.0:
			penalty_time = 0.0
			penalty_kind = -1
			_reset_modifiers()
			combat_event.emit("%s의 패널티가 끝났습니다" % display_name())

func _physics_process(delta: float) -> void:
	if not combat_enabled or not is_instance_valid(target):
		velocity = Vector3.ZERO
		return
	look_at(Vector3(target.global_position.x, global_position.y, target.global_position.z), Vector3.UP)
	if is_player:
		_player_control()
	else:
		_bot_control(delta)
	velocity.y = 0.0
	move_and_slide()
	global_position.x = clampf(global_position.x, -ARENA_LIMIT, ARENA_LIMIT)
	global_position.z = clampf(global_position.z, -ARENA_LIMIT, ARENA_LIMIT)

func _player_control() -> void:
	var input := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	velocity = Vector3(input.x, 0.0, input.y) * MOVE_SPEED * move_modifier
	guarding = Input.is_action_pressed("guard") and role != GameRules.Role.GUN and not block_active
	if guarding:
		velocity *= 0.45
	if Input.is_action_just_pressed("attack"):
		_try_attack(false)
	if Input.is_action_just_pressed("skill"):
		_use_skill()
	if Input.is_action_just_pressed("dodge") and dodge_cooldown <= 0.0:
		velocity = (global_position - target.global_position).normalized() * 12.0
		dodge_cooldown = 1.0
	if Input.is_action_just_pressed("bullet_block"):
		_start_bullet_block()

func _bot_control(delta: float) -> void:
	bot_think_time -= delta
	var distance := global_position.distance_to(target.global_position)
	var desired_range: float = 7.0 if role == GameRules.Role.GUN and ammo > 0 else float(profile.range) * 0.8
	var direction := (target.global_position - global_position).normalized()
	if distance > desired_range + 0.5:
		velocity = direction * MOVE_SPEED * 0.72 * move_modifier
	elif role == GameRules.Role.GUN and distance < 4.0:
		velocity = -direction * MOVE_SPEED * 0.6 * move_modifier
	else:
		velocity = Vector3.ZERO
	if role == GameRules.Role.HAMMER and target.role == GameRules.Role.GUN and target.attack_cooldown > 1.0 and block_cooldown <= 0.0:
		_start_bullet_block()
	if bot_think_time <= 0.0:
		bot_think_time = randf_range(0.35, 0.7)
		if distance <= float(profile.range) + 0.35 or role == GameRules.Role.GUN:
			_try_attack(false)
		elif skill_charges > 0 and distance < 4.5:
			_use_skill()

func _try_attack(is_skill: bool) -> void:
	if attack_cooldown > 0.0 or guarding or block_active:
		return
	if role == GameRules.Role.GUN and ammo > 0:
		ammo -= 1
		attack_cooldown = float(profile.cooldown)
		projectile_requested.emit(self, target, _roll_damage(float(profile.damage)))
		combat_event.emit("%s이(가) 발사했습니다 — 탄약 %d" % [display_name(), ammo])
		return
	var distance := global_position.distance_to(target.global_position)
	var attack_range: float = float(profile.range) + (0.8 if is_skill else 0.0)
	var base_damage: float = float(profile.damage) * (1.55 if is_skill else 1.0)
	attack_cooldown = float(profile.cooldown) * (1.25 if is_skill else 1.0)
	if distance <= attack_range:
		target.receive_attack(_roll_damage(base_damage), false, global_position)

func _use_skill() -> void:
	if skill_charges <= 0 or skill_cooldown > 0.0 or guarding or block_active:
		return
	skill_charges -= 1
	skill_cooldown = 5.0
	if role == GameRules.Role.SHIELD:
		health = minf(100.0, health + 12.0)
		health_changed.emit(health)
		combat_event.emit("방패 기사가 체력을 회복했습니다")
	else:
		velocity = (target.global_position - global_position).normalized() * 9.0
		_try_attack(true)

func _start_bullet_block() -> void:
	if role != GameRules.Role.HAMMER or block_cooldown > 0.0 or guarding:
		return
	block_active = true
	block_consumed = false
	block_time = 0.45
	block_cooldown = 2.0
	combat_event.emit("바위 기사가 총알 막기 자세를 취했습니다")

func receive_attack(raw_damage: float, is_bullet: bool, source_position: Vector3) -> void:
	if not combat_enabled or health <= 0.0:
		return
	var forward := -global_transform.basis.z.normalized()
	var incoming := (source_position - global_position).normalized()
	var angle := rad_to_deg(forward.angle_to(incoming))
	if GameRules.should_block_bullet(is_bullet, angle, block_active, block_consumed):
		block_consumed = true
		block_active = false
		combat_event.emit("주먹으로 총알을 막았습니다!")
		return
	var guard_modifier: float = float(profile.guard_multiplier) if guarding and not is_bullet and angle <= 60.0 else 1.0
	var damage := GameRules.calculate_damage(raw_damage, 1.0, defense, false, guard_modifier)
	health = maxf(0.0, health - damage)
	health_changed.emit(health)
	_flash_hit()
	if health <= 0.0:
		combat_enabled = false
		defeated.emit(self)

func display_name() -> String:
	return "플레이어" if is_player else "상대"

func role_name() -> String:
	return ["가위·검", "바위·해머", "보·방패", "가위·총"][role]

func _roll_damage(base_damage: float) -> float:
	return base_damage * attack_modifier * (1.5 if randf() < critical_chance else 1.0)

func _reset_modifiers() -> void:
	move_modifier = 1.0
	attack_modifier = 1.0
	defense = 20.0
	critical_chance = 0.15

func _build_visuals() -> void:
	var collision := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.48
	shape.height = 2.0
	collision.shape = shape
	collision.position.y = 1.0
	add_child(collision)
	body_mesh = MeshInstance3D.new()
	var capsule := CapsuleMesh.new()
	capsule.radius = 0.48
	capsule.height = 2.0
	body_mesh.mesh = capsule
	body_mesh.position.y = 1.0
	add_child(body_mesh)
	weapon_mesh = MeshInstance3D.new()
	weapon_mesh.position = Vector3(0.65, 1.05, -0.15)
	add_child(weapon_mesh)
	_update_visuals()

func _update_visuals() -> void:
	if not is_instance_valid(body_mesh):
		return
	var material := StandardMaterial3D.new()
	material.albedo_color = [Color("59a5ff"), Color("f08a4b"), Color("7fd19b"), Color("efcb55")][role]
	body_mesh.material_override = material
	var weapon_material := StandardMaterial3D.new()
	weapon_material.albedo_color = Color("e8edf4")
	var weapon: PrimitiveMesh
	if role == GameRules.Role.HAMMER:
		var box := BoxMesh.new(); box.size = Vector3(0.55, 0.55, 1.2); weapon = box
	elif role == GameRules.Role.SHIELD:
		var cylinder := CylinderMesh.new(); cylinder.top_radius = 0.65; cylinder.bottom_radius = 0.65; cylinder.height = 0.18; weapon = cylinder
	elif role == GameRules.Role.GUN:
		var gun := BoxMesh.new(); gun.size = Vector3(0.3, 0.25, 1.0); weapon = gun
	else:
		var sword := BoxMesh.new(); sword.size = Vector3(0.14, 0.14, 1.45); weapon = sword
	weapon_mesh.mesh = weapon
	weapon_mesh.material_override = weapon_material

func _flash_hit() -> void:
	if not is_instance_valid(body_mesh):
		return
	var original: Material = body_mesh.material_override
	var flash := StandardMaterial3D.new()
	flash.albedo_color = Color.WHITE
	body_mesh.material_override = flash
	get_tree().create_timer(0.08).timeout.connect(func():
		if is_instance_valid(body_mesh): body_mesh.material_override = original
	)
