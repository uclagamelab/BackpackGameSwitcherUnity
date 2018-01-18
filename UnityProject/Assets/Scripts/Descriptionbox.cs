using UnityEngine;
using System.Collections;

public class Descriptionbox : MonoBehaviour {
	public string title;
	public string author;
	public string description;
	public string executable;
	public string directory;
	public string image;
	public bool isUnityApp;

	public Texture2D imgTexture;

	TextMesh titleText;
	TextMesh authorText;
	//TextMesh descriptionText;
	public bool selected = false;
	public Color unselectedItem;
	public Color[] selectedColors;

	GameObject backgroundImage;
	// Use this for initialization
	IEnumerator Start () 
	{
		titleText = transform.Find("Title").gameObject.GetComponent<TextMesh>();
		authorText = transform.Find("Author").gameObject.GetComponent<TextMesh>();
		backgroundImage = GameObject.Find ("backgroundImage");
		if(backgroundImage == null)
			Debug.Log ("Couldn't find background image");
		//descriptionText = transform.FindChild("Description").gameObject.GetComponent<TextMesh>();

		assignStringsToText();

		WWW imgLoader = new WWW(@"file:///" + @directory +"/"+ @image);
		yield return imgLoader;
		imgTexture = new Texture2D(1024,1024);
		imgTexture = imgLoader.texture;
		//StartCoroutine(SelectedColorText());
	}
	
	// Update is called once per frame
	void Update () {
		assignStringsToText();
	}

	void assignStringsToText()
	{
		authorText.text = author;

		if(selected)
		{
			titleText.text = "> " + title + " <";
			titleText.characterSize = 0.08f;
			titleText.color = Color.Lerp (selectedColors[0], selectedColors[1], Mathf.Sin(Time.time*30f));
			authorText.color = Color.Lerp (selectedColors[0], selectedColors[1], Mathf.Sin(Time.time*30f));
			backgroundImage.GetComponent<Renderer>().material.mainTexture = imgTexture;

			//titleText.color = new Color(1f,1f,1f);
			//authorText.color = new Color(1f,1f,1f);
		}	
		else
		{
			titleText.text = title;
			titleText.characterSize = 0.06f;
			titleText.color = unselectedItem;
			authorText.color = unselectedItem;
		}
	}
}
