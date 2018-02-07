using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;


public class Parser
{
	private const int FIRST_CHUNK_QTY = 40;

	private Dictionary<string, int> dictionary;
	private Action callBack;


	public Parser(Action callBack)
	{
		this.callBack = callBack;
	}


	public void Parse(ref string[] lines, ref Dictionary<string, int> dictionary)
	{
		int number = 0;
		this.dictionary = dictionary;
		foreach (string line in lines) {
			Parse(line);
			if (number++ == FIRST_CHUNK_QTY) {
				callBack(); // first chunk
			}
		}
	}


	private void Parse(string line)
	{
		string[] words = Regex.Split(line, @"[^A-Za-z]+");
		foreach (string word in words) {
			string upperWord = word.ToUpper();
			int count;
			if (dictionary.TryGetValue(upperWord, out count)) {
				dictionary[upperWord] = ++count;
			} else {
				dictionary.Add(upperWord, 1);
			}
		}
	}

}
