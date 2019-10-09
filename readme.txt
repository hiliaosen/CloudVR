SDK版本0.0.1

1.编译：
	安装Visual Studio 2017打开VRServerSDK.sln编译此项目（需安装.NET Framework 4.6.1）
	编译方法：
	1）在工具栏“解决方案配置”选“Release”，“解决方案平台”选“x64”，“启动项目”选“VRServerSDK”
	2）参考相关指导获取服务端SDK KEY，替换\VRServerSDK\ServerConfig.cs中driverConfig.CLOUD_VR_SERVER_KEY = "your key"中的your key
	3）来到“解决方案资源管理器”窗口，在 “解决方案‘VRServerSDK’” 上 右键->生成解决方案
2.发布：
	双击release.bat发布程序到目录“release”下
	运行release\ServerDemo\VRServerSDK.exe可启动服务端程序（注意：本SDK仅支持华为云，请在华为云环境下运行）