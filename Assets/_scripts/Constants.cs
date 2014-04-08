using UnityEngine;

/**
 * various constants
 * */
public static class Constants
{
	// tags -- used to find particular game objects
	public const string TAG_STORY_CHAR = "StoryCharacter";
	public const string TAG_BACKGROUND_IMAGE = "BackgroundImage";
	public const string TAG_GAME_DIRECTOR = "GameDirector";
	public const string TAG_LIGHT = "Light";
	public const string TAG_NEXT = "Next";	
	public const string TAG_SNOWFLAKE = "Snowflake";
	public const string TAG_CLOUD = "Cloud";
	public const string TAG_STORM_TRIGGER = "StormTrigger";
	public const string TAG_ARROW = "Reset";
	public const string TAG_RECORDER = "RecordTrigger";
	public const string TAG_SESSION = "Session";
	public const string TAG_SR3 = "SR3_Session";
	public const string TAG_PERSIST = "PERSIST";
	
	// scenes (by index -- see list of scenes in build settings)
	public const int SCENE_INIT = 0;
	public const int SCENE_ICEBERG = 1;
	public const int SCENE_TREEMEADOW = 2;
	public const int SCENE_MARS = 3;
	public const int SCENE_PINEFOREST = 4;
	public const int SCENE_DINOSAUR = 5;
	public const int SCENE_PLAYGROUND = 6;
	public const int SCENE_HOUSE = 7;
	public const int SCENE_CASTLE = 8;
	public const int SCENE_START = 9;
	public const int SCENE_WAIT = 10;
	
	// scenes (by name)
	public const string NAME_ICEBERG = "Penguin";
	public const string NAME_TREEMEADOW = "Dragon";
	public const string NAME_MARS = "Alien";
	public const string NAME_PINEFOREST = "Squirrel";
	public const string NAME_DINOSAUR = "Dinosaur";
	public const string NAME_PLAYGROUND = "Playground";
	public const string NAME_HOUSE = "House";
	public const string NAME_CASTLE = "Castle";
	
	// edges of screen - used to make sure objects aren't dragged off the screen
	public const int LEFT_SIDE = -640;
	public const int RIGHT_SIDE = 640;
	public const int TOP_SIDE = 390;
	public const int BOTTOM_SIDE = -390;
	
	// messages from remote operator
	public const string SCENE_GO_BACK = "BACK";
	public const string SCENE_ADVANCE = "FORWARD";
	public const string PLAYBACK = "PLAYBACK";
	public const string DISABLE_TOUCH = "DISABLE";
	public const string ENABLE_TOUCH = "ENABLE";
	public const string PID = "PID";
	public const string RELOAD_LEVEL = "RELOAD";
	public const string SWITCH_TURN = "SWITCH";
	public const string ROBOT_TURN = "ROBOT";
	public const string CHILD_TURN = "CHILD";
	public const string RETURN_TO_BEG = "RESET";
	
	// order of stories for each session (8 sessions) - if 3 stories per session!
	/*public static int[][] SESSION_STORY_ORDERS = new int[][] {
		new int[] { SCENE_ICEBERG, SCENE_ICEBERG, SCENE_PLAYGROUND },
		new int[] { SCENE_PINEFOREST, SCENE_CASTLE, SCENE_MARS },
		new int[] { SCENE_HOUSE, SCENE_DRAGON, SCENE_DINOSAUR },
		new int[] { SCENE_MARS, SCENE_PINEFOREST, SCENE_CASTLE },
		new int[] { SCENE_DINOSAUR, SCENE_HOUSE, SCENE_DRAGON },
		new int[] { SCENE_CASTLE, SCENE_MARS, SCENE_PINEFOREST },
		new int[] { SCENE_DRAGON, SCENE_DINOSAUR, SCENE_HOUSE },
		new int[] { SCENE_ICEBERG, SCENE_ICEBERG, SCENE_PLAYGROUND }
	};*/
	
		// order of stories for each session (8 sessions) - if 2 stories per session!
	public static int[][] SESSION_STORY_ORDERS = new int[][] {
		new int[] { SCENE_ICEBERG, SCENE_PLAYGROUND },
		new int[] { SCENE_PINEFOREST, SCENE_CASTLE },
		new int[] { SCENE_HOUSE, SCENE_MARS },
		new int[] { SCENE_DINOSAUR, SCENE_TREEMEADOW },
		new int[] { SCENE_MARS, SCENE_PINEFOREST },
		new int[] { SCENE_CASTLE, SCENE_DINOSAUR },
		new int[] { SCENE_TREEMEADOW, SCENE_HOUSE },
		new int[] { SCENE_ICEBERG, SCENE_PLAYGROUND }
	};
	
	public static int[][] SR3_STORY_ORDERS = new int[][] {
		new int[] { SCENE_ICEBERG, SCENE_DINOSAUR },
		new int[] { SCENE_DINOSAUR, SCENE_ICEBERG }
	};
	
	// current session tracks which play session this is, because different
	// stories are told each session
	public static int currentSession = 0;
	// current scene we're on within the session
	public static int currentScene = -1; 
	public static bool SR3 = false; // TRUE if using for long-term story study (SR2), FALSE if for SR3
	
	// IP address for TCP (communicates with teleop)
	//TODO add text config file so we don't have to recompile just to change the IP address...
	
	public const string TCP_IP = "192.168.0.254"; // IP address of teleop laptop (static)
	//public const string TCP_IP = "127.0.0.1"; // loopback (if running in unity)
	
	// file paths
//	public const string LOG_FILE_PATH = @"Logs/"; // if playing in unity
	public const string LOG_FILE_PATH = "mnt/sdcard/Logs/"; // if playing on tablet
	public const string PLAYBACK_FILE_PATH = "playbacks";
	
}
