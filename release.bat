set outputRootDirName=release
set outputDirName=ServerDemo
mkdir %outputRootDirName%
mkdir %outputRootDirName%\%outputDirName%
mkdir %outputRootDirName%\%outputDirName%\logs
xcopy /y VRServerSDK\bin\x64\Release\VRServerSDK.exe %outputRootDirName%\%outputDirName%
xcopy /y CrashReport\bin\Release\CrashReport.exe %outputRootDirName%\%outputDirName%
xcopy /y VRServerSDK\bin\x64\Release\*.dll %outputRootDirName%\%outputDirName%
xcopy /y/s/e/i driver %outputRootDirName%\%outputDirName%\driver

pause
