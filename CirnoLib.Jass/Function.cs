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
        public static double Deg2Rad(double degrees) => Math.PI * degrees / 180.0;
        public static double Rad2Deg(double radians) => radians * (180.0 / Math.PI);

        public static double Sin(double radians) => Math.Sin(radians);
        public static double Cos(double radians) => Math.Cos(radians);
        public static double Tan(double radians) => Math.Tan(radians);

        // Expect values between -1 and 1...returns 0 for invalid input
        public static double Asin(double y) => Math.Asin(y);
        public static double Acos(double x) => Math.Acos(x);
        public static double Atan(double x) => Math.Atan(x);

        // Returns 0 if x and y are both 0
        public static double Atan2(double y, double x) => Math.Atan2(y, x);

        // Returns 0 if x <= 0
        public static double SquareRoot(double x) => Math.Sqrt(x);

        // computes x to the y power
        // y == 0.0             => 1
        // x == 0.0 and y < 0   => 0
        public static double Pow(double x, double power) => Math.Pow(x, power);
        #endregion

        #region [    String Utility API    ]
        public static double I2R(int i) => Convert.ToDouble(i);
        public static int R2I(double r)
        {
            if (r > int.MaxValue) return int.MaxValue;
            if (r < int.MinValue) return int.MinValue;
            //if (r % 1 == 0) return (int)(r - 1); //rounding bug
            return (int)r;
        }
        public static string I2S(int i) => i.ToString();
        public static string R2S(double r) => r.ToString();
        //public static string R2SW(double r, int width, int precision)
        public static int S2I(string s) => int.TryParse(s, out int result) ? result : 0;
        public static double S2R(string s) => double.TryParse(s, out double result) ? result : 0;
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
        public static rect Rect(double minx, double miny, double maxx, double maxy)
            => new rect { minx = minx, miny = miny, maxx = maxx, maxy = maxy };
        public static rect RectFromLoc(location min, location max)
            => Rect(min.x, min.y, max.x, max.y);
        public static void RemoveRect(rect whichRect) 
            => whichRect.minx = whichRect.miny = whichRect.maxx = whichRect.maxy = 0;
        public static void SetRect(rect whichRect, double minx, double miny, double maxx, double maxy)
        {
            whichRect.minx = minx;
            whichRect.miny = miny;
            whichRect.maxx = maxx;
            whichRect.maxy = maxy;
        }
        public static void SetRectFromLoc(rect whichRect, location min, location max)
            => SetRect(whichRect, min.x, min.y, max.x, max.y);
        public static void MoveRectTo(rect whichRect, double newCenterX, double newCenterY)
        {
            double halfWidth = (whichRect.maxx - whichRect.minx) / 2;
            double halfHeight = (whichRect.maxy - whichRect.miny) / 2;
            whichRect.minx = newCenterX - halfWidth;
            whichRect.maxx = newCenterX + halfWidth;
            whichRect.miny = newCenterY - halfHeight;
            whichRect.maxy = newCenterY + halfHeight;
        }
        public static void MoveRectToLoc(rect whichRect, location newCenter)
            => MoveRectTo(whichRect, newCenter.x, newCenter.y);

        public static double GetRectCenterX(rect whichRect) 
            => (whichRect.maxx - whichRect.minx) / 2 + whichRect.minx;
        public static double GetRectCenterY(rect whichRect)
            => (whichRect.maxy - whichRect.miny) / 2 + whichRect.miny;
        public static double GetRectMinX(rect whichRect) => whichRect.minx;
        public static double GetRectMinY(rect whichRect) => whichRect.miny;
        public static double GetRectMaxX(rect whichRect) => whichRect.maxx;
        public static double GetRectMaxY(rect whichRect) => whichRect.maxy;

        public static location Location(double x, double y) => new location { x = x, y = y };
        public static void RemoveLocation(location whichLocation)
            => whichLocation.x = whichLocation.y = 0;
        public static void MoveLocation(location whichLocation, double newX, double newY)
        {
            whichLocation.x = newX;
            whichLocation.y = newY;
        }
        public static double GetLocationX(location whichLocation) => whichLocation.x;
        public static double GetLocationY(location whichLocation) => whichLocation.y;
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
        public static void SaveReal(Hashtable table, int parentKey, int childKey, double value)
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
        public static double LoadReal(Hashtable table, int parentKey, int childKey)
        {
            if (table == null
            || !table.ContainsKey(parentKey)
            || !(table[parentKey] is Hashtable parent)
            || !parent.ContainsKey(childKey)
            || parent[childKey].GetType() != typeof(double)) return 0;
            return (double)parent[childKey];
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
            || parent[childKey].GetType() != typeof(double)) return false;
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
            || parent[childKey].GetType() != typeof(double)) return;
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
        public static double GetRandomReal(double lowBound, double highBound) => new double().GetRandom() * Math.Abs(lowBound - highBound) + lowBound;
        #endregion

        #region [    Visual API    ]

        #endregion

        #region [    Preload    ]
        public static void Preload(string filename) => _Preload?.WriteLine(filename);
        public static void PreloadEnd(double timeout) { }

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
        public const double bj_PI = 3.14159;
        public const double bj_E = 2.71828;
        public const double bj_CELLWIDTH = 128.0;
        public const double bj_CLIFFHEIGHT = 128.0;
        public const double bj_UNIT_FACING = 270.0;
        public const double bj_RADTODEG = 180.0 / bj_PI;
        public const double bj_DEGTORAD = bj_PI / 180.0;
        public const double bj_TEXT_DELAY_QUEST = 20.00;
        public const double bj_TEXT_DELAY_QUESTUPDATE = 20.00;
        public const double bj_TEXT_DELAY_QUESTDONE = 20.00;
        public const double bj_TEXT_DELAY_QUESTFAILED = 20.00;
        public const double bj_TEXT_DELAY_QUESTREQUIREMENT = 20.00;
        public const double bj_TEXT_DELAY_MISSIONFAILED = 20.00;
        public const double bj_TEXT_DELAY_ALWAYSHINT = 12.00;
        public const double bj_TEXT_DELAY_HINT = 12.00;
        public const double bj_TEXT_DELAY_SECRET = 10.00;
        public const double bj_TEXT_DELAY_UNITACQUIRED = 15.00;
        public const double bj_TEXT_DELAY_UNITAVAILABLE = 10.00;
        public const double bj_TEXT_DELAY_ITEMACQUIRED = 10.00;
        public const double bj_TEXT_DELAY_WARNING = 12.00;
        public const double bj_QUEUE_DELAY_QUEST = 5.00;
        public const double bj_QUEUE_DELAY_HINT = 5.00;
        public const double bj_QUEUE_DELAY_SECRET = 3.00;
        public const double bj_HANDICAP_EASY = 60.00;
        public const double bj_GAME_STARTED_THRESHOLD = 0.01;
        public const double bj_WAIT_FOR_COND_MIN_INTERVAL = 0.10;
        public const double bj_POLLED_WAIT_INTERVAL = 0.10;
        public const double bj_POLLED_WAIT_SKIP_THRESHOLD = 2.00;

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
        public const double bj_TOD_DAWN = 6.00;
        public const double bj_TOD_DUSK = 18.00;

        // Melee game settings:
        //   - Starting Time of Day (TOD)
        //   - Starting Gold
        //   - Starting Lumber
        //   - Starting Hero Tokens (free heroes)
        //   - Max heroes allowed per player
        //   - Max heroes allowed per hero type
        //   - Distance from start loc to search for nearby mines
        //
        public const double bj_MELEE_STARTING_TOD = 8.00;
        public const int bj_MELEE_STARTING_GOLD_V0 = 750;
        public const int bj_MELEE_STARTING_GOLD_V1 = 500;
        public const int bj_MELEE_STARTING_LUMBER_V0 = 200;
        public const int bj_MELEE_STARTING_LUMBER_V1 = 150;
        public const int bj_MELEE_STARTING_HERO_TOKENS = 1;
        public const int bj_MELEE_HERO_LIMIT = 3;
        public const int bj_MELEE_HERO_TYPE_LIMIT = 1;
        public const double bj_MELEE_MINE_SEARCH_RADIUS = 2000;
        public const double bj_MELEE_CLEAR_UNITS_RADIUS = 1500;
        public const double bj_MELEE_CRIPPLE_TIMEOUT = 120.00;
        public const double bj_MELEE_CRIPPLE_MSG_DURATION = 20.00;
        public const int bj_MELEE_MAX_TWINKED_HEROES_V0 = 3;
        public const int bj_MELEE_MAX_TWINKED_HEROES_V1 = 1;

        // Delay between a creep's death and the time it may drop an item.
        public const double bj_CREEP_ITEM_DELAY = 0.50;

        // Timing settings for Marketplace inventories.
        public const double bj_STOCK_RESTOCK_INITIAL_DELAY = 120;
        public const double bj_STOCK_RESTOCK_INTERVAL = 30;
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
        public const double bj_RESCUE_PING_TIME = 2.00;

        // Transmission behavior settings
        public const double bj_NOTHING_SOUND_DURATION = 5.00;
        public const double bj_TRANSMISSION_PING_TIME = 1.00;
        public const int bj_TRANSMISSION_IND_RED = 255;
        public const int bj_TRANSMISSION_IND_BLUE = 255;
        public const int bj_TRANSMISSION_IND_GREEN = 255;
        public const int bj_TRANSMISSION_IND_ALPHA = 255;
        public const double bj_TRANSMISSION_PORT_HANGTIME = 1.50;

        // Cinematic mode settings
        public const double bj_CINEMODE_INTERFACEFADE = 0.50;

        // Cinematic mode volume levels
        public const double bj_CINEMODE_VOLUME_UNITMOVEMENT = 0.40;
        public const double bj_CINEMODE_VOLUME_UNITSOUNDS = 0.00;
        public const double bj_CINEMODE_VOLUME_COMBAT = 0.40;
        public const double bj_CINEMODE_VOLUME_SPELLS = 0.40;
        public const double bj_CINEMODE_VOLUME_UI = 0.00;
        public const double bj_CINEMODE_VOLUME_MUSIC = 0.55;
        public const double bj_CINEMODE_VOLUME_AMBIENTSOUNDS = 1.00;
        public const double bj_CINEMODE_VOLUME_FIRE = 0.60;

        // Speech mode volume levels
        public const double bj_SPEECH_VOLUME_UNITMOVEMENT = 0.25;
        public const double bj_SPEECH_VOLUME_UNITSOUNDS = 0.00;
        public const double bj_SPEECH_VOLUME_COMBAT = 0.25;
        public const double bj_SPEECH_VOLUME_SPELLS = 0.25;
        public const double bj_SPEECH_VOLUME_UI = 0.00;
        public const double bj_SPEECH_VOLUME_MUSIC = 0.55;
        public const double bj_SPEECH_VOLUME_AMBIENTSOUNDS = 1.00;
        public const double bj_SPEECH_VOLUME_FIRE = 0.60;

        // Smart pan settings
        public const double bj_SMARTPAN_TRESHOLD_PAN = 500;
        public const double bj_SMARTPAN_TRESHOLD_SNAP = 3500;

        // QueuedTriggerExecute settings
        public const int bj_MAX_QUEUED_TRIGGERS = 100;
        public const double bj_QUEUED_TRIGGER_TIMEOUT = 180.00;

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
        public const int bj_GAMECACHE_bool = 0;
        public const int bj_GAMECACHE_int = 1;
        public const int bj_GAMECACHE_double = 2;
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
        public const double bj_CORPSE_MAX_DEATH_TIME = 8.00;

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
        public static double RMinBJ(double a, double b) => a < b ? a : b;
        public static double RMaxBJ(double a, double b) => a < b ? b : a;
        public static double RAbsBJ(double a) => a >= 0 ? a : -a;
        public static double RSignBJ(double a) => a >= 0 ? 1 : -1;
        public static int IMinBJ(int a, int b) => a < b ? a : b;
        public static int IMaxBJ(int a, int b) => a < b ? b : a;
        public static int IAbsBJ(int a) => a >= 0 ? a : -a;
        public static int ISignBJ(int a) => a >= 0 ? 1 : -1;
        public static double SinBJ(double degrees) => Sin(degrees * bj_DEGTORAD);
        public static double CosBJ(double degrees) => Cos(degrees * bj_DEGTORAD);
        public static double TanBJ(double degrees) => Tan(degrees * bj_DEGTORAD);
        public static double AsinBJ(double degrees) => Asin(degrees) * bj_RADTODEG;
        public static double AcosBJ(double degrees) => Acos(degrees) * bj_RADTODEG;
        public static double AtanBJ(double degrees) => Atan(degrees) * bj_RADTODEG;
        public static double Atan2BJ(double y, double x) => Atan2(y, x) * bj_RADTODEG;
        public static double AngleBetweenPoints(location locA, location locB)
            => bj_RADTODEG * Atan2(GetLocationY(locB) - GetLocationY(locA), GetLocationX(locB) - GetLocationX(locA));
        public static double DistanceBetweenPoints(location locA, location locB)
        {
            double dx = GetLocationX(locB) - GetLocationX(locA);
            double dy = GetLocationY(locB) - GetLocationY(locA);
            return SquareRoot(dx * dx + dy * dy);
        }
        public static location PolarProjectionBJ(location source, double dist, double angle)
        {
            double x = GetLocationX(source) + dist * Cos(angle * bj_DEGTORAD);
            double y = GetLocationY(source) + dist * Sin(angle * bj_DEGTORAD);
            return Location(x, y);
        }
        public static double GetRandomDirectionDeg() => GetRandomReal(0, 360);
        public static double GetRandomPercentageBJ() => GetRandomReal(0, 100);
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
        public static double ModuloReal(double dividend, double divisor)
        {
            double modulus = dividend - I2R(R2I(dividend / divisor)) * divisor;
            // If the dividend was negative, the above modulus calculation will
            // be negative, but within (-divisor..0).  We can add (divisor) to
            // shift this result into the desired range of (0..divisor).
            return modulus < 0 ? modulus + divisor : modulus;
        }

        public static location OffsetLocation(location loc, double dx, double dy)
            => Location(GetLocationX(loc) + dx, GetLocationY(loc) + dy);
        public static rect OffsetRectBJ(rect r, double dx, double dy)
            => Rect(GetRectMinX(r) + dx, GetRectMinY(r) + dy, GetRectMaxX(r) + dx, GetRectMaxY(r) + dy);
        public static rect RectFromCenterSizeBJ(location center, double width, double height)
        {
            double x = GetLocationX(center);
            double y = GetLocationY(center);
            return Rect(x - width * 0.5, y - height * 0.5, x + width * 0.5, y + height * 0.5);
        }
        public static bool RectContainsCoords(rect r, double x, double y)
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
        public static int PercentToInt(double percentage, int max)
        {
            int result = R2I(percentage * I2R(max) * 0.01);
            if (result < 0) return 0;
            else if (result > max) return max;
            return result;
        }
        public static int PercentTo255(double percentage)
            => PercentToInt(percentage, 255);
        public static bool CompareLocationsBJ(location A, location B)
            => GetLocationX(A) == GetLocationX(B) && GetLocationY(A) == GetLocationY(B);
        public static bool CompareRectsBJ(rect A, rect B)
            => GetRectMinX(A) == GetRectMinX(B) && GetRectMinY(A) == GetRectMinY(B) && GetRectMaxX(A) == GetRectMaxX(B) && GetRectMaxY(A) == GetRectMaxY(B);
        public static rect GetRectFromCircleBJ(location center, double radius)
        {
            double centerX = GetLocationX(center);
            double centerY = GetLocationY(center);
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
        public static double TextTagSize2Height(double size) => size * 0.023 / 10;

        //===========================================================================
        // Scale the speed linearly such that speed 128 equates to 0.071.
        // Screen-relative speeds are hard to grasp.
        public static double TextTagSpeed2Velocity(double speed) => speed * 0.071 / 128;
        #endregion

        #region [    Rescuable Unit Utility Functions    ]
        public static Hashtable InitHashtableBJ() => bj_lastCreatedHashtable = InitHashtable();
        public static Hashtable GetLastCreatedHashtableBJ() => bj_lastCreatedHashtable;
        public static void SaveRealBJ(double value, int key, int missionKey, Hashtable table)
            => SaveReal(table, missionKey, key, value);
        public static void SaveIntegerBJ(int value, int key, int missionKey, Hashtable table)
            => SaveInteger(table, missionKey, key, value);
        public static void SaveBooleanBJ(bool value, int key, int missionKey, Hashtable table)
            => SaveBoolean(table, missionKey, key, value);
        public static void SaveStringBJ(string value, int key, int missionKey, Hashtable table)
            => SaveStr(table, missionKey, key, value);
        public static double LoadRealBJ(int key, int missionKey, Hashtable table)
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
        public static int GetFadeFromSeconds(double seconds)
            => seconds == 0 ? 10000 : 128 / R2I(seconds);
        public static double GetFadeFromSecondsAsReal(double seconds)
            => seconds == 0 ? 10000 : 128 / seconds;
        public static double GetRectWidthBJ(rect r) => GetRectMaxX(r) - GetRectMinX(r);
        public static double GetRectHeightBJ(rect r) => GetRectMaxY(r) - GetRectMinY(r);
        #endregion

        #endregion
    }

    public class rect
    {
        public double minx, miny, maxx, maxy;
    }

    public class location
    {
        public double x, y;
    }
}