# Aeronautical-Station-Information
This software allows FlightSim users to get information on airports and stations.

# What is this software?
This software is called ASI (Aeronautical Station Information), is a OpenSource software that allows flightsim users to get information on airports and stations in a very simple way, just search the desired ICAO code of the airport.

## What information does this software provide?
It provides basic information on Airports by using different services:
* Name
* City
* Country
* Elevation
* Geographical Coordinates
* Runways
* METAR
* TAF
* Radio Frequencies
* NAVAIDS
* ATIS
* Aeronautical Charts

## What services does it use?
* AVWX Services (https://avwx.online) _[Personal API Token required]_
* OpenAip Services (https://www.openaip.net) _[Personal API Token required]_
* NOAA Weather (https://www.weather.gov) _[Personal API Token required]_
* IVAO (https://ivao.aero)
* Jeppesen (https://ww2.jeppesen.com)
* Lufthansa (https://www.lhsystems.com)

## How do services work?
Services require an internet connection to download data.
Some web services may require a personal API Token for functioning.
ASI already provides a default Token that you can use but it will be probably reach connection limit if many users are using the software, for this reason I recommend to get your free Token for each service you'd like to use.
To get the free WebAPI Token a link is provided within the software from the Options window.
