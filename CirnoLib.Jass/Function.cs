using CirnoLib;

using System;
using System.IO;
using System.Collections;

namespace CirnoLib.Jass
{
    public static class Function
    {
        private static Preload _Preload;

        #region [    Common.J    ]

        #region [    Constants    ]
        // pfff
        public const bool FALSE = false;
        public const bool TRUE = true;
        public const int JASS_MAX_ARRAY_SIZE = 8192;

        public const int PLAYER_NEUTRAL_PASSIVE = 15;
        public const int PLAYER_NEUTRAL_AGGRESSIVE = 12;

        //===================================================
        // Camera Margin constants for use with GetCameraMargin
        //===================================================
        public const int CAMERA_MARGIN_LEFT = 0;
        public const int CAMERA_MARGIN_RIGHT = 1;
        public const int CAMERA_MARGIN_TOP = 2;
        public const int CAMERA_MARGIN_BOTTOM = 3;
        #endregion

        #region [    Math API    ]
        public static float Deg2Rad(float degrees) => (float)Math.PI * degrees / 180.0f;
        public static float Rad2Deg(float radians) => radians * (180.0f / (float)Math.PI);

        public static float Sin(float radians) => (float)Math.Sin(radians);
        public static float Cos(float radians) => (float)Math.Cos(radians);
        public static float Tan(float radians) => (float)Math.Tan(radians);

        // Expect values between -1 and 1...returns 0 for invalid input
        public static float Asin(float y) => (float)Math.Asin(y);
        public static float Acos(float x) => (float)Math.Acos(x);
        public static float Atan(float x) => (float)Math.Atan(x);

        // Returns 0 if x and y are both 0
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);

        // Returns 0 if x <= 0
        public static float SquareRoot(float x) => (float)Math.Sqrt(x);

        // computes x to the y power
        // y == 0.0             => 1
        // x == 0.0 and y < 0   => 0
        public static float Pow(float x, float power) => (float)Math.Pow(x, power);
        #endregion

        #region [    String Utility API    ]
        public static float I2R(int i) => Convert.ToSingle(i);
        public static int R2I(float r)
        {
            if (r > int.MaxValue) return int.MaxValue;
            if (r < int.MinValue) return int.MinValue;
            //if (r % 1 == 0) return (int)(r - 1); //rounding bug
            return (int)r;
        }
        public static string I2S(int i) => i.ToString();
        public static string R2S(float r) => r.ToString();
        //public static string R2SW(float r, int width, int precision)
        
        public static int S2I(string s) => int.TryParse(s, out int result) ? result : 0;
        public static float S2R(string s) => float.TryParse(s, out float result) ? result : 0;
        public static string SubString(string source, int start, int end)
        {
            if (start < 0) start = 0;
            if (StringLength(source) < start || start > end) return string.Empty;
            if (StringLength(source) <= end) return source.GetBytes().SubArray(start).GetString();
            return source.GetBytes().SubArray(start, end - start).GetString();
        }
        public static int StringLength(string s) => s.GetByteCount();
        public static string StringCase(string source, bool upper) => upper ? source.ToUpper() : source.ToLower();
        public static int StringHash(string s) => s.StrHash();
        #endregion

        #region [    Region and Location API    ]
        public static rect Rect(float minx, float miny, float maxx, float maxy)
            => new rect { minx = minx, miny = miny, maxx = maxx, maxy = maxy };
        public static rect RectFromLoc(location min, location max)
            => Rect(min.x, min.y, max.x, max.y);
        public static void RemoveRect(rect whichRect) 
            => whichRect.minx = whichRect.miny = whichRect.maxx = whichRect.maxy = 0;
        public static void SetRect(rect whichRect, float minx, float miny, float maxx, float maxy)
        {
            whichRect.minx = minx;
            whichRect.miny = miny;
            whichRect.maxx = maxx;
            whichRect.maxy = maxy;
        }
        public static void SetRectFromLoc(rect whichRect, location min, location max)
            => SetRect(whichRect, min.x, min.y, max.x, max.y);
        public static void MoveRectTo(rect whichRect, float newCenterX, float newCenterY)
        {
            float halfWidth = (whichRect.maxx - whichRect.minx) / 2;
            float halfHeight = (whichRect.maxy - whichRect.miny) / 2;
            whichRect.minx = newCenterX - halfWidth;
            whichRect.maxx = newCenterX + halfWidth;
            whichRect.miny = newCenterY - halfHeight;
            whichRect.maxy = newCenterY + halfHeight;
        }
        public static void MoveRectToLoc(rect whichRect, location newCenter)
            => MoveRectTo(whichRect, newCenter.x, newCenter.y);

        public static float GetRectCenterX(rect whichRect) 
            => (whichRect.maxx - whichRect.minx) / 2 + whichRect.minx;
        public static float GetRectCenterY(rect whichRect)
            => (whichRect.maxy - whichRect.miny) / 2 + whichRect.miny;
        public static float GetRectMinX(rect whichRect) => whichRect.minx;
        public static float GetRectMinY(rect whichRect) => whichRect.miny;
        public static float GetRectMaxX(rect whichRect) => whichRect.maxx;
        public static float GetRectMaxY(rect whichRect) => whichRect.maxy;

        public static location Location(float x, float y) => new location { x = x, y = y };
        public static void RemoveLocation(location whichLocation)
            => whichLocation.x = whichLocation.y = 0;
        public static void MoveLocation(location whichLocation, float newX, float newY)
        {
            whichLocation.x = newX;
            whichLocation.y = newY;
        }
        public static float GetLocationX(location whichLocation) => whichLocation.x;
        public static float GetLocationY(location whichLocation) => whichLocation.y;
        #endregion

        #region [    Dialog API    ]
        public static Hashtable InitHashtable() => new Hashtable();

        public static void SaveInteger(Hashtable table, int parentKey, int childKey, int value)
        {
            if (!table.ContainsKey(parentKey)) table.Add(parentKey, new Hashtable());
            if (!(table[parentKey] is Hashtable parent)) return;
            if (parent.ContainsKey(childKey)) parent.Remove(childKey);
            parent.Add(childKey, value);
        }
        public static void SaveReal(Hashtable table, int parentKey, int childKey, float value)
        {
            if (!table.ContainsKey(parentKey)) table.Add(parentKey, new Hashtable());
            if (!(table[parentKey] is Hashtable parent)) return;
            if (parent.ContainsKey(childKey)) parent.Remove(childKey);
            parent.Add(childKey, value);
        }
        public static void SaveBoolean(Hashtable table, int parentKey, int childKey, bool value)
        {
            if (!table.ContainsKey(parentKey)) table.Add(parentKey, new Hashtable());
            if (!(table[parentKey] is Hashtable parent)) return;
            if (parent.ContainsKey(childKey)) parent.Remove(childKey);
            parent.Add(childKey, value);
        }
        public static void SaveStr(Hashtable table, int parentKey, int childKey, string value)
        {
            if (!table.ContainsKey(parentKey)) table.Add(parentKey, new Hashtable());
            if (!(table[parentKey] is Hashtable parent)) return;
            if (parent.ContainsKey(childKey)) parent.Remove(childKey);
            parent.Add(childKey, value);
        }

        public static int LoadInteger(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(int)) return 0;
            return (int)parent[childKey];
        }
        public static float LoadReal(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(float)) return 0;
            return (float)parent[childKey];
        }
        public static bool LoadBoolean(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(bool)) return false;
            return (bool)parent[childKey];
        }
        public static string LoadStr(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)) return null;
            return parent[childKey] as string;
        }

        public static bool HaveSavedInteger(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(int)) return false;
            return true;
        }
        public static bool HaveSavedReal(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(float)) return false;
            return true;
        }
        public static bool HaveSavedBoolean(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(bool)) return false;
            return true;
        }
        public static bool HaveSavedString(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(string)) return false;
            return true;
        }

        public static void RemoveSavedInteger(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(int)) return;
            parent.Remove(childKey);
            if (parent.Count == 0) table.Remove(parentKey);
        }
        public static void RemoveSavedReal(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(float)) return;
            parent.Remove(childKey);
            if (parent.Count == 0) table.Remove(parentKey);
        }
        public static void RemoveSavedBoolean(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(bool)) return;
            parent.Remove(childKey);
            if (parent.Count == 0) table.Remove(parentKey);
        }
        public static void RemoveSavedStr(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(string)) return;
            parent.Remove(childKey);
            if (parent.Count == 0) table.Remove(parentKey);

        }

        public static void FlushParentHashtable(Hashtable table) => table?.Clear();
        public static void FlushChildHashtable(Hashtable table, int parentKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)) return;
            parent.Clear();
        }
        #endregion

        #region [    Randomization API    ]
        public static int GetRandomInt(int lowBound, int highBound) => lowBound.GetRandom(Math.Abs(lowBound - highBound) + 1);
        public static float GetRandomReal(float lowBound, float highBound) => (float)new double().GetRandom() * Math.Abs(lowBound - highBound) + lowBound;
        #endregion

        #region [    Visual API    ]

        #endregion

        #region [    Preload    ]
        public static void Preload(string filename) => _Preload?.WriteLine(filename);
        public static void PreloadEnd(float timeout) { }

        public static void PreloadStart() { }
        public static void PreloadRefresh() { }
        public static void PreloadEndEx() { }

        public static void PreloadGenClear() => _Preload?.Clear();
        public static void PreloadGenStart() => _Preload = new Preload();
        public static void PreloadGenEnd(string filename)
        {
            if (_Preload == null) return;
            File.WriteAllText(filename, _Preload.ToString());
            _Preload = null;
        }
        public static void Preloader(string filename) { }
        #endregion

        #endregion

        #region [    Blizzard.J    ]
        //===========================================================================
        // Blizzard.j ( define Jass2 functions that need to be in every map script )
        //===========================================================================

        #region [    Constants    ]
        // Misc constants
        public const float bj_PI = 3.14159f;
        public const float bj_E = 2.71828f;
        public const float bj_CELLWIDTH = 128.0f;
        public const float bj_CLIFFHEIGHT = 128.0f;
        public const float bj_UNIT_FACING = 270.0f;
        public const float bj_RADTODEG = 180.0f / bj_PI;
        public const float bj_DEGTORAD = bj_PI / 180.0f;
        public const float bj_TEXT_DELAY_QUEST = 20.0f;
        public const float bj_TEXT_DELAY_QUESTUPDATE = 20.0f;
        public const float bj_TEXT_DELAY_QUESTDONE = 20.0f;
        public const float bj_TEXT_DELAY_QUESTFAILED = 20.0f;
        public const float bj_TEXT_DELAY_QUESTREQUIREMENT = 20.0f;
        public const float bj_TEXT_DELAY_MISSIONFAILED = 20.0f;
        public const float bj_TEXT_DELAY_ALWAYSHINT = 12.0f;
        public const float bj_TEXT_DELAY_HINT = 12.0f;
        public const float bj_TEXT_DELAY_SECRET = 10.0f;
        public const float bj_TEXT_DELAY_UNITACQUIRED = 15.0f;
        public const float bj_TEXT_DELAY_UNITAVAILABLE = 10.0f;
        public const float bj_TEXT_DELAY_ITEMACQUIRED = 10.0f;
        public const float bj_TEXT_DELAY_WARNING = 12.0f;
        public const float bj_QUEUE_DELAY_QUEST = 5.0f;
        public const float bj_QUEUE_DELAY_HINT = 5.0f;
        public const float bj_QUEUE_DELAY_SECRET = 3.0f;
        public const float bj_HANDICAP_EASY = 60.0f;
        public const float bj_GAME_STARTED_THRESHOLD = 0.01f;
        public const float bj_WAIT_FOR_COND_MIN_INTERVAL = 0.1f;
        public const float bj_POLLED_WAIT_INTERVAL = 0.1f;
        public const float bj_POLLED_WAIT_SKIP_THRESHOLD = 2.0f;

        // Game constants
        public const int bj_MAX_INVENTORY = 6;
        public const int bj_MAX_PLAYERS = 12;
        public const int bj_PLAYER_NEUTRAL_VICTIM = 13;
        public const int bj_PLAYER_NEUTRAL_EXTRA = 14;
        public const int bj_MAX_PLAYER_SLOTS = 16;
        public const int bj_MAX_SKELETONS = 25;
        public const int bj_MAX_STOCK_ITEM_SLOTS = 11;
        public const int bj_MAX_STOCK_UNIT_SLOTS = 11;
        public const int bj_MAX_ITEM_LEVEL = 10;

        // Ideally these would be looked up from Units/MiscData.txt,
        // but there is currently no script functionality exposed to do that
        public const float bj_TOD_DAWN = 6.0f;
        public const float bj_TOD_DUSK = 18.0f;

        // Melee game settings:
        //   - Starting Time of Day (TOD)
        //   - Starting Gold
        //   - Starting Lumber
        //   - Starting Hero Tokens (free heroes)
        //   - Max heroes allowed per player
        //   - Max heroes allowed per hero type
        //   - Distance from start loc to search for nearby mines
        //
        public const float bj_MELEE_STARTING_TOD = 8.0f;
        public const int bj_MELEE_STARTING_GOLD_V0 = 750;
        public const int bj_MELEE_STARTING_GOLD_V1 = 500;
        public const int bj_MELEE_STARTING_LUMBER_V0 = 200;
        public const int bj_MELEE_STARTING_LUMBER_V1 = 150;
        public const int bj_MELEE_STARTING_HERO_TOKENS = 1;
        public const int bj_MELEE_HERO_LIMIT = 3;
        public const int bj_MELEE_HERO_TYPE_LIMIT = 1;
        public const float bj_MELEE_MINE_SEARCH_RADIUS = 2000;
        public const float bj_MELEE_CLEAR_UNITS_RADIUS = 1500;
        public const float bj_MELEE_CRIPPLE_TIMEOUT = 120.0f;
        public const float bj_MELEE_CRIPPLE_MSG_DURATION = 20.0f;
        public const int bj_MELEE_MAX_TWINKED_HEROES_V0 = 3;
        public const int bj_MELEE_MAX_TWINKED_HEROES_V1 = 1;

        // Delay between a creep's death and the time it may drop an item.
        public const float bj_CREEP_ITEM_DELAY = 0.5f;

        // Timing settings for Marketplace inventories.
        public const float bj_STOCK_RESTOCK_INITIAL_DELAY = 120;
        public const float bj_STOCK_RESTOCK_INTERVAL = 30;
        public const int bj_STOCK_MAX_ITERATIONS = 20;

        // Max events registered by a single "dest dies in region" event.
        public const int bj_MAX_DEST_IN_REGION_EVENTS = 64;

        // Camera settings
        public const int bj_CAMERA_MIN_FARZ = 100;
        public const int bj_CAMERA_DEFAULT_DISTANCE = 1650;
        public const int bj_CAMERA_DEFAULT_FARZ = 5000;
        public const int bj_CAMERA_DEFAULT_AOA = 304;
        public const int bj_CAMERA_DEFAULT_FOV = 70;
        public const int bj_CAMERA_DEFAULT_ROLL = 0;
        public const int bj_CAMERA_DEFAULT_ROTATION = 90;

        // Rescue
        public const float bj_RESCUE_PING_TIME = 2.0f;

        // Transmission behavior settings
        public const float bj_NOTHING_SOUND_DURATION = 5.0f;
        public const float bj_TRANSMISSION_PING_TIME = 1.0f;
        public const int bj_TRANSMISSION_IND_RED = 255;
        public const int bj_TRANSMISSION_IND_BLUE = 255;
        public const int bj_TRANSMISSION_IND_GREEN = 255;
        public const int bj_TRANSMISSION_IND_ALPHA = 255;
        public const float bj_TRANSMISSION_PORT_HANGTIME = 1.5f;

        // Cinematic mode settings
        public const float bj_CINEMODE_INTERFACEFADE = 0.5f;

        // Cinematic mode volume levels
        public const float bj_CINEMODE_VOLUME_UNITMOVEMENT = 0.4f;
        public const float bj_CINEMODE_VOLUME_UNITSOUNDS = 0.0f;
        public const float bj_CINEMODE_VOLUME_COMBAT = 0.4f;
        public const float bj_CINEMODE_VOLUME_SPELLS = 0.4f;
        public const float bj_CINEMODE_VOLUME_UI = 0.0f;
        public const float bj_CINEMODE_VOLUME_MUSIC = 0.55f;
        public const float bj_CINEMODE_VOLUME_AMBIENTSOUNDS = 1.0f;
        public const float bj_CINEMODE_VOLUME_FIRE = 0.6f;

        // Speech mode volume levels
        public const float bj_SPEECH_VOLUME_UNITMOVEMENT = 0.25f;
        public const float bj_SPEECH_VOLUME_UNITSOUNDS = 0.0f;
        public const float bj_SPEECH_VOLUME_COMBAT = 0.25f;
        public const float bj_SPEECH_VOLUME_SPELLS = 0.25f;
        public const float bj_SPEECH_VOLUME_UI = 0.0f;
        public const float bj_SPEECH_VOLUME_MUSIC = 0.55f;
        public const float bj_SPEECH_VOLUME_AMBIENTSOUNDS = 1.0f;
        public const float bj_SPEECH_VOLUME_FIRE = 0.6f;

        // Smart pan settings
        public const float bj_SMARTPAN_TRESHOLD_PAN = 500;
        public const float bj_SMARTPAN_TRESHOLD_SNAP = 3500;

        // QueuedTriggerExecute settings
        public const int bj_MAX_QUEUED_TRIGGERS = 100;
        public const float bj_QUEUED_TRIGGER_TIMEOUT = 180.0f;

        // Campaign indexing constants
        public const int bj_CAMPAIGN_INDEX_T = 0;
        public const int bj_CAMPAIGN_INDEX_H = 1;
        public const int bj_CAMPAIGN_INDEX_U = 2;
        public const int bj_CAMPAIGN_INDEX_O = 3;
        public const int bj_CAMPAIGN_INDEX_N = 4;
        public const int bj_CAMPAIGN_INDEX_XN = 5;
        public const int bj_CAMPAIGN_INDEX_XH = 6;
        public const int bj_CAMPAIGN_INDEX_XU = 7;
        public const int bj_CAMPAIGN_INDEX_XO = 8;

        // Campaign offset constants (for mission indexing)
        public const int bj_CAMPAIGN_OFFSET_T = 0;
        public const int bj_CAMPAIGN_OFFSET_H = 1;
        public const int bj_CAMPAIGN_OFFSET_U = 2;
        public const int bj_CAMPAIGN_OFFSET_O = 3;
        public const int bj_CAMPAIGN_OFFSET_N = 4;
        public const int bj_CAMPAIGN_OFFSET_XN = 0;
        public const int bj_CAMPAIGN_OFFSET_XH = 1;
        public const int bj_CAMPAIGN_OFFSET_XU = 2;
        public const int bj_CAMPAIGN_OFFSET_XO = 3;

        // Mission indexing constants
        // Tutorial
        public const int bj_MISSION_INDEX_T00 = bj_CAMPAIGN_OFFSET_T * 1000 + 0;
        public const int bj_MISSION_INDEX_T01 = bj_CAMPAIGN_OFFSET_T * 1000 + 1;
        // Human
        public const int bj_MISSION_INDEX_H00 = bj_CAMPAIGN_OFFSET_H * 1000 + 0;
        public const int bj_MISSION_INDEX_H01 = bj_CAMPAIGN_OFFSET_H * 1000 + 1;
        public const int bj_MISSION_INDEX_H02 = bj_CAMPAIGN_OFFSET_H * 1000 + 2;
        public const int bj_MISSION_INDEX_H03 = bj_CAMPAIGN_OFFSET_H * 1000 + 3;
        public const int bj_MISSION_INDEX_H04 = bj_CAMPAIGN_OFFSET_H * 1000 + 4;
        public const int bj_MISSION_INDEX_H05 = bj_CAMPAIGN_OFFSET_H * 1000 + 5;
        public const int bj_MISSION_INDEX_H06 = bj_CAMPAIGN_OFFSET_H * 1000 + 6;
        public const int bj_MISSION_INDEX_H07 = bj_CAMPAIGN_OFFSET_H * 1000 + 7;
        public const int bj_MISSION_INDEX_H08 = bj_CAMPAIGN_OFFSET_H * 1000 + 8;
        public const int bj_MISSION_INDEX_H09 = bj_CAMPAIGN_OFFSET_H * 1000 + 9;
        public const int bj_MISSION_INDEX_H10 = bj_CAMPAIGN_OFFSET_H * 1000 + 10;
        public const int bj_MISSION_INDEX_H11 = bj_CAMPAIGN_OFFSET_H * 1000 + 11;
        // Undead
        public const int bj_MISSION_INDEX_U00 = bj_CAMPAIGN_OFFSET_U * 1000 + 0;
        public const int bj_MISSION_INDEX_U01 = bj_CAMPAIGN_OFFSET_U * 1000 + 1;
        public const int bj_MISSION_INDEX_U02 = bj_CAMPAIGN_OFFSET_U * 1000 + 2;
        public const int bj_MISSION_INDEX_U03 = bj_CAMPAIGN_OFFSET_U * 1000 + 3;
        public const int bj_MISSION_INDEX_U05 = bj_CAMPAIGN_OFFSET_U * 1000 + 4;
        public const int bj_MISSION_INDEX_U07 = bj_CAMPAIGN_OFFSET_U * 1000 + 5;
        public const int bj_MISSION_INDEX_U08 = bj_CAMPAIGN_OFFSET_U * 1000 + 6;
        public const int bj_MISSION_INDEX_U09 = bj_CAMPAIGN_OFFSET_U * 1000 + 7;
        public const int bj_MISSION_INDEX_U10 = bj_CAMPAIGN_OFFSET_U * 1000 + 8;
        public const int bj_MISSION_INDEX_U11 = bj_CAMPAIGN_OFFSET_U * 1000 + 9;
        // Orc
        public const int bj_MISSION_INDEX_O00 = bj_CAMPAIGN_OFFSET_O * 1000 + 0;
        public const int bj_MISSION_INDEX_O01 = bj_CAMPAIGN_OFFSET_O * 1000 + 1;
        public const int bj_MISSION_INDEX_O02 = bj_CAMPAIGN_OFFSET_O * 1000 + 2;
        public const int bj_MISSION_INDEX_O03 = bj_CAMPAIGN_OFFSET_O * 1000 + 3;
        public const int bj_MISSION_INDEX_O04 = bj_CAMPAIGN_OFFSET_O * 1000 + 4;
        public const int bj_MISSION_INDEX_O05 = bj_CAMPAIGN_OFFSET_O * 1000 + 5;
        public const int bj_MISSION_INDEX_O06 = bj_CAMPAIGN_OFFSET_O * 1000 + 6;
        public const int bj_MISSION_INDEX_O07 = bj_CAMPAIGN_OFFSET_O * 1000 + 7;
        public const int bj_MISSION_INDEX_O08 = bj_CAMPAIGN_OFFSET_O * 1000 + 8;
        public const int bj_MISSION_INDEX_O09 = bj_CAMPAIGN_OFFSET_O * 1000 + 9;
        public const int bj_MISSION_INDEX_O10 = bj_CAMPAIGN_OFFSET_O * 1000 + 10;
        // Night Elf
        public const int bj_MISSION_INDEX_N00 = bj_CAMPAIGN_OFFSET_N * 1000 + 0;
        public const int bj_MISSION_INDEX_N01 = bj_CAMPAIGN_OFFSET_N * 1000 + 1;
        public const int bj_MISSION_INDEX_N02 = bj_CAMPAIGN_OFFSET_N * 1000 + 2;
        public const int bj_MISSION_INDEX_N03 = bj_CAMPAIGN_OFFSET_N * 1000 + 3;
        public const int bj_MISSION_INDEX_N04 = bj_CAMPAIGN_OFFSET_N * 1000 + 4;
        public const int bj_MISSION_INDEX_N05 = bj_CAMPAIGN_OFFSET_N * 1000 + 5;
        public const int bj_MISSION_INDEX_N06 = bj_CAMPAIGN_OFFSET_N * 1000 + 6;
        public const int bj_MISSION_INDEX_N07 = bj_CAMPAIGN_OFFSET_N * 1000 + 7;
        public const int bj_MISSION_INDEX_N08 = bj_CAMPAIGN_OFFSET_N * 1000 + 8;
        public const int bj_MISSION_INDEX_N09 = bj_CAMPAIGN_OFFSET_N * 1000 + 9;
        // Expansion Night Elf
        public const int bj_MISSION_INDEX_XN00 = bj_CAMPAIGN_OFFSET_XN * 1000 + 0;
        public const int bj_MISSION_INDEX_XN01 = bj_CAMPAIGN_OFFSET_XN * 1000 + 1;
        public const int bj_MISSION_INDEX_XN02 = bj_CAMPAIGN_OFFSET_XN * 1000 + 2;
        public const int bj_MISSION_INDEX_XN03 = bj_CAMPAIGN_OFFSET_XN * 1000 + 3;
        public const int bj_MISSION_INDEX_XN04 = bj_CAMPAIGN_OFFSET_XN * 1000 + 4;
        public const int bj_MISSION_INDEX_XN05 = bj_CAMPAIGN_OFFSET_XN * 1000 + 5;
        public const int bj_MISSION_INDEX_XN06 = bj_CAMPAIGN_OFFSET_XN * 1000 + 6;
        public const int bj_MISSION_INDEX_XN07 = bj_CAMPAIGN_OFFSET_XN * 1000 + 7;
        public const int bj_MISSION_INDEX_XN08 = bj_CAMPAIGN_OFFSET_XN * 1000 + 8;
        public const int bj_MISSION_INDEX_XN09 = bj_CAMPAIGN_OFFSET_XN * 1000 + 9;
        public const int bj_MISSION_INDEX_XN10 = bj_CAMPAIGN_OFFSET_XN * 1000 + 10;
        // Expansion Human
        public const int bj_MISSION_INDEX_XH00 = bj_CAMPAIGN_OFFSET_XH * 1000 + 0;
        public const int bj_MISSION_INDEX_XH01 = bj_CAMPAIGN_OFFSET_XH * 1000 + 1;
        public const int bj_MISSION_INDEX_XH02 = bj_CAMPAIGN_OFFSET_XH * 1000 + 2;
        public const int bj_MISSION_INDEX_XH03 = bj_CAMPAIGN_OFFSET_XH * 1000 + 3;
        public const int bj_MISSION_INDEX_XH04 = bj_CAMPAIGN_OFFSET_XH * 1000 + 4;
        public const int bj_MISSION_INDEX_XH05 = bj_CAMPAIGN_OFFSET_XH * 1000 + 5;
        public const int bj_MISSION_INDEX_XH06 = bj_CAMPAIGN_OFFSET_XH * 1000 + 6;
        public const int bj_MISSION_INDEX_XH07 = bj_CAMPAIGN_OFFSET_XH * 1000 + 7;
        public const int bj_MISSION_INDEX_XH08 = bj_CAMPAIGN_OFFSET_XH * 1000 + 8;
        public const int bj_MISSION_INDEX_XH09 = bj_CAMPAIGN_OFFSET_XH * 1000 + 9;
        // Expansion Undead
        public const int bj_MISSION_INDEX_XU00 = bj_CAMPAIGN_OFFSET_XU * 1000 + 0;
        public const int bj_MISSION_INDEX_XU01 = bj_CAMPAIGN_OFFSET_XU * 1000 + 1;
        public const int bj_MISSION_INDEX_XU02 = bj_CAMPAIGN_OFFSET_XU * 1000 + 2;
        public const int bj_MISSION_INDEX_XU03 = bj_CAMPAIGN_OFFSET_XU * 1000 + 3;
        public const int bj_MISSION_INDEX_XU04 = bj_CAMPAIGN_OFFSET_XU * 1000 + 4;
        public const int bj_MISSION_INDEX_XU05 = bj_CAMPAIGN_OFFSET_XU * 1000 + 5;
        public const int bj_MISSION_INDEX_XU06 = bj_CAMPAIGN_OFFSET_XU * 1000 + 6;
        public const int bj_MISSION_INDEX_XU07 = bj_CAMPAIGN_OFFSET_XU * 1000 + 7;
        public const int bj_MISSION_INDEX_XU08 = bj_CAMPAIGN_OFFSET_XU * 1000 + 8;
        public const int bj_MISSION_INDEX_XU09 = bj_CAMPAIGN_OFFSET_XU * 1000 + 9;
        public const int bj_MISSION_INDEX_XU10 = bj_CAMPAIGN_OFFSET_XU * 1000 + 10;
        public const int bj_MISSION_INDEX_XU11 = bj_CAMPAIGN_OFFSET_XU * 1000 + 11;
        public const int bj_MISSION_INDEX_XU12 = bj_CAMPAIGN_OFFSET_XU * 1000 + 12;
        public const int bj_MISSION_INDEX_XU13 = bj_CAMPAIGN_OFFSET_XU * 1000 + 13;

        // Expansion Orc
        public const int bj_MISSION_INDEX_XO00 = bj_CAMPAIGN_OFFSET_XO * 1000 + 0;

        // Cinematic indexing constants
        public const int bj_CINEMATICINDEX_TOP = 0;
        public const int bj_CINEMATICINDEX_HOP = 1;
        public const int bj_CINEMATICINDEX_HED = 2;
        public const int bj_CINEMATICINDEX_OOP = 3;
        public const int bj_CINEMATICINDEX_OED = 4;
        public const int bj_CINEMATICINDEX_UOP = 5;
        public const int bj_CINEMATICINDEX_UED = 6;
        public const int bj_CINEMATICINDEX_NOP = 7;
        public const int bj_CINEMATICINDEX_NED = 8;
        public const int bj_CINEMATICINDEX_XOP = 9;
        public const int bj_CINEMATICINDEX_XED = 10;

        // Alliance settings
        public const int bj_ALLIANCE_UNALLIED = 0;
        public const int bj_ALLIANCE_UNALLIED_VISION = 1;
        public const int bj_ALLIANCE_ALLIED = 2;
        public const int bj_ALLIANCE_ALLIED_VISION = 3;
        public const int bj_ALLIANCE_ALLIED_UNITS = 4;
        public const int bj_ALLIANCE_ALLIED_ADVUNITS = 5;
        public const int bj_ALLIANCE_NEUTRAL = 6;
        public const int bj_ALLIANCE_NEUTRAL_VISION = 7;

        // Keyboard Event Types
        public const int bj_KEYEVENTTYPE_DEPRESS = 0;
        public const int bj_KEYEVENTTYPE_RELEASE = 1;

        // Keyboard Event Keys
        public const int bj_KEYEVENTKEY_LEFT = 0;
        public const int bj_KEYEVENTKEY_RIGHT = 1;
        public const int bj_KEYEVENTKEY_DOWN = 2;
        public const int bj_KEYEVENTKEY_UP = 3;

        // Transmission timing methods
        public const int bj_TIMETYPE_ADD = 0;
        public const int bj_TIMETYPE_SET = 1;
        public const int bj_TIMETYPE_SUB = 2;

        // Camera bounds adjustment methods
        public const int bj_CAMERABOUNDS_ADJUST_ADD = 0;
        public const int bj_CAMERABOUNDS_ADJUST_SUB = 1;

        // Quest creation states
        public const int bj_QUESTTYPE_REQ_DISCOVERED = 0;
        public const int bj_QUESTTYPE_REQ_UNDISCOVERED = 1;
        public const int bj_QUESTTYPE_OPT_DISCOVERED = 2;
        public const int bj_QUESTTYPE_OPT_UNDISCOVERED = 3;

        // Quest message types
        public const int bj_QUESTMESSAGE_DISCOVERED = 0;
        public const int bj_QUESTMESSAGE_UPDATED = 1;
        public const int bj_QUESTMESSAGE_COMPLETED = 2;
        public const int bj_QUESTMESSAGE_FAILED = 3;
        public const int bj_QUESTMESSAGE_REQUIREMENT = 4;
        public const int bj_QUESTMESSAGE_MISSIONFAILED = 5;
        public const int bj_QUESTMESSAGE_ALWAYSHINT = 6;
        public const int bj_QUESTMESSAGE_HINT = 7;
        public const int bj_QUESTMESSAGE_SECRET = 8;
        public const int bj_QUESTMESSAGE_UNITACQUIRED = 9;
        public const int bj_QUESTMESSAGE_UNITAVAILABLE = 10;
        public const int bj_QUESTMESSAGE_ITEMACQUIRED = 11;
        public const int bj_QUESTMESSAGE_WARNING = 12;

        // Leaderboard sorting methods
        public const int bj_SORTTYPE_SORTBYVALUE = 0;
        public const int bj_SORTTYPE_SORTBYPLAYER = 1;
        public const int bj_SORTTYPE_SORTBYLABEL = 2;

        // Cinematic fade filter methods
        public const int bj_CINEFADETYPE_FADEIN = 0;
        public const int bj_CINEFADETYPE_FADEOUT = 1;
        public const int bj_CINEFADETYPE_FADEOUTIN = 2;

        // Buff removal methods
        public const int bj_REMOVEBUFFS_POSITIVE = 0;
        public const int bj_REMOVEBUFFS_NEGATIVE = 1;
        public const int bj_REMOVEBUFFS_ALL = 2;
        public const int bj_REMOVEBUFFS_NONTLIFE = 3;

        // Buff properties - polarity
        public const int bj_BUFF_POLARITY_POSITIVE = 0;
        public const int bj_BUFF_POLARITY_NEGATIVE = 1;
        public const int bj_BUFF_POLARITY_EITHER = 2;

        // Buff properties - resist type
        public const int bj_BUFF_RESIST_MAGIC = 0;
        public const int bj_BUFF_RESIST_PHYSICAL = 1;
        public const int bj_BUFF_RESIST_EITHER = 2;
        public const int bj_BUFF_RESIST_BOTH = 3;

        // Hero stats
        public const int bj_HEROSTAT_STR = 0;
        public const int bj_HEROSTAT_AGI = 1;
        public const int bj_HEROSTAT_INT = 2;

        // Hero skill point modification methods
        public const int bj_MODIFYMETHOD_ADD = 0;
        public const int bj_MODIFYMETHOD_SUB = 1;
        public const int bj_MODIFYMETHOD_SET = 2;

        // Unit state adjustment methods (for replaced units)
        public const int bj_UNIT_STATE_METHOD_ABSOLUTE = 0;
        public const int bj_UNIT_STATE_METHOD_RELATIVE = 1;
        public const int bj_UNIT_STATE_METHOD_DEFAULTS = 2;
        public const int bj_UNIT_STATE_METHOD_MAXIMUM = 3;

        // Gate operations
        public const int bj_GATEOPERATION_CLOSE = 0;
        public const int bj_GATEOPERATION_OPEN = 1;
        public const int bj_GATEOPERATION_DESTROY = 2;

        // Game cache value types
        public const int bj_GAMECACHE_BOOLEAN = 0;
        public const int bj_GAMECACHE_INTEGER = 1;
        public const int bj_GAMECACHE_REAL = 2;
        public const int bj_GAMECACHE_UNIT = 3;
        public const int bj_GAMECACHE_STRING = 4;

        // Hashtable value types
        public const int bj_HASHTABLE_BOOLEAN = 0;
        public const int bj_HASHTABLE_INTEGER = 1;
        public const int bj_HASHTABLE_REAL = 2;
        public const int bj_HASHTABLE_STRING = 3;
        public const int bj_HASHTABLE_HANDLE = 4;

        // Item status types
        public const int bj_ITEM_STATUS_HIDDEN = 0;
        public const int bj_ITEM_STATUS_OWNED = 1;
        public const int bj_ITEM_STATUS_INVULNERABLE = 2;
        public const int bj_ITEM_STATUS_POWERUP = 3;
        public const int bj_ITEM_STATUS_SELLABLE = 4;
        public const int bj_ITEM_STATUS_PAWNABLE = 5;

        // Itemcode status types
        public const int bj_ITEMCODE_STATUS_POWERUP = 0;
        public const int bj_ITEMCODE_STATUS_SELLABLE = 1;
        public const int bj_ITEMCODE_STATUS_PAWNABLE = 2;

        // Minimap ping styles
        public const int bj_MINIMAPPINGSTYLE_SIMPLE = 0;
        public const int bj_MINIMAPPINGSTYLE_FLASHY = 1;
        public const int bj_MINIMAPPINGSTYLE_ATTACK = 2;

        // Corpse creation settings
        public const float bj_CORPSE_MAX_DEATH_TIME = 8.0f;

        // Corpse creation styles
        public const int bj_CORPSETYPE_FLESH = 0;
        public const int bj_CORPSETYPE_BONE = 1;

        // Elevator pathing-blocker destructable code
        public const int bj_ELEVATOR_BLOCKER_CODE = 0x44546570;
        public const int bj_ELEVATOR_CODE01 = 0x44547266;
        public const int bj_ELEVATOR_CODE02 = 0x44547278;

        // Elevator wall codes
        public const int bj_ELEVATOR_WALL_TYPE_ALL = 0;
        public const int bj_ELEVATOR_WALL_TYPE_EAST = 1;
        public const int bj_ELEVATOR_WALL_TYPE_NORTH = 2;
        public const int bj_ELEVATOR_WALL_TYPE_SOUTH = 3;
        public const int bj_ELEVATOR_WALL_TYPE_WEST = 4;
        #endregion

        #region [    Variables    ]
        // Utility function vars
        public static int bj_forLoopAIndex = 0;
        public static int bj_forLoopBIndex = 0;
        public static int bj_forLoopAIndexEnd = 0;
        public static int bj_forLoopBIndexEnd = 0;

        // Last X'd vars
        public static Hashtable bj_lastCreatedHashtable = null;
        #endregion

        #region [    Debugging Functions    ]
        public static void BJDebugMsg(string msg) => Console.WriteLine(msg);
        #endregion

        #region [    Math Utility Functions    ]
        public static float RMinBJ(float a, float b) => a < b ? a : b;
        public static float RMaxBJ(float a, float b) => a < b ? b : a;
        public static float RAbsBJ(float a) => a >= 0 ? a : -a;
        public static float RSignBJ(float a) => a >= 0 ? 1 : -1;
        public static int IMinBJ(int a, int b) => a < b ? a : b;
        public static int IMaxBJ(int a, int b) => a < b ? b : a;
        public static int IAbsBJ(int a) => a >= 0 ? a : -a;
        public static int ISignBJ(int a) => a >= 0 ? 1 : -1;
        public static float SinBJ(float degrees) => Sin(degrees * bj_DEGTORAD);
        public static float CosBJ(float degrees) => Cos(degrees * bj_DEGTORAD);
        public static float TanBJ(float degrees) => Tan(degrees * bj_DEGTORAD);
        public static float AsinBJ(float degrees) => Asin(degrees) * bj_RADTODEG;
        public static float AcosBJ(float degrees) => Acos(degrees) * bj_RADTODEG;
        public static float AtanBJ(float degrees) => Atan(degrees) * bj_RADTODEG;
        public static float Atan2BJ(float y, float x) => Atan2(y, x) * bj_RADTODEG;
        public static float AngleBetweenPoints(location locA, location locB)
            => bj_RADTODEG * Atan2(GetLocationY(locB) - GetLocationY(locA), GetLocationX(locB) - GetLocationX(locA));
        public static float DistanceBetweenPoints(location locA, location locB)
        {
            float dx = GetLocationX(locB) - GetLocationX(locA);
            float dy = GetLocationY(locB) - GetLocationY(locA);
            return SquareRoot(dx * dx + dy * dy);
        }
        public static location PolarProjectionBJ(location source, float dist, float angle)
        {
            float x = GetLocationX(source) + dist * Cos(angle * bj_DEGTORAD);
            float y = GetLocationY(source) + dist * Sin(angle * bj_DEGTORAD);
            return Location(x, y);
        }
        public static float GetRandomDirectionDeg() => GetRandomReal(0, 360);
        public static float GetRandomPercentageBJ() => GetRandomReal(0, 100);
        public static location GetRandomLocInRect(rect whichRect)
            => Location(GetRandomReal(GetRectMinX(whichRect), GetRectMaxX(whichRect)), GetRandomReal(GetRectMinY(whichRect), GetRectMaxY(whichRect)));

        //===========================================================================
        // Calculate the modulus/remainder of (dividend) divided by (divisor).
        // Examples:  18 mod 5 = 3.  15 mod 5 = 0.  -8 mod 5 = 2.
        public static int ModuloInteger(int dividend, int divisor)
        {
            int modulus = dividend - dividend / divisor * divisor;
            // If the dividend was negative, the above modulus calculation will
            // be negative, but within (-divisor..0).  We can add (divisor) to
            // shift this result into the desired range of (0..divisor).
            return modulus < 0 ? modulus + divisor : modulus;
        }

        //===========================================================================
        // Calculate the modulus/remainder of (dividend) divided by (divisor).
        // Examples:  13.000 mod 2.500 = 0.500.  -6.000 mod 2.500 = 1.500.
        public static float ModuloReal(float dividend, float divisor)
        {
            float modulus = dividend - I2R(R2I(dividend / divisor)) * divisor;
            // If the dividend was negative, the above modulus calculation will
            // be negative, but within (-divisor..0).  We can add (divisor) to
            // shift this result into the desired range of (0..divisor).
            return modulus < 0 ? modulus + divisor : modulus;
        }

        public static location OffsetLocation(location loc, float dx, float dy)
            => Location(GetLocationX(loc) + dx, GetLocationY(loc) + dy);
        public static rect OffsetRectBJ(rect r, float dx, float dy)
            => Rect(GetRectMinX(r) + dx, GetRectMinY(r) + dy, GetRectMaxX(r) + dx, GetRectMaxY(r) + dy);
        public static rect RectFromCenterSizeBJ(location center, float width, float height)
        {
            float x = GetLocationX(center);
            float y = GetLocationY(center);
            return Rect(x - width * 0.5f, y - height * 0.5f, x + width * 0.5f, y + height * 0.5f);
        }
        public static bool RectContainsCoords(rect r, float x, float y)
            => (GetRectMinX(r) <= x) && (x <= GetRectMaxX(r)) && (GetRectMinY(r) <= y) && (y <= GetRectMaxY(r));
        public static bool RectContainsLoc(rect r, location loc)
            => RectContainsCoords(r, GetLocationX(loc), GetLocationY(loc));
        #endregion

        #region [    General Utility Functions    ]
        public static void DoNothing() { }
        public static bool GetBooleanAnd(bool valueA, bool valueB) => valueA && valueB;
        public static bool GetBooleanOr(bool valueA, bool valueB) => valueA || valueB;

        //===========================================================================
        // Converts a percentage (real, 0..100) into a scaled integer (0..max),
        // clipping the result to 0..max in case the input is invalid.
        public static int PercentToInt(float percentage, int max)
        {
            int result = R2I(percentage * I2R(max) * 0.01f);
            if (result < 0) return 0;
            else if (result > max) return max;
            return result;
        }
        public static int PercentTo255(float percentage)
            => PercentToInt(percentage, 255);
        public static bool CompareLocationsBJ(location A, location B)
            => GetLocationX(A) == GetLocationX(B) && GetLocationY(A) == GetLocationY(B);
        public static bool CompareRectsBJ(rect A, rect B)
            => GetRectMinX(A) == GetRectMinX(B) && GetRectMinY(A) == GetRectMinY(B) && GetRectMaxX(A) == GetRectMaxX(B) && GetRectMaxY(A) == GetRectMaxY(B);
        public static rect GetRectFromCircleBJ(location center, float radius)
        {
            float centerX = GetLocationX(center);
            float centerY = GetLocationY(center);
            return Rect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        }
        #endregion

        #region [    Text Utility Functions    ]
        public static string SubStringBJ(string source, int start, int end)
            => SubString(source, start - 1, end);
        public static int StringHashBJ(string s) => StringHash(s);
        #endregion

        #region [    Text Tag Utility Functions    ]
        //===========================================================================
        // Scale the font size linearly such that size 10 equates to height 0.023.
        // Screen-relative font heights are harder to grasp and than font sizes.
        public static float TextTagSize2Height(float size) => size * 0.023f / 10;

        //===========================================================================
        // Scale the speed linearly such that speed 128 equates to 0.071.
        // Screen-relative speeds are hard to grasp.
        public static float TextTagSpeed2Velocity(float speed) => speed * 0.071f / 128;
        #endregion

        #region [    Rescuable Unit Utility Functions    ]
        public static Hashtable InitHashtableBJ() => bj_lastCreatedHashtable = InitHashtable();
        public static Hashtable GetLastCreatedHashtableBJ() => bj_lastCreatedHashtable;
        public static void SaveRealBJ(float value, int key, int missionKey, Hashtable table)
            => SaveReal(table, missionKey, key, value);
        public static void SaveIntegerBJ(int value, int key, int missionKey, Hashtable table)
            => SaveInteger(table, missionKey, key, value);
        public static void SaveBooleanBJ(bool value, int key, int missionKey, Hashtable table)
            => SaveBoolean(table, missionKey, key, value);
        public static void SaveStringBJ(string value, int key, int missionKey, Hashtable table)
            => SaveStr(table, missionKey, key, value);
        public static float LoadRealBJ(int key, int missionKey, Hashtable table)
            => LoadReal(table, missionKey, key);
        public static int LoadIntegerBJ(int key, int missionKey, Hashtable table)
            => LoadInteger(table, missionKey, key);
        public static bool LoadBooleanBJ(int key, int missionKey, Hashtable table)
            => LoadBoolean(table, missionKey, key);
        public static string LoadStringBJ(int key, int missionKey, Hashtable table)
            => LoadStr(table, missionKey, key) ?? string.Empty;
        public static void FlushParentHashtableBJ(Hashtable table) => FlushParentHashtable(table);
        public static void FlushChildHashtableBJ(int missionKey, Hashtable table) 
            => FlushChildHashtable(table, missionKey);
        public static bool HaveSavedValue(int key, int valueType, int missionKey, Hashtable table)
        {
            switch (valueType)
            {
                case bj_HASHTABLE_BOOLEAN:
                    return HaveSavedBoolean(table, missionKey, key);
                case bj_HASHTABLE_INTEGER:
                    return HaveSavedInteger(table, missionKey, key);
                case bj_HASHTABLE_REAL:
                    return HaveSavedReal(table, missionKey, key);
                case bj_HASHTABLE_STRING:
                    return HaveSavedString(table, missionKey, key);
                default: return false;
            }
        }
        #endregion

        #region [    Miscellaneous Utility Functions    ]
        public static location GetRectCenter(rect whichRect)
            => Location(GetRectCenterX(whichRect), GetRectCenterY(whichRect));
        public static int GetFadeFromSeconds(float seconds)
            => seconds == 0 ? 10000 : 128 / R2I(seconds);
        public static float GetFadeFromSecondsAsReal(float seconds)
            => seconds == 0 ? 10000 : 128 / seconds;
        public static float GetRectWidthBJ(rect r) => GetRectMaxX(r) - GetRectMinX(r);
        public static float GetRectHeightBJ(rect r) => GetRectMaxY(r) - GetRectMinY(r);
        #endregion

        #endregion
    }

    public class rect
    {
        public float minx, miny, maxx, maxy;
    }

    public class location
    {
        public float x, y;
    }
}