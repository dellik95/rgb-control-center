#include "WS2812FX.h"
#include <EncButton.h>
#include <EEPROM.h>;

#define BTN_PIN 5
#define EB_STEP 100
#define LED_COUNT 67
#define INPUT_PIN 7
#define LED_PIN 3
#define MAX_NUM_CHARS 16

struct MyObject {
  byte brightness;
  uint16_t speed;
  uint8_t mode;
  byte rColor;
  byte gColor;
  byte bColor;
  bool enabled;
};

MyObject currentSettings = {
  200,
  3000,
  FX_NOIZE_FIRE,
  0,
  0,
  0,
  false
};
Button butt1(BTN_PIN);
WS2812FX ws2812fx = WS2812FX(LED_COUNT, LED_PIN, NEO_RGB + NEO_KHZ800);

char scmd[MAX_NUM_CHARS];
bool scmd_complete = false;
bool enabled = false;

void setup() {
  pinMode(INPUT_PIN, INPUT);
  Serial.begin(115200);
  EEPROM.get(0, currentSettings);
  ws2812fx.init();

  enabled = currentSettings.enabled;
  ws2812fx.setBrightness(currentSettings.brightness);
  ws2812fx.setSpeed(currentSettings.speed);
  ws2812fx.setMode(currentSettings.mode);
  ws2812fx.setColor(currentSettings.rColor, currentSettings.gColor, currentSettings.bColor, 0);
  ws2812fx.start();
  printUsage();
}

void loop() {
  butt1.tick();
  ws2812fx.service();

  if (butt1.click()) {
    enabled = !enabled;
    if (enabled) {
      ws2812fx.setBrightness(currentSettings.brightness);
    }
    currentSettings.enabled = boolean(enabled);
    EEPROM.put(0, currentSettings);
  }


  if (!enabled) {
    ws2812fx.setBrightness(0);
    return;
  }

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
    increaseBrightness();
  }

  if (strcmp(scmd, "b-") == 0) {
    ws2812fx.decreaseBrightness(25);
    Serial.print(F("Decreased brightness by 25 to: "));
    Serial.println(ws2812fx.getBrightness());
    currentSettings.brightness = ws2812fx.getBrightness();
    EEPROM.put(0, currentSettings);
  }

  if (strncmp(scmd, "b ", 2) == 0) {
    uint8_t b = (uint8_t)atoi(scmd + 2);
    currentSettings.brightness = b;
    EEPROM.put(0, currentSettings);
    ws2812fx.setBrightness(b);
    Serial.print(F("Set brightness to: "));
    Serial.println(ws2812fx.getBrightness());
  }

  if (strcmp(scmd, "s+") == 0) {
    ws2812fx.setSpeed(ws2812fx.getSpeed() * 1.2);
    Serial.print(F("Increased speed by 20% to: "));
    Serial.println(ws2812fx.getSpeed());
    currentSettings.speed = ws2812fx.getSpeed();
    EEPROM.put(0, currentSettings);
  }

  if (strcmp(scmd, "s-") == 0) {
    ws2812fx.setSpeed(ws2812fx.getSpeed() * 0.8);
    Serial.print(F("Decreased speed by 20% to: "));
    Serial.println(ws2812fx.getSpeed());
    currentSettings.speed = ws2812fx.getSpeed();
    EEPROM.put(0, currentSettings);
  }

  if (strncmp(scmd, "s ", 2) == 0) {
    uint16_t s = (uint16_t)atoi(scmd + 2);
    ws2812fx.setSpeed(s);
    Serial.print(F("Set speed to: "));
    Serial.println(ws2812fx.getSpeed());
    currentSettings.speed = s;
    EEPROM.put(0, currentSettings);
  }

  if (strncmp(scmd, "m ", 2) == 0) {
    uint8_t m = (uint8_t)atoi(scmd + 2);
    ws2812fx.setMode(m);
    Serial.print(F("Set mode to: "));
    Serial.print(ws2812fx.getMode());
    Serial.print(" - ");
    Serial.println(ws2812fx.getModeName(ws2812fx.getMode()));
    currentSettings.mode = m;
    EEPROM.put(0, currentSettings);
  }

  if (strncmp(scmd, "c ", 2) == 0) {
    byte red, green, blue;
    unsigned long rgb = strtoul(scmd + 2, NULL, 16);

    red = rgb >> 16;
    green = (rgb & 0x00ff00) >> 8;
    blue = (rgb & 0x0000ff);

    currentSettings.rColor = red;
    currentSettings.gColor = green;
    currentSettings.bColor = blue;
    EEPROM.put(0, currentSettings);

    ws2812fx.setColor(red, green, blue, 0);
    Serial.print(F("Set color to: 0x"));
    Serial.println(ws2812fx.getColor(), HEX);
  }

  scmd[0] = '\0';         // reset the commandstring
  scmd_complete = false;  // reset command complete
}

void increaseBrightness() {
  ws2812fx.increaseBrightness(25);
  Serial.print(F("Increased brightness by 25 to: "));
  Serial.println(ws2812fx.getBrightness());
  currentSettings.brightness = ws2812fx.getBrightness();
  EEPROM.put(0, currentSettings);
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

void swithc() {
}
