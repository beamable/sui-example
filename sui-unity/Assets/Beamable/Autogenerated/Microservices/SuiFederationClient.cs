//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Beamable.Server.Clients
{
    using System;
    using Beamable.Platform.SDK;
    using Beamable.Server;
    
    
    /// <summary> A generated client for <see cref="Beamable.SuiFederation.SuiFederation"/> </summary
    public sealed class SuiFederationClient : MicroserviceClient, Beamable.Common.IHaveServiceName, Beamable.Common.ISupportsFederatedLogin<SuiFederationCommon.SuiWeb3Identity>, Beamable.Common.ISupportsFederatedInventory<SuiFederationCommon.SuiWeb3Identity>
    {
        
        public SuiFederationClient(BeamContext context = null) : 
                base(context)
        {
        }
        
        public string ServiceName
        {
            get
            {
                return "SuiFederation";
            }
        }
        
        /// <summary>
        /// Call the GetRealmAccount method on the SuiFederation microservice
        /// <see cref="Beamable.SuiFederation.SuiFederation.GetRealmAccount"/>
        /// </summary>
        public Beamable.Common.Promise<string> GetRealmAccount()
        {
            System.Collections.Generic.Dictionary<string, object> serializedFields = new System.Collections.Generic.Dictionary<string, object>();
            return this.Request<string>("SuiFederation", "GetRealmAccount", serializedFields);
        }
        
        /// <summary>
        /// Call the GenerateRealmAccount method on the SuiFederation microservice
        /// <see cref="Beamable.SuiFederation.SuiFederation.GenerateRealmAccount"/>
        /// </summary>
        public Beamable.Common.Promise<string> GenerateRealmAccount()
        {
            System.Collections.Generic.Dictionary<string, object> serializedFields = new System.Collections.Generic.Dictionary<string, object>();
            return this.Request<string>("SuiFederation", "GenerateRealmAccount", serializedFields);
        }
        
        /// <summary>
        /// Call the ImportRealmAccount method on the SuiFederation microservice
        /// <see cref="Beamable.SuiFederation.SuiFederation.ImportRealmAccount"/>
        /// </summary>
        public Beamable.Common.Promise<string> ImportRealmAccount(string privateKey)
        {
            object raw_privateKey = privateKey;
            System.Collections.Generic.Dictionary<string, object> serializedFields = new System.Collections.Generic.Dictionary<string, object>();
            serializedFields.Add("privateKey", raw_privateKey);
            return this.Request<string>("SuiFederation", "ImportRealmAccount", serializedFields);
        }
        
        /// <summary>
        /// Call the ImportAccount method on the SuiFederation microservice
        /// <see cref="Beamable.SuiFederation.SuiFederation.ImportAccount"/>
        /// </summary>
        public Beamable.Common.Promise<string> ImportAccount(string id, string privateKey)
        {
            object raw_id = id;
            object raw_privateKey = privateKey;
            System.Collections.Generic.Dictionary<string, object> serializedFields = new System.Collections.Generic.Dictionary<string, object>();
            serializedFields.Add("id", raw_id);
            serializedFields.Add("privateKey", raw_privateKey);
            return this.Request<string>("SuiFederation", "ImportAccount", serializedFields);
        }
        
        /// <summary>
        /// Call the InitializeContentContracts method on the SuiFederation microservice
        /// <see cref="Beamable.SuiFederation.SuiFederation.InitializeContentContracts"/>
        /// </summary>
        public Beamable.Common.Promise<string> InitializeContentContracts()
        {
            System.Collections.Generic.Dictionary<string, object> serializedFields = new System.Collections.Generic.Dictionary<string, object>();
            return this.Request<string>("SuiFederation", "InitializeContentContracts", serializedFields);
        }
        
        /// <summary>
        /// Call the Test method on the SuiFederation microservice
        /// <see cref="Beamable.SuiFederation.SuiFederation.Test"/>
        /// </summary>
        public Beamable.Common.Promise<Beamable.Common.Unit> Test()
        {
            System.Collections.Generic.Dictionary<string, object> serializedFields = new System.Collections.Generic.Dictionary<string, object>();
            return this.Request<Beamable.Common.Unit>("SuiFederation", "Test", serializedFields);
        }
    }
    
    internal sealed class MicroserviceParametersSuiFederationClient
    {
        
        [System.SerializableAttribute()]
        internal sealed class ParameterSystem_String : MicroserviceClientDataWrapper<string>
        {
        }
    }
    
    [BeamContextSystemAttribute()]
    public static class ExtensionsForSuiFederationClient
    {
        
        [Beamable.Common.Dependencies.RegisterBeamableDependenciesAttribute()]
        public static void RegisterService(Beamable.Common.Dependencies.IDependencyBuilder builder)
        {
            builder.AddScoped<SuiFederationClient>();
        }
        
        public static SuiFederationClient SuiFederation(this Beamable.Server.MicroserviceClients clients)
        {
            return clients.GetClient<SuiFederationClient>();
        }
    }
}
