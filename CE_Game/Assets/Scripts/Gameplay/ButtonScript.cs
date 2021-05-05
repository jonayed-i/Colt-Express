using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonScript : MonoBehaviour
{
	public Button yourButton;
	public GameObject character;

	void Start()
	{
		Button btn = yourButton.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}

	void TaskOnClick()
	{
		Vector3 charPos = character.transform.position;
		charPos.x -= 3.5f;
		character.transform.position = charPos;
		Button btn = yourButton.GetComponent<Button>();
		btn.gameObject.SetActive(false);
	}
}