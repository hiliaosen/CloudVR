using Codeplex.Data;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRServerSDK
{
    class ServerConfig
    {
        public dynamic driverConfig = new DynamicJson();//用于保存配置参数的成员

        private static readonly string APP_FILEMAPPING_NAME = "CLOUDVR_DRIVER_FILEMAPPING_0B124897-7730-4B84-AA32-088E9B92851F";//用于保存设置的内存文件名，请勿修改

        MemoryMappedFile memoryMappedFile;//用于保存设置的内存文件

        public ServerConfig()
        {
            driverConfig.CLOUD_VR_SERVER_KEY = "e5aefd214138b89868ae5b9b6fcbe7cf2aeab635c3e1ed71fc2bdcb308688979";//服务端KEY

            driverConfig.delayedListeningTime = 1000;//单位：ms，SteamVR每次启动后延迟监听头显接入的时间，此时间设置能够避免头显在StreamVR刚刚启动后就马上接入引起的SteamVR崩溃问题
            driverConfig.multiHmdSupport = false;//多种头显支持，为false时将仅支持以下列默认头显参数初始化头显，同时可适当调低driverConfig.delayedListeningTime


            int fps = 60;//帧率

            int bitrateInMBits = 30;//固定码率编码时的码率（Mbps）
            driverConfig.enableFixedBitrate = false;//是否开启固定码率
            
            driverConfig.enableDynamicBitrate = true;//是否开启动态码率，当driverConfig.enableFixedBitrate = true时，该项必须为false，反之亦然
            driverConfig.maxDynamicBitrateInMBits = 30;//开启动态码率后，设置码率上限（Mbps）
            driverConfig.pictureQuality = 50;//开启动态码率后设置画质,取值范围：1~100的整数，数值越小画质越差，占用带宽越小

            driverConfig.codec = 1; //编码格式 0: H264, 1: H265

            driverConfig.adapterIndex = 0;
            driverConfig.IPD = 0.064;//瞳距，单位：米
            driverConfig.secondsFromVsyncToPhotons = 0.005;
            driverConfig.displayFrequency = fps;
            driverConfig.listenPort = 9944;//监听头显接入的UDP端口
            driverConfig.listenHost = "0.0.0.0";//监听头显接入的IP地址
            driverConfig.sendingTimeslotUs = 500;
            driverConfig.limitTimeslotPackets = 0;
            driverConfig.controlListenPort = 9944;//与头显驱动通信的TCP端口，如需更改请同步更改ControlSocket.cs中“int m_Port = 9944”的赋值
            driverConfig.controlListenHost = "127.0.0.1";//与头显驱动通信的本地回环地址，请勿更改
            driverConfig.useKeyedMutex = true;
            driverConfig.nvencOptions = "-preset ll_hq -rc cbr_ll_hq -fps " + fps + " -bitrate " + bitrateInMBits + "M -maxbitrate " + bitrateInMBits + "M";//编码器初始化命令
            driverConfig.encodeFPS = fps;//编码帧率
            driverConfig.encodeBitrateInMBits = bitrateInMBits;//采用固定码率编码时有效
            
            driverConfig.debugOutputDir = Utils.GetOutputPath();//调试信息输出目录
            driverConfig.debugLog = false;//开启调试日志，输出到logs文件夹
            driverConfig.enableCsv = false;//输出CSV格式的体验数据
            driverConfig.debugFrameIndex = false;
            driverConfig.debugFrameOutput = false;
            driverConfig.debugCaptureOutput = false;
            driverConfig.useKeyedMutex = true;

            driverConfig.clientRecvBufferSize = GetBufferSizeKB() * 1000;
            driverConfig.frameQueueSize = GetFrameQueueSize(false);
            driverConfig.enableController = true;

            //From OpenVR EVRButtonId: None：-1, System：0, ApplicationMenu：1, Grip：2, DPad_Left：3, DPad_Up：4, DPad_Right：5, DPad_Down：6, A Button：7, B Button：8, X Button：9, Y Button：10, Trackpad：28, Trigger：24, Shoulder Left：13, Shoulder Right：14, Joystick Left：15, Joystick Right：18, Back：21, Guide：22, Start：23
            driverConfig.controllerTriggerMode = 24;
            driverConfig.controllerTrackpadClickMode = 28;
            driverConfig.controllerTrackpadTouchMode = 29;
            driverConfig.controllerBackMode = 0;

            // 0=Disabled, 1=Trigger, 2=Trackpad Click, 3=Trackpad Touch, 4=Back
            driverConfig.controllerRecenterButton = 0;
            driverConfig.useTrackingReference = false;

            driverConfig.enableOffsetPos = false;
            driverConfig.offsetPosX = Utils.ParseFloat("0.0");
            driverConfig.offsetPosY = Utils.ParseFloat("0.0");
            driverConfig.offsetPosZ = Utils.ParseFloat("0.0");

            driverConfig.trackingFrameOffset = Utils.ParseInt("0");

            driverConfig.enableSound = false;
            driverConfig.soundDevice = "";

            //xiaomi 
            driverConfig.hasTouch = false;//Go单手柄
            driverConfig.serialNumber = "HTCVive-001";
            driverConfig.trackingSystemName = "Vive Tracker";
            driverConfig.modelNumber = "Cloud VR driver server";
            driverConfig.manufacturerName = "HTC";
            driverConfig.renderModelName = "generic_hmd";
            driverConfig.registeredDeviceType = "vive";
            driverConfig.controllerTrackingSystemName = "Cloud VR Remote Controller";
            driverConfig.controllerManufacturerName = "Cloud VR ";
            driverConfig.controllerModelNumber = "Cloud VR Remote Controller";
            driverConfig.controllerRenderModelNameLeft = "vr_controller_vive_1_5";
            driverConfig.controllerRenderModelNameRight = "vr_controller_vive_1_5";
            driverConfig.controllerSerialNumber = "Cloud VR Remote Controller";
            driverConfig.controllerType = "vive_controller";
            driverConfig.controllerRegisteredDeviceType = "vive_controller";
            driverConfig.controllerLegacyInputProfile = "vive_controller";
            driverConfig.controllerInputProfilePath = "{cloudvr_server}/input/vive_controller_profile.json";
            driverConfig.renderWidth = 2560;//自行指定渲染画面的宽（pixel）
            driverConfig.renderHeight = 1440;//自行指定渲染画面的高（pixel）
            driverConfig.eyeFov = new double[] { 45, 45, 45, 45, 45, 45, 45, 45 };//左、右眼四向FOV配置（单位：°）{ 左眼：左FOV, 左眼：右FOV, 左眼：上FOV, 左眼：下FOV, 右眼：左FOV, 右眼：右FOV, 右眼：上FOV, 右眼：下FOV}
            

            /**默认Quest参数
            driverConfig.hasTouch = true;//Quest双手柄
            driverConfig.serialNumber = "WMHD000X000XXX";
            driverConfig.trackingSystemName = "oculus";
            driverConfig.modelNumber = "Oculus Rift S";
            driverConfig.manufacturerName = "Oculus driver 1.38.0";
            driverConfig.renderModelName = "generic_hmd";
            driverConfig.registeredDeviceType = "oculus_rifts";
            driverConfig.controllerTrackingSystemName = "oculus";
            driverConfig.controllerManufacturerName = "Oculus";
            driverConfig.controllerModelNumber = "Oculus Rift S";
            driverConfig.controllerRenderModelNameLeft = "oculus_rifts_controller_left";
            driverConfig.controllerRenderModelNameRight = "oculus_rifts_controller_right";
            driverConfig.controllerSerialNumber = "WMHD000X000XXX_Controller";
            driverConfig.controllerType = "oculus_touch";
            driverConfig.controllerRegisteredDeviceType = "oculus_touch";
            driverConfig.controllerLegacyInputProfile = "oculus_touch";
            driverConfig.controllerInputProfilePath = "{cloudvr_server}/input/touch_profile.json";
            driverConfig.renderWidth = 2880;//自行指定渲染画面的宽（pixel）
            driverConfig.renderHeight = 1440;//自行指定渲染画面的高（pixel）
            driverConfig.eyeFov = new double[] { 52, 42, 53, 47, 42, 52, 53, 47 };//注意：如画面显示不正常可尝试此配置，左、右眼四向FOV配置（单位：°）{ 左眼：左FOV, 左眼：右FOV, 左眼：上FOV, 左眼：下FOV, 右眼：左FOV, 右眼：右FOV, 右眼：上FOV, 右眼：下FOV}
        **/
        }

        //获取缓冲量，一般为200~2000，网络波动较大时可适当调大
        public int GetBufferSizeKB()
        {
                return 2000;
        }

        public int GetFrameQueueSize(bool suppressFrameDrop)
        {
            return suppressFrameDrop ? 5 : 1;
        }

        public bool Save(Demo context)
        {
            try
            {
                driverConfig.enableSound = context.soundDevices.Count() > 0 ? true : false;//如果有声音设备则开启声音
                driverConfig.soundDevice = context.soundDevices.Count() > 0 ? (context.soundDevices[0].id) : "";//context.soundDevices[0].id：默认声音设备
                //打印声音设备日志
                context.log(context.soundDevices.Count() > 0 ? "Use Sound device: " + context.soundDevices[0].name + " Sound device id: " + context.soundDevices[0].id : "No sound device.");

                //将配置保存到内存中供头显驱动使用
                byte[] bytes = Encoding.UTF8.GetBytes(driverConfig.ToString());
                memoryMappedFile = MemoryMappedFile.CreateOrOpen(APP_FILEMAPPING_NAME, sizeof(int) + bytes.Length);

                using (var mappedStream = memoryMappedFile.CreateViewStream())
                {
                    mappedStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
                    mappedStream.Write(bytes, 0, bytes.Length);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error on creating filemapping.\r\nPlease check the status of vrserver.exe and retry.");
                return false;
            }
            return true;
        }
    }
}
