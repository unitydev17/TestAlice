using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Linq;


public class GameCtrl : MonoBehaviour
{
	private const string PLAYER_DATA_NAME = "PlayerData";
	private const string PLAYER_TRIES_TAG = "Player.Tries";
	private const string PLAYER_SCORE_TAG = "Player.Score";
	private const string MESSAGE_BOX_NAME = "MessageBox";
	private const int DELAY_AFTER_GUESSED = 2;
	private const string GAME_OVER_MESSAGE = "Game over";
	private const string NO_MORE_WORDS_MESSAGE = "No more words";

	private LayoutCtrl layoutCtrl;
	private Config config;
	private GameObject messageBox;
	private Text triesTextUI;
	private Text scoreTextUI;

	private List<GameObject> wordLetters;
	private string word = null;
	private int guessedLettersQty;
	private TextAsset textAsset;
	private int parsePassCount;

	private IOrderedEnumerable<KeyValuePair<string, int>> sortedList;
	private Dictionary<string, int> allWordsDictionary;
	private HashSet<string> usedWords;


	void GameOverMessage()
	{
		layoutCtrl.FreezeButtons();
		messageBox.GetComponentInChildren<Text>().text = GAME_OVER_MESSAGE;
		messageBox.SetActive(true);
	}


	void NoMoreWordsMessage()
	{
		layoutCtrl.FreezeButtons();
		messageBox.GetComponentInChildren<Text>().text = NO_MORE_WORDS_MESSAGE;
		messageBox.SetActive(true);
	}


	void SetPredefinedDictionary()
	{
		allWordsDictionary.Clear();
		allWordsDictionary.Add("ONE", 1);
		allWordsDictionary.Add("TWO", 20);
		allWordsDictionary.Add("THREE", 3);
	}


	void Awake()
	{	
		allWordsDictionary = new Dictionary<string, int>();
		usedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		layoutCtrl = GetComponent<LayoutCtrl>();
		config = GetComponent<Config>();

		messageBox = GameObject.Find(MESSAGE_BOX_NAME);
		messageBox.SetActive(false);

		InitPlayerUI();
		PlayerModel.instance.Init(config.playerTries);

		GetComponent<LoadAssets>().eventHandler += (asset) => {
			textAsset = asset;
		};

		layoutCtrl.eventHandler += OnClickKeyPadButton;
		MessageBoxCtrl.eventHandler += Restart;
	}


	void InitPlayerUI()
	{
		GameObject playerData = GameObject.Find(PLAYER_DATA_NAME);
		Text[] innerTextComponents = playerData.GetComponentsInChildren<Text>();
		foreach (Text txt in innerTextComponents) {
			if (txt.tag.CompareTo(PLAYER_TRIES_TAG) == 0) {
				triesTextUI = txt;
			} else if (txt.tag.CompareTo(PLAYER_SCORE_TAG) == 0) {
				scoreTextUI = txt;
			}
		}
	}


	public void Restart()
	{
		usedWords.Clear();
		PlayerModel.instance.ResetAll();
		UpdatePlayerInfo();
		NextWord();
		messageBox.SetActive(false);
	}


	void Start()
	{
		PlayerModel.instance.ResetAll();
		UpdatePlayerInfo();

		if (config.predefinedDictionary) {
			SetPredefinedDictionary();
			Restart();
		} else {
			StartCoroutine(LoadTextAsset());
		}
	}


	IEnumerator LoadTextAsset()
	{
		while (textAsset == null) {
			yield return null;
		}
		ParseTextAssets();
	}


	public void ParseTextAssets()
	{
		string[] lines = Regex.Split(textAsset.text, "\n");
		Parser parser = new Parser(OnFirstChunkWordsReturn);
		parser.Parse(ref lines, ref allWordsDictionary);
	}


	public void OnFirstChunkWordsReturn()
	{
		parsePassCount = 1;
		NextWord();
	}


	void NextWord()
	{
		word = GetNextWord();

		if (word == null) {
			NoMoreWordsMessage();
			return;
		}

		ClearWordLetters();
		layoutCtrl.DrawKeyPad();
		layoutCtrl.DrawWord(word, out wordLetters);
		guessedLettersQty = 0;
	}


	void ClearWordLetters()
	{
		if (wordLetters != null) {
			foreach (GameObject letter in wordLetters) {
				Destroy(letter);
			}
			wordLetters.Clear();
		}
	}


	void UpdatePlayerInfo()
	{
		triesTextUI.text = PlayerModel.instance.tries.ToString();
		scoreTextUI.text = PlayerModel.instance.score.ToString();
	}


	void OnClickKeyPadButton(GameObject obj)
	{
		string key = obj.name;

		if (word.ToUpper().Contains(key)) {
			GuessLetter(key);
		} else {
			FaultLetter();
		}
		Destroy(obj);
	}


	void GuessLetter(string key)
	{
		guessedLettersQty += OpenGuessedLetter(key);
		if (guessedLettersQty == word.Length) {
			layoutCtrl.FreezeButtons();
			PlayerModel.instance.Guessed();
			UpdatePlayerInfo();
			StartCoroutine(NextWordDelayed());
		}
	}


	IEnumerator NextWordDelayed()
	{
		yield return new WaitForSeconds(DELAY_AFTER_GUESSED);
		PlayerModel.instance.ResetNew();
		UpdatePlayerInfo();
		NextWord();
	}


	void FaultLetter()
	{
		if (PlayerModel.instance.Fault()) {
			GameOverMessage();
		}
		UpdatePlayerInfo();
	}


	int OpenGuessedLetter(string key)
	{
		int count = 0;
		foreach (GameObject obj in wordLetters) {
			if (obj.name.ToUpper().CompareTo(key) == 0) {
				obj.GetComponentInChildren<Text>().text = key;
				count++;
			}
		}
		return count;
	}


	public string GetNextWord()
	{
		lock (allWordsDictionary) {

				
			bool frequencyChaos = config.useLessFrequentWords == config.useMoreFrequentWords;
			if (frequencyChaos) {
				foreach (string key in allWordsDictionary.Keys) {
					if (!usedWords.Contains(key) && key.Length >= config.minWordLength) {
						usedWords.Add(key);
						return key;
					}
				}
			} else {
				
				// Two parse passes exist. 
				// First is to fast find first word, second - all data is parsed.
				// So need sorting twice.

				if (parsePassCount <= 2) {
					SetSortedList();
					parsePassCount++;
				}

				foreach (KeyValuePair<string, int> pair in sortedList) {
					if (!usedWords.Contains(pair.Key) && pair.Key.Length >= config.minWordLength) {
						usedWords.Add(pair.Key);
						return pair.Key;
					}
				}
			}
		}
		return null;
	}


	void SetSortedList()
	{
		if (config.useLessFrequentWords) {
			sortedList = from entry in allWordsDictionary
			             orderby entry.Value ascending
			             select entry;
		} else if (config.useMoreFrequentWords) {
			sortedList = from entry in allWordsDictionary
				orderby entry.Value descending
			             select entry;
		}
	}
}
