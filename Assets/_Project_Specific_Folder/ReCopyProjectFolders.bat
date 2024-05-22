echo d | xcopy /s /y "D:\GameDev\KobGameSSDK-Slim\Assets\_Project_Specific_Folder\Scripts\Managers\Pool" "%cd%\Assets\_Project_Specific_Folder\Scripts\Managers\Pool"
echo d | xcopy /s /y "D:\GameDev\KobGameSSDK-Slim\Assets\_Project_Specific_Folder\Scripts\Enums" "%cd%\Assets\_Project_Specific_Folder\Scripts\Enums"
echo d | xcopy /s /y "D:\GameDev\KobGameSSDK-Slim\Assets\_Project_Specific_Folder\Scripts\Constants" "%cd%\Assets\_Project_Specific_Folder\Scripts\Constants"
echo d | xcopy /s "D:\GameDev\KobGameSSDK-Slim\Assets\_Project_Specific_Folder\Scripts\Managers\Game" "%cd%\Assets\_Project_Specific_Folder\Scripts\Managers\Game"
echo d | xcopy /s /y "D:\GameDev\KobGameSSDK-Slim\Assets\_Project_Specific_Folder\Scripts\GameConfig" "%cd%\Assets\_Project_Specific_Folder\Scripts\GameConfig"
echo d | xcopy /s /y "D:\GameDev\KobGameSSDK-Slim\Assets\_Project_Specific_Folder\Resources\FacebookSDK" "%cd%\Assets\_Project_Specific_Folder\Resources\FacebookSDK"
echo d | xcopy /s /y "D:\GameDev\KobGameSSDK-Slim\Assets\_Project_Specific_Folder\Resources\GameConfig.asset" "%cd%\Assets\_Project_Specific_Folder\Resources\"
echo d | del /F "%cd%\Assets\_Project_Specific_Folder\Scripts\Enums\eExecutionOrder.cs"

pause