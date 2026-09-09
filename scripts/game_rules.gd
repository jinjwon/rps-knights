class_name GameRules
extends RefCounted

enum Card { SCISSORS, ROCK, PAPER }
enum Outcome { LOSS = -1, TIE = 0, WIN = 1 }
enum Role { SWORD, HAMMER, SHIELD, GUN }

static func rps_result(player_card: int, opponent_card: int) -> int:
	if player_card == opponent_card:
		return Outcome.TIE
	if (player_card == Card.SCISSORS and opponent_card == Card.PAPER) \
	or (player_card == Card.ROCK and opponent_card == Card.SCISSORS) \
	or (player_card == Card.PAPER and opponent_card == Card.ROCK):
		return Outcome.WIN
	return Outcome.LOSS

static func calculate_damage(
	base_damage: float,
	attack_modifier: float,
	defense: float,
	critical: bool,
	guard_modifier: float
) -> float:
	var critical_modifier := 1.5 if critical else 1.0
	return base_damage * attack_modifier * critical_modifier * 100.0 / (100.0 + maxf(0.0, defense)) * guard_modifier

static func should_block_bullet(
	is_bullet: bool,
	angle_degrees: float,
	active: bool,
	consumed: bool
) -> bool:
	return is_bullet and active and not consumed and absf(angle_degrees) <= 60.0

static func round_winner(player_health: float, bot_health: float) -> int:
	if is_equal_approx(player_health, bot_health):
		return Outcome.TIE
	return Outcome.WIN if player_health > bot_health else Outcome.LOSS

static func is_match_over(player_wins: int, bot_wins: int, rounds_played: int) -> bool:
	return player_wins >= 3 or bot_wins >= 3 or rounds_played >= 7

static func role_profile(role: int) -> Dictionary:
	var profiles := [
		{"weapon": "검", "damage": 18.0, "range": 2.0, "cooldown": 0.55, "guard_multiplier": 0.7, "ammo": 0},
		{"weapon": "해머", "damage": 27.0, "range": 2.25, "cooldown": 0.95, "guard_multiplier": 0.7, "ammo": 0},
		{"weapon": "방패", "damage": 14.0, "range": 1.8, "cooldown": 0.65, "guard_multiplier": 0.4, "ammo": 0},
		{"weapon": "총", "damage": 24.0, "range": 12.0, "cooldown": 1.5, "guard_multiplier": 0.7, "ammo": 3},
	]
	return profiles[clampi(role, 0, profiles.size() - 1)].duplicate()

static func penalty_modifiers(penalty: int) -> Dictionary:
	var result := {"move_speed": 1.0, "attack": 1.0, "defense": 20.0, "critical_chance": 0.15}
	match penalty:
		0: result.move_speed = 0.9
		1: result.attack = 0.85
		2: result.defense = 10.0
		3: result.critical_chance = 0.05
	return result
