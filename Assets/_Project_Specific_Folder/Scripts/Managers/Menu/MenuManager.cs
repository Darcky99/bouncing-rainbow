using KobGamesSDKSlim.MenuManagerV1;

//TODO - Should be normal namespace so, when we want to use another version, we can jsut change above
namespace KobGamesSDKSlim.MenuManagerV1
{
    [ExecutionOrder(eExecutionOrder.MenuManager)]
    public class MenuManager : MenuManagerBase
    {
        public override void Start()
        {
            base.Start();

            if(!IsScreenOpened(nameof(Screen_Welcome)))
                OpenMenuScreen(nameof(Screen_Welcome));
        }
    }
}
