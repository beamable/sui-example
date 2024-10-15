using System.Threading.Tasks;
using Beamable.Server;

namespace Beamable.SuiFederation
{
	public class Program
	{
		/// <summary>
		/// The entry point for the <see cref="SuiFederation"/> service.
		/// </summary>
		public static async Task Main()
		{
			// inject data from the CLI.
			await MicroserviceBootstrapper.Prepare<SuiFederation>();

			// run the Microservice code
			await MicroserviceBootstrapper.Start<SuiFederation>();
		}
	}
}