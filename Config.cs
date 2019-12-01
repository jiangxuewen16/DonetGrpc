using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonetGrpc
{
    public static class Config
    {
        public static string Assembly;
        public static string ServiceVersion = "v1.0.0";
        public static string GrpcAddr = "0.0.0.0";
        public static int GrpcPort = 10007;
        public static string ConsulAddr;
        public static int ConsulPort;
        public static TimeSpan DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5);
        public static TimeSpan Interval = TimeSpan.FromSeconds(10);
        public static TimeSpan Timeout = TimeSpan.FromSeconds(5);
        public static string CheckHealthUrl;
    }


    public static class GrpcConfig
    {
        public static string GrpcAddr = "0.0.0.0";

        public static int GrpcPort = 10007;
    }
}
