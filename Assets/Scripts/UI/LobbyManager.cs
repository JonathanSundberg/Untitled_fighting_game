using System.Collections.Generic;
using Discord;
using Network;
using UnityEngine;

// ReSharper disable LocalVariableHidesMember

namespace UI
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private TextButton _createLobbyButton;
        [SerializeField] private GameObject _fightRequestTemplate;
        [SerializeField] private Transform _lobbyList;
        [SerializeField] private float _updateInterval;

        private List<GameObject> _fightRequests;
        private float _timeSinceLastUpdate;

        private void Start()
        {
            _fightRequests = new List<GameObject>();
        
            _createLobbyButton.OnClick += CreateLobby;
        }

        private async void CreateLobby()
        {
            var lobby = await DiscordLobby.Create((ref LobbyTransaction transaction) =>
            {
                transaction.SetType(LobbyType.Public);
                transaction.SetCapacity(2);
            });
        
            NetplayGameManager.CreateMatch(lobby);
            _lobbyList.gameObject.SetActive(false);
            _createLobbyButton.gameObject.SetActive(false);
        }

        private async void Update()
        {
            _timeSinceLastUpdate += Time.deltaTime;
            if (_timeSinceLastUpdate < _updateInterval) return;
            _timeSinceLastUpdate -= _updateInterval;

            var lobbies = await DiscordLobby.Search();
        
            foreach (var fightRequest in _fightRequests)
            {
                Destroy(fightRequest);
            }
        
            _fightRequests.Clear();
        
            foreach (var lobby in lobbies)
            {
                var gameObject = Instantiate(_fightRequestTemplate, _lobbyList);
                var button = gameObject.GetComponent<TextButton>();
                
                var owner = await lobby.Owner;

                gameObject.SetActive(true);
                button.Label = $"{owner.Username}#{owner.Discriminator}";

                if (owner.Id != DiscordManager.CurrentUser.Id)
                {
                    button.OnClick += () =>
                    {
                        NetplayGameManager.JoinMatch(lobby);
                        _lobbyList.gameObject.SetActive(false);
                        _createLobbyButton.gameObject.SetActive(false);
                    };
                }
                else
                {
                    button.OnClick += () =>
                    {
                        lobby.Delete();
                    };
                }

                _fightRequests.Add(gameObject);
            }
        }
    }
}
