using System.Collections.Generic;




public class Round {

    public string mEvent = null;

    public List<string> turns = new List<string>(); // tunnel reverse ... 

    private int nextPlayerIndex;
    private int numPlayedInThisRound;
    private int turnsCountTotal = 2;
    public string mCurrentTurn;
    

  

    public Round() {
        
        

    }

    public int turnCount() {
        return this.turns.Count;
    }

    public string startNextTurn() {
        string currentTurn = turns[0];
        mCurrentTurn = currentTurn;
        turns.RemoveAt(0);
        return currentTurn;
    }

    public int getNextPlayerIndex(int playerCount) {
        nextPlayerIndex++;
        numPlayedInThisRound++;
        return (nextPlayerIndex - 1) % playerCount;
    }

    public bool isRoundEnd() {
        return numPlayedInThisRound == turnsCountTotal;
    }
}
