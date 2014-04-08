using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/**
 * record user actions to file, and play them back later
 * */
public class RecordAndPlayback
{
	private string filename = "record-"; // file to record to
	private StreamWriter writer = null;
	private string header = "Object\tAction\tPosition\tTime";
	// then reconstruct on playback
	private StreamReader reader = null;
	private Dictionary<string, List<PlayableLine>> playbacks = new Dictionary<string, List<PlayableLine>>();
		
	/**
	 * constructor
	 * */
	public RecordAndPlayback()
	{
	}
	
	/**
	 * setup recording file
	 * returns true if successful; false otherwise
	 * */
	public bool SetupRecordingFile()
	{
		return SetupRecordingFile("");
	}
	public bool SetupRecordingFile(String filename)
	{
		if (!filename.Equals(""))
			this.filename = filename;
		
		// check if we already have a recording file
		// if so, close it and create a new one with the new name
		if (this.writer != null)
		{
			this.CloseRecordingFile(); // try closing
		}
		
		// create recording file
		try
		{
			this.writer = new StreamWriter(Constants.LOG_FILE_PATH + 
				this.filename
				+ System.DateTime.Today.ToShortDateString().Replace("/", "-").Replace ("\\", "-")
				+ System.DateTime.Now.ToShortTimeString().Replace (":", "-") + ".txt", true);
			
			this.writer.WriteLine(this.header);
			return true;
		}
		
		catch (Exception e)
		{
			Debug.Log ("Exception: " + e.ToString());
			return false;
		}
	}
	
	/**
	 * record an action to file (e.g., dragging an object)
	 * */
	public void RecordAction(string obj, string tag, Vector3 posn, string time)
	{
		if (this.writer != null)
		{
			// write line
			this.writer.WriteLine (obj + "\t" + tag + "\t" +
				posn.ToString() + "\t" + time);		
			this.writer.Flush();
		}
	}
		
	/**
	 * close recording file
	 * */
	public void CloseRecordingFile()
	{
		try
		{
			this.writer.Flush();
			this.writer.Close();
		}
		catch (Exception e)
		{
			Debug.Log("Exception: " + e.ToString());
		}
	}
	
	/*
	 * load all playback files in the specified directory
	 * */
	public void LoadAllPlaybackFiles()
	{
		LoadAllPlaybackFiles(Constants.LOG_FILE_PATH + Constants.PLAYBACK_FILE_PATH);
	}
	public void LoadAllPlaybackFiles(string directory)
	{
		string[] files = Directory.GetFiles(directory);
		foreach (string infile in files)
		{
			try
            {
               // check file extensions for text files only
               if (Path.GetExtension(infile).Equals(".txt"))
               {
					Debug.Log (".....trying to load " + infile);
                  	// load file
					if(LoadPlaybackFile(infile)) 
						Debug.Log (".....loaded " + Path.GetFileNameWithoutExtension(infile));
               }
               else
               {
                  Debug.Log("Error with " + infile +
                      "\t" + "Not a valid text file.");
               }
            }
            catch (Exception ex)
            {
               Debug.Log("Error with " + infile +
                   "\t" + ex.Message);
            }
         }	
	}
	
	/**
	 * load a playback file for playing back
	 * returns true if successfully loaded
	 **/
	public bool LoadPlaybackFile(string file)
	{
		List<PlayableLine> lines = new List<PlayableLine>();

		try 
		{
			// create reader for file
			this.reader = new StreamReader(file);
			String line = ""; // to hold each line
			// while we have a line
			while((line = this.reader.ReadLine()) != null)
			{
				if (line.Contains("Object")) continue; // skip lines that have the header
				if (line.Contains ("Finger")) continue; // skip FingerDown/Up lines
				
				String[] tokens = line.Split('\t'); // split on tabs
				if (tokens.Length < 2) continue; // make sure we only process lines with stuff
				
				// obj tag posn time
				// tokens[2] is posn vector (1,1,1)
				string v = tokens[2];
				// vector string has parentheses; remove them before splitting into numbers
				string[] vs = v.Replace("(", "").Replace (")","").Split(',');
				// format the time string
				// throws formatexception if the format string doesn't exactly match
				// but sometimes milliseconds are one digit, two digits, or three
				// (e.g., 19:58:11.7 or 19:58:11.23 or 19:58:11.813)
				// so here we account for that by checking the length of the time string
				// and setting the number of ms digits accordingly
				string format = "HH:mm:ss.f" + ((tokens[3].Length == 10) ? "" : 
					(tokens[3].Length == 11 ? "f" : "ff"));
				
				DateTime t = new DateTime();
				try
				{
					t = DateTime.ParseExact(tokens[3], format, 
						System.Globalization.CultureInfo.InvariantCulture);
				}
				catch (System.FormatException fe)
				{
					Debug.Log ("ERROR! Line was " + line 
						+ "\nException: " + fe.ToString());
				}
				
				Vector3 vec = new Vector3(
					float.Parse(vs[0]), float.Parse(vs[1]), float.Parse(vs[2]));
				
				PlayableLine pl = new PlayableLine(tokens[0], tokens[1],
					vec, t);
				lines.Add(pl);				
			}
		}
		catch (Exception e)
		{
			Debug.Log ("Exception: " + e.ToString());
		}
		
		
		// okay now the text file is in a list of playback lines
		// go through and find the actions
		List<PlayableLine> actions = new List<PlayableLine>();
		DateTime start = DateTime.MinValue;
		DateTime prevActionEnd = DateTime.MinValue;
		List<Vector3> movePosns = new List<Vector3>();
		List<Vector3> lightPosns = new List<Vector3>();
		
		TimeSpan dif = new TimeSpan(0);
		float fdif = 0f;
		
		Debug.Log (".....converting to actions");
		// go through list of playback lines and condense into actions
		for (int i = 0; i < lines.Count; i++)
		{
			switch (lines[i].GetTag())
			{
				case "Tap":
					// add tap action
					// get difference between start time and end of last action
					// this is how long to wait before starting this action
					if ( prevActionEnd != DateTime.MinValue )
					{
						dif = lines[i].GetStartTime().Subtract(prevActionEnd);
						fdif = (float) dif.Seconds + (float) (dif.Milliseconds / 1000f);
					}
					actions.Add(new PlayableTapAction(lines[i], fdif));
					prevActionEnd = lines[i].GetStartTime(); // set action time
				break;
			
				case "DragBegin":
					start = lines[i].GetStartTime(); // save drag start time for later
					
				break;
				
				case "DragMove":
					movePosns.Add(lines[i].GetPosn()); // add position to move positions
					lightPosns.Add(new Vector3(lines[i].GetPosn().x, lines[i].GetPosn().y, -3)); 
					// and also need the highlight's move positions, which are at a different z
				break;
				
				case "DragEnd":
					// get difference between end time and start time
					// this is the total time the object moves (i.e., move time)
					// start time should never be the minvalue?? TODO
					if ( start != DateTime.MinValue ) // if we don't have a start time yet
					{
						dif = lines[i].GetStartTime().Subtract(start);
						fdif = (float) dif.Seconds + (float) (dif.Milliseconds / 1000f);
					}

					// get difference between start time and end of last action
					// this is how long to wait before starting this action
					float delay = 0f;
					if ( prevActionEnd != DateTime.MinValue )
					{
						TimeSpan delayDif = start.Subtract(prevActionEnd);
						delay = (float) delayDif.Seconds + (float) (delayDif.Milliseconds / 1000f);
					}
					// add move action with object name, tag, end position, and drag time
					actions.Add(new PlayableMoveAction(lines[i].GetObject(), "Drag",
						lines[i].GetPosn(), lines[i].GetStartTime(), fdif, delay, movePosns.ToArray(),
						lightPosns.ToArray()));
					movePosns.Clear(); // reset move positions list
					lightPosns.Clear(); // reset light list
					prevActionEnd = lines[i].GetStartTime(); // set action time
				break;
			}	// end switch
		} // end for
		
		// add this playback file name and its associated list of actions to master list
		this.playbacks.Add(Path.GetFileNameWithoutExtension(file), actions); 
		Debug.Log (".....done creating actions for " + file);
		return true;		
	}
	
	/**
	 * playback all lines in a playback file
	 * */
	public IEnumerator PlaybackFile(string command)
	{
		Debug.Log ("Attempting to load playback file... (" + command + ")");

		// play back all lines in playback file
		List<PlayableLine> actions = new List<PlayableLine>();
		if(this.playbacks.TryGetValue(command, out actions))
		{
			// play file!
			Debug.Log ("Got playback file! (" + command + ")");
			
			for (int i = 0; i < actions.Count; i++)
			{
				if (actions[i].GetTag().Equals("Drag"))
				{
					PlaybackNextMoveAction((PlayableMoveAction) actions[i]);
					yield return new WaitForSeconds(((PlayableMoveAction)actions[i]).GetDelayTime());
					// make light go away between drag actions?
					GameObject.FindGameObjectWithTag(Constants.TAG_LIGHT).transform.position = new Vector3(-1000, -1000,-2);
				}
				else
				{
					yield return new WaitForSeconds(((PlayableTapAction)actions[i]).GetDelayTime());
					PlaybackNextTapAction((PlayableTapAction) actions[i]);
				}
				
				yield return new WaitForSeconds(.1f); // wait a little bit before next action
			}
			// and turn off light at end of playback file
			GameObject.Find("storymainstuff").GetComponent("StoryspaceMainInteraction").SendMessage(
			"LightOff");
		}
	}
	
	/**
	 * playback the next move (drag) action
	 **/
	public void PlaybackNextMoveAction(PlayableMoveAction action)
	{
		Debug.Log ("PLAYBACK: " + action.GetObject() + " " + action.GetTag() 
			+ " waiting for " + action.GetDelayTime());
		
		// move object
		iTween.MoveTo(GameObject.Find(action.GetObject()), 
			iTween.Hash ("position", action.GetEndPosn(), "time", action.GetMoveTime(),
			//"easeType", "easeInSine",  // may not want ease type sine?
			"delay", action.GetDelayTime(), "path", action.GetMovePath()));	
		
		// turn light on		// get highlight & move
		GameObject.Find("storymainstuff").GetComponent("StoryspaceMainInteraction").SendMessage(
			"LightOn", new Vector3(-1000,-1000,-2));
	
		// then tell light to move like the object
		iTween.MoveTo (GameObject.FindGameObjectWithTag(Constants.TAG_LIGHT), 
			iTween.Hash ("position", new Vector3 (action.GetEndPosn().x, action.GetEndPosn().y, -2),
			"time", action.GetMoveTime(), "movetopath", false,
			//"easeType", "easeInSine",  // may not want ease type sine?
			"delay", action.GetDelayTime(), "path", action.GetLightMovePath()));
			// for the light: need separate path with z=-3, so there's a special list for the light path

	}
	
	/* playback the next tap action */
	public void PlaybackNextTapAction(PlayableTapAction action)
	{
		// TODO 
		Debug.Log("PLAYBACK: " + action.GetObject() + " " + action.GetTag() 
			+ " waiting for " + action.GetDelayTime());
		// get highlight & move
		//GameObject.Find("Director").GetComponent("StoryspaceMainInteraction").SendMessage(
		//	"LightOn", action.GetPosn());
	}
	
	// playback line
	public class PlayableLine 
	{
		protected string obj = "";
		protected string tag = "";
		protected Vector3 posn = new Vector3(0f,0f,0f);
		protected DateTime time;
		
		public PlayableLine()
		{
		}
		
		public PlayableLine(PlayableLine line)
		{
			this.obj = line.GetObject();
			this.tag = line.GetTag();
			this.posn = line.GetPosn();
			this.time = line.GetStartTime();
		}
		
		public PlayableLine(string obj, string tag, Vector3 posn, DateTime time)
		{
			this.obj = obj;
			this.tag = tag;
			this.posn = posn;
			this.time = time;
		}
		
		public string GetObject()
		{
			return this.obj;
		}
		public string GetTag()
		{
			return this.tag;
		}
		public Vector3 GetPosn()
		{
			return this.posn;
		}
		public DateTime GetStartTime()
		{
			return this.time;
		}
		public override string ToString()
		{
			return GetObject() + " " + GetTag() + " " + GetPosn() + " " + GetStartTime();
		}
	}
	
	// move action (i.e., drag)
	// moves the selected object across the screen from wherever it is to the end position
	// the info we need is:
	// which object this is, delay before start, time of movement, points to move through, end posn
	public class PlayableMoveAction : PlayableLine
	{
		private float moveTime = 0f;
		private float delayTime = 0f;
		private Vector3[] posns;
		private Vector3[] lightposns;
		
		public PlayableMoveAction(PlayableLine line, float moveTime, float delayTime, 
			Vector3[] posns, Vector3[] lightposns) : base (line)
		{
			this.moveTime = moveTime;
			this.delayTime = delayTime;
			this.posns = posns;
			this.lightposns = lightposns;
		}
		
		public PlayableMoveAction(string obj, string tag, Vector3 posn, 
			DateTime startTime, float moveTime, float delayTime, Vector3[] posns,
			Vector3[] lightposns)
		{
			this.obj = obj;
			this.tag = tag;
			this.posn = posn;
			this.time = startTime;
			this.moveTime = moveTime;
			this.delayTime = delayTime;
			this.posns = posns;
			this.lightposns = lightposns;
		}
		
		public string GetObjectName()
		{
			return this.obj;
		}
		public string GetActionType()
		{
			return this.tag;
		}
		public Vector3 GetEndPosn()
		{
			return this.posn;
		}
		public float GetMoveTime()
		{
			return this.moveTime;
		}		
		public float GetDelayTime()
		{
			return this.delayTime;
		}
		public Vector3[] GetMovePath()
		{
			return this.posns;
		}
		public override string ToString()
		{
			return base.ToString() + " " + GetMoveTime() + " " + GetDelayTime();
		}
		public Vector3[] GetLightMovePath()
		{
			return this.lightposns;
		}
	}
	
	// tap action
	// all this does is move the highlight behind the selected object
	// so the info we need is: 
	// which object, time action occurs, position to move highlight to
	public class PlayableTapAction : PlayableLine
	{
		private float delayTime = 0f;
				
		public PlayableTapAction(PlayableLine line, float delayTime) : base (line)
		{
			this.delayTime = delayTime;
		}
		
		public PlayableTapAction(string obj, string tag, Vector3 posn, DateTime time, float delayTime)
		{
			this.obj = obj;
			this.tag = tag;
			this.posn = posn;
			this.time = time;
		}
		
		public string GetObjectName()
		{
			return this.obj;
		}
		public string GetActionType()
		{
			return this.tag;
		}
		public float GetDelayTime()
		{
			return this.delayTime;
		}
		public override string ToString()
		{
			return base.ToString() + " " + GetDelayTime();
		}
	}
	
	
	
}

