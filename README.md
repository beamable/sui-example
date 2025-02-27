# Beamable SUI Sample

Welcome to the Beamable SUI Sample project that demonstrates how
to integrate the [SUI](https://sui.io/) services into a [Beamable](https://beamable.com/) powered game. We'll
use two Beamable federation features:

- [Federated Authentication](https://github.com/beamable/FederatedAuthentication) - use SUI non-custodial wallet or automatically 
  create one for each player
- [Federated Inventory](https://github.com/beamable/FederatedInventory) - mint NFTs for federated inventory items
  and FTs for federated inventory currency

## Requirements

Before getting started, please head to [Beamable](https://beamable.com/) and [SUI](https://blog.sui.io/sui-wallets/) to create
required accounts.
You should have the following tools installed on your development machine.

1. [Unity 2022](https://unity.com/download)
2. [Docker](https://www.docker.com/products/docker-desktop/)
3. [Net8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
6. [NodeJS/NPM](https://nodejs.org/en/download)

## Repository structure
- /sui-unity/microservices - Beamable standalone microservice that implements federation features and communicates with
  SUI.
- /sui-unity - Unity project with Beamable SDK and basic UI to showcase the E2E flow.

## Getting Started
Navigate into the ```/SuiFederationService``` directory and run this commands to initialize the Beamable CLI and
connect the service to your Beamable organization and game:

```shell
dotnet tool restore
dotnet beam init --save-to-file
```

### Configuration
Configuration defaults are hard-coded inside **SuiFederationService/services/SuiFederation/Configuration.cs**  
You can override the values using the realm configuration.  
![Realm Configuration Example](Screenshots/realm-config.png)

**Default values:**  
| **Namespace** | **Key**          |**Default value** |**Description**                                         |
|---------------|------------------|------------------|--------------------------------------------------------|
| sui           | SuiEnvironment   | devnet           | SUI network name (mainnet, testnet, devnet, localnet)  |


## Running Locally
The SuiFederation project can be run locally by starting it from an IDE, or by executing the `dotnet run` command.
When a Microservice is built, a web-page will be opened automatically directed to the local swagger documentation for the service.

The project may also be run in Docker by executing the following command. However, it is recommended to 
run the dotnet process locally for workflow speed. 
```shell
dotnet beam services run --ids SuiFederation
```

#### BeamableTool
By default, both SuiFederation and SuiFederationCommon have the BeamCLI installed as a project tool. 
However, it is likely that `beam` is installed as a global tool, and as such, the `BeamableTool` value
could be changed to `beam`. 

#### OpenLocalSwaggerOnRun
If the `OpenLocalSwaggerOnRun` setting is set to `false`, then the local swagger will not open when the project is
built. However, it can be opened manually by the following shell command. Optionally, pass the `--remote` flag to open the
remote swagger. 
```shell
dotnet beam project open-swagger SuiFederation
```

### Connecting to a Unity Project
The project can be linked to a number of Unity Projects. The `./beamable/.linkedProjects.json` file 
contains the relative paths to linked Unity Projects. You can add an association by editing that file, 
or by executing
```shell
dotnet beam project add-unity-project .
```

When the project is built, the `generate-client` msbuild target will generate a C# client file
at each linked Unity project's `Assets/Beamable/Microservice/Microservices/SuiFederationClient.cs`. 

Additionally, the `share-code` msbuild target in the SuiFederationCommon project will copy their
built `.dll` files into the `Assets/Beamable/Microservices/CommonDlls` directory. Unity will be able
to share the `.dll` files meant for the shared library. 

## Deploying
The project can be deployed to the remote realm by running the follow commands. 
```shell
dotnet beam deploy plan
dotnet beam deploy release --latest-plan
```