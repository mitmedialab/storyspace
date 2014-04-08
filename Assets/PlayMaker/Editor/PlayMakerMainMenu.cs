// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using System.ComponentModel;
using HutongGames.PlayMakerEditor;
using UnityEditor;
using UnityEngine;

[Localizable(false)]
static class PlayMakerMainMenu
{
	[MenuItem("PlayMaker/PlayMaker Editor", false, 1)]
	public static void OpenFsmEditor()
	{
		FsmEditorWindow.OpenWindow();
	}

	#region EDITOR WINDOWS 

    // priority starts at 10, leaving room for more items above

	[MenuItem("PlayMaker/Editor Windows/FSM Browser", true)]
	public static bool ValidateOpenFsmSelectorWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/FSM Browser", false, 10)]
	public static void OpenFsmSelectorWindow()
	{
		FsmEditor.OpenFsmSelectorWindow();
	}

	[MenuItem("PlayMaker/Editor Windows/State Browser", true)]
	public static bool ValidateOpenStateSelectorWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/State Browser", false, 11)]
	public static void OpenStateSelectorWindow()
	{
		FsmEditor.OpenStateSelectorWindow();
	}

	[MenuItem("PlayMaker/Editor Windows/Templates Browser", true)]
	public static bool ValidateOpenFsmTemplateWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/Templates Browser", false, 12)]
	public static void OpenFsmTemplateWindow()
	{
		FsmEditor.OpenFsmTemplateWindow();
	}

	[MenuItem("PlayMaker/Editor Windows/Edit Tool Window", true)]
	public static bool ValidateOpenToolWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/Edit Tool Window", false, 13)]
	public static void OpenToolWindow()
	{
		FsmEditor.OpenToolWindow();
	}

	[MenuItem("PlayMaker/Editor Windows/Action Browser", true)]
	public static bool ValidateOpenActionWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/Action Browser", false, 14)]
	public static void OpenActionWindow()
	{
		FsmEditor.OpenActionWindow();
	}

	[MenuItem("PlayMaker/Editor Windows/Global Variables", true)]
	public static bool ValidateOpenGlobalVariablesWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/Global Variables", false, 15)]
	public static void OpenGlobalVariablesWindow()
	{
		FsmEditor.OpenGlobalVariablesWindow();
	}

	[MenuItem("PlayMaker/Editor Windows/Event Browser", true)]
	public static bool ValidateOpenGlobalEventsWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/Event Browser", false, 16)]
	public static void OpenGlobalEventsWindow()
	{
		FsmEditor.OpenGlobalEventsWindow();
	}

	[MenuItem("PlayMaker/Editor Windows/Log Window", true)]
	public static bool ValidateOpenFsmLogWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/Log Window", false, 17)]
	public static void OpenFsmLogWindow()
	{
		FsmEditor.OpenFsmLogWindow();
	}

	[MenuItem("PlayMaker/Editor Windows/Editor Log", true)]
	public static bool ValidateOpenReportWindow()
	{
		return FsmEditorWindow.IsOpen();
	}

	[MenuItem("PlayMaker/Editor Windows/Editor Log", false, 18)]
	public static void OpenReportWindow()
	{
		FsmEditor.OpenReportWindow();
	}

	#endregion

	#region COMPONENTS

    // priority starts at 30, leaving room for more items above

	[MenuItem("PlayMaker/Components/Add FSM To Selected Objects", true)]
	public static bool ValidateAddFsmToSelected()
	{
		return Selection.activeGameObject != null;
	}

	[MenuItem("PlayMaker/Components/Add FSM To Selected Objects", false, 19)]
	public static void AddFsmToSelected()
	{
		FsmBuilder.AddFsmToSelected();
		//PlayMakerFSM playmakerFSM = Selection.activeGameObject.AddComponent<PlayMakerFSM>();
		//FsmEditor.SelectFsm(playmakerFSM.Fsm);
	}

	[MenuItem("PlayMaker/Components/Add PlayMakerGUI to Scene", true)]
	public static bool ValidateAddPlayMakerGUI()
	{
		return (Object.FindObjectOfType(typeof(PlayMakerGUI)) as PlayMakerGUI) == null;
	}

	[MenuItem("PlayMaker/Components/Add PlayMakerGUI to Scene", false, 20)]
	public static void AddPlayMakerGUI()
	{
		PlayMakerGUI.Instance.enabled = true;
	}

	#endregion

	#region TOOLS

	[MenuItem("PlayMaker/Tools/Load All PlayMaker Prefabs In Project", false, 25)]
	public static void LoadAllPrefabsInProject()
	{
		var paths = FsmEditorUtility.LoadAllPrefabsInProject();
		var output = "";

		foreach (var path in paths)
		{
			output += path + "\n";
		}

		if (output == "")
		{
			EditorUtility.DisplayDialog("Loading PlayMaker Prefabs", "No PlayMaker Prefabs Found!", "OK");
		}
		else
		{
			EditorUtility.DisplayDialog("Loaded PlayMaker Prefabs", output, "OK");
		}
	}

	[MenuItem("PlayMaker/Tools/Custom Action Wizard", false, 26)]
    public static void CreateWizard()
	{
		EditorWindow.GetWindow<PlayMakerCustomActionWizard>(true);
	}

	[MenuItem("PlayMaker/Tools/Export Globals", false, 27)]
    public static void ExportGlobals()
	{
		FsmEditorUtility.ExportGlobals();
	}

	
	[MenuItem("PlayMaker/Tools/Import Globals", false, 28)]
    public static void ImportGlobals()
	{
		FsmEditorUtility.ImportGlobals();
	}

	[MenuItem("PlayMaker/Tools/Documentation Helpers", false, 29)]
    public static void DocHelpers()
	{
		EditorWindow.GetWindow<PlayMakerDocHelpers>(true);
	}


	#endregion

	#region DOCUMENTATION

	[MenuItem("PlayMaker/Online Resources/HutongGames", false, 35)]
	public static void HutongGames()
	{
		Application.OpenURL("http://www.hutonggames.com/");
	}

    [MenuItem("PlayMaker/Online Resources/Online Manual", false, 36)]
	public static void OnlineManual()
	{
		EditorCommands.OpenWikiHelp();
		//Application.OpenURL("https://hutonggames.fogbugz.com/default.asp?W1");
	}

    [MenuItem("PlayMaker/Online Resources/Video Tutorials", false, 37)]
	public static void VideoTutorials()
	{
		Application.OpenURL("http://www.screencast.com/users/HutongGames/folders/PlayMaker");
	}

    [MenuItem("PlayMaker/Online Resources/YouTube Channel", false, 38)]
	public static void YouTubeChannel()
	{
		Application.OpenURL("http://www.youtube.com/user/HutongGamesLLC");
	}

    [MenuItem("PlayMaker/Online Resources/PlayMaker Forums", false, 39)]
	public static void PlayMakerForum()
	{
		Application.OpenURL("http://hutonggames.com/playmakerforum/");
	}

	//[MenuItem("PlayMaker/Documentation/")]
    [MenuItem("PlayMaker/Online Resources/Release Notes", false, 40)]
	public static void ReleaseNotes()
	{
		EditorCommands.OpenWikiPage(WikiPages.ReleaseNotes);
		//Application.OpenURL("https://hutonggames.fogbugz.com/default.asp?W311");
	}

	#endregion

    [MenuItem("PlayMaker/Tools/Submit Bug Report", false, 30)]
    public static void SubmitBug()
	{
		EditorWindow.GetWindow<PlayMakerBugReportWindow>(true);
	}

    [MenuItem("PlayMaker/Welcome Screen", false, 45)]
	public static void OpenWelcomeWindow()
	{
		EditorWindow.GetWindow<PlayMakerWelcomeWindow>(true);
	}

	//http://u3d.as/content/hutong-games-llc/playmaker/1Az

/*	[MenuItem("PlayMaker/Check For Updates")]
	public static void CheckForUpdates()
	{
		AssetStore.Open("1z");
	}*/

	[MenuItem("PlayMaker/About PlayMaker...", false, 46)]
	public static void OpenAboutWindow()
	{
		EditorWindow.GetWindow<AboutWindow>(true);
	}
}
