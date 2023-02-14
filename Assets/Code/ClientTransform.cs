using UnityEngine;
using Unity.Netcode.Components;


[DisallowMultipleComponent]
public class ClientTransform : NetworkTransform
{
	protected override bool OnIsServerAuthoritative()
	{
		return	false;
	}
}
