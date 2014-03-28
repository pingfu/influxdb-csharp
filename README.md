influxdb-csharp
===============

A CSharp client for InfluxDB, a scalable open-source time-series, events and metrics database.


Project Status
--------------

Early stage development, not really suitable for use yet. Currently only supports querying and basic insertion.


For Developers
--------------

### Usage

Include the library in your project and create an instance.

```csharp
using Influxdb;
...
var db = new InfluxDb("http://localhost:8086", "root", "root", "sampledatabase");
```

Insert some data into a time series.

```csharp
db.WritePoints("weather_station", new[] { "temperature", "windspeed" }, new[] { "11", "8" });

```

Or

```csharp
var data = new[]
	{
		new[] {12, 7},
		new[] {13, 9},
		new[] {14, 10},
		new[] {15, 8},
	};

db.WritePoints("weather_station", new[] { "temperature", "windspeed" }, data);
```

Query the database

```csharp
var result = db.Query("SELECT * FROM weather_station");

foreach (var point in result.Points)
{
	var date = InfluxDb.UnixEpoch.AddMilliseconds(point[0]).ToLocalTime();
	long sequenceNumber = point[1];
	int temperature = point[2];
	int windspeed = point[3];

	Console.WriteLine("{0} ({1}) : {2}°C {3} mph", date.ToLongTimeString(), temperature, windspeed);
}
```