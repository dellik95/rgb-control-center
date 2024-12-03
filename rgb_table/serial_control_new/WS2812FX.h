#include <Adafruit_NeoPixel.h>
#include <FastLED.h>
#define WS2812FX_h

#define FSH(x) (__FlashStringHelper*)(x)
#define MAX_MILLIS (0UL - 1UL) /* ULONG_MAX */

#define DEFAULT_BRIGHTNESS (uint8_t)50
#define DEFAULT_MODE (uint8_t)0
#define DEFAULT_SPEED (uint16_t)1000
#define DEFAULT_COLOR (uint32_t)0xFF0000
#define DEFAULT_COLORS \
  { RED, GREEN, BLUE }
#define COLORS(...) \
  (const uint32_t[]) { \
    __VA_ARGS__ \
  }

#define SPEED_MIN (uint16_t)10
#define SPEED_MAX (uint16_t)65535

#define BRIGHTNESS_MIN (uint8_t)0
#define BRIGHTNESS_MAX (uint8_t)255

/* each segment uses 36 bytes of SRAM memory, so if you're compile fails
  because of insufficient flash memory, decreasing MAX_NUM_SEGMENTS may help */
#define MAX_NUM_SEGMENTS 10
#define MAX_NUM_ACTIVE_SEGMENTS 10
#define INACTIVE_SEGMENT 255 /* max uint_8 */
#define MAX_NUM_COLORS 3     /* number of colors per segment */
#define MAX_CUSTOM_MODES 8

// some common colors
#define RED (uint32_t)0xFF0000
#define GREEN (uint32_t)0x00FF00
#define BLUE (uint32_t)0x0000FF
#define WHITE (uint32_t)0xFFFFFF
#define BLACK (uint32_t)0x000000
#define YELLOW (uint32_t)0xFFFF00
#define CYAN (uint32_t)0x00FFFF
#define MAGENTA (uint32_t)0xFF00FF
#define PURPLE (uint32_t)0x400080
#define ORANGE (uint32_t)0xFF3000
#define PINK (uint32_t)0xFF1493
#define GRAY (uint32_t)0x101010
#define ULTRAWHITE (uint32_t)0xFFFFFFFF
#define DIM(c) (uint32_t)((c >> 2) & 0x3f3f3f3f)   // color at 25% intensity
#define DARK(c) (uint32_t)((c >> 4) & 0x0f0f0f0f)  // color at  6% intensity

// segment options
// bit    7: reverse animation
// bits 4-6: fade rate (0-7)
// bit    3: gamma correction
// bits 1-2: size
// bits   0: TBD
#define NO_OPTIONS (uint8_t)0b00000000
#define REVERSE (uint8_t)0b10000000
#define IS_REVERSE ((_seg->options & REVERSE) == REVERSE)
#define FADE_XFAST (uint8_t)0b00010000
#define FADE_FAST (uint8_t)0b00100000
#define FADE_MEDIUM (uint8_t)0b00110000
#define FADE_SLOW (uint8_t)0b01000000
#define FADE_XSLOW (uint8_t)0b01010000
#define FADE_XXSLOW (uint8_t)0b01100000
#define FADE_GLACIAL (uint8_t)0b01110000
#define FADE_RATE ((_seg->options >> 4) & 7)
#define GAMMA (uint8_t)0b00001000
#define IS_GAMMA ((_seg->options & GAMMA) == GAMMA)
#define SIZE_SMALL (uint8_t)0b00000000
#define SIZE_MEDIUM (uint8_t)0b00000010
#define SIZE_LARGE (uint8_t)0b00000100
#define SIZE_XLARGE (uint8_t)0b00000110
#define SIZE_OPTION ((_seg->options >> 1) & 3)

// segment runtime options (aux_param2)
#define FRAME (uint8_t)0b10000000
#define SET_FRAME (_seg_rt->aux_param2 |= FRAME)
#define CLR_FRAME (_seg_rt->aux_param2 &= ~FRAME)
#define CYCLE (uint8_t)0b01000000
#define SET_CYCLE (_seg_rt->aux_param2 |= CYCLE)
#define CLR_CYCLE (_seg_rt->aux_param2 &= ~CYCLE)
#define CLR_FRAME_CYCLE (_seg_rt->aux_param2 &= ~(FRAME | CYCLE))

#define HUE_GAP 30 
#define MIN_BRIGHT 50   
#define MAX_BRIGHT 255  
#define MIN_SAT 180     
#define MAX_SAT 255   

class WS2812FX : public Adafruit_NeoPixel {

public:
  typedef uint16_t (WS2812FX::*mode_ptr)(void);

  // segment parameters
  typedef struct Segment {  // 20 bytes
    uint16_t start;
    uint16_t stop;
    uint16_t speed;
    uint8_t mode;
    uint8_t options;
    uint32_t colors[MAX_NUM_COLORS];
  } segment;

  // segment runtime parameters
  typedef struct Segment_runtime {  // 20 bytes for Arduino, 24 bytes for ESP
    unsigned long next_time;
    uint32_t counter_mode_step;
    uint32_t counter_mode_call;
    uint8_t aux_param;           // auxilary param (usually stores a color_wheel index)
    uint8_t aux_param2;          // auxilary param (usually stores bitwise options)
    uint16_t aux_param3;         // auxilary param (usually stores a segment index)
    uint8_t* extDataSrc = NULL;  // external data array
    uint16_t extDataCnt = 0;     // number of elements in the external data array
  } segment_runtime;

  WS2812FX(uint16_t num_leds, uint8_t pin, neoPixelType type,
           uint8_t max_num_segments = MAX_NUM_SEGMENTS,
           uint8_t max_num_active_segments = MAX_NUM_ACTIVE_SEGMENTS)
    : Adafruit_NeoPixel(num_leds, pin, type) {

    brightness = DEFAULT_BRIGHTNESS + 1;  // Adafruit_NeoPixel internally offsets brightness by 1
    _running = false;

    _segments_len = max_num_segments;
    _active_segments_len = max_num_active_segments;

    // create all the segment arrays and init to zeros
    _segments = new segment[_segments_len]();
    _active_segments = new uint8_t[_active_segments_len]();
    _segment_runtimes = new segment_runtime[_active_segments_len]();

    // init segment pointers
    _seg = _segments;
    _seg_rt = _segment_runtimes;

    resetSegments();
    setSegment(0, 0, num_leds - 1, DEFAULT_MODE, DEFAULT_COLOR, DEFAULT_SPEED, NO_OPTIONS);
  };

  void
    init(void),
    start(void),
    fade_out(void),
    fade_out(uint32_t),
    setMode(uint8_t m),
    setMode(uint8_t seg, uint8_t m),
    setSpeed(uint16_t s),
    setSpeed(uint8_t seg, uint16_t s),
    increaseSpeed(uint8_t s),
    decreaseSpeed(uint8_t s),
    setColor(uint8_t r, uint8_t g, uint8_t b, uint8_t w),
    setColor(uint32_t c),
    setColor(uint8_t seg, uint32_t c),
    setColors(uint8_t seg, uint32_t*c),
    fill(uint32_t c, uint16_t f, uint16_t cnt),
    setBrightness(uint8_t b),
    increaseBrightness(uint8_t s),
    decreaseBrightness(uint8_t s),
    setNumSegments(uint8_t n),
    setSegment(uint8_t n, uint16_t start, uint16_t stop, uint8_t mode, const uint32_t colors[], uint16_t speed, uint8_t options),
    addActiveSegment(uint8_t seg),

    resetSegments(void),
    resetSegmentRuntimes(void),
    resetSegmentRuntime(uint8_t),
    setPixelColor(uint16_t n, uint32_t c),
    setPixelColor(uint16_t n, uint8_t r, uint8_t g, uint8_t b),
    setPixelColor(uint16_t n, uint8_t r, uint8_t g, uint8_t b, uint8_t w),
    setRawPixelColor(uint16_t n, uint32_t c),
    copyPixels(uint16_t d, uint16_t s, uint16_t c),
    show(void),
    rebuildFirePalete(void);

  bool 
    service(void);

  CRGB getFireColor(int val);

  uint8_t
    brightness,
    random8(void),
    random8(uint8_t),
    getMode(void),
    getMode(uint8_t),
    getModeCount(void),
    getNumSegments(void),
    get_random_wheel_index(uint8_t),
    getNumBytesPerPixel(void);
  newGamma8(uint8_t);

  uint16_t
  random16(void),
    random16(uint16_t),
    getSpeed(void),
    getSpeed(uint8_t),
    getLength(void),
    getNumBytes(void);

  uint32_t
    color_wheel(uint8_t),
    getColor(void),
    getColor(uint8_t),
    getPixColor(CRGB thisPixel);

  uint32_t* getColors(uint8_t);
  uint8_t* getActiveSegments(void);
  uint8_t* blend(uint8_t*, uint8_t*, uint8_t*, uint16_t, uint8_t);

  const __FlashStringHelper* getModeName(uint8_t m);

  // mode helper functions
  uint16_t
    blink(uint32_t, uint32_t, bool strobe),
    color_wipe(uint32_t, uint32_t, bool),
    twinkle(uint32_t, uint32_t),
    twinkle_fade(uint32_t),
    sparkle(uint32_t, uint32_t),
    chase(uint32_t, uint32_t, uint32_t),
    chase_flash(uint32_t, uint32_t),
    running(uint32_t, uint32_t),
    fireworks(uint32_t),
    fire_flicker(int),
    tricolor_chase(uint32_t, uint32_t, uint32_t),
    scan(uint32_t, uint32_t, bool);

  uint32_t
    color_blend(uint32_t, uint32_t, uint8_t),
    getRawPixelColor(uint16_t n);

  // builtin modes
  uint16_t
    mode_static(void),
    mode_blink(void),
    mode_blink_rainbow(void),
    mode_strobe(void),
    mode_strobe_rainbow(void),
    mode_color_wipe(void),
    mode_color_wipe_inv(void),
    mode_color_wipe_rev(void),
    mode_color_wipe_rev_inv(void),
    mode_color_wipe_random(void),
    mode_color_sweep_random(void),
    mode_random_color(void),
    mode_single_dynamic(void),
    mode_multi_dynamic(void),
    mode_breath(void),
    mode_fade(void),
    mode_scan(void),
    mode_dual_scan(void),
    mode_theater_chase(void),
    mode_theater_chase_rainbow(void),
    mode_rainbow(void),
    mode_rainbow_cycle(void),
    mode_running_lights(void),
    mode_twinkle(void),
    mode_twinkle_random(void),
    mode_twinkle_fade(void),
    mode_twinkle_fade_random(void),
    mode_sparkle(void),
    mode_flash_sparkle(void),
    mode_hyper_sparkle(void),
    mode_multi_strobe(void),
    mode_chase_white(void),
    mode_chase_color(void),
    mode_chase_random(void),
    mode_chase_rainbow(void),
    mode_chase_flash(void),
    mode_chase_flash_random(void),
    mode_chase_rainbow_white(void),
    mode_chase_blackout(void),
    mode_chase_blackout_rainbow(void),
    mode_running_color(void),
    mode_running_red_blue(void),
    mode_running_random(void),
    mode_larson_scanner(void),
    mode_comet(void),
    mode_fireworks(void),
    mode_fireworks_random(void),
    mode_merry_christmas(void),
    mode_halloween(void),
    mode_fire_flicker(void),
    mode_fire_flicker_soft(void),
    mode_fire_flicker_intense(void),
    mode_circus_combustus(void),
    mode_bicolor_chase(void),
    mode_tricolor_chase(void),
    mode_twinkleFOX(void),
    mode_rain(void),
    mode_block_dissolve(void),
    mode_icu(void),
    mode_dual_larson(void),
    mode_running_random2(void),
    mode_filler_up(void),
    mode_rainbow_larson(void),
    mode_rainbow_fireworks(void),
    mode_trifade(void),
    mode_vu_meter(void),
    mode_heartbeat(void),
    mode_bits(void),
    mode_multi_comet(void),
    mode_flipbook(void),
    mode_popcorn(void),
    mode_oscillator(void),
    mode_noize_fire(void);

  int 
    counter,
    hue_start;

  CRGBPalette16 
    firePalette;

private:
  uint16_t _rand16seed;

  bool
    _running,
    _triggered;

  segment* _segments;                  // array of segments (20 bytes per element)
  segment_runtime* _segment_runtimes;  // array of segment runtimes (16 bytes per element)
  uint8_t* _active_segments;           // array of active segments (1 bytes per element)

  uint8_t _segments_len = 0;         // size of _segments array
  uint8_t _active_segments_len = 0;  // size of _segments_runtime and _active_segments arrays
  uint8_t _num_segments = 0;         // number of configured segments in the _segments array

  segment* _seg;             // currently active segment (20 bytes)
  segment_runtime* _seg_rt;  // currently active segment runtime (16 bytes)

  uint16_t _seg_len;  // num LEDs in the currently active segment

  void 
    RGBtoHSV(byte rgb[], double hsv[]);
};

// data struct used by the flipbook effect
struct Flipbook {
  int8_t numPages;
  int8_t numRows;
  int8_t numCols;
  uint32_t* colors;
};

// data struct used by the popcorn effect
struct Popcorn {
  float position;
  float velocity;
  int32_t color;
};

// data struct used by the oscillator effect
struct Oscillator {
  uint8_t size;
  int16_t pos;
  int8_t speed;
};

#include "modes_arduino.h"
