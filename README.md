# Manage Your Education and Skills Funding View Your Funding Allocation Feed Reader Function

The Manage Your Education and Skills Funding (MYESF) View Your Funding (VYF) allocation feed reader performs the following:

- Read the funding allocation notifications from the Calculate Funding Service (CFS) service and stores them in a Cosmos DB collection for use by the VYF service.
- The feed reader is triggered by a timer, which will run on a schedule defined by a CRON expression in the application settings, where it will process allocations notifications for funding streams which have been configured to be read automatically.
- The feed reader can also be triggered manually via an HTTP request, which will process all funding streams regardless of whether they have been configured to be read automatically or not.

## Provider

[The Department for Education](https://www.gov.uk/government/organisations/department-for-education)

## About this project

This project is a .Net 6 Azure Function project utilizing an Azure Function App for deployment.

**Note:** The project is currently being updated to be containerised via Docker where the deployment method and target will change, this document will be updated when these changes have been finalised.

# Local Configuration Guide

In order to run the application locally a valid `local.settings.json` file will need to be created in the `pdsazurefunctionfundingfeedreader` projects Below, and included in the repo, there is `local.settings.example.json` which can be used as a base and populated with the required values, which can be retrieved from the Azure Portal.

## Application Settings (`local.settings.json`)

```json
{
  "IsEncrypted": false,
  "Values": {
    "ai:environment": "",
    "ai:instrumentationKey": "",
    "as:adminKey": "",
    "as:name": "",
    "auth:useAuthentication": "true",
    "auth:authority": "",
    "auth:tenantId": "",
    "auth:clientId": "",
    "auth:clientSecret": "",
    "auth:appIdUri": "",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "cdb:endpointUri": "",
    "cdb:endpointKey": "",
    "cdb:dbName": "",
    "cdb:fundingGroupCollectionName": "",
    "cdb:providerFundingCollectionName": "",
    "cdb:auditCollectionName": "",
    "cdb:throughputSize": "",
    "cdb:throughputWaitTimeSeconds": "",
    "cdb:programaticallyChangeThroughput": "",
    "environment": "local",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "FUNCTIONS_EXTENSION_VERSION": "~4",
    "fundingsApi:pageSize": "250",
    "fundingsApi:baseUrl": "",
    "runMode": "",
    "sb:connectionString": "",
    "taskBatchSize": "",
    "timerInterval": "",
    "vyf:baseUri": "",
    "vyf:autoPullEndpointUri": "",
    "vyf:apiKey": ""
  }
}
```

### Setting Details

- **`ai:environment`**  
  The environment name used by the application logging framework for sending telemetry and diagnostics to Application Insights.

- **`ai:InstrumentationKey`**  
  The instrumentation key used by the application logging framework for sending telemetry and diagnostics to Application Insights.

- **`as:adminKey`**  
  The unique key used by the Azure Search service for funding data indexing and searching.
  
- **`as:name`**  
  The name of the Azure Search service used for funding data indexing and searching.

- **`auth:useAuthentication`**  
  The value which determines whether authentication is used for the funding feed reader process.
  
- **`auth:authority`**  
  The url of the azure ad service used to authenticate the CFS api.

- **`auth:tenantId`**  
  The unique identifier for the CFS api azure ad tenant.

- **`auth:clientId`**  
  The application (client) ID registered in azure ad for the CFS api.
  
- **`auth:clientSecret`**  
  The application (client) secret registered in azure ad for the CFS api.

- **`auth:appIdUri`**  
  The intended recipient of the microsoft azure authentication token for the CFS api.

- **`AzureWebJobsStorage`**  
  The Azure Storage connection string required by the Azure Functions runtime for operation and trigger management.

- **`AzureWebJobsDashboard`**  
  The Azure Storage jobs dashboard configuration setting to resolve issues with local running.

- **`cdb:endpointUri`**  
  The url of the Cosmos Db resource.

- **`cdb:endpointKey`**  
  The unique key used to access the Cosmos Db resource.

- **`cdb:dbName`**  
  The name of the Cosmos Db database used for funding data.

- **`cdb:fundingGroupCollectionName`**  
  The name of the Cosmos Db collection used for funding data.
  
- **`cdb:providerFundingCollectionName`**  
  The name of the Cosmos Db collection used for provider funding data.
  
- **`cdb:auditCollectionName`**  
  The name of the Cosmos Db collection used for audit purposes.

- **`cdb:throughputSize`**  
  The value which determines the throughput rate for the Cosmos Db resource.

- **`cdb:throughputWaitTimeSeconds`**  
  The value which determines the wait time for updating throughput rate for Cosmos Db.

- **`cdb:programaticallyChangeThroughput`**  
  The value which determines whether the throughput rate for the Cosmos Db resource can be updated programmatically.

- **`environment`**  
  The environment which the application is running.
 
- **`FUNCTIONS_WORKER_RUNTIME`**  
  The worker runtime used by the Function App.

- **`FUNCTIONS_EXTENSION_VERSION`**  
  The Azure Functions runtime version used by the application.

- **`fundingsApi:pageSize`**  
  The value which determines the page size used in requests to the CFS api for funding allocation notifications.

- **`fundingsApi:baseUrl`**  
  The url of the CFS api.

- **`runMode`**  
  The value which determines whether the funding feed reader should process when triggers are executed.

- **`sb:connectionString`**  
  The connection string for the Service Bus resource.

- **`taskBatchSize`**  
  The value which determines the number of funding allocations which will be processed in a single batch.

- **`timerInterval`**  
  The CRON expression defining the schedule used by the timer-triggered funding feed reader process.

- **`vyf:baseUri`**  
  The url of View Your Funding external api.

- **`vyf:autoPullEndpointUri`**  
  The url of the View Your Funding external api auto pull configured funding streams endpoint.

- **`vyf:apiKey`**  
  The secret key of View Your Funding external api.

## Build and Test

To build and test locally, you can either use Visual Studio, Visual Studio Code or simply use dotnet CLI `dotnet build` and `dotnet test` more information in dotnet CLI can be found at <https://docs.microsoft.com/en-us/dotnet/core/tools/>.

## Contribute

To contribute,

- If you are part of the team then create a branch for changes and then submit your changes for review by creating a pull request.
- If you are external to the organisation then fork this repository and make necessary changes and then submit your changes for review by creating a pull request.
