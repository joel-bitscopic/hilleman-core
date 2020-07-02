using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.domain.hl7;
using com.bitscopic.hilleman.core.utils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace com.bitscopic.hilleman.core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && string.Equals(args[0], "HL7", System.StringComparison.CurrentCultureIgnoreCase))
            {
                IConfigurationBuilder config = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.Build();

                //fetch port from config - if available from command line, override config setting
                String hl7Port = MyConfigurationManager.getValue("HL7_LISTENER_PORT");
                if (args.Length > 1 && !String.IsNullOrEmpty(args[1]) && args[1].ToLower().Contains("port="))
                {
                    hl7Port = StringUtils.split(args[1], '=')[1].Trim();
                }

                Int32 iHL7Port = 5000; // default if not available at all
                if (!String.IsNullOrEmpty(hl7Port))
                {
                    Int32.TryParse(hl7Port, out iHL7Port);
                }

                if (args.Length > 2 && args[2] == "test")
                {
                    MyConfigurationManager.setValue("HL7_MESSAGE_ROUTER_TYPE", "test");
                }

                System.Console.WriteLine("Starting HL7 server on port " + iHL7Port.ToString());

                HL7Listener listener = new HL7Listener("127.0.0.1", iHL7Port);
                listener.startSync();

                return;
            }

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.UseKestrel(kestrel => kestrel.AddServerHeader = false)
                .UseStartup<Startup>();
    }
}
