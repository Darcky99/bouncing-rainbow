using KobGamesSDKSlim.GameManagerV1;
using KobGamesSDKSlim.MenuManagerV1;

namespace KobGamesSDKSlim
{
    /// <summary>
    /// This class should handle the core game loop of your game. The main types of things
    /// that you should expect to do within this class is preparing and starting levels, as
    /// well as handling level completion/failure
    ///
    /// Under the hood, the game manager handles various internal processes at each stage of
    /// the loop, such as sending analytic events to our back-ends to monitor the behaviour
    /// of players, or displaying ads/popups at certain levels and so on.
    ///
    /// So, and most importantly, remember that any methods that you override, follow the
    /// general practice of calling the base method before implementing your own code
    ///
    /// Some additional notes:
	/// - you will be calling LevelStarted when the player starts playing a level
    /// - you will be calling GameOver if the player fails to complete the current level
    /// - you will be calling LevelCompleted if the player successfully completes the level
    /// - calling ResetGame will reset the game and then load the current level (similar to a scene reload)
    /// - calling ResetLevel will reset the current level to the beginning
    /// </summary>
    [ExecutionOrder(eExecutionOrder.GameManager)]
    public class GameManager : GameManagerBase
    {
        #region Unity Functions
        //If you need to use the Awake method, please use the override version
        protected override void OnAwakeEvent() {
            base.OnAwakeEvent();
            
            //note that StorageManager.Instance.CurrentLevel is not a persistent property, but HighScoreLevel is
            //so, one thing you will likely need to do here is inherit the current HighScoreLevel on startup so 
            //that the player starts from the last level they finished on:
            StorageManager.Instance.CurrentLevel = StorageManager.Instance.HighScoreLevel;
        }

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        //If you need to use the Start method, please use the override version
        public override void Start() {
            base.Start();
            
            //your code goes here...
        }

        protected override void Update()
        {
            base.Update();

            //your code goes here...
        }
        #endregion

        #region LevelLoop
        /// <summary>
        /// This method is the very first process in your core loop.
        ///
        /// This method is called automatically when when ResetGame or ResetLevel is called
        /// or when the game opens
        /// </summary>
        protected override void LevelLoaded() {
            base.LevelLoaded();
            
            //your code goes here, for example, preparing the current level before playing
        }
        
        /// <summary>
        /// This method is triggered by you, when the game starts. For example,
        /// when the player taps "TAP TO PLAY"
        /// </summary>
        public override void LevelStarted() {
            base.LevelStarted();

            //your code goes here, for example, hiding the main screen and starting the game
        }

        /// <summary>
        /// This method is triggered when you call GameOver(false), which is the usual process
        /// </summary>
        protected override void LevelFailedNoContinue() {
            base.LevelFailedNoContinue();

            //your code goes here, for example, show level failed UI before calling ResetLevel
            if (!GameConfig.Instance.Menus.OpenLevelFailedScreenAfterReviveScreen && MenuManager.Instance.IsScreenOpened(nameof(Screen_LevelFailedwithRevive)))
                ResetGame();
            else
                MenuManager.Instance.OpenMenuScreen(nameof(Screen_LevelFailed));
        }

        /// <summary>
        /// This method is triggered when you call GameOver(true), which is normally not used
        /// </summary>
        protected override void LevelFailed() {
            base.LevelFailed();

            //your code goes here, for example, show level failed UI before calling ResetLevel
            MenuManager.Instance.OpenMenuScreen(nameof(Screen_LevelFailedwithRevive));
        }

        /// <summary>
        /// This method is triggered by you, when the player completes the current level
        /// </summary>
        public override void LevelCompleted()
        {
            base.LevelCompleted();

            MenuManager.Instance.OpenMenuScreen(nameof(Screen_LevelCompleted));

            //your code goes here, for example, increase the current level before the game resets:
            StorageManager.Instance.CurrentLevel++;
        }
        #endregion 
    }
}