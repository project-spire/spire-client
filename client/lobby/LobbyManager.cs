using Godot;
using System.Net;
using System.Text;
using System.Text.Json;
using HttpClient = System.Net.Http.HttpClient;
using spire.config;

namespace spire.lobby;

public partial class LobbyManager : Node
{
    private readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });
    private Button? AccountEnterButton { get; set; }
    
    public override void _Ready()
    {
        AccountEnterButton = GetNode<Button>("AccountContainer/EnterButton");
        AccountEnterButton.Pressed += OnAccountEnterButtonPressed;
    }

    private void OnAccountEnterButtonPressed()
    {
        var devId = AccountEnterButton!.Text;
        
        Task.Run(async () =>
        {
            var response = await RequestMyDevAccount(devId);
            if (response == null)
                return;
        });
    }

    private async Task<AccountResponse?> RequestMyDevAccount(string devId)
    {
        var url = $"{Config.LobbyAddress}/account/dev/me";
        var data = new Dictionary<string, object>
        {
            { "dev_id", devId }
        };
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
            return await JsonSerializer.DeserializeAsync<AccountResponse>(stream);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error posting {url}: {e}");
            return null;
        }
    }

    private class AccountResponse
    {
        public ulong AccountId { get; init; }
    }
}
