using CirnoLib.MPQ.Struct;

namespace CirnoLib.MPQ
{
    public static class Constant
    {
        /// <summary>
        /// 워크래프트 3 맵임을 알리는 고유값입니다.
        /// </summary>
        public const uint W3M_HEADER_SIGNATURE = 0x57334D48;   // "HM3W"

        /// <summary>
        /// <see cref="MPQHeader"/>의 고유값 입니다.
        /// </summary>
        public const uint MPQ_HEADER_SIGNATURE = 0x1A51504D;    // "MPQ\x1A"

        /// <summary>
        /// <see cref="MPQHash"/>의 고유 암호값입니다.
        /// </summary>
        public const uint MPQ_HASH_KEY = 0xC3AF3770;    // 70 37 AF C3

        /// <summary>
        /// <see cref="MPQBlock"/>의 고유 암호값입니다.
        /// </summary>
        public const uint MPQ_BLOCK_KEY = 0xEC83B3A3;   // A3 B3 83 EC

        /// <summary>
        /// <see cref="W3MHeader.Flags"/>에 사용되는 속성에 대한 값입니다.
        /// </summary>
        public const int
            W3M_HIDE_MINIMAP_IN_PREVIEW_SCREENS                                  =    0x1,
            W3M_MODIFY_ALLY_PRIORITIES                                           =    0x2,
            W3M_MELEE_MAP                                                        =    0x4,
            W3M_PLAYABLE_MAP_SIZE_WAS_LARGE_AND_HAS_NEVER_BEEN_REDUCED_TO_MEDIUM =    0x8,
            W3M_MASKED_AREA_ARE_PARTIALLY_VISIBLE                                =   0x10,
            W3M_FIXED_PLAYER_SETTING_FOR_CUSTOM_FORCES                           =   0x20,
            W3M_USE_CUSTOM_FORCES                                                =   0x40,
            W3M_USE_CUSTOM_TECHTREE                                              =   0x80,
            W3M_USE_CUSTOM_ABILITIES                                             =  0x100,
            W3M_USE_CUSTOM_UPGRADES                                              =  0x200,
            W3M_MAP_PROPERTIES_MENU_OPENED_AT_LEAST_ONCE_SINCE_MAP_CREATION      =  0x400,
            W3M_SHOW_WATER_WAVES_ON_CLIFF_SHORES                                 =  0x800,
            W3M_SHOW_WATER_WAVES_ON_ROLLING_SHORES                               = 0x1000;
        /// <summary>
        /// <see cref="MPQBlock.Flags"/>에 사용되는 속성에 대한 값입니다.
        /// </summary>
        public const uint
            MPQ_FILE_IMPLODE       =      0x100,
            MPQ_FILE_COMPRESS      =      0x200,
            MPQ_FILE_ENCRYPTED     =    0x10000,
            MPQ_FILE_FIX_KEY       =    0x20000,
            MPQ_FILE_PATCH_FILE    =   0x100000,
            MPQ_FILE_SINGLE_UNIT   =  0x1000000,
            MPQ_FILE_DELETE_MARKER =  0x2000000,
            MPQ_FILE_SECTOR_CRC    =  0x4000000,
            MPQ_FILE_EXISTS        = 0x80000000;

        /// <summary>
        /// <see cref="MPQHash.Locale"/>에 사용되는 값입니다.
        /// </summary>
        public const ushort
            MPQ_LOCALE_NEUTRAL    = 0x000,
            MPQ_LOCALE_CHINESE    = 0x404,
            MPQ_LOCALE_CZECH      = 0x405,
            MPQ_LOCALE_GERMAN     = 0x407,
            MPQ_LOCALE_ENGLISH    = 0x409,
            MPQ_LOCALE_SPANISH    = 0x40A,
            MPQ_LOCALE_FRENCH     = 0x40C,
            MPQ_LOCALE_ITALIAN    = 0x410,
            MPQ_LOCALE_JAPANESE   = 0x411,
            MPQ_LOCALE_KOREAN     = 0x412,
            MPQ_LOCALE_POLISH     = 0x415,
            MPQ_LOCALE_PORTUGUESE = 0x416,
            MPQ_LOCALE_RUSSIAN    = 0x419,
            MPQ_LOCALE_UKENGISH   = 0x809;
    }
}
