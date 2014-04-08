using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

// received message event -- fire when we get a message
// so others can listen for the messages
public delegate void ReceivedMessageEventHandler(object sender, String msg);

/**
 * tcp client
 * for communication with the teleop / remote controller of app
 * */
public class TCPClient
{
	private const int READ_BUFFER_SIZE = 255; // may not need to be this big
	private const int PORT_NUM = 8080;
	private TcpClient clientSocket;
	private byte[] readBuffer = new byte[READ_BUFFER_SIZE];
	public event ReceivedMessageEventHandler receivedMsgEvent;
	
	/**
	 * constructor
	 * */
	public TCPClient()
	{
	}
	
	
	/** 
	 * run
	 * */
	public void run(string IP)
	{
		try
		{
			// let's try connecting
			// create client and try connecting
			this.clientSocket = new TcpClient(IP, PORT_NUM);
			
			// async read
			this.clientSocket.GetStream().BeginRead(this.readBuffer, 0, READ_BUFFER_SIZE, 
				new AsyncCallback(ReadFromServer), null);
			
			Debug.Log("Client socket connected!");			
		}
		catch (Exception e)
		{
			Debug.Log("ERROR: Could not connect " + e.ToString());
			return;
		}
		
		this.SendToServer("MSG 005 TEST ");
		
	}
	
	/**
	 * read message over tcp from server
	 * */
	private void ReadFromServer(IAsyncResult ar)
	{
		int bytesRead = 0;
		int msgLength = -1;
		try
		{
			// finish async read into readBuffer, return number of bytes read
			bytesRead = this.clientSocket.GetStream().EndRead(ar);
			
			// if we got bytes, great!
			if (bytesRead > 0)
			{
				// now do something with the bytes
				// convert only the number of bytes read into a string for processing
				String msg = Encoding.ASCII.GetString(this.readBuffer, 0, bytesRead);
				Debug.Log ("Got message: " + msg);

				// check length of string:
				// if length > 7 (i.e., length of "MSG ###")
                // then we could have the the start, length, and some message
				if (msg.Length > 7)
				{
					// see if the MSG start string is present anywhere
					MatchCollection matches = Regex.Matches(msg, "MSG");
					
					// while we have MSG string found (loop to make sure we process ALL
					// possible messages within the bytes received)
					foreach (Match match in matches)
					{
						// we found the MSG start somewhere - at the index matcher.start()
                    	// now we want to see if we have a length, so check to see if 
                    	// the string starting from MSG is more than 7 characters (i.e.,
                    	// that we have at least MSG_LEN in our message)						
						// if so, process further
						// if (substring from start of MSG to end of packet > 7)
						if (msg.Substring(match.Index, msg.Length - match.Index).Length > 7)
						{
							// the length field is the 4th-6th characters (3 characters)
                        	// format is:  MSG_LEN_actualmessage
							// String len = substring that is LEN;
							String len = msg.Substring(match.Index + 4, 3);
							
							// parse length field
							if (!Int32.TryParse(len, out msgLength))
							{
								// could not parse length field
								Debug.Log ("ERROR! Could not parse message length field");
							}
							// otherwise, we got the length
							//
							// then check length of string after length (i.e., length of
                        	// the actual command plus arguments part of the message)
                        	// is it equal or greater than length?
							//
							// if so, process further (from start of actual message to length)
							if (msgLength <= msg.Substring(match.Index + 7, msgLength).Length)
							{
								// if so, we have a message! process it appropriately
								// send receivedMsgEvent here, and let other function do the 
								// tokenizing and dealing with arguments
								//
								// fire event indicating that we received a full message
								if (this.receivedMsgEvent != null)
								{
									// only send subset of msg that is actual message
									//Debug.Log("Got message! " + msg.Substring(match.Index + 7, msgLength));
									this.receivedMsgEvent(this, msg.Substring(match.Index + 7, msgLength));
								}
								else
								{
									Debug.Log ("Error: received msg event was null");
								}
							} // end have packet with bytes >= LEN
						} // end if have MSG of more than 7 chars
						
						 // clear buffer after processing everything currently in it
	                     // because all the positions used above are relative to the
	                     // substring starting at match.Index, which is the index
	                     // of the most recently found match in the string
	                     //
	                     // though this does assume that we haven't received
	                     // anything in the buffer in the meantime - which is true,
	                     // because we read in bytes, process them, then read in more
	                     //
	                     // also, we want to delete all but the last token...
	                     //
	                     // so do we delete from 0 -> matcher.start() + 8 + LEN + 1
	                     // to be at the end of the last message processed?
	                     // because matcher.start() is start of MSG
	                     // the initial string, MSG_LEN_ is 8 characters
	                     // msgLength is the size of the message, plus a space (delimiter)
	                     //
	                     // set position in the buffer to the end of the previous data
						
						 // clear from 0 to the end of last message processed
	                     Array.Clear (this.readBuffer, 0, match.Index + 7 + msgLength);                     
						
	                     // if there is another MSG in the buffer, we'll loop
	                     // around to check and process it
					} // end while MSG
										
					// we had more than 7 characters, but no MSG string
                    // so that means we can discard all of the characters we have
                    // because there's no message anywhere here
					if (this.readBuffer.Length > 100)
					{
                    	Array.Clear(this.readBuffer, 0, this.readBuffer.Length-1); // empties everything from buffer
					}
				} // end if have at least 7 characters
				// wait to get at least 7 characters to process
				
				// TODO send replies when?
				//this.SendToServer("MSG 009 Received "); // testing
				
			}
			
			// otherwise, didn't get anything
			//start new async read into readBuffer to get next stuff
			this.clientSocket.GetStream().BeginRead(this.readBuffer, 0, 
				READ_BUFFER_SIZE, new AsyncCallback(ReadFromServer), null);
			
		}
		catch (Exception e)
		{
			Debug.Log("ERROR: failed read " + e.ToString());
			return;
		}
		
	}
	
	
	/**
	 * public send message function
	 * */
	public bool SendMessage(String msg)
	{
		if (this.clientSocket.Connected)
		{
			return this.SendToServer(msg);
		}
		else
		{
			return false;
		}
	}
	
	/**
	 * send string message to server
	 * */
	private bool SendToServer(String msg)
	{
		// build message:
		// MSG ### ACTUALCOMMAND ARGS \n
		//   MSG = tag for start of all messages
		//   ### = length of message (counting only actual command and args)
		//   ACTUALCOMMAND = command to send, such as 'BACK' or 'START' or 'PLAY'
		//   ARGS = arguments, such as name of file to play
    
		// TODO FORMAT LENGTH
		// build up message, using space as delimiter
		//String fullMsg = "MSG " + String.Format (TODO FORMAT, msg.Length) + " " + msg;
		String fullMsg = "MSG 005 TEST ";
		
		// try sending to server
		try
		{
			StreamWriter socketWriter = new StreamWriter(this.clientSocket.GetStream());
			socketWriter.WriteLine(fullMsg.ToCharArray()); // write to socket
			socketWriter.Flush();
			return true; // success!
		}
		catch (Exception e)
		{
			Debug.Log("ERROR: failed to send " + e.ToString());
			return false; // fail :(
		}
	}
	
	
	/**
	 * destructor
	 * closes socket properly
	 * */
	~TCPClient()
	{
		try
		{
		 	// close socket
			if (this.clientSocket != null)
				this.clientSocket.Close();
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
	}
	
	
}
