using System.Linq;
using System.Reflection;
using Beamable.Common.Dependencies;
using Beamable.SuiFederationService;
using SuiFederationService.Extensions;

namespace SuiFederationService.Features;

public static class ServiceRegistration
{
    public static void AddFeatures(this IDependencyBuilder builder)
    {
        Assembly.GetExecutingAssembly()
            .GetDerivedTypes<IService>()
            .ToList()
            .ForEach(serviceType => builder.AddSingleton(serviceType));
    }
}