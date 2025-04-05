using Godot;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using FileAccess = Godot.FileAccess;

namespace spire.config;

public static class Config
{
    public static readonly string LobbyHost;
    public static readonly ushort LobbyPort;
    public static string LobbyAddress => $"{LobbyHost}:{LobbyPort}";
    
    public static readonly string GameHost;
    public static readonly ushort GamePort;
    public static string GameAddress => $"{GameHost}:{GamePort}";
    
    static Config()
    {
        GD.Print("Loading config...");
        
        using var file = FileAccess.Open("res://config.yaml", FileAccess.ModeFlags.Read);
        var content = file.GetAsText();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var data = deserializer.Deserialize<Data>(content);

        LobbyHost = data.LobbyHost;
        LobbyPort = data.LobbyPort;
        
        GameHost = data.GameHost;
        GamePort = data.GamePort;
        
        GD.Print("Loading config done!");
    }

    private struct Data
    {
        public string LobbyHost { get; init; }
        public ushort LobbyPort { get; init; }
        
        public string GameHost { get; init; }
        public ushort GamePort { get; init; }
    }
}