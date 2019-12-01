# DonetGrpc
.net grpc

# 支持.net版本
.net framework 4.5 +

# 依赖库
Consul v0.7.2.6  Google.Protobuf v3.10.1  Grpc v2.25.0

# .net mvc 集成实例：
//mvc启动文件 Global.asax
public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //配置文件
            Config.Assembly = Assembly.GetExecutingAssembly().GetName().Name;
            Config.GrpcAddr = "0.0.0.0";
            Config.GrpcPort = 10007;
            Config.ServiceVersion = "v1.0.0";
            Config.ConsulAddr = "192.168.22.103";
            Config.ConsulPort = 8500;
            Config.DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5);
            Config.Interval = TimeSpan.FromSeconds(10);
            Config.Timeout = TimeSpan.FromSeconds(5);
            Config.CheckHealthUrl = "http://192.168.16.46:9684/api/health";

            Thread thread = new Thread(new ThreadStart(Start));
            thread.Start();

            //客户端的使用
            var request = new HelloRequest { Name = "xdarfas=fsa=trf=ewa=fw" };
            var channel = GrpcClient.getChannel("/helloworld.Greeter/SayHello");
            var client = new Greeter.GreeterClient(channel).SayHello(request);
        }

        //启动服务端
        private static void Start()
        {
            var grpcServer = new GrpcServer(10007);

            List<Object> GrpcServiceList = new List<Object>();

            GrpcServiceList.Add(new GrpcImpl());

            grpcServer.AddService(GrpcServiceList);
            grpcServer.Start();
        }
    }
