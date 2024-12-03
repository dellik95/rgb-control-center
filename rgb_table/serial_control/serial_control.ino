
#define REDUCED_MODES  // sketch too big for Arduino Leonardo flash, so invoke reduced modes
#include "WS2812FX.h"

#define LED_COUNT 67
#define LED_PIN 3
#define MAX_NUM_CHARS 16  // maximum number of characters read from the Serial Monitor

// Parameter 1 = number of pixels in strip
// Parameter 2 = Arduino pin number (most are valid)
// Parameter 3 = pixel type flags, add together as needed:
//   NEO_KHZ800  800 KHz bitstream (most NeoPixel products w/WS2812 LEDs)
//   NEO_KHZ400  400 KHz (classic 'v1' (not v2) FLORA pixels, WS2811 drivers)
//   NEO_GRB     Pixels are wired for GRB bitstream (most NeoPixel products)
//   NEO_RGB     Pixels are wired for RGB bitstream (v1 FLORA pixels, not v2)
//   NEO_RGBW    Pixels are wired for RGBW bitstream (NeoPixel RGBW products)
WS2812FX ws2812fx = WS2812FX(LED_COUNT, LED_PIN, NEO_RGB + NEO_KHZ800);

char scmd[MAX_NUM_CHARS];    // char[] to store incoming serial commands
bool scmd_complete = false;  // whether the command string is complete


void setup() {

  Serial.begin(57600);

  ws2812fx.init();
  ws2812fx.setBrightness(130);
  ws2812fx.setSpeed(3000);
  ws2812fx.setMode(FX_MODE_RAINBOW_CYCLE);
  ws2812fx.start();

  printUsage();
}

void loop() {
  ws2812fx.service();

  recvChar();  // read serial comm

  if (scmd_complete) {
    process_command();
  }
}

/*
 * Checks received command and calls corresponding functions.
 */
void process_command() {

  if (strcmp(scmd, "getModes") == 0) {
    printModes();
  }

  if (strcmp(scmd, "ping") == 0) {
    Serial.println("pong");
  }

  if (strcmp(scmd, "b+") == 0) {
    ws2812fx.increaseBrightness(25);
    Serial.print(F("Increased brightness by 25 to: "));
    Serial.println(ws2812fx.getBrightness());
  }

  if (strcmp(scmd, "b-") == 0) {
    ws2812fx.decreaseBrightness(25);
    Serial.print(F("Decreased brightness by 25 to: "));
    Serial.println(ws2812fx.getBrightness());
  }

  if (strncmp(scmd, "b ", 2) == 0) {
    uint8_t b = (uint8_t)atoi(scmd + 2);
    ws2812fx.setBrightness(b);
    Serial.print(F("Set brightness to: "));
    Serial.println(ws2812fx.getBrightness());
  }

  if (strcmp(scmd, "s+") == 0) {
    ws2812fx.setSpeed(ws2812fx.getSpeed() * 1.2);
    Serial.print(F("Increased speed by 20% to: "));
    Serial.println(ws2812fx.getSpeed());
  }

  if (strcmp(scmd, "s-") == 0) {
    ws2812fx.setSpeed(ws2812fx.getSpeed() * 0.8);
    Serial.print(F("Decreased speed by 20% to: "));
    Serial.println(ws2812fx.getSpeed());
  }

  if (strncmp(scmd, "s ", 2) == 0) {
    uint16_t s = (uint16_t)atoi(scmd + 2);
    ws2812fx.setSpeed(s);
    Serial.print(F("Set speed to: "));
    Serial.println(ws2812fx.getSpeed());
  }

  if (strncmp(scmd, "m ", 2) == 0) {
    uint8_t m = (uint8_t)atoi(scmd + 2);
    ws2812fx.setMode(m);
    Serial.print(F("Set mode to: "));
    Serial.print(ws2812fx.getMode());
    Serial.print(" - ");
    Serial.println(ws2812fx.getModeName(ws2812fx.getMode()));
  }

  if (strncmp(scmd, "c ", 2) == 0) {
    byte red, green, blue;
    unsigned long rgb = strtoul(scmd + 2, NULL, 16);

    red = rgb >> 16;
    green = (rgb & 0x00ff00) >> 8;
    blue = (rgb & 0x0000ff);

    ws2812fx.setColor(red, green, blue);
    Serial.print(F("Set color to: 0x"));
    Serial.println(ws2812fx.getColor(), HEX);
  }

  scmd[0] = '\0';         // reset the commandstring
  scmd_complete = false;  // reset command complete
}

const char usageText[] PROGMEM = R"=====(
Usage:
m <n> : select mode <n>

b+    : increase brightness
b-    : decrease brightness
b <n> : set brightness to <n>

s+    : increase speed
s-    : decrease speed
s <n> : set speed to <n>

p : ping (return 'pong' with current config)

c 0x007BFF : set color to 0x007BFF
)=====";

void printUsage() {
  Serial.println((const __FlashStringHelper *)usageText);
}


/*
 * Prints all available WS2812FX blinken modes.
 */
void printModes() {
  for (int i = 0; i < ws2812fx.getModeCount(); i++) {
    Serial.print(i);
    Serial.print('-');
    Serial.print(ws2812fx.getModeName(i));
    Serial.print("\n");
  }
  Serial.println();
}


/*
 * Reads new input from serial to scmd string. Command is completed on \n
 */
void recvChar(void) {
  static byte index = 0;
  while (Serial.available() > 0 && scmd_complete == false) {
    char rc = Serial.read();
    if (rc != '\n') {
      if (index < MAX_NUM_CHARS) scmd[index++] = rc;
    } else {
      scmd[index] = '\0';  // terminate the string
      index = 0;
      scmd_complete = true;
    }
  }
}
