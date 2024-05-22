rmdir /s /q "%cd%\Assets\Plugins\Android"
rmdir /s /q "%cd%\Assets\Plugins\iOS"
rmdir /s /q "%cd%\Assets\Plugins\x86_64"
del /q "%cd%\Assets\Plugins\PluginsVersion.asset"
del /q "%cd%\Assets\Plugins\PluginsVersion.asset.meta"
del /q "%cd%\Assets\Plugins\google-services.json.meta"
del /q "%cd%\Assets\Plugins\PluginsVersion.asset"

echo d | xcopy /s /y "D:/GameDev/KobGameSSDK-Slim/Assets/Plugins/Resources" "%cd%\Assets\Plugins\Resources"
echo d | xcopy /s /y "D:/GameDev/KobGameSSDK-Slim/Assets/Plugins/Android" "%cd%\Assets\Plugins\Android"
echo d | xcopy /s /y "D:/GameDev/KobGameSSDK-Slim/Assets/Plugins/iOS" "%cd%\Assets\Plugins\iOS"
rem echo d | xcopy /s /y "D:/GameDev/KobGameSSDK-Slim/Assets/Plugins/x86_64" "%cd%\Assets\Plugins\x86_64"

pause