=============================================================================
STORYSPACE

This app has eight story scenes. Each scene includes a background image and 
three or more draggable characters. You can make up stories!


app created by Jacqueline Kory (2013-2014)
all images created by Jacqueline Kory


=============================================================================
NOTES
Two different starting scenes are provided.
'init-choice-scene' allows the user to select any story scene to play.
'init-session-scene' takes the user to the stories for a particular SR2 or SR3 study session.

Currently everything is set up to use the session selector.

When switching back to using the choice scene at the start instead, one would
change several things:
-- the behavior of the 'back' arrow (tagged with TAG_ARROW; when touched, go to
   CheckTouch, which will leads to ReturnToBeginning, but desired behavior might
   be in ReturnToSessionSelection, or just mod ReturnToBeginning to go to the correct
   init scene)
-- which scene is in the build order (check init-choice, uncheck init-session)

=============================================================================
SCENE LIST

---start&end---
end-scene : shows 'continue' button that leads back to inital scene
init-choice-scene : shows an array of characters, each leading to a different story scene
init-session-scene : shows an array of buttons, each leading to a the set of stories
  for the selected SR2 or SR3 study session
init-start-scene : shows 'start' button so you can advance to the first story scene
mid-wait-scene : shows 'continue' button so you can continue to the next story scene

---storyscenes---
note that the story numbers below reflect the order in which the scenes were created,
not some other more important numbering schema, so they can be disregarded
-----------------
story01-penguins :
story02-aliens : usually referred to as the Mars scene
story03-dragon : usually referred to as the Treemeadow scene
story04-pineforest : 
story05-dinosaur : usually referred to as the Dinoville scene
story06-playground : 
story07-house : 
story08-castle : 

=============================================================================



=============================================================================

