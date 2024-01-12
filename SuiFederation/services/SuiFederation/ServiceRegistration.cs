using System.Linq;
using System.Reflection;
using Beamable.Common.Dependencies;
using Beamable.SuiFederation;
using SuiFederation.Extensions;

namespace Beamable.SuiFederation;

public static class ServiceRegistration
{
    public static void RegisterServices(this IDependencyBuilder builder)
    {
        Assembly.GetExecutingAssembly()
            .GetDerivedTypes<IService>()
            .ToList()
            .ForEach(serviceType => builder.AddSingleton(serviceType));
    }
}