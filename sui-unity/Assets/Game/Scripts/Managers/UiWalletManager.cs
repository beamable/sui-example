using System;
using System.Runtime.InteropServices;
using Beamable.Common;
using Beamable.Player;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Beam;

namespace MoeBeam.Game.Scripts.Managers
{
    public class UiWalletManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI walletAddressText;
        [SerializeField] private TMP_InputField messageInput;

        private string _walletAddress;
        private string _challenge;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ConnectWallet(string callbackMethod);

        [DllImport("__Internal")]
        private static extern void DisconnectWallet(string callbackMethod);

        [DllImport("__Internal")]
        private static extern void SignMessage(string message, string callbackMethod);
#else
        private static void ConnectWallet(string callbackMethod)
        {
            Debug.Log($"[Editor Stub] ConnectWallet({callbackMethod})");
        }

        private static void DisconnectWallet(string callbackMethod)
        {
            Debug.Log($"[Editor Stub] DisconnectWallet({callbackMethod})");
        }

        private static void SignMessage(string message, string callbackMethod)
        {
            Debug.Log($"[Editor Stub] SignMessage({message}, {callbackMethod})");
        }
#endif

        private const string WalletName = "Stashed";

        public void OnClickConnectWallet()
        {
            try
            {
                ConnectWallet(nameof(OnWalletConnected));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting wallet: {e.Message}");
            }
        }

        public void OnClickDisconnectWallet()
        {
            try
            {
                DisconnectWallet(nameof(OnWalletDisconnected));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error disconnecting wallet: {e.Message}");
            }
        }

        public void OnClickSignMessage()
        {
            try
            {
                var msg = messageInput.text;
                SignMessage(msg, nameof(OnMessageSigned));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error signing message: {e.Message}");
            }
        }

        // Called by JS
        public void OnWalletConnected(string address)
        {
            walletAddressText.text = $"Wallet: {address}";
            Debug.Log($"Wallet connected: {address}");

            if (!string.IsNullOrEmpty(address))
            {
                _walletAddress = address;

                try
                {
                    AsyncChallengeHandler challengeHandler = new AsyncChallengeHandler(e =>
                    {
                        Debug.Log($"Challenge = {e}");
                        return Promise<string>.Successful(e);
                    });
                    messageInput.text = address;

                    // BeamAccountManager.Instance.AddStashedExternalIdentity(address, challengeHandler).ContinueWith(
                    //     result =>
                    //     {
                    //         Debug.Log($"Stashed external identity done");
                    //         //messageInput.text = result.account.token;
                    //     });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error connecting to wallet: {e.Message}");
                }
            }
        }

        public void OnWalletDisconnected(string _ = null)
        {
            walletAddressText.text = "Disconnected";
            Debug.Log("Wallet disconnected");
        }

        public void OnMessageSigned(string signed)
        {
            try
            {
                AsyncChallengeHandler challengeHandler = new AsyncChallengeHandler(e =>
                {
                    Debug.Log($"Challenge = {signed}");
                    return Promise<string>.Successful(signed);
                });
                BeamAccountManager.Instance.AddStashedExternalIdentity(signed, null).ContinueWith(
                    result =>
                    {
                        messageInput.text = result.account.ExternalIdentities[0].userId;
                        Debug.Log($"Stashed external identity {messageInput.text}");
                    });
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting to wallet: {e.Message}");
            }
            Debug.Log($"Signed Message: {signed}");
        }
    }
}
