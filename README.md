# Manage Your Education and Skills Funding Feed Reader Function

The Manage Your Education and Skills Funding (MYESF) funding feed reader allows the following:

- ??

## Provider

[The Department for Education](https://www.gov.uk/government/organisations/department-for-education)

## About this project

This project is a .Net 8 Isolated Worker Azure Function project utilizing an Azure Function App for deployment.

**Note:** The project is currently being updated to be containerised via Docker where the deployment method and target will change, this document will be updated when these changes have been finalised.

# Local Configuration Guide

In order to run the application locally a valid `local.settings.json` file will need to be created in the `pdsazurefunctionfundingfeedreader` projects Below, and included in the repo, there is `local.settings.example.json` which can be used as a base and populated with the required values, which can be retrieved from the Azure Portal.

## Application Settings (`local.settings.json`)

```json
 {

    "IsEncrypted": false,

    "Values": {

      "AzureWebJobsStorage": "",

      "FUNCTIONS_WORKER_RUNTIME": "dotnet",

      "timerInterval": "",

      "runMode": "", 

      "taskBatchSize": "",

      "includeReindex": ,

      "cdb:endpointUri": "",

      "cdb:endpointKey": "",

      "cdb:dbName": "",

      "cdb:fundingGroupCollectionName": "",

      "cdb:providerFundingCollectionName": "",

      "cdb:auditCollectionName": "",

      "cdb:throughputSize": "",

      "cdb:throughputWaitTimeSeconds": "",

      "cdb:programaticallyChangeThroughput": "",

      "LOCAL_cdb:endpointUri": "",

      "LOCAL_cdb:endpointKey": "",

      "DEV_cdb:endpointUri": "",

      "DEV_cdb:endpointKey": "",

      "LocalfundingsApi:baseUrl": "",

      "fundingsApi:pageSize": "250",

      "LOCAL_fundingsApi:baseUrl": "",

      "CFSTest_fundingsApi:baseUrl": "",

      "fundingsApi:baseUrl": "",

      "ai:environment": "",

      "ai:InstrumentationKey": "",

      "as:adminKey": "",

      "as:name": "",

      "auth:useAuthentication": "true",

      "auth:authority": "",

      "auth:tenantId": "",

      "local_auth:clientId": "",

      "local_auth:clientSecret": "",

      "auth:clientId": "",

      "auth:clientSecret": "",

      "auth:appIdUri": "",

      "CFSTest_auth:clientId": "",

      "CFSTest_auth:clientSecret": "",

      "CFSTest_auth:appIdUri": "",

      "vyf:baseUri": "",

      "vyf:autoPullEndpointUri": "",

      "vyf:apiKey": "",

      "sb:connectionString": ""

    }

  }
 
```

### Setting Details

- **`AzureWebJobsStorage`**  
  The Azure Storage connection string required by the Azure Functions runtime for operation and trigger management.
 
- **`FUNCTIONS_WORKER_RUNTIME`**  
  dotnet.

- **`timerInterval`**  
  The CRON expression defining the schedule used by the timer-triggered funding feed reader process.

- **`runMode`**  
  The conditional value for how the process should handle feed reader functions based on environment.

- **`taskBatchSize`**  
  Integer value of batch size of funding feed reader to process.

- **`includeReindex`**  
  Boolean value to include reindexing or not.

- **`cdb:endpointUri`**  
  Unique Cosmos Db URI to use for local environment.

- **`cdb:endpointKey`**  
  Unique Cosmos Db end point connection string key value.

- **`cdb:dbName`**  
  Name of the Cosmos Db database to use.

- **`cdb:fundingGroupCollectionName`**  
  The name of the Cosmos Db collection used for funding data.
  
- **`cdb:providerFundingCollectionName`**  
  The name of the Cosmos Db collection used for provider funding data.
  
- **`cdb:auditCollectionName`**  
  The name of the Cosmos Db collection used for audit purposes.

- **`cdb:throughputSize`**  
  Numeric value of max throughput size of the Cosmos Db results.

- **`cdb:throughputWaitTimeSeconds`**  
  Numeric value of time designated for throughput function to work to get data from Cosmos Db.

- **`cdb:programaticallyChangeThroughput`**  
  Boolean value for throughput change for Cosmos Db.
  
- **`LOCAL_cdb:endpointUri`**  
  Unique Cosmos Db URI link for the local environment.
  
- **`LOCAL_cdb:endpointKey`**  
  Unique Cosmos Db URI connection string key for the local environment.
  
- **`DEV_cdb:endpointUri`**  
  Unique Cosmos Db URI link for the developer environment.
  
- **`DEV_cdb:endpointKey`**  
  Unique Cosmos Db URI connection string key for the developer environment.

- **`LocalfundingsApi:baseUrl`**  
  Unique local funding base api url for mockicg.
  
- **`fundingsApi:pageSize`**  
  Maximum numeric value of funding api page size.

- **`LOCAL_fundingsApi:baseUrl`**  
  Unique local funding base api url. //potentially duplicate of LocalfundingsApi:baseUrl.

- **`CFSTest_fundingsApi:baseUrl`**  
  Url link for calculate funding service test api.

- **`fundingsApi:baseUrl`**  
  Unique funding base url for calculate funding swrvice.
  
- **`ai:environment`**  
  Target environemnt to use value.

- **`ai:InstrumentationKey`**  
  Unique ai instrumentation key value.

- **`as:adminKey`**  
  Unique Azure service admin key value.
  
- **`as:name`**  
  Name of the Azure service environemnt.
  
- **`auth:useAuthentication`**  
  Boolean value to use authentication or not.
  
- **`auth:authority`**  
  Microsoft url link for authentication.

- **`auth:tenantId`**  
  Unique microsoft tenant id.
  
- **`local_auth:clientId`**  
  Unique local environemnt client id authentication key.
  
- **`local_auth:clientSecret`**  
  Unique local environment client authentication secret key value.

- **`auth:clientId`**  
  Unique client id authentication key.
  
- **`auth:clientSecret`**  
  Unique client authentication secret key value.
  
- **`CFSTest_auth:clientId`**  
  Unique calculate funding service test environment client id value.
  
- **`CFSTest_auth:clientSecret`**  
  Unique calculate funding service test environment client secret value.

- **`CFSTest_auth:appIdUri`**  
  Uri for calculate funding service test environment.

- **`vyf:baseUri`**  
  The url of View Your Funding external api.

- **`vyf:autoPullEndpointUri`**  
  Uri path for View Your Funding endpoint.

- **`vyf:apiKey`**  
  The api secret key of View Your Funding external api.
  
- **`sb:connectionString`**  
  Unique connection string for the sandbox environment.

## Build and Test

To build and test locally, you can either use Visual Studio, Visual Studio Code or simply use dotnet CLI `dotnet build` and `dotnet test` more information in dotnet CLI can be found at <https://docs.microsoft.com/en-us/dotnet/core/tools/>.

## Contribute

To contribute,

- If you are part of the team then create a branch for changes and then submit your changes for review by creating a pull request.
- If you are external to the organisation then fork this repository and make necessary changes and then submit your changes for review by creating a pull request.
