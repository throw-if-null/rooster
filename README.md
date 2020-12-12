[![Actions Status](https://github.com/MirzaMerdovic/rooster/workflows/Docker/badge.svg)](https://github.com/MirzaMerdovic/rooster/actions)
[![Docker Pulls](https://img.shields.io/docker/pulls/mirzamerdovic/rooster?style=flat)](https://hub.docker.com/r/mirzamerdovic/rooster)

# What is it?
Rooster :rooster: is docker log extractor for Azure Web Apps on Linux.  
It extracts docker logs about new container deployments (`docker run`) using Kudu API and persits them to:
* Sql Database
* MongoDb  

or reports them to:  
* Slack
* AppInsights

# Motivation

Running Docker container using Azure Linux App Services can be inconvenient but they also support many different setups like classic .Net services run under IIS, so I understand that they are in it's core a compromise between two worlds, but not being able to access docker logs is a pain.  
Now if you try getting the logs from Kudu API, you will see that in your logs you will find some docker logs too! And I was not able to find that information in any other place. When I set diagnostic setting to push Console Logs to Log analytics I got all my app's console logs but not the stuff that gets written by docker, so when there's a need there's a hacky way to do it and the hacky way is :rooster: which solely purpose is to get list of the log urls and then open each one of them and search for the entries that contain docker as a key word. All the found entries will be persisted or forwarded to a configurable destination.

# How it works

In a nutshell it feteches Docker log list from endpoint provided by Kudu `https://{appservice-name}.scm.azurewebsites.net/api/logs/docker` and sends them to a configurable location.

![Rooster processing workflow](https://raw.githubusercontent.com/MirzaMerdovic/rooster/master/src/docs/rooster-container-diagram.svg)
## Configuration
Rooster is a .net core console app that uses appsettings.json to store the configuration as most of us .net developers already now you can override appsettings.json values with environment variables. If you don't want to use environment variables you can use exteranal appsettings.json that would contain values specific to your use case and just mount that file to your container.

### Polling 
Depending on how you plan to deploy the Rooster your polling needs may change.  
If you are deploying Rooster as a console app then you will have to use its internal poller which can be configured with options below:
```
AppHostOptions__UseInternalPoller=true
AppHostOptions__PoolingIntervalInSeconds=60
AppHostOptions__CurrentDateVarianceInMinutes=180
```

If you plan to run Rooster as a job, so it is trigger by a scheduler then you just need to disasble the internal poller:
```
AppHostOptions__UseInternalPoller=false
```

Note:  
It is import to be aware that `PoolingIntervalInSeconds` and `CurrentDateVarianceInMinutes` are connected and you should make sure that `PoolingIntervalInSeconds` is alway greater or equal to `CurrentDateVarianceInMinutes` so you don't loose any logs. Rooster will decrease current date time by `CurrentDateVarianceInMinutes` when looking for the latest logs, so if you poll every 10 minutes and your variance is 5 you may loose the logs that occured in the first 5 minutes of polling wait time interval. 

### Log Sources
Rooster communicates with Kudu API to get the list of log files. Those log files are then processed and any log line that contain: `docker run` will be extracted.  
You can specify as many Kudu sources as you want and to do so you will need to add the configuration bellow:
```
Adapters__KuduAdapterOptions__0__User=$my-user
Adapters__KuduAdapterOptions__0__Password=xxx
Adapters__KuduAdapterOptions__0__BaseUri=https://my-service.scm.azurewebsites.net/


Adapters__KuduAdapterOptions__1__User=$my-user-2
Adapters__KuduAdapterOptions__1__Password=xxx
Adapters__KuduAdapterOptions__1__BaseUri=https://my-service-2.scm.azurewebsites.net/
```

### Persist or Report
Rooster can be configured to persist extracted logs to: SQL or MongoDb database if you want, or it can be configured to send extracted logs to Slack channel or to AppInsights.

### Persisting
Keep in mind that Rooster expect to have a database created and for 
* SQL server a table named: `LogEntry`, or 
* MongoDb a collection  

#### SQL Server
To have Rooster save extracted logs to SQL server you need these settings configured:
```
AppHostOptions__Enginge=SqlServer

DataStores__Sql__ConnectionFactoryOptions__ConnectionString=Data Source=localhost;Initial Catalog=Rooster;User ID=rooster_app;Password=rooster_app;Connect Timeout=30;
```
Note: Database and table called LogEntry must be created, you can check [seed scripts](src/SqlScripts/scripts/) to get the schema details.

#### MongoDb
To have Rooster save extracted logs to MongoDb database you need these settings configured:
```
AppHostOptions__Engine=MongoDb

DataStores__MongoDb__ClientFactoryOptions__Url=mongodb://localhost:27017
```
In case you want to change the default database/collection name, or use some existing database/collection then:
```
DataStores__MongoDb__DatabaseFactoryOptions__Name=rooster
DataStores__MongoDb__CollectionFactoryOptions__LogEntryCollectionFactoryOptions__Name=LogEntry
```
Note: 
Database and collection must be created.

### Reporting
To have Rooster send you extracted logs to Slack or AppInsights you will need to have these settings configured.

#### Slack
Reporting to slack is achieved via incoming webhooks which is the easist way to send message to Slack in my opinion. Required configuration is:
```
AppHostOptions__Engine=Slack

Reporters__Slack__WebHookReporterOptions__Url=services/xxxxxx
```

Optionally you can change the timeout value or change User-Agent header, or add additional header:
```
Reporters__Slack__WebHookReporterOptions__TimeoutInMs=3000
Reporters__Slack__Headers__0__Name=User-Agent
Reporters__Slack__Headers__0__Value=Rooster
```

In case you need to specify authorization header:
```
Reporters__Slack__Authorization__Scheme=xxx
Reporters__Slack__Authorization__Parameter=xxx
```

#### AppInsights
To send logs to AppInsights you need to provider an instrumentation key:
```
AppHostOptions__Engine=AppInsights

Reporters__AppInsights__TelemetryReporterOptions__InstrumentationKey=xxx
```

### Logging
Rooster uses [Serilog](https://serilog.net/) as a logging provider of choice the only installed sink is for logging to console, if you want ot override the minimum logging level you can use the environment variable below:
```
Serilog__MinimumLevel=Warning
```  

other log level options are: `Verbose`, `Debug`, `Information`, `Warning`, `Error` and `Fatal` and you can also check the [offical documentation](https://github.com/serilog/serilog/wiki/Configuration-Basics#minimum-level). Keep in mind that loggin abstraction used in code is still Microsoft.Logging.Extension.ILogger and as you might already know its logging levels are slightly different, so `Trace` is `Verbose` and `Critical` is `Fatal`.

You can check out [systemSettings.json](src/Rooster.DependencyInjection/systemSettings.json) for more details on Serilog configuration if you are interested.

### HttpClient Resilency

HTTP client's retry delays by default have values of: 50, 100 and 200 miliseconds with added variable jitter value, that doesn't exceed 100 miliseconds, so the maximum delays are: 150, 200 and 300 miliseconds respectively.  
HTTP retries are applied to any HTTP communication happening in rooster, for example Rooster -> Kudu API communication.

If you want to overrid the default values and/or jitter maximum value you can do it via environment variables:
```
RetryProviderOptions__JitterMaxium=50

RetryProviderOptions__Delays__0=10
RetryProviderOptions__Delays__1=30
RetryProviderOptions__Delays__2=100
```
