using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Web.Script.Serialization;
using System.Threading;


namespace VRServerSDK
{
    public partial class Demo : Form
    {
        ControlSocket socket = new ControlSocket();//与头显驱动通信使用的Socket
        ServerConfig config = new ServerConfig();//服务端参数配置
        ClientList clientList = new ClientList("");//待接入的头盔列表
        public List<DeviceQuery.SoundDevice> soundDevices = new List<DeviceQuery.SoundDevice>();//用于存储声卡信息的列表
        bool previousConnectionState = false;//保存先前是否有头盔已经接入
        string logDisplayNow = "";//记录当前显示的Log,用于避免同一条Log连续打印

        ClientList.Client previousHmd = new ClientList.Client();//记录已连接头盔的信息
        bool needToRestartSteamVR = false;//标记SteamVR是否需要重启
        bool startingSteamVR = false;//用于标记SteamVR是否正在启动中

        //next 4 lines AosenUpdate 
        string clientStatus = "";
        string driverStatus = "";
        string steamvrStatus = "";
        string monitoringData;
        string taskStatus = ""; //任务执行状态 null Executing Failure Success
        Task<Collection<PSObject>> task = null;
        string deviceBrand = "";
        string deviceID = "";
        string master_ip = "";
       


        int count = 0;

        public Demo()
        {
            InitializeComponent();
            //LaunchServer();
        }

        //当程序启动时被调用
        private void Demo_Load(object sender, EventArgs e)
        {
            try
            {
                soundDevices = DeviceQuery.GetSoundDeviceList();
                int i = 0;
                foreach (var device in soundDevices)
                {
                    string text = device.name;
                    if (device.isDefault)
                    {
                        text = "(Default) " + text;
                    }
                    log("Sound device found: " + text);
                    i++;
                }
            }
            catch (Exception)
            {
                log("Get sound device error!");
                Application.Exit();
                return;
            }

            config.Save(this);//将服务端配置映射到内存供头显驱动使用

            //安装头显驱动
            DriverInstaller.CheckDriverPath();//检查驱动安装路径
            DriverInstaller.RemoveOtherDriverInstallations();//删除驱动
            DriverInstaller.InstallDriver();//重新安装驱动
            CheckDriverInstallStatus();//提示驱动安装情况

            UpdateServerStatus();//显示服务端运行状态

            //从默认配置初始化IP和端口
            socket.m_Host = (string)config.driverConfig.controlListenHost;
            socket.m_Port = (int)config.driverConfig.controlListenPort;

            socket.Update();//尝试与头显驱动取得通信

            try
            {
               master_ip = MasterManagement.readConfig();
            }
            catch
            {
                log("getIP failed");
                Application.Exit();
            }
          
            timer1.Start();//定时刷新服务端状态，继续尝试与头显驱动取得通信，并等待头显接入，内部实现见本类timer1_Tick函数
            timer2.Start();//AosenUpdate 
        }

        //显示头显驱动是否已经安装
        private void CheckDriverInstallStatus()
        {
            if (DriverInstaller.CheckInstalled())
            {
                driverStatus = "Installed";//AosenUpdate
                //log("Driver is installed");
            }
            else
            {
                driverStatus = "Uninstalled";//AosenUpdate
                //log("Driver is not installed");
            }
        }

        //显示SteamVR是否正在运行
        private void UpdateServerStatus(string text = "")
        {
           
            if (socket.status == ControlSocket.ServerStatus.CONNECTED)//SteamVR已启动
            {
                //next 2 lines AosenUpdate
                steamvrStatus = "Running";
                //log("SteamVr Running");
                if ("" == text)
                {
                    serverStatusTextBox.Text = "Server is now runnning. Please open Cloud VR client app on HMD.";
                }
                else
                    serverStatusTextBox.Text = text;
                startButton.Enabled = false;//禁用开始按钮
            }
            else//SteamVR未启动
            {
                //next 2 lines AosenUpdate
                steamvrStatus = "Stopped";
                log("SteamVR Stopped");
                if ("" == text)
                {
                    serverStatusTextBox.Text = "Server is not runnning. Please press the 'Start' button.";
                }
                else
                    serverStatusTextBox.Text = text;
                startButton.Enabled = true;//启用开始按钮
            }
            
        }

        //向主界面文本框输出日志
        public void log(string text)
        {
            if(text != logDisplayNow)//同样的Log不连续显示
            { 
                LogTextBox.Text = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]  ") + text + "\r\n" + LogTextBox.Text;
                logDisplayNow = text;
            }
        }

        //每秒进行一次的定时任务
        private void timer1_Tick(object sender, EventArgs e)
        {
            socket.Update();//尝试与头显驱动取得通信
            UpdateClients();//尝试连接头显与更新体验数据
            UpdateServerStatus();//更新服务器状态
        }

        //next method AosenUpdate
        private void timer2_Tick(object sender,EventArgs e)
        {
            submitStatus();
        }


        //更新连接状态
        private void UpdateConnectionState(bool connected, string args = "")
        {
            if (!previousConnectionState && connected)//如果之前没有接入头显且当前要更新为有头显接入
            {
                previousConnectionState = connected;//更新已经有头显接入
            }
            else if (previousConnectionState && !connected)//如果之前有接入头显且当前要更新为没有头显接入
            {
                previousConnectionState = connected;//更新已经有头显接入
            }
        }

        //解析字符串中的字段及取值
        private Dictionary<string, string> ParsePacket(string str)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var line in str.Split("\n".ToCharArray()))
            {
                var elem = line.Split(" ".ToCharArray(), 2);
                if (elem.Length != 2)
                {
                    continue;
                }
                dict.Add(elem[0], elem[1]);
            }
            return dict;
        }

        //连接头盔
		async private void UpdateClients()
        {
            if (!socket.Connected)//未与头显驱动取得连接
            {
                //AosenUpdatelog("SteamVR is not running.");
                UpdateConnectionState(false);//更新连接状态为false
                clientStatus = "Disconnected";//AosenUpdate
                //log("client Disconnected");
                if(needToRestartSteamVR)//需要重启SteamVR
                {
                    needToRestartSteamVR = false;
                    LaunchServer();//启动SteamVR
                    //log("Restart SteamVR.");
                }
                return;
            }
            string str = await socket.SendCommand("GetConfig");//获取配置及头显接入等信息
            if (str == "")//未能成功获取配置信息
            {
                clientStatus = "Disconnected";//AosenUpdate
                //log("client Disconnected"); //AosenUpdate
                UpdateConnectionState(false);//更新连接状态为false
                return;
            }

            string result = await socket.SendCommand("GetRestartingFlag");//获取SteamVR是否正在重启动过程中
            if("true" == result)
            {
                startingSteamVR = true;
                //log("SteamVR is still running, and it needs to be restarted.");
            }
            else
            {
                startingSteamVR = false;
                //log("SteamVR is running.");
            }


            var configs = ParsePacket(str);
            if (configs["Connected"] == "1")//已有头显接入
            {
                deviceBrand = configs["ClientName"];
                deviceID = configs["ClientId"];
                clientStatus = "Connected";//AosenUpdate
                //log("client Connected");//AosenUpdate
                UpdateConnectionState(true);//设置为已有头盔接入

                //更新服务器状态
                UpdateServerStatus("Connected!\r\n\r\n"  + configs["ClientName"] + ":" + configs["ClientId"] + "\r\n"
                      + "ServerPort:" + configs["ServerPort"] + "\r\n" + configs["RefreshRate"] + " FPS");//

                UpdateClientStatistics();//更新体验指标显示
                return;
            }
            deviceBrand = "";
            UpdateConnectionState(false);//更新状态为没有头盔接入
            //next 2 lines AosenUpdate
            clientStatus = "Disconnected";
            //log("client Disconnected");

            string clientsStr = await socket.SendCommand("GetRequests");//获取等待接入的头盔信息
            if("" == clientsStr)
                return;
            var clients = clientList.ParseRequests(clientsStr);//解析当前有哪些头盔等待连接

            //如果有头盔接入就直接把该头盔写入到自动连接列表
            foreach (var client in clients)
            {
                if (client.Name == "" || client.Address == "")
                {
                    break;
                }
                clientList.EnableAutoConnect = true;//设置自动连接头盔
                if (!clientList.getList().Contains(client))//该头盔之前在自动连接列表中不存在
                {
                    //增加该头盔到自动连接列表
                    clientList.AddAutoConnect(client);
                    return;//这里只添加自动连接列表中的第一个头盔
                }
            }

            //获取一个需要自动连接的头盔
            var autoConnect = clientList.GetAutoConnectableClient();
            if (autoConnect != null)
            {
                if(config.driverConfig.multiHmdSupport)//设置了支持多种头显
                { 
                    if(null == previousHmd.Address || previousHmd.Equals(autoConnect))//如果头盔型号不变或者从未连接过头盔则直接连接
                    { 
                        if (false == startingSteamVR)//如果SteamVR已启动完毕
                        {
                            //连接头显前可以修改一些配置
                            refreshConfig(autoConnect);//根据不同种类的头显刷新
                            await clientList.Connect(socket, autoConnect);//与头盔建立连接
                            previousHmd = autoConnect.Copy() as ClientList.Client;//记录已连接的头盔型号
                            log("Try to connect HMD "+ autoConnect.Name + ".");
                        }
                    }
                    else//否则标记SteamVR需要重启
                    {
                        startingSteamVR = true;//标记头显驱动正在启动
                        await socket.SendCommand("SetRestarting true");//设置头显驱动内部重启标志位为true
                        ShutDownSteamVR();//关闭头显驱动
                        previousHmd = new ClientList.Client();//重置已连接头显的信息
                        needToRestartSteamVR = true;//标记需要重启SteamVR
                        log("Different HMD is detected, shutdown SteamVR in order to restart it.");
                    }
                }
                else//不支持多种头显
                {
                    await clientList.Connect(socket, autoConnect);//与头盔建立连接
                    previousHmd = autoConnect.Copy() as ClientList.Client;//记录已连接的头盔型号
                }
            }
        }

        //更新体验指标的显示
        async private void UpdateClientStatistics()
        {
            string str = await socket.SendCommand("GetStat");//取得指标数据
            experienceDataTextBox.Text = "";//清除原有显示内容
            int i = 0;

            string width = "";
            string heigth = "";
            foreach (var line in str.Split("\n".ToCharArray()))//按行切分数据指标，每行代表一个数据指标
            {
                var elem = line.Split(" ".ToCharArray(), 4);//对每行按空格切分后，elem[1]代表指标中文名称，elem[0]代表指标英文名称，elem[2]代表指标取值，elem[3]代表指标单位
                if (elem.Length != 4)//跳过异常数据
                {
                    continue;
                }

                
                //处理异常数值
                string defaultRender = "16.7 ms";
                if ("GameRenderLatency" == elem[0])
                {
                    if (elem[2].Length > 8)
                    {
                        elem[2] = defaultRender;
                    }
                }

                if ("RenderWidth" == elem[0])
                {
                    if ("0" != elem[2])
                    {
                        width = elem[2];
                    }
                }
                if ("RenderHeight" == elem[0])
                {
                    if ("0" != elem[2])
                    {
                        heigth = elem[2];
                    }
                }

                experienceDataTextBox.Text += elem[1] + "（" + elem[0] + "）: " + elem[2] + elem[3] + "\r\n";//向界面展示一个指标

                i++;
            }
        }

        //Start按钮点击事件响应
        private void startButton_Click(object sender, EventArgs e)
        {
            LaunchServer();
        }

        //启动SteamVR
        private void LaunchServer()
        {
            if (!DriverInstaller.InstallDriver())
            {
                CheckDriverInstallStatus();
                return;
            }
            CheckDriverInstallStatus();

            if (!SaveConfig())
            {
                return;
            }

             //Utils.LaunchOnlySteamVR();//仅启动SteamVR
             Utils.LaunchSteam();//启动Steam和SteamVR
        }

        //保存头显驱动配置到内存
        private bool SaveConfig()
        {
            if (!config.Save(this))
            {
                Application.Exit();
                return false;
            }
            return true;
        }

        /*
         * 根据头显类型刷新配置，当config.driverConfig.multiHmdSupport = true时有效
         */
        private bool refreshConfig(ClientList.Client client)
        {            
            if (2 == client.deviceType)//Quest（设备类型：0：Unknown；1：Go；2：Quest）
            {
                config.driverConfig.hasTouch = true;//Quest双手柄
                config.driverConfig.serialNumber = "WMHD000X000XXX";
                config.driverConfig.trackingSystemName = "oculus";
                config.driverConfig.modelNumber = "Oculus Rift S";
                config.driverConfig.manufacturerName = "Oculus driver 1.38.0";
                config.driverConfig.renderModelName = "generic_hmd";
                config.driverConfig.registeredDeviceType = "oculus_rifts";
                config.driverConfig.controllerTrackingSystemName = "oculus";
                config.driverConfig.controllerManufacturerName = "Oculus";
                config.driverConfig.controllerModelNumber = "Oculus Rift S";
                config.driverConfig.controllerRenderModelNameLeft = "oculus_rifts_controller_left";
                config.driverConfig.controllerRenderModelNameRight = "oculus_rifts_controller_right";
                config.driverConfig.controllerSerialNumber = "WMHD000X000XXX_Controller";
                config.driverConfig.controllerType = "oculus_touch";
                config.driverConfig.controllerRegisteredDeviceType = "oculus_touch";
                config.driverConfig.controllerLegacyInputProfile = "oculus_touch";
                config.driverConfig.controllerInputProfilePath = "{cloudvr_server}/input/touch_profile.json";
                config.driverConfig.renderWidth = client.renderWidth;//使用头盔侧上报的渲染画面的宽（pixel）
                config.driverConfig.renderHeight = client.renderHeight;//使用头盔侧上报的渲染画面的高（pixel）
                //config.driverConfig.renderWidth = 2880;//自行指定渲染画面的宽（pixel）
                //config.driverConfig.renderHeight = 1440;//自行指定渲染画面的高（pixel）
                config.driverConfig.eyeFov = new double[] {
                    client.leftEyeFovLeft, client.leftEyeFovRight, client.leftEyeFovTop, client.leftEyeFovBottom,
                    client.rightEyeFovLeft, client.rightEyeFovRight, client.rightEyeFovTop, client.rightEyeFovBottom
                };//使用头盔侧上报的FOV参数
                //config.driverConfig.eyeFov = new double[] { 52, 42, 53, 47, 42, 52, 53, 47 };//注意：如画面显示不正常可尝试此配置，左、右眼四向FOV配置（单位：°）{ 左眼：左FOV, 左眼：右FOV, 左眼：上FOV, 左眼：下FOV, 右眼：左FOV, 右眼：右FOV, 右眼：上FOV, 右眼：下FOV}

            }
            else if(1 == client.deviceType)//Go
            {
                config.driverConfig.hasTouch = false;//Go单手柄
                config.driverConfig.serialNumber = "HTCVive-001";
                config.driverConfig.trackingSystemName = "Vive Tracker";
                config.driverConfig.modelNumber = "Cloud VR driver server";
                config.driverConfig.manufacturerName = "HTC";
                config.driverConfig.renderModelName = "generic_hmd";
                config.driverConfig.registeredDeviceType = "vive";
                config.driverConfig.controllerTrackingSystemName = "Cloud VR Remote Controller";
                config.driverConfig.controllerManufacturerName = "Cloud VR ";
                config.driverConfig.controllerModelNumber = "Cloud VR Remote Controller";
                config.driverConfig.controllerRenderModelNameLeft = "vr_controller_vive_1_5";
                config.driverConfig.controllerRenderModelNameRight = "vr_controller_vive_1_5";
                config.driverConfig.controllerSerialNumber = "Cloud VR Remote Controller";
                config.driverConfig.controllerType = "vive_controller";
                config.driverConfig.controllerRegisteredDeviceType = "vive_controller";
                config.driverConfig.controllerLegacyInputProfile = "vive_controller";
                config.driverConfig.controllerInputProfilePath = "{cloudvr_server}/input/vive_controller_profile.json";
                config.driverConfig.renderWidth = client.renderWidth;//使用头盔侧上报的渲染画面的宽（pixel）
                config.driverConfig.renderHeight = client.renderHeight;//使用头盔侧上报的渲染画面的高（pixel）
                //config.driverConfig.renderWidth = 2560;//自行指定渲染画面的宽（pixel）
                //config.driverConfig.renderHeight = 1440;//自行指定渲染画面的高（pixel）
                config.driverConfig.eyeFov = new double[] {
                    client.leftEyeFovLeft, client.leftEyeFovRight, client.leftEyeFovTop, client.leftEyeFovBottom,
                    client.rightEyeFovLeft, client.rightEyeFovRight, client.rightEyeFovTop, client.rightEyeFovBottom
                };//使用头盔侧上报的FOV参数
                //config.driverConfig.eyeFov = new double[] { 45, 45, 45, 45, 45, 45, 45, 45 };//左、右眼四向FOV配置（单位：°）{ 左眼：左FOV, 左眼：右FOV, 左眼：上FOV, 左眼：下FOV, 右眼：左FOV, 右眼：右FOV, 右眼：上FOV, 右眼：下FOV}
            }
            else//其它头盔
            {
                config.driverConfig.hasTouch = false;//Go单手柄
                config.driverConfig.serialNumber = "HTCVive-001";
                config.driverConfig.trackingSystemName = "Vive Tracker";
                config.driverConfig.modelNumber = "Cloud VR driver server";
                config.driverConfig.manufacturerName = "HTC";
                config.driverConfig.renderModelName = "generic_hmd";
                config.driverConfig.registeredDeviceType = "vive";
                config.driverConfig.controllerTrackingSystemName = "Cloud VR Remote Controller";
                config.driverConfig.controllerManufacturerName = "Cloud VR ";
                config.driverConfig.controllerModelNumber = "Cloud VR Remote Controller";
                config.driverConfig.controllerRenderModelNameLeft = "vr_controller_vive_1_5";
                config.driverConfig.controllerRenderModelNameRight = "vr_controller_vive_1_5";
                config.driverConfig.controllerSerialNumber = "Cloud VR Remote Controller";
                config.driverConfig.controllerType = "vive_controller";
                config.driverConfig.controllerRegisteredDeviceType = "vive_controller";
                config.driverConfig.controllerLegacyInputProfile = "vive_controller";
                config.driverConfig.controllerInputProfilePath = "{cloudvr_server}/input/vive_controller_profile.json";
                config.driverConfig.renderWidth = 2560;//自行指定渲染画面的宽（pixel）
                config.driverConfig.renderHeight = 1440;//自行指定渲染画面的高（pixel）
                config.driverConfig.eyeFov = new double[] { 45, 45, 45, 45, 45, 45, 45, 45 };//左、右眼四向FOV配置（单位：°）{ 左眼：左FOV, 左眼：右FOV, 左眼：上FOV, 左眼：下FOV, 右眼：左FOV, 右眼：右FOV, 右眼：上FOV, 右眼：下FOV}
            }
            return config.Save(this);//保存配置
        }

        private void ShutDownSteamVR()
        {
            Process[] processList = Process.GetProcessesByName("vrmonitor");
            if (processList.Count() > 0)//判断如果存在
            {
                processList[0].Kill();//关闭程序
            }
        }

        private void Demo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (socket.status == ControlSocket.ServerStatus.CONNECTED)//如果SteamVR是启动状态的则关闭SteamVR，以免下次启动本程序时相关状态标志位时错误的
                ShutDownSteamVR();
        }


        //Aosenupdate Next Method
        async private void submitStatus()
        {
            string url;
            string postData = "{";
            string instanceId;
            string linkStatus = "";
            string errorReason = "";

            if (!steamvrStatus.Equals(""))
            {
                if (steamvrStatus.Equals("Stopped"))
                {
                    linkStatus = "error";
                    if (driverStatus.Equals("Installed"))
                    {
                        errorReason = "SteamVR Not Running";
                    }
                    else
                        errorReason = "Both SteamVR and Driver Not Ready";
                }
                else // SteamVR Running{
                {
                    if (driverStatus.Equals("Uninstalled"))
                    {
                        linkStatus = "error";
                        errorReason = "Driver Not Installed";
                    } else // Driver Installed
                    {
                        if (clientStatus.Equals("Connected"))
                        {
                            linkStatus = "use";
                        }
                        else
                        {
                            linkStatus = "free";
                        }
                    }
                }
            }
            IRestResponse serviceResponse = HttpService.getServerID(); //需要移出该方法
            instanceId = serviceResponse.Content.Split(",".ToCharArray())[1].Split(":".ToCharArray())[1].Replace("\"", "").Trim();
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);  // 获取系统时间
            string linkStatusTime = Convert.ToInt64(ts.TotalMilliseconds).ToString(); //转换为毫秒级别时间戳
            //log(response.Content);
            //log(instanceId);
            postData += "\"" + "nodeID" + "\":" + "\"" + instanceId + "\"" + ",";
            postData += "\"" + "linkStatusTime" + "\":" + "\"" + linkStatusTime + "\"" + ",";
            postData += "\"" + "linkStatus" + "\":" + "\"" + linkStatus + "\"" + ",";
            postData += "\"" + "errorReason" + "\":" + "\"" + errorReason + "\"" + ",";
            postData += "\"" + "deviceBrand" + "\":" + "\"" + deviceBrand + "\"" + ",";
            postData += "\"" + "deviceID" + "\":" + "\"" + deviceID + "\"" + ",";
            //log("before" + postData);
            if (clientStatus.Equals("Connected"))
            {
                //log("Enter");
                int i = 0;
                string width = "";
                string heigth = "";
                monitoringData = await socket.SendCommand("GetStat");//取得指标数据
                foreach (var line in monitoringData.Split("\n".ToCharArray()))
                {
                    if (i == 0)
                    {
                        i++;
                        continue;
                    }
                    var elem = line.Split(" ".ToCharArray(), 4);
                    if (elem.Length != 4)
                    {
                        continue;
                    }
                    //处理异常数值
                    string defaultRender = "16.7 ms";
                    if ("GameRenderLatency" == elem[0])
                    {
                        if (elem[2].Length > 8)
                        {
                            elem[2] = defaultRender;
                        }
                    }

                    if ("RenderWidth" == elem[0])
                    {
                        if ("0" != elem[2])
                        {
                            width = elem[2];
                        }
                    }
                    if ("RenderHeight" == elem[0])
                    {
                        if ("0" != elem[2])
                        {
                            heigth = elem[2];
                        }
                    }
                    postData += "\"" + elem[0] + "\":" + elem[2] + ",";
                }
            }
            else
            {
                monitoringData = "null" ;
                postData += "\"" + "monitoringData" +  "\":" + "\"" +  monitoringData + "\"" + ",";
            }
            postData += "\"" + "taskStatus" + "\":" +  "\"" + taskStatus + "\"}";
            url = "http://" + master_ip + ":80/cloudvr/node/link/servercheck/";
            log(postData);
            IRestResponse submitMessageResponse = HttpService.submitMessage(url, postData);
            // 若上一次任务已结束，则修改状态为空
            if (((int)submitMessageResponse.StatusCode == 200) && !taskStatus.Equals("Executing"))
            {
                taskStatus = "";
                task = null;
            }
            log(submitMessageResponse.Content);
            if(submitMessageResponse.Content == null)
            {
                log("1234sdhfushd78b t1 2 bx jasbdby8172t12g3h7b xshadhb128yx1yd12dq1d2dds1d");
            }
            if(existTask(submitMessageResponse.Content))
            {
                task = Task.Run (() => AppManagement.runPowershell(@"C:\Users\Administrator\Desktop\test.ps1"));                                            
               
                log("Task Executing");
            }
            if(task == null)
            {
                return;
            }
            if (!task.IsCompleted)
            {
                taskStatus = "Executing";
            }else
            {
                string result = "";
                foreach(PSObject res in task.Result)
                {
                    log("123" + res);
                    result += res;
                }
                log("Connected result   1" + result);
                if (result.Equals("success"))
                {
                    taskStatus = "Success";
                }else
                {
                    taskStatus = "Failure";
                }
                   
            }

            //log(response.Content + response.StatusCode)

        }

        private Boolean existTask(string response)
        {

            Dictionary<string, object> dic = JsonToDictionary(response);
            if (!dic.ContainsKey("controlTask"))
            {
                log("No task receives");
                return false;
            }
            log("Task Received");
            return true;
            //Dictionary<string, object> dataSet = (Dictionary<string, object>)dic["controlTask"];

        }

        private Dictionary<string,object> JsonToDictionary(string response)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                return jss.Deserialize<Dictionary<string, object>>(response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
