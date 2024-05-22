using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace KobGamesSDKSlim.MenuManagerV1
{
    [ExecutionOrder(eExecutionOrder.MenuManager)]
	public class MenuManagerBase : Singleton<MenuManager>
	{
		[ReadOnly] public MenuScreensRefs MenuScreensRefs;

        private readonly List<string> OpenedScreens = new List<string>();

        public override void Start()
        {
            base.Start();
        }

        protected virtual void OnEnable()
        {
            GameManager.OnGameReset += OnCoreLoop_Reset;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            GameManager.OnGameReset -= OnCoreLoop_Reset;
        }

        public virtual void OnCoreLoop_Reset()
        {
            OpenedScreens.Clear();

            foreach (var item in MenuScreensRefs.MenuScreenList)
            {
                item.Value.Reset();
            }
        }

        #region Open/Close
        //TODO - don't like it this way
        public void OpenMenuScreen(string i_MenuScreen, bool i_Animate)
        {
            if (MenuScreensRefs != null && MenuScreensRefs.MenuScreenList.ContainsKey(i_MenuScreen))
            {
                MenuScreensRefs.MenuScreenList[i_MenuScreen].Open(i_Animate);
            }
            else
                Debug.LogError("Can't open " + i_MenuScreen + " Menu. Null Ref.");
        }

        public void CloseMenuScreen(string i_MenuScreen, bool i_Animate)
        {
            if (MenuScreensRefs != null && MenuScreensRefs.MenuScreenList.ContainsKey(i_MenuScreen))
            {
                MenuScreensRefs.MenuScreenList[i_MenuScreen].Close(i_Animate);
            }
            else
                Debug.LogError("Can't close " + i_MenuScreen + " Menu. Null Ref.");
        }

        public void OpenMenuScreen(string i_MenuScreen)
		{
            if (MenuScreensRefs != null && MenuScreensRefs.MenuScreenList.ContainsKey(i_MenuScreen))
            {
                MenuScreensRefs.MenuScreenList[i_MenuScreen].Open();
            }
			else
				Debug.LogError("Can't open " + i_MenuScreen + " Menu. Null Ref.");
		}

		public void CloseMenuScreen(string i_MenuScreen)
		{
            if (MenuScreensRefs != null && MenuScreensRefs.MenuScreenList.ContainsKey(i_MenuScreen))
            {
                MenuScreensRefs.MenuScreenList[i_MenuScreen].Close();
            }
			else
				Debug.LogError("Can't close " + i_MenuScreen + " Menu. Null Ref.");
		}
        public void CloseAll()
        {
            if (MenuScreensRefs != null)
            {
                foreach (var item in MenuScreensRefs.MenuScreenList)
                {
                    item.Value.Close();
                }
            }
        }
        #endregion

        #region Helpers
        public T GetMenuScreen<T>() where T : MenuScreenBase
        {
            T result = default;

            if (MenuScreensRefs != null && MenuScreensRefs.MenuScreenList.ContainsKey(typeof(T).Name))
            {
                result = MenuScreensRefs.MenuScreenList[typeof(T).Name] as T;
            }

            return result;
        }

        public bool IsScreenOpened(string i_MenuScreen)
        {
            if (OpenedScreens.Contains(MenuScreensRefs.MenuScreenList[i_MenuScreen].name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region IMenuEvents call
        public virtual void OnMenu_ScreenOpenStart(MenuScreenBase i_MenuScreen)
		{	
		}

		public virtual void OnMenu_ScreenOpenEnd(MenuScreenBase i_MenuScreen)
		{	
            OpenedScreens.Add(i_MenuScreen.name);
        }

		public virtual void OnMenu_ScreenCloseStart(MenuScreenBase i_MenuScreen)
		{
		}

		public virtual void OnMenu_ScreenCloseEnd(MenuScreenBase i_MenuScreen)
		{
            OpenedScreens.Remove(i_MenuScreen.name);
        }
        #endregion
        
    }
}
