using Godot;

namespace spire.lobby;

public partial class CharacterSlot : Control
{
    [Export]
    public required Label NameLabel { get; set; }
    
    [Export]
    public required Label RaceLabel { get; set; }
    
    public ulong? CharacterId { get; set; } 
}
