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

1. [Unity 2021](https://unity.com/download)
2. [Docker](https://www.docker.com/products/docker-desktop/)
3. [Net7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
4. [Git](https://git-scm.com/downloads)
5. [Sui CLI](https://docs.sui.io/guides/developer/getting-started/sui-install)
6. [NodeJS/NPM](https://nodejs.org/en/download)

## Repository structure
- /SuiFederationService - Beamable standalone microservice that implements federation features and communicates with
  SUI.
- /SampleUnityProject - Unity project with Beamable SDK and basic UI to showcase the E2E flow.
- /Move/beamable_sui_example - SUI [Move](https://docs.sui.io/concepts/sui-move-concepts) smart contracts

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
| sui           | SecretKey        | -                | SUI client private key in HEX format                   |
| sui           | PackageId        | -                | ObjectID of the published smart contract (Move package)|

### Deploying Move package
Before the microservice can interact with the SUI network, Move package must be built and deployed.  
One Move package can house definitions/modules for multiple NFTs and custom coins. 
Source for example Move package can be found under the ```/Move/beamable_sui_example/sources``` directory.
Every Move package has a package manifest in the form of a Move.toml file. Dependencies section in the Move.toml file defines a GitHub 
branch against witch the code will be built. For development purposes you can use ```framework/devnet``` and for production ```main```.
To build and publish Move package run the following commands un der the ```/Move/beamable_sui_example``` directory:
```shell
sui move build
sui client publish .\sources\game_item.move --gas-budget 20000000
```

Once you publish a Move package you can save the created packageID to the realm config:
```shell
dotnet config realm set --key-values 'sui|PackageId::value'
```

## Running Locally
The SuiFederation project can be run locally by starting it from an IDE, or by executing the `dotnet run` command.
When a Microservice is built, a web-page will be opened automatically directed to the local swagger documentation for the service.

The project may also be run in Docker by executing the following command. However, it is recommended to 
run the dotnet process locally for workflow speed. 
```shell
dotnet beam services deploy --ids SuiFederation
```

### MSBuild Properties
The `SuiFederation.csproj` file has a few configuration options that can be set.
```xml
<PropertyGroup>
    <!-- The tool path for the beamCLI. "dotnet beam" will refer to the local project tool, and "beam" would install to a globally installed tool -->
    <BeamableTool>dotnet beam</BeamableTool>

    <!-- When "true", this will open a website to the local swagger page for the running service -->
    <OpenLocalSwaggerOnRun>true</OpenLocalSwaggerOnRun>

    <!-- When "true", this will auto-generate client code to any linked unity projects -->
    <GenerateClientCode>true</GenerateClientCode>
</PropertyGroup>
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
The project can be deployed to the remote realm by running the follow command. 
```shell
dotnet beam services ps --remote
dotnet beam services deploy --remote
```

However, the realm that is used will depend on the current value of the beam CLI. The current value
can be viewed by inspecting the `./beamable/.config-defaults.json` file, or by running the following
command.
```shell
dotnet beam config
```

If the CLI is not authenticated, then you must log in and pass the `--saveToFile` flag. Otherwise, 
you will need to add the `--refresh-token` option to the `beam services` commands. 
```shell
dotnet beam login --saveToFile
```