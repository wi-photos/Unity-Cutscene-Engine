using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;
using System.IO;

public class SubtitleEngine : MonoBehaviour
{
	[SerializeField] public Text Text;
	

	static TextAsset textFile = null;
    public float CutSceneEngineOn = 0;

    public float CutSceneTime = 0;
    public int CurrentCutSceneSeq = 0;
	public string[] CutsceneTimeLines;
	public string[] CutsceneTextLines;
	public string[] CutsceneVideoLines;
	public string[] CutsceneAudioLines;
	public string[] CutsceneFuncLines;
	
	VideoClip clip;
	private List<VideoPlayer> videoPlayerList;
    // Start is called before the first frame update
    void Start()
    {
		StartCutScene("subs","ON");
    }
    // Update is called once per frame
    void Update()
    {
		
        if (CutSceneEngineOn == 1)
        {
            CutSceneTime += Time.deltaTime;
        }
		
        if (CutSceneTime == float.Parse(CutsceneTimeLines[CurrentCutSceneSeq])*0.5f|| CutSceneTime > float.Parse(CutsceneTimeLines[CurrentCutSceneSeq])*0.5f) 
        {
			PreLoadVideoClip(CutsceneVideoLines[CurrentCutSceneSeq]);	
        }
		
        if (CutSceneTime == float.Parse(CutsceneTimeLines[CurrentCutSceneSeq])|| CutSceneTime > float.Parse(CutsceneTimeLines[CurrentCutSceneSeq])) 
        {
			Text.text = CutsceneTextLines[CurrentCutSceneSeq];
			//PreLoadVideoClip(CutsceneVideoLines[CurrentCutSceneSeq]);	
			PlayVideo();
			if (CutsceneAudioLines[CurrentCutSceneSeq] != "")
			{
	       	 	PlayAudioClip(CutsceneAudioLines[CurrentCutSceneSeq]);
			}

	       	RunFunction(CutsceneFuncLines[CurrentCutSceneSeq]);
				
			// this syncs the cut scene time, accounting for any errors
		    CutSceneTime = float.Parse(CutsceneTimeLines[CurrentCutSceneSeq]);
			// update sequence
			CurrentCutSceneSeq = CurrentCutSceneSeq +1;	
        }
    }
    void StartCutScene(string TextFile, string SceneID)
    {
		CutSceneEngineOn = 1;
		CutsceneTimeLines = LoadCSData(TextFile, SceneID, 2).ToString().Split('\n');
		CutsceneVideoLines = LoadCSData(TextFile, SceneID, 3).ToString().Split('\n');
		CutsceneTextLines = LoadCSData(TextFile, SceneID, 4).ToString().Split('\n');
		CutsceneAudioLines = LoadCSData(TextFile, SceneID, 5).ToString().Split('\n');
		CutsceneFuncLines = LoadCSData(TextFile, SceneID, 6).ToString().Split('\n');
		// must be called after initilizing variables.
		InitVideoEngine();
    }
    void PreLoadVideoClip(string VideoName)
    {
		videoPlayerList[CurrentCutSceneSeq].clip = Resources.Load<VideoClip>(VideoName) as VideoClip;
        videoPlayerList[CurrentCutSceneSeq].Prepare();	
    }
    void PlayAudioClip(string AudioName)
    {

      AudioSource audioSource = gameObject.AddComponent<AudioSource>();
      gameObject.GetComponent<AudioSource>().clip = Resources.Load(AudioName) as AudioClip;
      gameObject.GetComponent<AudioSource>().Play();	
    }
    public void RunFunction(string Function)
	{
		if (Function.StartsWith("PAUSE")) 
		{
			CutSceneEngineOn = 0;
		}
		if (Function.StartsWith("END")) 
		{
			EndCutScene();	
		}
	}
    void PlayVideo()
    {
		videoPlayerList[CurrentCutSceneSeq].Play();
		// destroy last video player. Delay destroy to create smooth transition
		Destroy(videoPlayerList[CurrentCutSceneSeq - 1],0.3f);	
    }
    void EndCutScene()
    {
	    for (var i = 0; i < CutsceneVideoLines.Length; i++) 
		{
	        Destroy(videoPlayerList[i]);
	    }
		Text.text = "";
		CutSceneEngineOn = 0;
		CutSceneTime = 0;
		CurrentCutSceneSeq= 0;
		Destroy(videoPlayerList[CurrentCutSceneSeq - 1]);
		Destroy(videoPlayerList[CurrentCutSceneSeq + 1]);
		Destroy(videoPlayerList[CurrentCutSceneSeq]);
    }
    void InitVideoEngine()
    {
        videoPlayerList = new List<VideoPlayer>();
        for (int i = 0; i < CutsceneVideoLines.Length; i++)
        {
	        GameObject camera = GameObject.Find("Main Camera");
            //Add VideoPlayer to the GameObject
            VideoPlayer videoPlayer = camera.AddComponent<VideoPlayer>();
	        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraFarPlane;
            videoPlayerList.Add(videoPlayer);
			videoPlayer.waitForFirstFrame = false;
            //Add AudioSource to the camera
            AudioSource audioSource = camera.AddComponent<AudioSource>();
            //Disable Play on Awake for both Video and Audio
            videoPlayer.playOnAwake = false;
            audioSource.playOnAwake = false;
            //We want to play from video clip not from url
            videoPlayer.source = VideoSource.VideoClip;
            //Set Audio Output to AudioSource
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            //Assign the Audio from Video to AudioSource to be played
			videoPlayer.EnableAudioTrack(0, true);
			videoPlayer.SetTargetAudioSource(0, audioSource);
        }
		
    }
    public string LoadCSData(string CutSceneFile, string CutSceneID, int IndexNum)
	{
		string csdata = "";
	  	string tempcsdata = "";
	  	try
	  	{
			#if UNITY_WEBGL
				var textFile = Resources.Load<TextAsset>(CutSceneFile);
				string[] lines = textFile.ToString().Split('\n');
			#elif UNITY_ANDROID
				var textFile = Resources.Load<TextAsset>(CutSceneFile);
				string[] lines = textFile.ToString().Split('\n');	
			#else
				string[] lines = File.ReadAllLines(Application.streamingAssetsPath + "/" + CutSceneFile + ".txt"); 
			#endif
			foreach (string line in lines)
			{
			    if (line.Contains("|"+ CutSceneID + "|"))
			    {
		  		    string[] col = line.Split('|');
	  			    tempcsdata = csdata;
					csdata = tempcsdata + "\n" + col[IndexNum];
			    }
			}		
	  	}
	  	catch
	  	{
			Debug.Log("Cutscene text file loading failed");
	  	}
		return csdata;
    }
}