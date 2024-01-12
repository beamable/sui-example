using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
			// inject data from the CLI
			await Prepare<SuiFederation>();
			
			// load environment variables from local file
			LoadEnvironmentVariables();
			
			// run the Microservice code
			await MicroserviceBootstrapper.Start<SuiFederation>();
		}
		
		static void LoadEnvironmentVariables(string filePath=".env")
		{
			if (!File.Exists(filePath))
				throw new Exception($"No environment file found at path=[{filePath}]");

			foreach (var line in File.ReadAllLines(filePath))
			{
				var parts = line.Split(
					'=',
					StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length != 2)
					continue;

				Environment.SetEnvironmentVariable(parts[0], parts[1]);
			}
		}

		static async Task Prepare<TMicroservice>(string? customArgs = null) where TMicroservice : Microservice
		{
			var inDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
			if (inDocker) return;

			MicroserviceAttribute attribute = typeof(TMicroservice).GetCustomAttribute<MicroserviceAttribute>();
			var serviceName = attribute.MicroserviceName;

			customArgs ??= ". --auto-deploy";

			using var process = new Process();

			process.StartInfo.FileName = "dotnet";
			process.StartInfo.Arguments = $"beam project generate-env {serviceName} {customArgs}";
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;

			process.Start();
			await process.WaitForExitAsync();

			var result = await process.StandardOutput.ReadToEndAsync();
			Console.WriteLine(result);
			if (process.ExitCode != 0)
			{
				throw new Exception($"Failed to generate-env message=[{result}]");
			}
		}
	}
}