﻿using System;
using Beamable;
using Beamable.Player;
using Beamable.Server.Clients;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Managers;
using SuiFederationCommon;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Beam
{
    public class BeamAccountManager : GenericSingleton<BeamAccountManager>
    {
        #region EXPOSED_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES

        private BeamContext _beamContext;

        #endregion

        #region PUBLIC_VARIABLES

        public bool NewAccountCreated { get; private set; } = false;
        public long PlayerId { get; private set; }
        public PlayerAccount CurrentAccount { get; private set; }

        #endregion

        #region Actions

        public static event Action<PlayerAccount> OnSetCurrentAccount;

        #endregion

        #region UNITY_CALLS

        private async void Start()
        {
            try
            {
                await UniTask.WaitUntil(() => BeamManager.IsReady);
                _beamContext = BeamManager.BeamContext;
                await _beamContext.Accounts.OnReady;
            }
            catch (Exception e)
            {
                Debug.LogError($"AccountSignIn Start error: {e.Message}");
            }
        }

        #endregion

        #region PRIVATE_METHODS

        private async UniTask SwitchAccount(PlayerAccount newAccount)
        {
            await _beamContext.Accounts.SwitchToAccount(newAccount);
            if (newAccount is {HasDeviceId: false})
            {
                await _beamContext.Accounts.AddDeviceId(newAccount);
            }

            UpdateCurrentAccount(newAccount);
        }

        private void UpdateCurrentAccount(PlayerAccount newAccount)
        {
            CurrentAccount = newAccount;
            PlayerId = CurrentAccount?.GamerTag ?? 0;

            OnSetCurrentAccount?.Invoke(newAccount);
        }

        #endregion

        #region PUBLIC_METHODS
        
        public async UniTask ChangeAlias(string alias)
        {
            try
            {
                await _beamContext.Accounts.SetAlias(alias, CurrentAccount);
                UpdateCurrentAccount(CurrentAccount);
                NewAccountCreated = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"ChangeName {alias} error: {e.Message}");
            }
        }

        public async UniTask CreateNewAccount()
        {
            try
            {
                var newAccount = await _beamContext.Accounts.CreateNewAccount();
                await SwitchAccount(newAccount);
                var result = await _beamContext.Accounts.AddExternalIdentity<SuiWeb3Identity, SuiFederationClient>("", (AsyncChallengeHandler) null, newAccount);
                UpdateCurrentAccount(newAccount);
                
            }
            catch (Exception e)
            {
                Debug.LogError($"Create New Account error: {e.Message}");
            }
        }

        #endregion

    }
}