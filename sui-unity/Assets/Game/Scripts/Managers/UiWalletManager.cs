using System;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Beam;
using TMPro;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Managers
{
    public class UiWalletManager : MonoBehaviour
    {
        // public void OnWalletConnected(string address)
        // {
        //     Debug.Log("Wallet connected: " + address);
        // }
        //
        // public void OnWalletDisconnected()
        // {
        //     Debug.Log("Wallet disconnected");
        // }
        //
        // public void OnMessageSigned(string signedMessage)
        // {
        //     Debug.Log("Signed Message: " + signedMessage);
        // }
        
        [SerializeField] private TextMeshProUGUI walletAddressText;
        [SerializeField] private TMP_InputField messageInput;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ConnectWallet();

    [DllImport("__Internal")]
    private static extern void DisconnectWallet();

    [DllImport("__Internal")]
    private static extern void SignMessage(string message);
#else
        private static void ConnectWallet() => Debug.Log("ConnectWallet() - Editor stub");
        private static void DisconnectWallet() => Debug.Log("DisconnectWallet() - Editor stub");
        private static void SignMessage(string message) => Debug.Log("SignMessage() - Editor stub");
#endif

        //1
        public void OnClickConnectWallet()
        {
            try
            {
                ConnectWallet();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting wallet: {e.Message}");
            }
        }
        
        // These will be called from JS via SendMessage
        public void OnWalletConnected(string address)
        {
            walletAddressText.text = $"Wallet: {address}";
            Debug.Log($"Wallet connected: {address}");
            //messageInput.text = address;
            try
            {
                BeamAccountManager.Instance.AddStashedExternalIdentity(address).ContinueWith(result =>
                {
                    messageInput.text = result.ToString();
                    Debug.Log($"Stashed external identity {result.ToString()}");
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Error adding stashed external identity: {e.Message}");
            }
        }

        public void OnClickDisconnectWallet()
        {
            DisconnectWallet();
        }

        public void OnClickSignMessage()
        {
            try
            {
                SignMessage(messageInput.text);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error signing message: {e.Message}");
            }
        }

        

        public void OnWalletDisconnected()
        {
            walletAddressText.text = "Disconnected";
            Debug.Log("Wallet disconnected");
        }

        public void OnMessageSigned(string signed)
        {
            Debug.Log($"Signed Message: {signed}");
        }
    }
}