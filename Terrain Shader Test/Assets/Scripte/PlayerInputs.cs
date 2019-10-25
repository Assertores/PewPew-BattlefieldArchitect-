using UnityEngine;
using System;
using System.Collections;


public class PlayerInputs : MonoBehaviour
{
	public static PlayerInputs instance
	{
		get { return Instance; }
	}

	protected static PlayerInputs Instance;

	[HideInInspector]
	public bool playerControllerInputBlocked;

	protected Vector2 m_Movement;
	protected Vector2 m_Camera;
	protected bool m_Pause;
	protected bool m_ExternalInputBlocked;
	protected bool m_Interactiv;

	public Vector2 MoveInput
	{
		get
		{
			if (playerControllerInputBlocked || m_ExternalInputBlocked)
				return Vector2.zero;
			return m_Movement;
		}
	}

	public Vector2 CameraInput
	{
		get
		{
			if (playerControllerInputBlocked || m_ExternalInputBlocked)
				return Vector2.zero;
			return m_Camera;
		}
	}

	public bool Pause
	{
		get { return m_Pause; }
	}

	public bool Interactiv
	{
		get { return m_Interactiv && !playerControllerInputBlocked && !m_ExternalInputBlocked; }
	}
	
	void Awake()
	{
		if (Instance == null)
			Instance = this;
		else if (Instance != this)
			throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + Instance.name + " and " + name + ".");
	}

	void Update()
	{
		m_Movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		m_Camera.Set(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		m_Interactiv = Input.GetButtonDown("Fire1");

		m_Pause = Input.GetButtonDown("Pause");
	}

	public bool HaveControl()
	{
		return !m_ExternalInputBlocked;
	}

	public void ReleaseControl()
	{
		m_ExternalInputBlocked = true;
	}

	public void GainControl()
	{
		m_ExternalInputBlocked = false;
	}
}
