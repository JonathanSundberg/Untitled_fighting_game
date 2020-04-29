using System.Collections;
using System.Collections.Generic;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTester : MonoBehaviour
{
    [SerializeField] public Toggle _instanceToggle;
    [SerializeField] public Button _createLobbyButton;
    [SerializeField] public Button _searchForLobbies;
    [Space]
    [SerializeField] public Transform _lobbyList;
    [SerializeField] public GameObject _lobbyTemplate;
    
    // Start is called before the first frame update
    void Start()
    {
        _instanceToggle.onValueChanged.AddListener(ToggleInstance);
        _createLobbyButton.onClick.AddListener(CreateLobby);
        _searchForLobbies.onClick.AddListener(SearchForLobbies);
    }

    private void ToggleInstance(bool value)
    {
        DiscordManager.UseSecondInstance = value;
    }

    private async void CreateLobby()
    {
        var lobby = await DiscordManager.CreateLobby();
    }
    
    private async void SearchForLobbies()
    {
        var lobbies = await DiscordManager.SearchForLobbies();
        
        foreach (var child in _lobbyList.GetChildren())
        {
            Destroy(child.gameObject);
        }

        foreach (var lobby in lobbies)
        {
            Instantiate(_lobbyTemplate, _lobbyList)
                .GetComponent<TextMeshProUGUI>()
                .text = lobby.ToString();
        }
    }
}
