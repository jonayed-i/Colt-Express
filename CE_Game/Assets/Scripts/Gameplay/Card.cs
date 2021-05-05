[System.Serializable]
public class Card {
    public string name;
    public string owner;
    public int action;
    public bool isHidden;

    public Card(string name, string owner, int action) {
        this.name = name;
        this.owner = owner;
        this.action = action;
        this.isHidden = false;

    }
}
