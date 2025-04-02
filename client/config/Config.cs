using Godot;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using FileAccess = Godot.FileAccess;

namespace spire.config;

public static class Config
{
    public static readonly string LobbyHost;
    public static readonly ushort LobbyPort;
    public static string LobbyAddress => LobbyHost + LobbyPort;
    
    public static readonly string GameHost;
    public static readonly ushort GamePort;
    public static string GameAddress => GameHost + GamePort;
    
    static Config()
    {
        GD.Print("Loading config...");
        
        using var file = FileAccess.Open("res://config.yaml", FileAccess.ModeFlags.Read);
        var content = file.GetAsText();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var data = deserializer.Deserialize<Data>(content);

        LobbyHost = data.lobbyHost;
        LobbyPort = data.lobbyPort;
        
        GameHost = data.gameHost;
        GamePort = data.gamePort;
        
        GD.Print("Loading config done!");
    }

    private struct Data
    {
        public string lobbyHost;
        public ushort lobbyPort;
        
        public string gameHost;
        public ushort gamePort;
    }
}