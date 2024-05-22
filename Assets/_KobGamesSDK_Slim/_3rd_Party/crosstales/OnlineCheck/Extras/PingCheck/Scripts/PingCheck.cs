using System.Collections;
using UnityEngine;

namespace Crosstales.OnlineCheck.Tool
{
   [System.Serializable]
   public class PingCompleteEvent : UnityEngine.Events.UnityEvent<float>
   {
   }

   /// <summary>Checks the Ping to an Internet address.</summary>
   [ExecuteInEditMode]
   [DisallowMultipleComponent]
   [HelpURL("https://www.crosstales.com/media/data/assets/OnlineCheck/api/class_crosstales_1_1_online_check_1_1_tool_1_1_ping_check.html")]
   public class PingCheck : Crosstales.Common.Util.Singleton<PingCheck>
   {
      #region Variables

      [UnityEngine.Serialization.FormerlySerializedAsAttribute("HostName")] [Header("General Settings"), Tooltip("Hostname or IP for the Ping."), SerializeField]
      private string hostName = "google.com";

      [UnityEngine.Serialization.FormerlySerializedAsAttribute("Timeout")] [Tooltip("Timeout for the Ping in seconds (default: 3)."), Range(1, 10), SerializeField]
      private float timeout = 3f;


      [UnityEngine.Serialization.FormerlySerializedAsAttribute("RunOnStart")] [Header("Behaviour Settings"), Tooltip("Start at runtime (default: false)."), SerializeField]
      private bool runOnStart;

      private int lastPingTimeMs;

      #endregion


      #region Events

      [Header("Events")] public PingCompleteEvent OnPingComplete;

      /// <summary>Callback to determine whether the Ping-call has completed.</summary>
      public delegate void PingCompleted(string host, string ip, float time);

      /// <summary>An event triggered whenever the Ping-call has completed.</summary>
      public event PingCompleted OnPingCompleted;

      #endregion


      #region Properties

      /// <summary>Hostname or IP for the Ping.</summary>
      public string HostName
      {
         get => hostName;
         set => hostName = value;
      }

      /// <summary>Timeout for the Ping in seconds (default: 3, range: 1 - 10).</summary>
      public float Timeout
      {
         get => timeout;
         set => timeout = Mathf.Clamp(value, 1f, 10f);
      }

      /// <summary>Start at runtime.</summary>
      public bool RunOnStart
      {
         get => runOnStart;
         set => runOnStart = value;
      }

      /// <summary>Returns the last host.</summary>
      /// <returns>Last host.</returns>
      public string LastHost { get; private set; }

      /// <summary>Returns the last IP.</summary>
      /// <returns>Last IP.</returns>
      public string LastIP { get; private set; }

      /// <summary>Returns the last ping time in seconds.</summary>
      /// <returns>Last ping time in seconds.</returns>
      public float LastPingTime => LastPingTimeMilliseconds / 1000f;

      /// <summary>Returns the last ping time in milliseconds.</summary>
      /// <returns>Last ping time in milliseconds.</returns>
      public int LastPingTimeMilliseconds
      {
         get => lastPingTimeMs;
         private set => lastPingTimeMs = Mathf.Clamp(value, 0, 9999999);
      }

      /// <summary>Returns true if SpeedTest is busy.</summary>
      /// <returns>True if if SpeedTest is busy.</returns>
      public bool isBusy { get; private set; }

      /// <summary>Indicates if PingCheck is supporting the current platform.</summary>
      /// <returns>True if PingCheck supports current platform.</returns>
      public bool isPlatformSupported => !Util.Helper.isWebPlatform;

      #endregion


      #region MonoBehaviour methods

      protected override void Awake()
      {
         base.Awake();

         if (Instance == this)
            LastPingTimeMilliseconds = 0;
      }

      private void Start()
      {
         if (runOnStart || Util.Helper.isEditorMode)
            Ping();
      }

      #endregion


      #region Public methods

      /// <summary>Checks the ping with the 'HostName'-variable.</summary>
      public void Ping()
      {
         Ping(hostName);
      }

      /// <summary>Checks the ping with the given host name.</summary>
      /// <param name="hostname">Host name or IP for the ping</param>
      public void Ping(string hostname)
      {
         if (this != null && !isActiveAndEnabled)
            return;

         if (!isBusy)
            StartCoroutine(ping(hostname));
      }

      #endregion


      #region Private methods

      private IEnumerator ping(string hostname)
      {
//#if UNITY_WSA && !UNITY_EDITOR
#if !UNITY_WEBGL || UNITY_EDITOR
         LastPingTimeMilliseconds = 0;
         LastHost = hostname;

         if (string.IsNullOrEmpty(hostname))
         {
            Debug.LogWarning("Hostname is null or empty! Please add a valid host or ip.", this);
         }
         else
         {
            isBusy = true;

            string ip = LastIP = Crosstales.Common.Util.NetworkHelper.GetIP(hostname);

            Ping ping = new Ping(ip);

            float elapsed = 0;

            do
            {
               yield return null;
               elapsed += Time.deltaTime;
            } while (!ping.isDone && elapsed <= timeout);

            LastPingTimeMilliseconds = ping.time;
            onPingCompleted(hostname, ip, LastPingTime);

            isBusy = false;
         }
#else
			Debug.LogWarning("'PingCheck' is not supported under WebGL!", this);
			yield return null;
#endif
/*
#else
			string ip = string.Empty;
			long duration = 0;

			System.Threading.Thread worker = new System.Threading.Thread(() => pingCheck(hostname, out ip, out duration));
			worker.Start();

			do
			{
				yield return null;
			} while (worker.IsAlive);

			Debug.Log(ip, this);

			LastPingTimeMilliseconds = (int)duration;

			onPingCompleted(hostname, ip, duration / 1000f);
#endif
*/
      }

/*
#if !UNITY_WSA || UNITY_EDITOR
		public IPAddress ToIPAddress(string hostNameOrAddress, bool favorIpV6 = false)
		{
			var favoredFamily = favorIpV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
			var addrs = Dns.GetHostAddresses(hostNameOrAddress);
			return addrs.FirstOrDefault(addr => addr.AddressFamily == favoredFamily)
			       ??
			       addrs.FirstOrDefault();
		}

		private void pingCheck(string hostname, out string address, out long duration)
		{
			isBusy = true;

			IPAddress ip = ToIPAddress(hostname);

			address = ip.ToString();
			duration = -1;

			try
			{
				System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
				System.Net.NetworkInformation.PingReply r = p.Send(ip, 5000);

				if (r.Status == System.Net.NetworkInformation.IPStatus.Success)
				{
					//address = r.Address.ToString();
					duration = r.RoundtripTime;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Could not ping the host '" + hostname + "': " + ex, this);
			}

			isBusy = false;
		}
#endif
*/

      #endregion


      #region Event-trigger methods

      private void onPingCompleted(string host, string ip, float time)
      {
         if (Util.Config.DEBUG)
            Debug.Log($"onPingCompleted: {host} ({ip}) - {time}", this);

         if (!Util.Helper.isEditorMode)
            OnPingComplete?.Invoke(time);

         OnPingCompleted?.Invoke(host, ip, time);
      }

      #endregion
   }
}
// © 2020-2021 crosstales LLC (https://www.crosstales.com)