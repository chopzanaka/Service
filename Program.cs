﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Service
{
    public class Program
    {
        //static X509Certificate2 certificate;
        public static void Main(string[] args)
        {
             var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .AddJsonFile("certificate.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"certificate.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
            .Build();

        var certificateSettings = config.GetSection("certificateSettings");
        string certificateFileName = certificateSettings.GetValue<string>("filename");
        string certificatePassword = certificateSettings.GetValue<string>("password");

        var certificate = new X509Certificate2(certificateFileName, certificatePassword);
        
        var host = new WebHostBuilder()
            .UseKestrel(
                options =>
                {
                    options.AddServerHeader = false;
                    options.Listen(IPAddress.Loopback, 44321, listenOptions =>
                    {
                        listenOptions.UseHttps(certificate);
                    });
                }
            )
            .UseConfiguration(config)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
                .UseUrls("https://localhost:44321/ap/GetMember")
            .Build();

        host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
              WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 5000);
                    options.Listen(IPAddress.Any, 5001, listenOptions =>
                    {
                        listenOptions.UseHttps("localhost.pfx", "password");
                    });
                })
                .Build();
    }
}
