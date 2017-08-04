# Project Description
When JBoss enterprise applications cluster, they typically use mod_cluster, which has been integrated tightly into the JBoss application server.  Without this integration, it is not possible to easily cluster IIS hosted ASP.NET applications using mod_proxy or to add ASP.NET applications or services into an existing mod_cluster.

# Why Did This Project Get Started?
At my current day job, we have a large, enterprise application written primarily in java using JBoss and clustered with mod_cluster.  There are a hand-full of ASP.NET applications that I wished I could add as contexts in the cluster.  So I began writing this during my off-hours so I could include my own IIS contexts.

# Project Goals
This project should:

* provide zero-code insertion into mod_cluster
* allow additional customizations/overrides of built-in behavior without the need to modify the codebase
* allow upgrading versions of MCMP.NET without the need to re-apply customizations to the codebase

# Example
To get an ASP.NET application to join a known cluster, the mcmp.dll should be copied to the bin directory and the following added to the web.config. That's it. Cluster will be joined and status/load messages will be periodically sent to the cluster.

```
<configuration>
    <configSections>
         <section name="clusterConfig" type="MCMP.Configuration.AppConfiguration, MCMP"/>
     </configSections>
     <clusterConfig>
         <cluster hosts="http://1.2.3.4:12345" />
    </clusterConfig>
     <system.webServer>
         <modules runAllManagedModulesForAllRequests="true">
             <add name="McmpBootstrap" type="MCMP.Bootstrap, MCMP"/>
         </modules>
     </system.webServer>
<configuration>
```

Note that the McmpBootstrap module is only used to receive notification of application startup and shutdown. This allows MCMP.NET to not require any code added to the Global.asax.

## Multicasting Example to Register to One or More Clusters
To get an ASP.NET application to join a cluster by listening for advertisements, the mcmp.dll should be copied to the bin directory and the following added to the web.config. That's it. All clusters will be joined when the first advertisement is received and status/load messages will be periodically sent to the cluster(s).

```
<configuration>
    <configSections>
         <section name="clusterConfig" type="MCMP.Configuration.AppConfiguration, MCMP"/>
     </configSections>
     <clusterConfig>
         <cluster multicastEnabled="true" multicastAddress="udp://224.0.1.105:23364" />
    </clusterConfig>
     <system.webServer>
         <modules runAllManagedModulesForAllRequests="true">
             <add name="McmpBootstrap" type="MCMP.Bootstrap, MCMP"/>
         </modules>
     </system.webServer>
<configuration>
```
