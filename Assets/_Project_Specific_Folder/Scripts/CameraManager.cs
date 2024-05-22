using System;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecutionOrder(eExecutionOrder.CameraManager)]
public class CameraManager : Singleton<CameraManager>
{
	[SerializeField, ReadOnly] private Transform m_Rig;
	[SerializeField, ReadOnly] private Transform m_Gimbal;
	[SerializeField, ReadOnly] private Transform m_Dolly;

	[SerializeField, ReadOnly] private Camera m_MainCamera;
	[SerializeField, ReadOnly] private Camera m_UICamera;

	public Camera MainCamera => m_MainCamera;
	public Camera UICamera   => m_UICamera;

	private CameraVariablesEditor m_CameraVars => GameConfig.Instance.Camera;

	[Button]
	private void SetRef()
	{
		m_Rig    = transform;
		m_Gimbal = transform.FindDeepChild("Gimbal");
		m_Dolly  = transform.FindDeepChild("Dolly");

		m_MainCamera = Camera.main;
		m_UICamera   = GameObject.Find("Camera UI").GetComponent<Camera>();
	}

	private void OnEnable()
	{
		if (GameConfig.Instance.Camera.ZoomType != 0)
			InputManager.OnTouchZoomDelta += onTouchZoomDelta;
	}

	public override void OnDisable()
	{
		if (GameConfig.Instance.Camera.ZoomType != 0)
			InputManager.OnTouchZoomDelta -= onTouchZoomDelta;
	}

	private void onTouchZoomDelta(float i_DeltaZoom)
	{
		if (GameManager.Instance.GameState != eGameState.Playing)
			return;
		
		if (GameConfig.Instance.Camera.ZoomType.HasFlag(eCameraZoomType.Dolly))
				m_Dolly.localPosition = m_Dolly.localPosition.SetPositionZ(Mathf.Clamp(m_Dolly.localPosition.z + i_DeltaZoom * m_CameraVars.ZoomDolly.Sensitivity, m_CameraVars.ZoomDolly.Limits.x, m_CameraVars.ZoomDolly.Limits.y));
		if (GameConfig.Instance.Camera.ZoomType.HasFlag(eCameraZoomType.Ortho))
				m_MainCamera.orthographicSize = Mathf.Clamp(m_MainCamera.orthographicSize + i_DeltaZoom * m_CameraVars.ZoomOrtho.Sensitivity, m_CameraVars.ZoomOrtho.Limits.x, m_CameraVars.ZoomOrtho.Limits.y) ;
		if (GameConfig.Instance.Camera.ZoomType.HasFlag(eCameraZoomType.Perspective))
				m_MainCamera.fieldOfView = Mathf.Clamp(m_MainCamera.fieldOfView + i_DeltaZoom * m_CameraVars.ZoomPerspective.Sensitivity, m_CameraVars.ZoomPerspective.Limits.x, m_CameraVars.ZoomPerspective.Limits.y) ;
	}
}