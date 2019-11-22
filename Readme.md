# Azure Frontdoor sample

Repository used to demonstrate various frontdoor capabilities.

Contains:

* ARM template to deploy two websites in different regions (frontdoor instance must be configured manually)
* code to be deployed to both servers
* ping test tool that continuously logs received content
* perf test tool that averages roundtrip time

## Prerequisites

* Deploy two app services in different regions (ideally EU and US) (ensure deployed code is identical; yaml pipeline is already set up to do this)
* Create a frontdoor instance that uses both apps as backend endpoints

Ensure setup is working: When navigating to `https://<app1>/` and `https://<app1>/` it should display the region names of each data center respectively (rest of documentation will use `West Europe` and `Central UL`).

# Closest region test

When connecting to `https://<name>.azurefd.net` it should always return the response from the closest datacenter.

# Unhealthy region test

Use command `Frontdoor.Ping.exe https://<name>.azurefd.net`. It will continuously ping the instance and return the content (data center name).

Stop the webapp that you have been connecting to and watch as the ping of `https://<name>.azurefd.net` fails over to the alternative region (with some intermittent failures).

Note that the switch **will not be instant** (you will observe some failures). The duration depends on your rule configuration:

* Duration between pings
* Number of samples to collect
* Required number of failed samples

E.g. if the rule is set up to run every 10 seconds and requires 4 successful samples before failover then you are looking at a minimum of 40 seconds before the failover happens.

(For the fastest failover for demo purposes you can configure the rule to the minimum of 1 sample and execution in 5 second intervals).

After the duration the response of `https://<name>.azurefd.net` should be successful again and will return data from the alternative data center.

# Ping both app services parallel

(Make sure both webapps are online again).

Run commands `Frontdoor.Ping.exe https://<app1>/` and `Frontdoor.Ping.exe https://<app2>/` in parallel.

The result should be that the speed of the closer data center is much faster, resulting in more requests executed per second.

# Perf test individual backends

Run commands `Frontdoor.Perf.exe https://<app1>/logo.png 20` and `Frontdoor.Perf.exe https://<app2>/logo.png 20` to establish a baseline of both webapps (we are using the download of a ~100kb image without caching as a baseline test as it is closest to the size of a real website).

The result should be that the closer datacenter performs better (who would have guessed?).

## Example results

(Average of multiple runs from a consumer computer out of Europe to West Europe and Central US)

| Data Center | Average |
| -- | -- |
West Europe | 80ms
Central US | 500ms

## Why

With the distance between the user and webapp increased so does the latency of any request. Obviously the increased distance travelled adds to the connection speed (which is capped at the speed of light) but also with increased distance the number of hops increases as well as more routing points are hit along the way.

# Ping test frontdoor

With both backends running frontdoor should always connect to the closest datacenter.

Run the command: `Frontdoor.Perf.exe https://<name>.azurefd.net/logo.png`

The result will be that it is slightly faster (or about the same) then when pinging the closer webapp.

## Example results

(Average of multiple runs from a consumer computer out of Europe to West Europe and Frontdoor with only West Europe backend)

| Data Center | Average |
| -- | -- |
West Europe | 80ms (from baseline test)
Frontdoor | **75ms!**

## Why

Frontdoor is deployed to many Azure Datacenters. Any connection via the frontdoor service will always automatically pick the closest datacenter.

Frontdoor will then handle TLS termination at the edge and forward the request across a "hot" SSL connection to the destination Azure Data center.

The "hot" SSL connection allows transmitting data slightly faster as the connection establish/termination overhead must not be paid again.

Azure also has a worldwide backbone infrastructure allowing any requests to travel between datacenters without ever leaving said network.

This helps with transmition speed as the datacenter connections are not subject to regular internet routing problems (which might often be inefficient due to congestion, inefficient routing, failing infrastructure, ..). While there are no guarantees for Azure Datacenter to datacenter connection speed they are almost always faster than going across the public internet.

# Ping test farther datacenter

Stop the webapp that is closest to you and ensure frontdoor is failed over to the other datacenter (by checking the region name of `https://<name>.azurefd.net` e.g. via `Frontdoor.Ping.exe https://<name>.azurefd.net`).

After the failover has succeeeded run the command: `Frontdoor.Perf.exe https://<name>.azurefd.net/logo.png 20`

The result will be that the connection is significantly faster than when connecting to the backend directly!

## Example results

(Average of multiple runs from a consumer computer out of Europe to Frontdoor with only Central US backend)

| Data Center | Average |
| -- | -- |
Central US | 500ms (from baseline test)
Frontdoor | **250ms!**

## Why

Much like the test above we again observe the performance benefit of reusing existing SSL connections and traveling inside the global Azure network.

The effect is stronger this time because the increased distance makes it even more obvious how much benefit using Azure backbone infrastructure and SSL offloading has.