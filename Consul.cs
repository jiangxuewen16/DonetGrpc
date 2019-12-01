using Consul;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace DonetGrpc
{
    public class ConsulServer
    {
        private static ConsulServer instance;

        public string consulAddr;

        public int consulPort;

        private ConsulServer(string address, int port)
        {
            consulAddr = address;
            consulPort = port;
            
        }

        public static ConsulServer GetInstance(string address = "127.0.0.1", int port = 8500)

        {
            if (instance == null)
            {
                instance = new ConsulServer(address, port);
            }
            return instance;
        }

        public ConsulClient GetConsuleClient()
        {
            return new ConsulClient(x => x.Address = new Uri($"http://{consulAddr}:{consulPort}"));
        }
    }

    public static class ConsulServiceGovernance
    {
        public static List<string> GrpcUrls = new List<string>();        //Grpc 地址列表
        // 服务注册
        public static void Register(ConsulClient consulClient, GrpcRegisterEntity grpcRegisterEntity)
        {

            var serviceName = grpcRegisterEntity.GrpcUrl.Replace("/", ".").TrimStart('.');

            var httpCheck = new AgentServiceCheck()
            { 
                DeregisterCriticalServiceAfter = Config.DeregisterCriticalServiceAfter,//服务启动多久后注册
                Interval = Config.Interval,//健康检查时间间隔，或者称为心跳间隔
                HTTP = Config.CheckHealthUrl,//健康检查地址
                Timeout = Config.Timeout
            };

            var meta = new Dictionary<string, string>();
            meta.Add("version",Config.ServiceVersion);
            var registration = new AgentServiceRegistration()
            {
                Checks = new[] { httpCheck },
                ID = BuildServiceId(grpcRegisterEntity.ServerAddress, grpcRegisterEntity.ServerPort, serviceName),
                Name = serviceName,
                Address = grpcRegisterEntity.ServerAddress,
                Port = grpcRegisterEntity.ServerPort,
                Meta = meta,
                Tags = new[] { "HTTP", "GRPC", ".NET", grpcRegisterEntity.GrpcUrl } //添加格式的 tag 标签
            };

            consulClient.Agent.ServiceRegister(registration).Wait();    //服务启动时注册，内部实现其实就是使用 Consul API 进行注册（HttpClient发起）
            GrpcUrls.Add(grpcRegisterEntity.GrpcUrl);       //Grpc 地址新增
        }

        public static void Deregister(ConsulClient consulClient, string address, int port, string serviceName)
        {
            var ID = BuildServiceId(address, port, serviceName);
            consulClient.Agent.ServiceDeregister(ID).Wait();//服务停止时取消注册
        }

        public static ServiceEntry GetServiceAddr(ConsulClient consulClient, string grpcUrl, string tag = "", bool passingOnly = true)
        {
            var serviceName = grpcUrl.Replace("/", ".").TrimStart('.');
            var queryResult = consulClient.Health.Service(serviceName, tag,  passingOnly).Result;
            ServiceEntry[] serviceEntrys = queryResult.Response;
            if (serviceEntrys.Length <= 0)
            {
                throw new Exception("服务中心无此服务，请确定服务是否正常运行！");
            }
            Random rnd = new Random();
            int index = rnd.Next(serviceEntrys.Length);
            return serviceEntrys[index];

        }

        private static string BuildServiceId(string address, int port, string serviceName)
        {
            var str = $"{address}{port}{serviceName}";
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.Default.GetBytes(str))).Replace("-", "");
        }


    }

    public class GrpcRegisterEntity
    {
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string GrpcUrl { get; set; }
    }
}
