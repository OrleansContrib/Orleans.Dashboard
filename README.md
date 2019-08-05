# Orleans.Dashboard

## W.I.P.

> **Warning**: Do not attempt to use this code now. It is a work-in-progress and things will break every day!

> **Warning**: This is **not** a replacement for [OrleansDashboard](https://github.com/OrleansContrib/OrleansDashboard). It is a new implementation with different features/design.

> **Warning**: This project uses bleeding edge .Net Core 3 features and **will not work** on .Net Framework or previous versions of .Net Core.

> **Warning**: You've beeing warned :)

The primary/initial goals on this project are:

1. Be based on a rich experience of AspNet Core Blazor (server-side) for its web views;
2. Be capable of be hosted in multiple ways but tries to avoid hosting the web application on the silos running the application workload;
3. Avoid as much as possible adding code to the application workload silos;
4. Have a vey thin agent deployed on each application workload silo to just collect (no aggregation) and forward reports to the dashboard silo for processing;
5. Allow users to have a dedicated and secured silo to host the Dashboard application so the processing of reports doesn't affect the application workload;
6. Allow live updates of what and when is being monitored like live log stream, profiling, etc;
7. Provide a console to invoke grains from the Dashboard securely;
8. TBD

Contributions are always welcome! 

Stay tune for more news.