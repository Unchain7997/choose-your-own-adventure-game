// Grimtusk, the cave troll guarding the mountain pass on the West path (see GLOSSARY.md: Troll Encounter).
// Shared stats, attack rules and stats display live in Monster; the Troll supplies his name,
// d12/d10/d12 stats, club and messages.
public class Troll : Monster
{
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

    // Beaten for the rest of the adventure. He starts at 1-12 HealthPoints, so in normal play this
    // becomes true only after a killing blow; a Grimtusk starting at 0 or below is a test fixture.
    public bool IsDefeated => HealthPoints <= 0;

    // The Gold in his stash: a d10 rolled when he is beaten. _die is the shared Game Die: StartGame creates
    // one Die and hands that same object to the Dragon and to each adventure's Grimtusk.
    // Internal: only Game calls it, so it isn't part of the public API.
    internal int RollStashGold()
    {
        return _die.Roll(10);
    }

    protected override string StatsHeaderKey => "troll_stats_header";
    protected override string CombatHeaderKey => "combat_troll_header";
    protected override string DefeatedNarrativeKey => "troll_defeated_narrative";
    protected override string PlayerAttackIntroKey => "troll_player_attack_intro";
    protected override IReadOnlyList<string> DefenseTauntKeys => _defenseTauntKeys;
    protected override IReadOnlyList<string> DamageReplyKeys => _damageReplyKeys;
}
