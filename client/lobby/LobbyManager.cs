using Godot;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HttpClient = System.Net.Http.HttpClient;
using spire.config;

namespace spire.lobby;

public partial class LobbyManager : Node
{
    private readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });
    private Button? AccountEnterButton { get; set; }
    private LineEdit? AccountEnterInput { get; set; }
    
    public override void _Ready()
    {
        AccountEnterButton = GetNode<Button>("AccountEnter/Button");
        AccountEnterButton.Pressed += OnAccountEnterButton;

        AccountEnterInput = GetNode<LineEdit>("AccountEnter/Input");

        // Load Config
        _ = Config.LobbyAddress;
    }

    private void OnAccountEnterButton()
    {
        var devId = AccountEnterInput!.Text;
        
        //TODO: Disable button
        
        Task.Run(async () =>
        {
            var characterList = await AcquireCharacterList(devId);
            if (characterList == null)
                return;
            
            DisplayCharacterList(characterList);
        });
    }

    private async Task<Character[]?> AcquireCharacterList(string devId)
    {
        var myData = new Dictionary<string, object>
        {
            { "dev_id", devId }
        };
        var myResponse = await Request<MyDevAccountResponse>(
            $"https://{Config.LobbyAddress}/account/dev/me", myData);
        if (myResponse == null)
            return null;

        var accountId = myResponse.AccountId;
        if (!myResponse.Found)
        {
            GD.Print($"Dev account {devId} not found.");
                
            var createData = new Dictionary<string, object>
            {
                { "dev_id", devId }
            };
            var createResponse = await Request<DevAccountCreateResponse>(
                $"https://{Config.LobbyAddress}/account/dev/create", createData);
            if (createResponse == null)
                return null;

            accountId = createResponse.AccountId;
        }
            
        var characterListData = new Dictionary<string, object>
        {
            { "account_id", accountId }
        };
        var characterListResponse = await Request<CharacterListResponse>(
            $"https://{Config.LobbyAddress}/character/list", characterListData);
        return characterListResponse?.Characters;
    }

    private void DisplayCharacterList(Character[] characters)
    {
        var characterSlotContainer = GetNode("CharacterPage/CharacterSlots")!;
        var characterSlotScene = GD.Load<PackedScene>("res://lobby/character_slot.tscn");
        
        foreach (var character in characters)
        {
            var characterSlot = characterSlotScene.Instantiate<CharacterSlot>();
            characterSlot.CharacterId = character.Id;
            characterSlot.NameLabel.Text = character.Name;
            characterSlot.RaceLabel.Text = character.Race;
            
            characterSlotContainer.AddChild(characterSlot);
        }
    }

    private async Task<TResponse?> Request<TResponse>(string url, Dictionary<string, object> data)
    {
        GD.Print($"Requesting to {url}: {data}");
        var content = new StringContent
        (
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<TResponse>(stream);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error posting {url}: {e}");
            return default;
        }
    }

    private class MyDevAccountResponse
    {
        [JsonPropertyName("found")]
        public bool Found { get; init; }
        [JsonPropertyName("account_id")]
        public ulong AccountId { get; init; }
    }
    
    private class DevAccountCreateResponse
    {
        [JsonPropertyName("account_id")]
        public ulong AccountId { get; init; }
    }

    private class CharacterListResponse
    {
        [JsonPropertyName("characters")]
        public Character[] Characters { get; init; }
    }

    private record Character
    {
        [JsonPropertyName("id")]
        public ulong Id { get; init; }
        [JsonPropertyName("name")]
        public string Name { get; init; }
        [JsonPropertyName("race")]
        public string Race { get; init; }
    }
}
