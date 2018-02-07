
public class PlayerModel
{

	public static PlayerModel instance = new PlayerModel();
	private int maxTries;

	public int tries;
	public int score;


	public void Init(int maxTries) {
		this.maxTries = maxTries;
	}

	public void ResetAll()
	{
		ResetNew();
		score = 0;
	}


	public void ResetNew()
	{
		tries = maxTries;
	}


	public void Guessed()
	{
		score += tries;
	}


	public bool Fault()
	{
		return --tries < 0;
	}

}
