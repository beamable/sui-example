using Beamable.Server;

namespace Beamable.SuiFederationService
{
	[Microservice("SuiFederationService")]
	public class SuiFederationService : Microservice
	{
		[ClientCallable]
		public int Add(int a, int b)
		{
			return a + b;
		}
	}
}
