namespace KobGamesSDKSlim.ProjectValidator.Modules.Build
{
    /// <summary>
    /// Result of Validator Modules
    /// </summary>
    public enum eBuildValidatorResult
    {
        AllGood,
        WarningsOnly,
        ErrorsWithModuleDisabled,
        ErrorsNeedFixing
    }
}