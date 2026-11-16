// Smolderfang, the Monster at the end of the North path. Shared stats, attack rules and
// stats display live in Monster; the Dragon supplies its name, d20 stats, claws and messages.
public class Dragon : Monster
{
    private static readonly IReadOnlyList<string> _defenseTauntKeys = new[]
    {
        "dragon_defense_taunt_1",
        "dragon_defense_taunt_2",
        "dragon_defense_taunt_3",
        "dragon_defense_taunt_4"
    };

    private static readonly IReadOnlyList<string> _damageReplyKeys = new[]
    {
        "dragon_damage_reply_1",
        "dragon_damage_reply_2",
        "dragon_damage_reply_3",
        "dragon_damage_reply_4"
    };

    // The dragon uses a placeholder weapon object for now.
    // This can be refined later if combat requirements expand.
    public Dragon(Die die) : base(die, new Weapon("claws", 12, "<<< claws >>>"))
    {
        Name = "Smolderfang";

        // Dragon stats are initialized using Die so the class aligns better
        // with the sequence/start-game diagrams and shared repo structure.
        Strength = _die.Roll(20);
        Agility = _die.Roll(20);
        HealthPoints = _die.Roll(20);
    }

    protected override string StatsHeaderKey => "dragon_stats_header";
    protected override string CombatHeaderKey => "combat_dragon_header";
    protected override string DefeatedNarrativeKey => "dragon_defeated_narrative";
    protected override string PlayerAttackIntroKey => "player_attack_intro";
    protected override IReadOnlyList<string> DefenseTauntKeys => _defenseTauntKeys;
    protected override IReadOnlyList<string> DamageReplyKeys => _damageReplyKeys;
}
