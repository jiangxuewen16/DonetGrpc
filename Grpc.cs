using Consul;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using static Grpc.Core.Server;

namespace DonetGrpc
{


    public class GrpcServer
    {
        private Server server;

        private string localIp;
        private int localPort;

        public GrpcServer(int port)
        {
            localPort = port;
            localIp = GetAddressIP();

            server = new Server
            {
                Ports = { new ServerPort(Config.GrpcAddr, port, ServerCredentials.Insecure) }
            };
        }

        public void Start()
        {
            server.Start();
            // server.ShutdownAsync().Wait();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
            while (true) { Thread.Sleep(60 * 60 * 24 * 1000); }     //阻塞线程，防止线程结束，导致grpc监听端口结束
        }

        public void AddService(List<Object> grpcServiceList)
        {

            var consulServer = ConsulServer.GetInstance(Config.ConsulAddr, Config.ConsulPort).GetConsuleClient();

            foreach (object grpcService in grpcServiceList)
            {
                var ass = Assembly.Load(Config.Assembly);
                var a = grpcService.GetType().BaseType.DeclaringType.FullName;
                Type type = ass.GetType(grpcService.GetType().BaseType.DeclaringType.FullName);//加载类型
                Object[] param = new Object[1];
                param[0] = grpcService;
                var service = type.InvokeMember("BindService", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, param);
                var serviceDefinition = (ServerServiceDefinition)service;        //设置ServerServiceDefinition

                var grpcUrlPre = type.GetField("__ServiceName", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);       //获取grpc service_name

                var grpcRegisterEntity = new GrpcRegisterEntity
                {
                    ServerAddress = localIp,
                    ServerPort = localPort,
                };

                var methods = grpcService.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                if (methods.Length <= 0)
                {
                    throw new Exception("请实现grpc service 方法！");
                }

                foreach (MethodInfo methodInfo in methods)
                {
                    grpcRegisterEntity.GrpcUrl = $"/{grpcUrlPre}/{methodInfo.Name}";
                    ConsulServiceGovernance.Register(consulServer, grpcRegisterEntity);
                }

                server.Services.Add(serviceDefinition);
            }
        }

        private string GetAddressIP()
        {
            ///获取服务端
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;

        }
    }

    public static class GrpcClient
    {

        public static ChannelBase getChannel(string grpcUrl)
        {
            var consulServer = ConsulServer.GetInstance(Config.ConsulAddr, Config.ConsulPort).GetConsuleClient();
            var serviceEntry = ConsulServiceGovernance.GetServiceAddr(consulServer, grpcUrl);

            var address = serviceEntry.Service.Address;
            var port = serviceEntry.Service.Port;

            return new Channel($"{address}:{port}", ChannelCredentials.Insecure);
        }
    }
}
