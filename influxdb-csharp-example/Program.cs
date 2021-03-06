﻿using System;
using System.Diagnostics;
using System.Threading;
using Influxdb;

namespace influxdb_csharp_example
{
    public class Program
    {
        private static readonly PerformanceCounter RamCounter = new PerformanceCounter("Memory", "Available MBytes");

        static void Main()
        {
            var influxdb = new InfluxDb("http://localhost:8086", "root", "root", "sampledatabase");

            try
            {
                // test the connection
                influxdb.TestCredentials();

                // list available databases
                foreach (var db in influxdb.ListDatabase())
                {
                    Console.WriteLine("Database: {0}", db.Name);
                }

                // list available time series
                foreach (var series in influxdb.ListSeries())
                {
                    Console.WriteLine(" Series: {0}", series.Name);
                }

                // insert data
                for (var i = 0; i < 10; i++)
                {
                    influxdb.WritePoints("localhost_memory", new[] { "free" }, new[] { RamCounter.NextValue() });
                    Thread.Sleep(1000);
                }

                // query the data
                var result = influxdb.Query("SELECT free FROM localhost_memory");

                // display
                foreach (var point in result.Points)
                {
                    long unixDate = point[0];
                    long sequenceNumber = point[1];
                    long free = point[2];

                    var date = InfluxDb.UnixEpoch.AddMilliseconds(unixDate).ToLocalTime();

                    Console.WriteLine(" {0} ({1}) : {2}MB", date.ToLongTimeString(), sequenceNumber, free);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadLine();   
            }
        }
    }
}