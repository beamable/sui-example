using Beamable;
using Beamable.Server;
using Beamable.Server.Clients;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Beam
{
    public class BeamManager : GenericSingleton<BeamManager>
    {
        #region EXPOSED_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES
        
        public static BeamContext BeamContext { get; private set; }
        //public static SkullRevengeServiceClient SkullClient {get; private set;}
        public static SuiFederationClient SuiClient { get; private set; }
        public static bool IsReady { get; private set; }

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            Init().Forget();
            DontDestroyOnLoad(this);
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS

        private async UniTask Init()
        {
            BeamContext = BeamContext.Default;
            await BeamContext.OnReady;
            //SkullClient = new SkullRevengeServiceClient(BeamContext);
            SuiClient = new SuiFederationClient(BeamContext);
            await UniTask.Delay(1000); //fake delay
            IsReady = true;
        }

        #endregion

        
    }
}