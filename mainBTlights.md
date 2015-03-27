


# Introduction #
This project will be about a bluetooth control for a 16 channel EL wire dimmer. The hardware is realized with Netduino / Arduino and additional peripherie.

<a href='http://www.youtube.com/watch?feature=player_embedded&v=AkGEFzkgKrQ' target='_blank'><img src='http://img.youtube.com/vi/AkGEFzkgKrQ/0.jpg' width='425' height=344 /></a>

PROGRESS:
  * Almost all channel commands are implemented (documentation will follow, so far only in code comments and in text file)
  * Some general commands are done, the most critical I guess
  * Still some work to do for the buffer control (too many overflows in  the error log)
  * Android App has been started. Phew Bluetooth looks a bit different here. :/

# Documentation #
**[Serial Protocol](doumentationBTProtocol.md)**

# Requirements: #

## Open: ##
  * Android implementation
  * Portation from Netduino to Arduino


## Closed: ##
  * Library for using dimming hardware
  * Dimming hardware for 8 and 16 channels
  * protocol for BT interface on Netduino
  * Find BT firmware that can be used with Android Phones.
  * Test Firmware on Netduino to configure BT module and send commands