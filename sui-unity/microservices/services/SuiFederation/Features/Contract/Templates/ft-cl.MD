# Fungible token/Closed loop token

Smart contract for the Closed loop token lives [here.](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/Features/Contract/Templates/game_coin.move)  
The file contains regular Move code but it is used as a template, general best practice example on how a Regular currency token contract should look like with the most common functions.   
The microservice uses [Handlebars](https://www.nuget.org/packages/Handlebars.Net/) package to generate the final contract code by filling in the package and struct names from the Content system. Additional data from the content system that is used to construct the final contract are:
- name
- symbol
- number of decimal places
- image
- description
- initial supply
- allowed actions

## Creating a Closed loop token smart contract
Creating a Closed loop token contract is initiated by Beamable's [content system](https://docs.beamable.com/docs/content-guide). Adding a new content item will automatically create a new contract from the template, compile and deploy. The sample project already contains one example of content type meant to act as an Closed loop token [here.](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederationCommon/FederationContent/InGameCurrency.cs)

## Smart contract deployment  
The smart contract deployment happens at the time of: 
- microservice deployment, or
- content publish operation

## Smart contract deployment details  
The microservice deployment holds a [SUI CLI tool executable](https://github.com/beamable/sui-example/tree/main/sui-unity/microservices/services/SuiFederation/SuiTools) that is used to compile and deploy the smart contract. 
The full list of steps are:  
- the wallet used for deploying the contracts is created on the microservice [startup](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/SuiFederation.cs#L40)
- on the devnet environment SUI coins are requested [automatically](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/Features/Contract/SuiClientWrapper/SuiClient.cs#L54). On testnet environment, SUI coins should be requested manually [here](https://faucet.sui.io/)
- microservice [compiles and deploys](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/Features/Contract/SuiClientWrapper/SuiClient.cs#L74) all of the defined content items

In the case of SUI CLI tool update, a new version can be installed using the provided [script.](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/install_sui_tools.ps1)  

## Smart contract interaction
Bridging C# and SUI SDK TypeScript is done using the [Jering package](https://www.nuget.org/packages/Jering.Javascript.NodeJS). All smart contract and chain operations are done using [this service](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/Features/SuiApi/SuiApiService.cs) which is used to call specific [SUI SDK functions](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/Features/SuiApi/ts/bridge.ts).

## Smart contract functions
### Mint
The entry point for the mint function is [here.](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/Features/Contract/Templates/coin.move#L41) It requires:
- AdminCap object, ensuring only owner can mint new items
- treasury object
- amount
- target wallet

### Burn
The entry point for the burn function is [here.](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/Features/Contract/Templates/coin.move#L52) It requires:
- treasury object
- coin to burn

The burn operation [handles](https://github.com/beamable/sui-example/blob/main/sui-unity/microservices/services/SuiFederation/Features/SuiApi/ts/bridge.ts#L361) coin fragmentation and automatically splits/merges the required target amount. 
