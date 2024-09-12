# Website API

The code contained within this repository is the necessary code to spin up a C# ASP.NET based API using Docker.

The API contains a single endpoint at this time, /contact which is used to send an HTTP Post a Discord Webhook URL. 

To see this functioning, http://addisxn.me/#/contact - my personal website built using Flutter

Secrets such as the Discord webhook url are managed in an appsettings.json file (intentionally excluded here)
