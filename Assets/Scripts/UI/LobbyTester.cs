using System;
using Discord;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Network;
using UI;

public class LobbyTester : MonoBehaviour
{
    [SerializeField] public Button _createLobbyButton;
    [SerializeField] public Button _searchForLobbies;
    [Space]
    [SerializeField] public Transform _lobbyList;
    [SerializeField] public GameObject _lobbyTemplate;
    
    private void Start()
    {
        _createLobbyButton.onClick.AddListener(CreateLobby);
        _searchForLobbies.onClick.AddListener(SearchForLobbies);
    }

    private async void CreateLobby()
    {
        var lobby = await DiscordLobby.Create((ref LobbyTransaction transaction) =>
        {
            transaction.SetCapacity(32);
            transaction.SetType(LobbyType.Public);
        });
        
        lobby.OnNetworkMessage += LobbyOnOnNetworkMessage;
    }

    private void LobbyOnOnNetworkMessage(long userId, byte[] data)
    {
        var value = BitConverter.ToInt32(data, 0);
    }

    private async void SearchForLobbies()
    {
        var lobbies = await DiscordLobby.Search();
        
        foreach (var child in _lobbyList.GetChildren())
        {
            Destroy(child.gameObject);
        }

        foreach (var lobby in lobbies)
        {
            var button = Instantiate(_lobbyTemplate, _lobbyList).GetComponent<LobbyUI>();
                
            button.gameObject.SetActive(true);
            button.Initialize(lobby);
        }
    }
}
