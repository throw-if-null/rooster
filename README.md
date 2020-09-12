[![Actions Status](https://github.com/MirzaMerdovic/rooster/workflows/Docker/badge.svg)](https://github.com/MirzaMerdovic/rooster/actions)
[![Docker Pulls](https://img.shields.io/docker/pulls/mirzamerdovic/rooster?style=flat)](https://hub.docker.com/r/mirzamerdovic/rooster)

# What is it?
Rooster :rooster: is docker log extractor for Azure Web Apps on Linux.  
It extracts docker logs about new container deployments (`docker run`) using Kudu API and persits them to:
* Sql Database
* MongoDb  

and/or reports them to:  
* Slack
* AppInsights

# Motivation

Running Docker container using Azure Linux App Services can be inconvenient but they also support many different setups like classic .Net services run under IIS, so I understand that they are in it's core a compromise between two worlds, but not being able to access docker logs is a pain.  
Now if you try getting the logs from Kudu API, you will see that in your logs you will find some docker logs too! And I was not able to find that information in any other place. When I set diagnostic setting to push Console Logs to Log analytics I got all my app's console logs but not the stuff that gets written by docker, so when there's a need there's a hacky way to do it and the hacky way is :rooster: which solely purpose is to get list of the log urls and then open each one of them and search for the entries that contain docker as a key word. All the found entries will be persisted or forwarded to a configurable destination.

# How it works

In a nutshell it feteches Docker log list from endpoint provided by Kudu `https://{appservice-name}.scm.azurewebsites.net/api/logs/docker` and sends them to a configurable location.
