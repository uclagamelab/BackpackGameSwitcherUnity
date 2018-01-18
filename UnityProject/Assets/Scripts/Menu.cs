using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

/*
 * This script instantiates menu items and copies data to them from GameData objects in FindGames.CS
 * It then handles selection, and calls openProcess t
 */
public class Menu : MonoBehaviour 
{
	public GameObject selectedMenuItem; //the currently selected menu item.
	public GameObject menuItem; 		//the prefab we'll instantiate menu items from.

	public float menuBoxStartOffset = 1f;
	public float menuBoxSpacing = 1.2f;

	ProcessRunner processRunner;
	GameCatalog findGames;
	ArrayList textFiles;	//references the data collected by findGames
	ArrayList menuItems;

	int index = 0;	//index of selected item in menuItems
	int mode = 0;	//showing list of all games or?
	bool isAxisInUse = false; //use this to treat joystick input as button press

	TextMesh selectGame;
	TextMesh descriptionText;


	void Start()
	{
		processRunner = gameObject.GetComponent<ProcessRunner>();
		findGames = gameObject.GetComponent<GameCatalog>();

		selectGame = GameObject.Find ("SelectGame").GetComponent<TextMesh>();
		descriptionText = GameObject.Find ("Description").GetComponent<TextMesh>();

		menuItems = new ArrayList();

		spawnDescriptionBoxes ();
	}

	void Update()
	{
		if(mode == 0)
		{
			selectMenuItem ();
		}else if (mode == 1)
		{
			confirmSelection();
		}
	}

	void confirmSelection()
	{
	}


	void selectMenuItem()
	{
		if( Input.GetAxis("Vertical") != 0)
		{
			if(isAxisInUse == false)
			{
				index -= (int) (Mathf.Sign ( Input.GetAxis("Vertical") ) * 1);

				if(index>menuItems.Count-1)
					index = 0;
				else if(index<0)
					index = menuItems.Count-1;


				selectedMenuItem.GetComponent<Descriptionbox>().selected = false;
				GameObject nextBox = (GameObject) menuItems[index];

				Descriptionbox nextScript = nextBox.GetComponent<Descriptionbox>();

				nextScript.selected = true;

				string newD = "";
				bool overLineLimit = false;
				for(int i = 0; i!= nextScript.description.Length; i++)
				{
					newD +=nextScript.description[i];
					if( i%30 == 0 && i!=0)
					{
						overLineLimit = true;
					}

					if(overLineLimit && newD[i]==' ')
					{
						newD+='\n';
						overLineLimit = false;
					}
				}
				descriptionText.text = newD;

				selectedMenuItem = nextBox;
				GetComponent<AudioSource>().pitch = 1 + Random.Range (-0.3f,0.3f);
				GetComponent<AudioSource>().Play ();

				isAxisInUse = true;
			}
		}else{
		
			if( Input.GetAxis("Vertical") == 0)
				isAxisInUse = false; 
		}

		if(Input.GetButtonDown ("Confirm"))
		{
			UnityEngine.Debug.Log ("PushedConfirm");
			GetComponent<AudioSource>().Play();
			string exe = selectedMenuItem.GetComponent<Descriptionbox>().executable;
			string directory = selectedMenuItem.GetComponent<Descriptionbox>().directory;
			bool isUnityApp = selectedMenuItem.GetComponent<Descriptionbox>().isUnityApp;
			UnityEngine.Debug.Log (directory);
			UnityEngine.Debug.Log (@directory);
			UnityEngine.Debug.Log (exe);


			//processRunner.OpenProcess(directory, exe, "-popupwindow -screen-width 1920 -screen-height 1080");
			if(isUnityApp)
			{
				processRunner.OpenProcess (@directory, exe, "-popupwindow -screen-width 1920 -screen-height 1080", "");
			}
			else
			{
				processRunner.OpenProcess (@directory, exe, "", "");
			}

			//open joy2Key with appropriate config file

		}
	}
	
	void spawnDescriptionBoxes()
	{
		int i = 0;
		foreach(GameData g in textFiles)
		{

			GameObject dBox =(GameObject) GameObject.Instantiate (menuItem);
			Descriptionbox dboxScript = dBox.GetComponent<Descriptionbox>();

			dboxScript.title = g.title;
			dboxScript.author = g.author;
			dboxScript.description = g.description;
			dboxScript.executable = g.executable;
			dboxScript.directory = g.directory;
			dboxScript.image = g.image;
			dboxScript.isUnityApp = g.isUnityApp;
			if(!dboxScript.isUnityApp)
				UnityEngine.Debug.Log("falseness detected spawning description boxes");



			menuItems.Add (dBox);
			dboxScript.transform.position = new Vector3(0, selectGame.gameObject.transform.position.y - menuBoxStartOffset - menuItems.Count * menuBoxSpacing,-7);

			if(i == 0){
				selectedMenuItem = dBox;
				dboxScript.selected = true;
				descriptionText.text = dboxScript.description;
			}
			else
				dboxScript.selected = false;
			i++;
		}
	}
}
