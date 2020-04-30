using System;
using System.Text;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextButton _lobbyConnectButton;
        [SerializeField] private TextButton _lobbyDisconnectButton;
        [SerializeField] private TextButton _networkConnectButton;
        [SerializeField] private TextButton _networkDisconnectButton;
        [SerializeField] private TextButton _deleteButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TextButton _sendButton;

        private DiscordLobby _lobby;
        
        public void Initialize(DiscordLobby lobby)
        {
            _lobby = lobby;
            
            _lobbyConnectButton.OnClick += async () =>
            {
                await _lobby.Connect();
                _lobbyConnectButton.gameObject.SetActive(false);
                _lobbyDisconnectButton.gameObject.SetActive(true);
            };
            
            _lobbyDisconnectButton.OnClick += async () =>
            {
                await _lobby.Disconnect();
                _lobbyConnectButton.gameObject.SetActive(true);
                _lobbyDisconnectButton.gameObject.SetActive(false);
            };
            
            _networkConnectButton.OnClick += async () =>
            {
                _lobby.ConnectNetwork();
                _networkConnectButton.gameObject.SetActive(false);
                _networkDisconnectButton.gameObject.SetActive(true);
            };
            
            _networkDisconnectButton.OnClick += () =>
            {
                _lobby.DisconnectNetwork();
                _networkConnectButton.gameObject.SetActive(true);
                _networkDisconnectButton.gameObject.SetActive(false);
            };
            
            _deleteButton.OnClick += () => _lobby.Delete();
            
            _sendButton.OnClick += () => _lobby.SendNetworkMessage(Encoding.UTF8.GetBytes(_inputField.text));
            
            _lobby.OnNetworkMessage += NetworkMessage;
        }

        private void NetworkMessage(long userid, byte[] data)
        {
            _inputField.text = Encoding.UTF8.GetString(data);
        }
    }
}