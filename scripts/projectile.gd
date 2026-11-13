class_name BattleProjectile
extends Node3D

var owner_fighter: Fighter
var target: Fighter
var direction := Vector3.ZERO
var speed := 12.0
var remaining_distance := 12.0
var damage := 24.0

func setup(source: Fighter, target_fighter: Fighter, shot_damage: float) -> void:
	owner_fighter = source
	target = target_fighter
	damage = shot_damage
	global_position = source.global_position + Vector3.UP * 1.2
	direction = (target.global_position + Vector3.UP - global_position).normalized()
	var mesh := MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 0.13
	sphere.height = 0.26
	mesh.mesh = sphere
	var material := StandardMaterial3D.new()
	material.albedo_color = Color(1.0, 0.78, 0.12)
	material.emission_enabled = true
	material.emission = Color(1.0, 0.35, 0.02)
	mesh.material_override = material
	add_child(mesh)

func _physics_process(delta: float) -> void:
	if not is_instance_valid(target) or not target.combat_enabled:
		queue_free()
		return
	var step := speed * delta
	global_position += direction * step
	remaining_distance -= step
	if global_position.distance_to(target.global_position + Vector3.UP) < 0.7:
		target.receive_attack(damage, true, owner_fighter.global_position)
		queue_free()
	elif remaining_distance <= 0.0:
		queue_free()
