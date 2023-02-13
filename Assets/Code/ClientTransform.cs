using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;


public class ClientTransform : NetworkTransform
{
	protected override bool OnIsServerAuthoritative()
	{
		return	true;
	}
}
