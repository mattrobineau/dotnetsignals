# What is stories?
Stories is a link aggregator written in C# and running on .Net Core.

## Install dotnet on linux
The following bash commands will install the `.NET Core 1.0.5 runtime (LTS)`. If you wish to change the version, replace the download link in the `curl` line.

``` bash
sudo apt-get install curl libunwind8 gettext
curl -sSL -o dotnet.tar.gz https://download.microsoft.com/download/5/F/0/5F0362BD-7D0A-4A9D-9BF9-022C6B15B04D/dotnet-runtime-2.0.0-linux-x64.tar.gz
sudo mkdir -p /opt/dotnet && sudo tar zxf dotnet.tar.gz -C /opt/dotnet
sudo ln -s /opt/dotnet/dotnet /usr/local/bin
```

## Install RabbitMQ
Please go to the [RabbitMQ Documentation](https://www.rabbitmq.com/install-debian.html) to install RabbitMQ. Our sample script looks like:

```bash
echo "deb http://www.rabbitmq.com/debian/ testing main" | sudo tee /etc/apt/sources.list.d/rabbitmq.list     
wget -O- https://www.rabbitmq.com/rabbitmq-release-signing-key.asc | sudo apt-key add -    
sudo apt-get update
sudo apt-get install rabbitmq-server
```

### Setup RabbitMQ
```bash
sudo rabbitmqctl add_user username password # change password to actual password
sudo rabbitmqctl set_user_tags username administrator
sudo rabbitmqctl set_permissions -p / username ".*" ".*" ".*"

./rabbitmqadmin declare exchange name=stories.ranking type=direct
./rabbitmqadmin declare queue name=stories.queues.stories durable=true
./rabbitmqadmin declare queue name=stories.queues.comments durable=true
./rabbitmqadmin declare binding source="stories.ranking" destination_type="queue" destination="stories.queues.stories" routing_key="story"
./rabbitmqadmin declare binding source="stories.ranking" destination_type="queue" destination="stories.queues.comments" routing_key="comment"
```

Remember to disable the default `Guest` user.

Also, update the `appsettings.json` files to use the correct `username` and `password`

## Deploy Stories Website

To deploy the application using Powershell, navigate to the `src/Stories` folder and run

```Powershell
dotnet publish -c release -r debian.8-x64
```

A folder will be created under `~\src\Stories\bin\Release\netcoreapp1.2\debian.8-x64\publish`
Zip and send to your server.

Make sure to setup `kestrel` to run the application. See [Microsoft documentation](https://docs.microsoft.com/en-us/aspnet/core/publishing/linuxproduction)

## Install RankConsumer
Publishing the RankConsumer contains the same steps in the `Deploy Stories Website` section.
The publish folder will be located in the `bin` of the `Stories.Messaging.RankComsumer` project.

The `RankConsumer` can also be run by `systemd` using this configuration.
```ini
[Unit]
Description=.Net Signals Rank Consumer

[Service]
WorkingDirectory=/opt/stories/rankconsumer
ExecStart=/usr/local/bin/dotnet /opt/stories/rankconsumer/Stories.Messaging.RankConsumer.dll
Restart=always
RestartSec=10
SyslogIdentifier=dotnet-stories-rankconsumer
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

## Install StaleScoreUpdateTask

Publishing the `StaleScoreUpdateTask` uses the same previous steps.

This application will update the scores of stories/comments that haven't had any activity within the last 12h.

### Setup A Cronjob

Add the following to `crontab`

```bash
1 */12 * * *  /usr/local/bin/dotnet /opt/stories/stalescoreupdater/Stories.Jobs.StaleScoreUpdateTask.dll
```

Make sure to change the location of your dll.

### Environment Variable

We will add an environment variable of `Production` for the system so that the task can run with the right config.

```bash
echo ASPNETCORE_ENVIRONMENT=Production >> /etc/environment
```

# Dependencies

The application has been tested with the following dependencies

+ .Net Core Runtime v2.0.5
+ RabbitMQ 3.6.10
+ Debian Jessie (8)
