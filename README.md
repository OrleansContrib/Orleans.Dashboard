<p align="center">
  <img src="https://github.com/dotnet/orleans/blob/gh-pages/assets/logo.png" alt="SignalR.Orleans" width="150px"> 
  <h1>Orleans.Dashboard</h1>
</p>

[![Build Status](https://dotnet-ci.visualstudio.com/DotnetCI/_apis/build/status/Orleans-Dashboard-CI?branchName=master)](https://dotnet-ci.visualstudio.com/DotnetCI/_build/latest?definitionId=21&branchName=master)
[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/orleans?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
[![License](https://img.shields.io/github/license/OrleansContrib/Orleans.Dashboard.svg)](https://github.com/OrleansContrib/Orleans.Dashboard/blob/master/LICENSE)

[Orleans](https://github.com/dotnet/orleans) is a framework that provides a straight-forward approach to building distributed high-scale computing applications, without the need to learn and apply complex concurrency or other scaling patterns. 

[Blazor](https://blazor.net) is a framework for building interactive client-side web UI with .NET.

## W.I.P.

> **Warning**: Do not attempt to use this code now. It is a work-in-progress and things will break every day!

> **Warning**: This is **not** a replacement for [OrleansDashboard](https://github.com/OrleansContrib/OrleansDashboard). It is a new implementation with different features/design.

> **Warning**: This project uses bleeding edge .Net Core 3 features and **will not work** on .Net Framework or previous versions of .Net Core.

> **Warning**: You've beeing warned :)

The primary/initial goals on this project are:

1. Be based on a rich experience of AspNet Core Blazor (server-side) for its web views;
2. Be capable of be hosted in multiple ways but tries to avoid hosting the web application on the silos running the application workload;
3. Avoid as much as possible adding code to the application workload silos;
4. Have a very thin agent deployed on each application workload silo to just collect (no aggregation) and forward reports to the dashboard silo for processing;
5. Allow users to have a dedicated and secured silo to host the Dashboard application so the processing of reports doesn't affect the application workload;
6. Allow live updates of what and when is being monitored like live log stream, profiling, etc;
7. Provide a console to invoke grains from the Dashboard securely;
8. TBD

Contributions are always welcome! 

Stay tune for more news.