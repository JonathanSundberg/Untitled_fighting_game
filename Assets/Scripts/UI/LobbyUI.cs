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
        [SerializeField] private TextButton _connectButton;
        [SerializeField] private TextButton _disconnectButton;
        [SerializeField] private TextButton _deleteButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TextButton _sendButton;

        private DiscordLobby _lobby;
        
        public void Initialize(DiscordLobby lobby)
        {
            _lobby = lobby;
            _connectButton.OnClick += () =>
            {
                _lobby.ConnectNetwork();
                _connectButton.gameObject.SetActive(false);
                _disconnectButton.gameObject.SetActive(true);
            };
            
            _disconnectButton.OnClick += () =>
            {
                _lobby.DisconnectNetwork();
                _connectButton.gameObject.SetActive(true);
                _disconnectButton.gameObject.SetActive(false);
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