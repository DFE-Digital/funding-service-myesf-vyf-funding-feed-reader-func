# Manage Your Education and Skills Funding User Interface

The Manage Your Education and Skills Funding (MYESF) funding feed reader allows the following:

- ??

## Provider

[The Department for Education](https://www.gov.uk/government/organisations/department-for-education)

## About this project

This project is an ASP.NET Core 8 web api utilising Azure App Service for deployment.

The web api runs on an Azure App service on Azure.

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
  Value indicating which azure storage to use.
 
- **`FUNCTIONS_WORKER_RUNTIME`**  
  dotnet.

- **`timerInterval`**  
  Timer interval numeric value.

- **`runMode`**  
  Indicating which mode to run, recovery or increment.

- **`taskBatchSize`**  
  Integer value of batch size to process.

- **`includeReindex`**  
  Boolean value to include reindexing or not.

- **`cdb:endpointUri`**  
  Unique local cdp URI link.

- **`cdb:endpointKey`**  
  Unique cdb end point connection string key value.

- **`cdb:dbName`**  
  cdb database name.

- **`cdb:fundingGroupCollectionName`**  
  Value of funding group for cdb to use.
  
- **`cdb:providerFundingCollectionName`**  
  cdb provider funding collection name value.
  
- **`cdb:auditCollectionName`**  
  cdb audit collection name value.

- **`cdb:throughputSize`**  
  Numeric value of max throughput size.

- **`cdb:throughputWaitTimeSeconds`**  
  Numeric value of time designated for throughput function to work.

- **`cdb:programaticallyChangeThroughput`**  
  Boolean value for throughput change.
  
- **`LOCAL_cdb:endpointUri`**  
  Unique cdb local URI link.
  
- **`LOCAL_cdb:endpointKey`**  
  Unique cdb local connection string key value.
  
- **`DEV_cdb:endpointUri`**  
   Unique cdb dev environment URI link.
  
- **`DEV_cdb:endpointKey`**  
  Unique cdb dev environment connection string key value.

- **`LocalfundingsApi:baseUrl`**  
  Unique local funding base api url.
  
- **`fundingsApi:pageSize`**  
  Maximum numeric value of funding api page size.

- **`LOCAL_fundingsApi:baseUrl`**  
  Unique local funding base api url. //potentially duplicate of LocalfundingsApi:baseUrl.

- **`CFSTest_fundingsApi:baseUrl`**  
  Url link for calculate funding service test api.

- **`fundingsApi:baseUrl`**  
  Unique funding base api url.
  
- **`ai:environment`**  
  Target environemnt to use value.

- **`ai:InstrumentationKey`**  
  Unique instrumentation key value.

- **`as:adminKey`**  
  Unique admin key value.
  
- **`as:name`**  
  Name of the environemnt.
  
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
  VYF uri link.

- **`vyf:autoPullEndpointUri`**  
  Uri path for VYF endpoint.

- **`vyf:apiKey`**  
  Unique VYF api key value.
  
- **`sb:connectionString`**  
  Unique connection string for the sandbox environment.

## Test execution

### Tests

All of the tests can be found in the test.csproj. No local settings are required to run the tests.
