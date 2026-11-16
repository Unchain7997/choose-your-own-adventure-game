// Grimtusk, the cave troll guarding the mountain pass on the West path (see GLOSSARY.md: Troll Encounter).
// Shared stats, attack rules and stats display live in Monster; the Troll supplies his name,
// d12/d10/d12 stats, club and messages.
public class Troll : Monster
{
    // Combat with Grimtusk arrives in Ticket 03, which adds these messages to the language data.
    // Until then no code path can show them.
    private static readonly IReadOnlyList<string> _defenseTauntKeys = new[]
    {
        "troll_defense_taunt_1",
        "troll_defense_taunt_2",
        "troll_defense_taunt_3",
        "troll_defense_taunt_4"
    };

    private static readonly IReadOnlyList<string> _damageReplyKeys = new[]
    {
        "troll_damage_reply_1",
        "troll_damage_reply_2",
        "troll_damage_reply_3",
        "troll_damage_reply_4"
    };

    public Troll(Die die) : base(die, new Weapon("club", 8, "(@)======"))
    {
        Name = "Grimtusk";

        Strength = _die.Roll(12);
        Agility = _die.Roll(10);
        HealthPoints = _die.Roll(12);
    }

    protected override string StatsHeaderKey => "troll_stats_header";
    protected override string CombatHeaderKey => "combat_troll_header";
    protected override string DefeatedNarrativeKey => "troll_defeated_narrative";
    protected override string PlayerAttackIntroKey => "troll_player_attack_intro";
    protected override IReadOnlyList<string> DefenseTauntKeys => _defenseTauntKeys;
    protected override IReadOnlyList<string> DamageReplyKeys => _damageReplyKeys;
}
