public class DOTweenIDs : DOTWeenIDsBase
{
    public const int Example = 1;

    //This ids won't be killed by OnGameReset. Only add if necessary
    public static object[] Ids = { Example };
}
