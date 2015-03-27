# Bluetooth Control for EL Wire #

UPDATE:
Replaced the BT module with anotehr (better) one: http://www.amazon.de/gp/product/B005AFPJLU/ref=oh_details_o05_s00_i00
It's faster, cheaper and easier to configure. Still have to improve the range (guess I have a grounding problem).

A protocol update will be released soon which simplifies the command structure.

App is progressing and wokring almost like a charm :)

This project uses SPP bluetooth module to connect to a netduino / arduino.
Actually this part is very generic and could be used wherever needed. It contains an Android app that supports a protocoll used by the netduino / arduino firmware (also available here)
The main purpose is to control a 10 channel dimmer for EL wires.

<a href='http://www.youtube.com/watch?feature=player_embedded&v=AkGEFzkgKrQ' target='_blank'><img src='http://img.youtube.com/vi/AkGEFzkgKrQ/0.jpg' width='425' height=344 /></a>

# Images of PCB: #

|![http://bt-lights.googlecode.com/svn/trunk/Documentation/images/IMG_20120724_200334.jpg](http://bt-lights.googlecode.com/svn/trunk/Documentation/images/IMG_20120724_200334.jpg)|![http://bt-lights.googlecode.com/svn/trunk/Documentation/images/IMG_20120724_200425.jpg](http://bt-lights.googlecode.com/svn/trunk/Documentation/images/IMG_20120724_200425.jpg)|
|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|![http://bt-lights.googlecode.com/svn/trunk/Documentation/images/IMG_20120724_200438.jpg](http://bt-lights.googlecode.com/svn/trunk/Documentation/images/IMG_20120724_200438.jpg)|![http://bt-lights.googlecode.com/svn/trunk/Documentation/images/IMG_20120724_200455.jpg](http://bt-lights.googlecode.com/svn/trunk/Documentation/images/IMG_20120724_200455.jpg)|

PROGRESS:

  * Almost all channel commands are implemented (documentation will follow, so far only in code comments and in text file)
  * Some general commands are done, the most critical I guess
  * Still some work to do for the buffer control (too many overflows in the error log)
  * Android App has been started. Phew Bluetooth looks a bit different here. :/


---

## Requirements ##
  * Bluetooth protocol for Netduino and/or Arduino communication
  * GUI for Android and PC

## Open: ##
  * Android implementation
  * Portation from Netduino to Arduino


## Closed: ##
  * Library for using dimming hardware
  * Dimming hardware for 8 and 16 channels
  * protocol for BT interface on Netduino
  * Find BT firmware that can be used with Android Phones.
  * Test Firmware on Netduino to configure BT module and send commands