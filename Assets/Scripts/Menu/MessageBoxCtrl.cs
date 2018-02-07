using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MessageBoxCtrl : MonoBehaviour
{

	public static event Action eventHandler;

	public void OnRetry()
	{
		eventHandler.Invoke();
	}

}
