# rooster
Rooster :rooster: is log extractor. It extracts docker logs like `docker run` from your Azure App Service console logs and sends them to a configurable location. Currently supported locations include:
* Sql Database
* MongoDb
* Slack

# Motivation

Running Docker container using Azure Linux App Services can be inconvenient but they also support many different setups like classic .Net services run under IIS, so I understand that they are in it's core a compromise between two worlds, but not being able to access docker logs is a pain.
Now if you try getting the logs from Kudu API, you will see that in your logs you will find some docker logs too! And I was not able to find that information in any other place. When I set diagnostic setting to push Console Logs to Log analytics I got all my app's console logs but not the stuff that gets written by docker, so when there's a need there's a hacky way to do it and the hacky way is :rooster: which solely purpose is to get list of the log urls and then open each one of them and search for the entries that contain docker as a key word. All the entries that if finds will be send to Application Insight.

# How it works

Simple console app tha fetech the Docker logs list from endpoint provided by Kudu `https://{appservice-name}.scm.azurewebsites.net/api/logs/docker`. App remembers the timestamp of the last match if found in logs, in order to avoid sending the same entries many times. Entries that match will be sent to Application Insight as structured logs.
