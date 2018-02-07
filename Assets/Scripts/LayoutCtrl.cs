using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class LayoutCtrl : MonoBehaviour
{

	[Header("KeyPad")]
	public int padLeft;
	public int padRight;
	public int padBottom;
	private int padRows = 2;
	private int padColumns = 12;


	public event Action<GameObject> eventHandler;


	public GameObject buttonPrefab;
	public GameObject letterPrefab;

	private GameObject keyPad;
	private GameObject wordPad;


	void Awake()
	{
		keyPad = GameObject.Find("KeyPad");
		wordPad = GameObject.Find("WordPad");
	}


	void ClearKeyPad()
	{
		foreach (Transform child in keyPad.transform) {
			Destroy(child.gameObject);
		}
	}


	/// <summary>
	/// Draws the key pad.
	/// </summary>
	public void DrawKeyPad()
	{
		ClearKeyPad();

		char key = 'A';
		int cellSize = GetCellSize();
		int halfCell = cellSize / 2;
		for (int i = 0; i < padRows; i++) {
			for (int j = 0; j <= padColumns; j++) {
				GameObject button = Instantiate(buttonPrefab);
				button.transform.position = new Vector3(padLeft + halfCell + j * cellSize, halfCell + padBottom + (padRows - 1 - i) * cellSize);
				button.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);
				button.transform.SetParent(keyPad.transform);
				button.name = key++.ToString();
				button.GetComponentInChildren<Text>().text = button.name;
				button.GetComponent<Button>().onClick.AddListener(() => OnClick(button));
			}
		}
	}


	public void OnClick(GameObject button)
	{
		eventHandler.Invoke(button);
	}


	private int GetCellSize()
	{
		return (Screen.width - padLeft - padRight) / (padColumns + 1);
	}


	/// <summary>
	/// Draws the word.
	/// </summary>
	/// <param name="word">Word.</param>
	/// <param name="wordLetters">Word letters.</param>
	public void DrawWord(string word, out List<GameObject> wordLetters)
	{
		wordLetters = new List<GameObject>();

		int cellSize = GetCellSize();
		int startX = Screen.width / 2 + (1 - word.Length) * cellSize / 2;
		int startY = 2 * Screen.height / 3;

		for (int i = 0; i < word.Length; i++) {
			GameObject letter = Instantiate(letterPrefab);
			letter.transform.position = new Vector3(startX + i * cellSize, startY);
			letter.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);
			letter.transform.SetParent(wordPad.transform);
			letter.name = word.Substring(i, 1).ToUpper();
			wordLetters.Add(letter);
		}
	}


	public void FreezeButtons()
	{
		foreach (Transform child in keyPad.transform) {
			child.GetComponent<Button>().interactable = false;
		}
	}


}
