using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Config : MonoBehaviour
{

	public int minWordLength;
	public int playerTries;

	[Header("Dictionary")]
	public bool predefinedDictionary;
	public bool useMoreFrequentWords;
	public bool useLessFrequentWords;

}
