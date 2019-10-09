using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRServerSDK
{
    class HttpService
    {
        
        public static string test()
        {
            var client = new RestClient("http://114.116.9.67:80/cloudvr/node/link/servercheck/467382grfhejwgfeywuf");
            var request = new RestRequest(Method.PUT);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("content-length", "834");
            request.AddHeader("accept-encoding", "gzip, deflate");
            request.AddHeader("Host", "localhost:8888");
            request.AddHeader("Postman-Token", "c0582e2a-db0b-44e7-8c03-6abcb628e7ea,0edb43ac-dd4b-46a7-b4c2-69dc16bd7134");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "PostmanRuntime/7.15.0");
            request.AddHeader("Content-Type", "text/plain");
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string currentTime = Convert.ToInt64(ts.TotalSeconds).ToString();
            request.AddParameter("undefined", "{\r\n    \"ConnectTime\":" + currentTime + "," +
            "\r\n    \"InstanceId\": \"78924ceb-fdc1-49f4-bbb4-ce961c764ee3\"," +
            "\r\n    \"Status\":\"conneceted\",\r\n    \"TotalPackets\": 378," +
            "\r\n    \"PacketRate\": 121,\r\n    \"PacketsLostTotal\": 0," +
            "\r\n    \"PacketsLostInSecond\": 0,\r\n    \"PacketsLostPercentage\": 1.05," +
            "\r\n    \"TotalSent\": 5.6,\r\n    \"SentRate\": 0.7," +
            "\r\n    \"TotalRecv\": 5.6,\r\n    \"RecvRate\": 1.01," +
            "\r\n    \"PhysicalLatency\": 2835188.5,\r\n    \"PredictionInterval\": 0.01," +
            "\r\n    \"PerceptionLatency\": 0.01,\r\n    \"FrameTransLatency\": 0.01," +
            "\r\n    \"DecodeLatency\": 0.1,\r\n    \"ClientRenderLatency\": 0.1," +
            "\r\n    \"NetworkRTT\": 1,\r\n    \"TrackInfoTransLatency\": 1," +
            "\r\n    \"GameRenderLatency\": 0.1,\r\n    \"EncodeLatency\": 0.1," +
            "\r\n    \"FecEncodeLatency\": 0.1,\r\n    \"ClientTotalRecv\": 1," +
            "\r\n    \"ClientRecvRate\": 0.01,\r\n    \"TargetCodec\": 1," +
            "\r\n    \"ErrorInDegree\": 0.01,\r\n    \"RenderFps\": 1," +
            "\r\n    \"RenderWidth\": 1,\r\n    \"RenderHeight\": 1\r\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response.Content + response.ErrorMessage;
        }

        public static IRestResponse submitMessage(string url, string postData)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.PUT);
            //request.AddHeader("Postman-Token", "0b544565-2a53-489e-bd79-6f25e5aaa32d");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", postData, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response;
        }

        public static IRestResponse getServerID()
        {    
            var url = "http://169.254.169.254/openstack/latest/meta_data.json";
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            //request.AddHeader("Postman-Token", "0b544565-2a53-489e-bd79-6f25e5aaa32d");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = client.Execute(request);
            return response;
        }

        /**读取配置文件中的masterip值**/
        private static string readConfig()
        {
            try
            {
                return System.IO.File.ReadAllText("C:\\Users\\l00464248\\Desktop\\config.txt").Split(":".ToCharArray())[1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "File does not exist";
            }
            finally { }
        }



    }
}
