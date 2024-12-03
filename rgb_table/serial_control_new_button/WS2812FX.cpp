#include "WS2812FX.h"

void WS2812FX::init() {
  resetSegmentRuntimes();
  Adafruit_NeoPixel::begin();
}

bool WS2812FX::service() {
  bool doShow = false;
  if (_running || _triggered) {
    unsigned long now = millis();
    for (uint8_t i = 0; i < _active_segments_len; i++) {
      if (_active_segments[i] != INACTIVE_SEGMENT) {
        _seg = &_segments[_active_segments[i]];
        _seg_len = (uint16_t)(_seg->stop - _seg->start + 1);
        _seg_rt = &_segment_runtimes[i];
        CLR_FRAME_CYCLE;
        if (now > _seg_rt->next_time || _triggered) {
          SET_FRAME;
          doShow = true;
          uint16_t delay = (MODE_PTR(_seg->mode))();
          _seg_rt->next_time = now + max(delay, SPEED_MIN);
          _seg_rt->counter_mode_call++;
        }
      }
    }
    if (doShow) {
      delay(1);  // for ESP32 (see https://forums.adafruit.com/viewtopic.php?f=47&t=117327)
      Adafruit_NeoPixel::show();
    }
    _triggered = false;
  }
  return doShow;
}

// overload setPixelColor() functions so we can use gamma correction
// (see https://learn.adafruit.com/led-tricks-gamma-correction/the-issue)
void WS2812FX::setPixelColor(uint16_t n, uint32_t c) {
  uint8_t w = (c >> 24) & 0xFF;
  uint8_t r = (c >> 16) & 0xFF;
  uint8_t g = (c >> 8) & 0xFF;
  uint8_t b = c & 0xFF;
  setPixelColor(n, r, g, b, w);
}

void WS2812FX::setPixelColor(uint16_t n, uint8_t r, uint8_t g, uint8_t b) {
  setPixelColor(n, r, g, b, 0);
}

void WS2812FX::setPixelColor(uint16_t n, uint8_t r, uint8_t g, uint8_t b, uint8_t w) {
  if (IS_GAMMA) {
    Adafruit_NeoPixel::setPixelColor(n, gamma8(r), gamma8(g), gamma8(b), gamma8(w));
  } else {
    Adafruit_NeoPixel::setPixelColor(n, r, g, b, w);
  }
}

void WS2812FX::setRawPixelColor(uint16_t n, uint32_t c) {
  if (n < numLEDs) {
    uint8_t* p = (wOffset == rOffset) ? &pixels[n * 3] : &pixels[n * 4];
    uint8_t w = (uint8_t)(c >> 24), r = (uint8_t)(c >> 16), g = (uint8_t)(c >> 8), b = (uint8_t)c;

    p[wOffset] = w;
    p[rOffset] = r;
    p[gOffset] = g;
    p[bOffset] = b;
  }
}

uint32_t WS2812FX::getRawPixelColor(uint16_t n) {
  if (n >= numLEDs) return 0;  // Out of bounds, return no color.

  if (wOffset == rOffset) {  // RGB
    uint8_t* p = &pixels[n * 3];
    return ((uint32_t)p[rOffset] << 16) | ((uint32_t)p[gOffset] << 8) | (uint32_t)p[bOffset];
  } else {  // RGBW
    uint8_t* p = &pixels[n * 4];
    return ((uint32_t)p[wOffset] << 24) | ((uint32_t)p[rOffset] << 16) | ((uint32_t)p[gOffset] << 8) | (uint32_t)p[bOffset];
  }
}

void WS2812FX::copyPixels(uint16_t dest, uint16_t src, uint16_t count) {
  uint8_t* pixels = getPixels();
  uint8_t bytesPerPixel = getNumBytesPerPixel();  // 3=RGB, 4=RGBW

  memmove(pixels + (dest * bytesPerPixel), pixels + (src * bytesPerPixel), count * bytesPerPixel);
}

void WS2812FX::start() {
  resetSegmentRuntimes();
  _running = true;
}

void WS2812FX::setMode(uint8_t m) {
  setMode(0, m);
}

void WS2812FX::setMode(uint8_t seg, uint8_t m) {
  resetSegmentRuntime(seg);
  _segments[seg].mode = constrain(m, 0, MODE_COUNT - 1);
}

void WS2812FX::setSpeed(uint16_t s) {
  setSpeed(0, s);
}

void WS2812FX::setSpeed(uint8_t seg, uint16_t s) {
  _segments[seg].speed = constrain(s, SPEED_MIN, SPEED_MAX);
}

void WS2812FX::increaseSpeed(uint8_t s) {
  uint16_t newSpeed = constrain(_seg->speed + s, SPEED_MIN, SPEED_MAX);
  setSpeed(newSpeed);
}

void WS2812FX::decreaseSpeed(uint8_t s) {
  uint16_t newSpeed = constrain(_seg->speed - s, SPEED_MIN, SPEED_MAX);
  setSpeed(newSpeed);
}

void WS2812FX::setColor(uint8_t r, uint8_t g, uint8_t b) {
  setColor(((uint32_t)r << 8) | ((uint32_t)g << 16) | b);
}

void WS2812FX::setColor(uint8_t r, uint8_t g, uint8_t b, uint8_t w) {
  setColor((((uint32_t)w << 24) | ((uint32_t)r << 16) | ((uint32_t)g << 8) | ((uint32_t)b)));
}

void WS2812FX::setColor(uint32_t c) {
  Serial.println(c);
  setColor(0, c);
}

void WS2812FX::setColor(uint8_t seg, uint32_t c) {
  _segments[seg].colors[0] = c;
}

void WS2812FX::setColors(uint8_t seg, uint32_t* c) {
  for (uint8_t i = 0; i < MAX_NUM_COLORS; i++) {
    _segments[seg].colors[i] = c[i];
  }
}

void WS2812FX::setBrightness(uint8_t b) {
  Adafruit_NeoPixel::setBrightness(b);
  Adafruit_NeoPixel::show();
}

void WS2812FX::increaseBrightness(uint8_t s) {
  //s = constrain(getBrightness() + s, BRIGHTNESS_MIN, BRIGHTNESS_MAX);
  setBrightness(getBrightness() + s);
}

void WS2812FX::decreaseBrightness(uint8_t s) {
  setBrightness(getBrightness() - s);
}

uint8_t WS2812FX::getMode(void) {
  return getMode(0);
}

uint8_t WS2812FX::getMode(uint8_t seg) {
  return _segments[seg].mode;
}

uint16_t WS2812FX::getSpeed(void) {
  return getSpeed(0);
}

uint16_t WS2812FX::getSpeed(uint8_t seg) {
  return _segments[seg].speed;
}

uint16_t WS2812FX::getLength(void) {
  return numPixels();
}

uint16_t WS2812FX::getNumBytes(void) {
  return numBytes;
}

uint8_t WS2812FX::getNumBytesPerPixel(void) {
  return (wOffset == rOffset) ? 3 : 4;  // 3=RGB, 4=RGBW
}

uint8_t WS2812FX::getModeCount(void) {
  return MODE_COUNT;
}

uint8_t WS2812FX::getNumSegments(void) {
  return _num_segments;
}

void WS2812FX::setNumSegments(uint8_t n) {
  _num_segments = n;
}

uint32_t WS2812FX::getColor(void) {
  return getColor(0);
}

uint32_t WS2812FX::getColor(uint8_t seg) {
  return _segments[seg].colors[0];
}

uint32_t* WS2812FX::getColors(uint8_t seg) {
  return _segments[seg].colors;
}

uint8_t* WS2812FX::getActiveSegments(void) {
  return _active_segments;
}

const __FlashStringHelper* WS2812FX::getModeName(uint8_t m) {
  if (m < MODE_COUNT) {
    return MODE_NAME(m);
  } else {
    return F("");
  }
}

void WS2812FX::setSegment(uint8_t n, uint16_t start, uint16_t stop, uint8_t mode, const uint32_t colors[], uint16_t speed, uint8_t options) {
  if (n < _segments_len) {
    if (n + 1 > _num_segments) _num_segments = n + 1;
    _segments[n].start = start;
    _segments[n].stop = stop;
    _segments[n].mode = mode;
    _segments[n].speed = speed;
    _segments[n].options = options;

    setColors(n, (uint32_t*)colors);

    if (n < _active_segments_len) addActiveSegment(n);
  }
}


void WS2812FX::addActiveSegment(uint8_t seg) {
  uint8_t* ptr = (uint8_t*)memchr(_active_segments, seg, _active_segments_len);
  if (ptr != NULL) return;  // segment already active
  for (uint8_t i = 0; i < _active_segments_len; i++) {
    if (_active_segments[i] == INACTIVE_SEGMENT) {
      _active_segments[i] = seg;
      resetSegmentRuntime(seg);
      break;
    }
  }
}

void WS2812FX::resetSegments() {
  resetSegmentRuntimes();
  memset(_segments, 0, _segments_len * sizeof(Segment));
  memset(_active_segments, INACTIVE_SEGMENT, _active_segments_len);
  _num_segments = 0;
}

void WS2812FX::resetSegmentRuntimes() {
  for (uint8_t i = 0; i < _segments_len; i++) {
    resetSegmentRuntime(i);
  };
}

void WS2812FX::resetSegmentRuntime(uint8_t seg) {
  uint8_t* ptr = (uint8_t*)memchr(_active_segments, seg, _active_segments_len);
  if (ptr == NULL) return;  // segment not active
  _segment_runtimes[seg].next_time = 0;
  _segment_runtimes[seg].counter_mode_step = 0;
  _segment_runtimes[seg].counter_mode_call = 0;
  _segment_runtimes[seg].aux_param = 0;
  _segment_runtimes[seg].aux_param2 = 0;
  _segment_runtimes[seg].aux_param3 = 0;
}

uint32_t WS2812FX::color_wheel(uint8_t pos) {
  pos = 255 - pos;
  if (pos < 85) {
    return ((uint32_t)(255 - pos * 3) << 16) | ((uint32_t)(0) << 8) | (pos * 3);
  } else if (pos < 170) {
    pos -= 85;
    return ((uint32_t)(0) << 16) | ((uint32_t)(pos * 3) << 8) | (255 - pos * 3);
  } else {
    pos -= 170;
    return ((uint32_t)(pos * 3) << 16) | ((uint32_t)(255 - pos * 3) << 8) | (0);
  }
}

uint8_t WS2812FX::get_random_wheel_index(uint8_t pos) {
  uint8_t r = 0;
  uint8_t x = 0;
  uint8_t y = 0;
  uint8_t d = 0;

  while (d < 42) {
    r = random8();
    x = abs(pos - r);
    y = 255 - x;
    d = min(x, y);
  }

  return r;
}

uint8_t WS2812FX::random8() {
  _rand16seed = (_rand16seed * 2053) + 13849;
  return (uint8_t)((_rand16seed + (_rand16seed >> 8)) & 0xFF);
}

uint8_t WS2812FX::random8(uint8_t lim) {
  uint8_t r = random8();
  r = ((uint16_t)r * lim) >> 8;
  return r;
}

uint16_t WS2812FX::random16() {
  return (uint16_t)random8() * 256 + random8();
}

uint16_t WS2812FX::random16(uint16_t lim) {
  uint16_t r = random16();
  r = ((uint32_t)r * lim) >> 16;
  return r;
}
