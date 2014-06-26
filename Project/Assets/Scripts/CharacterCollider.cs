using UnityEngine;
using System.Collections;

public class CharacterCollider : MonoBehaviour {
	public Character character;

	public Character GetCharacter(){

		if(character == null)
			Debug.LogError("Missing character on character collider", gameObject);

		return character;
	}
}
