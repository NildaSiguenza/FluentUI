using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.IconFonts;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// Mynaui 图标枚举
    /// (共 1272 个可用图标)
    /// </summary>
    public enum MynauiIconChar
    {
        /// <summary>
        /// mynaui-a-arrow-down
        /// Unicode: U+ea01
        /// </summary>
        MynauiAArrowDown = 0xEA01,

        /// <summary>
        /// mynaui-a-arrow-up
        /// Unicode: U+ea02
        /// </summary>
        MynauiAArrowUp = 0xEA02,

        /// <summary>
        /// mynaui-academic-hat
        /// Unicode: U+ea03
        /// </summary>
        MynauiAcademicHat = 0xEA03,

        /// <summary>
        /// mynaui-accessibility
        /// Unicode: U+ea04
        /// </summary>
        MynauiAccessibility = 0xEA04,

        /// <summary>
        /// mynaui-activity
        /// Unicode: U+ea06
        /// </summary>
        MynauiActivity = 0xEA06,

        /// <summary>
        /// mynaui-activity-square
        /// Unicode: U+ea05
        /// </summary>
        MynauiActivitySquare = 0xEA05,

        /// <summary>
        /// mynaui-add-queue
        /// Unicode: U+ea07
        /// </summary>
        MynauiAddQueue = 0xEA07,

        /// <summary>
        /// mynaui-aeroplane
        /// Unicode: U+ea08
        /// </summary>
        MynauiAeroplane = 0xEA08,

        /// <summary>
        /// mynaui-air-conditioner
        /// Unicode: U+ea09
        /// </summary>
        MynauiAirConditioner = 0xEA09,

        /// <summary>
        /// mynaui-airplay
        /// Unicode: U+ea0a
        /// </summary>
        MynauiAirplay = 0xEA0A,

        /// <summary>
        /// mynaui-airpods
        /// Unicode: U+ea0b
        /// </summary>
        MynauiAirpods = 0xEA0B,

        /// <summary>
        /// mynaui-alarm
        /// Unicode: U+ea12
        /// </summary>
        MynauiAlarm = 0xEA12,

        /// <summary>
        /// mynaui-alarm-check
        /// Unicode: U+ea0c
        /// </summary>
        MynauiAlarmCheck = 0xEA0C,

        /// <summary>
        /// mynaui-alarm-minus
        /// Unicode: U+ea0d
        /// </summary>
        MynauiAlarmMinus = 0xEA0D,

        /// <summary>
        /// mynaui-alarm-plus
        /// Unicode: U+ea0e
        /// </summary>
        MynauiAlarmPlus = 0xEA0E,

        /// <summary>
        /// mynaui-alarm-smoke
        /// Unicode: U+ea0f
        /// </summary>
        MynauiAlarmSmoke = 0xEA0F,

        /// <summary>
        /// mynaui-alarm-snooze
        /// Unicode: U+ea10
        /// </summary>
        MynauiAlarmSnooze = 0xEA10,

        /// <summary>
        /// mynaui-alarm-x
        /// Unicode: U+ea11
        /// </summary>
        MynauiAlarmX = 0xEA11,

        /// <summary>
        /// mynaui-album
        /// Unicode: U+ea13
        /// </summary>
        MynauiAlbum = 0xEA13,

        /// <summary>
        /// mynaui-align-bottom
        /// Unicode: U+ea14
        /// </summary>
        MynauiAlignBottom = 0xEA14,

        /// <summary>
        /// mynaui-align-horizontal
        /// Unicode: U+ea15
        /// </summary>
        MynauiAlignHorizontal = 0xEA15,

        /// <summary>
        /// mynaui-align-left
        /// Unicode: U+ea16
        /// </summary>
        MynauiAlignLeft = 0xEA16,

        /// <summary>
        /// mynaui-align-right
        /// Unicode: U+ea17
        /// </summary>
        MynauiAlignRight = 0xEA17,

        /// <summary>
        /// mynaui-align-top
        /// Unicode: U+ea18
        /// </summary>
        MynauiAlignTop = 0xEA18,

        /// <summary>
        /// mynaui-align-vertical
        /// Unicode: U+ea19
        /// </summary>
        MynauiAlignVertical = 0xEA19,

        /// <summary>
        /// mynaui-alt
        /// Unicode: U+ea1a
        /// </summary>
        MynauiAlt = 0xEA1A,

        /// <summary>
        /// mynaui-ambulance
        /// Unicode: U+ea1b
        /// </summary>
        MynauiAmbulance = 0xEA1B,

        /// <summary>
        /// mynaui-ampersand
        /// Unicode: U+ea1d
        /// </summary>
        MynauiAmpersand = 0xEA1D,

        /// <summary>
        /// mynaui-ampersands
        /// Unicode: U+ea1e
        /// </summary>
        MynauiAmpersands = 0xEA1E,

        /// <summary>
        /// mynaui-ampersand-square
        /// Unicode: U+ea1c
        /// </summary>
        MynauiAmpersandSquare = 0xEA1C,

        /// <summary>
        /// mynaui-anchor
        /// Unicode: U+ea1f
        /// </summary>
        MynauiAnchor = 0xEA1F,

        /// <summary>
        /// mynaui-angry-circle
        /// Unicode: U+ea20
        /// </summary>
        MynauiAngryCircle = 0xEA20,

        /// <summary>
        /// mynaui-angry-ghost
        /// Unicode: U+ea21
        /// </summary>
        MynauiAngryGhost = 0xEA21,

        /// <summary>
        /// mynaui-angry-square
        /// Unicode: U+ea22
        /// </summary>
        MynauiAngrySquare = 0xEA22,

        /// <summary>
        /// mynaui-annoyed-circle
        /// Unicode: U+ea23
        /// </summary>
        MynauiAnnoyedCircle = 0xEA23,

        /// <summary>
        /// mynaui-annoyed-ghost
        /// Unicode: U+ea24
        /// </summary>
        MynauiAnnoyedGhost = 0xEA24,

        /// <summary>
        /// mynaui-annoyed-square
        /// Unicode: U+ea25
        /// </summary>
        MynauiAnnoyedSquare = 0xEA25,

        /// <summary>
        /// mynaui-aperture
        /// Unicode: U+ea26
        /// </summary>
        MynauiAperture = 0xEA26,

        /// <summary>
        /// mynaui-api
        /// Unicode: U+ea27
        /// </summary>
        MynauiApi = 0xEA27,

        /// <summary>
        /// mynaui-ar
        /// Unicode: U+ea28
        /// </summary>
        MynauiAr = 0xEA28,

        /// <summary>
        /// mynaui-archive
        /// Unicode: U+ea29
        /// </summary>
        MynauiArchive = 0xEA29,

        /// <summary>
        /// mynaui-arrow-diagonal-one
        /// Unicode: U+ea2a
        /// </summary>
        MynauiArrowDiagonalOne = 0xEA2A,

        /// <summary>
        /// mynaui-arrow-diagonal-two
        /// Unicode: U+ea2b
        /// </summary>
        MynauiArrowDiagonalTwo = 0xEA2B,

        /// <summary>
        /// mynaui-arrow-down
        /// Unicode: U+ea37
        /// </summary>
        MynauiArrowDown = 0xEA37,

        /// <summary>
        /// mynaui-arrow-down-circle
        /// Unicode: U+ea2c
        /// </summary>
        MynauiArrowDownCircle = 0xEA2C,

        /// <summary>
        /// mynaui-arrow-down-left
        /// Unicode: U+ea30
        /// </summary>
        MynauiArrowDownLeft = 0xEA30,

        /// <summary>
        /// mynaui-arrow-down-left-circle
        /// Unicode: U+ea2d
        /// </summary>
        MynauiArrowDownLeftCircle = 0xEA2D,

        /// <summary>
        /// mynaui-arrow-down-left-square
        /// Unicode: U+ea2e
        /// </summary>
        MynauiArrowDownLeftSquare = 0xEA2E,

        /// <summary>
        /// mynaui-arrow-down-left-waves
        /// Unicode: U+ea2f
        /// </summary>
        MynauiArrowDownLeftWaves = 0xEA2F,

        /// <summary>
        /// mynaui-arrow-down-right
        /// Unicode: U+ea34
        /// </summary>
        MynauiArrowDownRight = 0xEA34,

        /// <summary>
        /// mynaui-arrow-down-right-circle
        /// Unicode: U+ea31
        /// </summary>
        MynauiArrowDownRightCircle = 0xEA31,

        /// <summary>
        /// mynaui-arrow-down-right-square
        /// Unicode: U+ea32
        /// </summary>
        MynauiArrowDownRightSquare = 0xEA32,

        /// <summary>
        /// mynaui-arrow-down-right-waves
        /// Unicode: U+ea33
        /// </summary>
        MynauiArrowDownRightWaves = 0xEA33,

        /// <summary>
        /// mynaui-arrow-down-square
        /// Unicode: U+ea35
        /// </summary>
        MynauiArrowDownSquare = 0xEA35,

        /// <summary>
        /// mynaui-arrow-down-waves
        /// Unicode: U+ea36
        /// </summary>
        MynauiArrowDownWaves = 0xEA36,

        /// <summary>
        /// mynaui-arrow-left
        /// Unicode: U+ea3c
        /// </summary>
        MynauiArrowLeft = 0xEA3C,

        /// <summary>
        /// mynaui-arrow-left-circle
        /// Unicode: U+ea38
        /// </summary>
        MynauiArrowLeftCircle = 0xEA38,

        /// <summary>
        /// mynaui-arrow-left-right
        /// Unicode: U+ea39
        /// </summary>
        MynauiArrowLeftRight = 0xEA39,

        /// <summary>
        /// mynaui-arrow-left-square
        /// Unicode: U+ea3a
        /// </summary>
        MynauiArrowLeftSquare = 0xEA3A,

        /// <summary>
        /// mynaui-arrow-left-waves
        /// Unicode: U+ea3b
        /// </summary>
        MynauiArrowLeftWaves = 0xEA3B,

        /// <summary>
        /// mynaui-arrow-long-down
        /// Unicode: U+ea3f
        /// </summary>
        MynauiArrowLongDown = 0xEA3F,

        /// <summary>
        /// mynaui-arrow-long-down-left
        /// Unicode: U+ea3d
        /// </summary>
        MynauiArrowLongDownLeft = 0xEA3D,

        /// <summary>
        /// mynaui-arrow-long-down-right
        /// Unicode: U+ea3e
        /// </summary>
        MynauiArrowLongDownRight = 0xEA3E,

        /// <summary>
        /// mynaui-arrow-long-left
        /// Unicode: U+ea40
        /// </summary>
        MynauiArrowLongLeft = 0xEA40,

        /// <summary>
        /// mynaui-arrow-long-right
        /// Unicode: U+ea41
        /// </summary>
        MynauiArrowLongRight = 0xEA41,

        /// <summary>
        /// mynaui-arrow-long-up
        /// Unicode: U+ea44
        /// </summary>
        MynauiArrowLongUp = 0xEA44,

        /// <summary>
        /// mynaui-arrow-long-up-left
        /// Unicode: U+ea42
        /// </summary>
        MynauiArrowLongUpLeft = 0xEA42,

        /// <summary>
        /// mynaui-arrow-long-up-right
        /// Unicode: U+ea43
        /// </summary>
        MynauiArrowLongUpRight = 0xEA43,

        /// <summary>
        /// mynaui-arrow-right
        /// Unicode: U+ea48
        /// </summary>
        MynauiArrowRight = 0xEA48,

        /// <summary>
        /// mynaui-arrow-right-circle
        /// Unicode: U+ea45
        /// </summary>
        MynauiArrowRightCircle = 0xEA45,

        /// <summary>
        /// mynaui-arrow-right-square
        /// Unicode: U+ea46
        /// </summary>
        MynauiArrowRightSquare = 0xEA46,

        /// <summary>
        /// mynaui-arrow-right-waves
        /// Unicode: U+ea47
        /// </summary>
        MynauiArrowRightWaves = 0xEA47,

        /// <summary>
        /// mynaui-arrow-up
        /// Unicode: U+ea55
        /// </summary>
        MynauiArrowUp = 0xEA55,

        /// <summary>
        /// mynaui-arrow-up-circle
        /// Unicode: U+ea49
        /// </summary>
        MynauiArrowUpCircle = 0xEA49,

        /// <summary>
        /// mynaui-arrow-up-down
        /// Unicode: U+ea4a
        /// </summary>
        MynauiArrowUpDown = 0xEA4A,

        /// <summary>
        /// mynaui-arrow-up-left
        /// Unicode: U+ea4e
        /// </summary>
        MynauiArrowUpLeft = 0xEA4E,

        /// <summary>
        /// mynaui-arrow-up-left-circle
        /// Unicode: U+ea4b
        /// </summary>
        MynauiArrowUpLeftCircle = 0xEA4B,

        /// <summary>
        /// mynaui-arrow-up-left-square
        /// Unicode: U+ea4c
        /// </summary>
        MynauiArrowUpLeftSquare = 0xEA4C,

        /// <summary>
        /// mynaui-arrow-up-left-waves
        /// Unicode: U+ea4d
        /// </summary>
        MynauiArrowUpLeftWaves = 0xEA4D,

        /// <summary>
        /// mynaui-arrow-up-right
        /// Unicode: U+ea52
        /// </summary>
        MynauiArrowUpRight = 0xEA52,

        /// <summary>
        /// mynaui-arrow-up-right-circle
        /// Unicode: U+ea4f
        /// </summary>
        MynauiArrowUpRightCircle = 0xEA4F,

        /// <summary>
        /// mynaui-arrow-up-right-square
        /// Unicode: U+ea50
        /// </summary>
        MynauiArrowUpRightSquare = 0xEA50,

        /// <summary>
        /// mynaui-arrow-up-right-waves
        /// Unicode: U+ea51
        /// </summary>
        MynauiArrowUpRightWaves = 0xEA51,

        /// <summary>
        /// mynaui-arrow-up-square
        /// Unicode: U+ea53
        /// </summary>
        MynauiArrowUpSquare = 0xEA53,

        /// <summary>
        /// mynaui-arrow-up-waves
        /// Unicode: U+ea54
        /// </summary>
        MynauiArrowUpWaves = 0xEA54,

        /// <summary>
        /// mynaui-asterisk-circle
        /// Unicode: U+ea56
        /// </summary>
        MynauiAsteriskCircle = 0xEA56,

        /// <summary>
        /// mynaui-asterisk-diamond
        /// Unicode: U+ea57
        /// </summary>
        MynauiAsteriskDiamond = 0xEA57,

        /// <summary>
        /// mynaui-asterisk-hexagon
        /// Unicode: U+ea58
        /// </summary>
        MynauiAsteriskHexagon = 0xEA58,

        /// <summary>
        /// mynaui-asterisk-octagon
        /// Unicode: U+ea59
        /// </summary>
        MynauiAsteriskOctagon = 0xEA59,

        /// <summary>
        /// mynaui-asterisk-square
        /// Unicode: U+ea5a
        /// </summary>
        MynauiAsteriskSquare = 0xEA5A,

        /// <summary>
        /// mynaui-asterisk-waves
        /// Unicode: U+ea5b
        /// </summary>
        MynauiAsteriskWaves = 0xEA5B,

        /// <summary>
        /// mynaui-at
        /// Unicode: U+ea5c
        /// </summary>
        MynauiAt = 0xEA5C,

        /// <summary>
        /// mynaui-atom
        /// Unicode: U+ea5d
        /// </summary>
        MynauiAtom = 0xEA5D,

        /// <summary>
        /// mynaui-baby
        /// Unicode: U+ea5e
        /// </summary>
        MynauiBaby = 0xEA5E,

        /// <summary>
        /// mynaui-backpack
        /// Unicode: U+ea5f
        /// </summary>
        MynauiBackpack = 0xEA5F,

        /// <summary>
        /// mynaui-badge
        /// Unicode: U+ea60
        /// </summary>
        MynauiBadge = 0xEA60,

        /// <summary>
        /// mynaui-baggage-claim
        /// Unicode: U+ea61
        /// </summary>
        MynauiBaggageClaim = 0xEA61,

        /// <summary>
        /// mynaui-ban
        /// Unicode: U+ea62
        /// </summary>
        MynauiBan = 0xEA62,

        /// <summary>
        /// mynaui-bank
        /// Unicode: U+ea63
        /// </summary>
        MynauiBank = 0xEA63,

        /// <summary>
        /// mynaui-baseball
        /// Unicode: U+ea64
        /// </summary>
        MynauiBaseball = 0xEA64,

        /// <summary>
        /// mynaui-bath
        /// Unicode: U+ea65
        /// </summary>
        MynauiBath = 0xEA65,

        /// <summary>
        /// mynaui-battery-charging
        /// Unicode: U+ea6a
        /// </summary>
        MynauiBatteryCharging = 0xEA6A,

        /// <summary>
        /// mynaui-battery-charging-four
        /// Unicode: U+ea66
        /// </summary>
        MynauiBatteryChargingFour = 0xEA66,

        /// <summary>
        /// mynaui-battery-charging-one
        /// Unicode: U+ea67
        /// </summary>
        MynauiBatteryChargingOne = 0xEA67,

        /// <summary>
        /// mynaui-battery-charging-three
        /// Unicode: U+ea68
        /// </summary>
        MynauiBatteryChargingThree = 0xEA68,

        /// <summary>
        /// mynaui-battery-charging-two
        /// Unicode: U+ea69
        /// </summary>
        MynauiBatteryChargingTwo = 0xEA69,

        /// <summary>
        /// mynaui-battery-check
        /// Unicode: U+ea6b
        /// </summary>
        MynauiBatteryCheck = 0xEA6B,

        /// <summary>
        /// mynaui-battery-empty
        /// Unicode: U+ea6c
        /// </summary>
        MynauiBatteryEmpty = 0xEA6C,

        /// <summary>
        /// mynaui-battery-full
        /// Unicode: U+ea6d
        /// </summary>
        MynauiBatteryFull = 0xEA6D,

        /// <summary>
        /// mynaui-battery-minus
        /// Unicode: U+ea6e
        /// </summary>
        MynauiBatteryMinus = 0xEA6E,

        /// <summary>
        /// mynaui-battery-plus
        /// Unicode: U+ea6f
        /// </summary>
        MynauiBatteryPlus = 0xEA6F,

        /// <summary>
        /// mynaui-battery-x
        /// Unicode: U+ea70
        /// </summary>
        MynauiBatteryX = 0xEA70,

        /// <summary>
        /// mynaui-bell
        /// Unicode: U+ea7a
        /// </summary>
        MynauiBell = 0xEA7A,

        /// <summary>
        /// mynaui-bell-check
        /// Unicode: U+ea71
        /// </summary>
        MynauiBellCheck = 0xEA71,

        /// <summary>
        /// mynaui-bell-home
        /// Unicode: U+ea72
        /// </summary>
        MynauiBellHome = 0xEA72,

        /// <summary>
        /// mynaui-bell-minus
        /// Unicode: U+ea73
        /// </summary>
        MynauiBellMinus = 0xEA73,

        /// <summary>
        /// mynaui-bell-on
        /// Unicode: U+ea74
        /// </summary>
        MynauiBellOn = 0xEA74,

        /// <summary>
        /// mynaui-bell-plus
        /// Unicode: U+ea75
        /// </summary>
        MynauiBellPlus = 0xEA75,

        /// <summary>
        /// mynaui-bell-slash
        /// Unicode: U+ea76
        /// </summary>
        MynauiBellSlash = 0xEA76,

        /// <summary>
        /// mynaui-bell-snooze
        /// Unicode: U+ea77
        /// </summary>
        MynauiBellSnooze = 0xEA77,

        /// <summary>
        /// mynaui-bell-user
        /// Unicode: U+ea78
        /// </summary>
        MynauiBellUser = 0xEA78,

        /// <summary>
        /// mynaui-bell-x
        /// Unicode: U+ea79
        /// </summary>
        MynauiBellX = 0xEA79,

        /// <summary>
        /// mynaui-binoculars
        /// Unicode: U+ea7b
        /// </summary>
        MynauiBinoculars = 0xEA7B,

        /// <summary>
        /// mynaui-bitcoin
        /// Unicode: U+ea82
        /// </summary>
        MynauiBitcoin = 0xEA82,

        /// <summary>
        /// mynaui-bitcoin-circle
        /// Unicode: U+ea7c
        /// </summary>
        MynauiBitcoinCircle = 0xEA7C,

        /// <summary>
        /// mynaui-bitcoin-diamond
        /// Unicode: U+ea7d
        /// </summary>
        MynauiBitcoinDiamond = 0xEA7D,

        /// <summary>
        /// mynaui-bitcoin-hexagon
        /// Unicode: U+ea7e
        /// </summary>
        MynauiBitcoinHexagon = 0xEA7E,

        /// <summary>
        /// mynaui-bitcoin-octagon
        /// Unicode: U+ea7f
        /// </summary>
        MynauiBitcoinOctagon = 0xEA7F,

        /// <summary>
        /// mynaui-bitcoin-square
        /// Unicode: U+ea80
        /// </summary>
        MynauiBitcoinSquare = 0xEA80,

        /// <summary>
        /// mynaui-bitcoin-waves
        /// Unicode: U+ea81
        /// </summary>
        MynauiBitcoinWaves = 0xEA81,

        /// <summary>
        /// mynaui-bluetooth
        /// Unicode: U+ea83
        /// </summary>
        MynauiBluetooth = 0xEA83,

        /// <summary>
        /// mynaui-boat
        /// Unicode: U+ea84
        /// </summary>
        MynauiBoat = 0xEA84,

        /// <summary>
        /// mynaui-book
        /// Unicode: U+ea90
        /// </summary>
        MynauiBook = 0xEA90,

        /// <summary>
        /// mynaui-book-check
        /// Unicode: U+ea85
        /// </summary>
        MynauiBookCheck = 0xEA85,

        /// <summary>
        /// mynaui-book-dot
        /// Unicode: U+ea86
        /// </summary>
        MynauiBookDot = 0xEA86,

        /// <summary>
        /// mynaui-book-home
        /// Unicode: U+ea87
        /// </summary>
        MynauiBookHome = 0xEA87,

        /// <summary>
        /// mynaui-book-image
        /// Unicode: U+ea88
        /// </summary>
        MynauiBookImage = 0xEA88,

        /// <summary>
        /// mynaui-bookmark
        /// Unicode: U+ea9a
        /// </summary>
        MynauiBookmark = 0xEA9A,

        /// <summary>
        /// mynaui-bookmark-check
        /// Unicode: U+ea91
        /// </summary>
        MynauiBookmarkCheck = 0xEA91,

        /// <summary>
        /// mynaui-bookmark-dot
        /// Unicode: U+ea92
        /// </summary>
        MynauiBookmarkDot = 0xEA92,

        /// <summary>
        /// mynaui-bookmark-home
        /// Unicode: U+ea93
        /// </summary>
        MynauiBookmarkHome = 0xEA93,

        /// <summary>
        /// mynaui-bookmark-minus
        /// Unicode: U+ea94
        /// </summary>
        MynauiBookmarkMinus = 0xEA94,

        /// <summary>
        /// mynaui-bookmark-plus
        /// Unicode: U+ea95
        /// </summary>
        MynauiBookmarkPlus = 0xEA95,

        /// <summary>
        /// mynaui-bookmark-slash
        /// Unicode: U+ea96
        /// </summary>
        MynauiBookmarkSlash = 0xEA96,

        /// <summary>
        /// mynaui-bookmark-snooze
        /// Unicode: U+ea97
        /// </summary>
        MynauiBookmarkSnooze = 0xEA97,

        /// <summary>
        /// mynaui-bookmark-user
        /// Unicode: U+ea98
        /// </summary>
        MynauiBookmarkUser = 0xEA98,

        /// <summary>
        /// mynaui-bookmark-x
        /// Unicode: U+ea99
        /// </summary>
        MynauiBookmarkX = 0xEA99,

        /// <summary>
        /// mynaui-book-minus
        /// Unicode: U+ea89
        /// </summary>
        MynauiBookMinus = 0xEA89,

        /// <summary>
        /// mynaui-book-open
        /// Unicode: U+ea8a
        /// </summary>
        MynauiBookOpen = 0xEA8A,

        /// <summary>
        /// mynaui-book-plus
        /// Unicode: U+ea8b
        /// </summary>
        MynauiBookPlus = 0xEA8B,

        /// <summary>
        /// mynaui-book-slash
        /// Unicode: U+ea8c
        /// </summary>
        MynauiBookSlash = 0xEA8C,

        /// <summary>
        /// mynaui-book-snooze
        /// Unicode: U+ea8d
        /// </summary>
        MynauiBookSnooze = 0xEA8D,

        /// <summary>
        /// mynaui-book-user
        /// Unicode: U+ea8e
        /// </summary>
        MynauiBookUser = 0xEA8E,

        /// <summary>
        /// mynaui-book-x
        /// Unicode: U+ea8f
        /// </summary>
        MynauiBookX = 0xEA8F,

        /// <summary>
        /// mynaui-bounding-box
        /// Unicode: U+ea9b
        /// </summary>
        MynauiBoundingBox = 0xEA9B,

        /// <summary>
        /// mynaui-bowl
        /// Unicode: U+ea9c
        /// </summary>
        MynauiBowl = 0xEA9C,

        /// <summary>
        /// mynaui-box
        /// Unicode: U+ea9d
        /// </summary>
        MynauiBox = 0xEA9D,

        /// <summary>
        /// mynaui-brand-chrome
        /// Unicode: U+ea9e
        /// </summary>
        MynauiBrandChrome = 0xEA9E,

        /// <summary>
        /// mynaui-brand-codepen
        /// Unicode: U+ea9f
        /// </summary>
        MynauiBrandCodepen = 0xEA9F,

        /// <summary>
        /// mynaui-brand-codesandbox
        /// Unicode: U+eaa0
        /// </summary>
        MynauiBrandCodesandbox = 0xEAA0,

        /// <summary>
        /// mynaui-brand-dribbble
        /// Unicode: U+eaa1
        /// </summary>
        MynauiBrandDribbble = 0xEAA1,

        /// <summary>
        /// mynaui-brand-facebook
        /// Unicode: U+eaa2
        /// </summary>
        MynauiBrandFacebook = 0xEAA2,

        /// <summary>
        /// mynaui-brand-figma
        /// Unicode: U+eaa3
        /// </summary>
        MynauiBrandFigma = 0xEAA3,

        /// <summary>
        /// mynaui-brand-framer
        /// Unicode: U+eaa4
        /// </summary>
        MynauiBrandFramer = 0xEAA4,

        /// <summary>
        /// mynaui-brand-github
        /// Unicode: U+eaa5
        /// </summary>
        MynauiBrandGithub = 0xEAA5,

        /// <summary>
        /// mynaui-brand-gitlab
        /// Unicode: U+eaa6
        /// </summary>
        MynauiBrandGitlab = 0xEAA6,

        /// <summary>
        /// mynaui-brand-google
        /// Unicode: U+eaa7
        /// </summary>
        MynauiBrandGoogle = 0xEAA7,

        /// <summary>
        /// mynaui-brand-instagram
        /// Unicode: U+eaa8
        /// </summary>
        MynauiBrandInstagram = 0xEAA8,

        /// <summary>
        /// mynaui-brand-linkedin
        /// Unicode: U+eaa9
        /// </summary>
        MynauiBrandLinkedin = 0xEAA9,

        /// <summary>
        /// mynaui-brand-pinterest
        /// Unicode: U+eaaa
        /// </summary>
        MynauiBrandPinterest = 0xEAAA,

        /// <summary>
        /// mynaui-brand-pocket
        /// Unicode: U+eaab
        /// </summary>
        MynauiBrandPocket = 0xEAAB,

        /// <summary>
        /// mynaui-brand-slack
        /// Unicode: U+eaac
        /// </summary>
        MynauiBrandSlack = 0xEAAC,

        /// <summary>
        /// mynaui-brand-spotify
        /// Unicode: U+eaad
        /// </summary>
        MynauiBrandSpotify = 0xEAAD,

        /// <summary>
        /// mynaui-brand-telegram
        /// Unicode: U+eaae
        /// </summary>
        MynauiBrandTelegram = 0xEAAE,

        /// <summary>
        /// mynaui-brand-threads
        /// Unicode: U+eaaf
        /// </summary>
        MynauiBrandThreads = 0xEAAF,

        /// <summary>
        /// mynaui-brand-trello
        /// Unicode: U+eab0
        /// </summary>
        MynauiBrandTrello = 0xEAB0,

        /// <summary>
        /// mynaui-brand-twitch
        /// Unicode: U+eab1
        /// </summary>
        MynauiBrandTwitch = 0xEAB1,

        /// <summary>
        /// mynaui-brand-twitter
        /// Unicode: U+eab2
        /// </summary>
        MynauiBrandTwitter = 0xEAB2,

        /// <summary>
        /// mynaui-brand-x
        /// Unicode: U+eab3
        /// </summary>
        MynauiBrandX = 0xEAB3,

        /// <summary>
        /// mynaui-brand-youtube
        /// Unicode: U+eab4
        /// </summary>
        MynauiBrandYoutube = 0xEAB4,

        /// <summary>
        /// mynaui-briefcase
        /// Unicode: U+eab6
        /// </summary>
        MynauiBriefcase = 0xEAB6,

        /// <summary>
        /// mynaui-briefcase-conveyor-belt
        /// Unicode: U+eab5
        /// </summary>
        MynauiBriefcaseConveyorBelt = 0xEAB5,

        /// <summary>
        /// mynaui-brightness-high
        /// Unicode: U+eab7
        /// </summary>
        MynauiBrightnessHigh = 0xEAB7,

        /// <summary>
        /// mynaui-brightness-low
        /// Unicode: U+eab8
        /// </summary>
        MynauiBrightnessLow = 0xEAB8,

        /// <summary>
        /// mynaui-bubbles
        /// Unicode: U+eab9
        /// </summary>
        MynauiBubbles = 0xEAB9,

        /// <summary>
        /// mynaui-building
        /// Unicode: U+eabb
        /// </summary>
        MynauiBuilding = 0xEABB,

        /// <summary>
        /// mynaui-building-one
        /// Unicode: U+eaba
        /// </summary>
        MynauiBuildingOne = 0xEABA,

        /// <summary>
        /// mynaui-cable-car
        /// Unicode: U+eabc
        /// </summary>
        MynauiCableCar = 0xEABC,

        /// <summary>
        /// mynaui-cake
        /// Unicode: U+eabd
        /// </summary>
        MynauiCake = 0xEABD,

        /// <summary>
        /// mynaui-calendar
        /// Unicode: U+eac5
        /// </summary>
        MynauiCalendar = 0xEAC5,

        /// <summary>
        /// mynaui-calendar-check
        /// Unicode: U+eabe
        /// </summary>
        MynauiCalendarCheck = 0xEABE,

        /// <summary>
        /// mynaui-calendar-down
        /// Unicode: U+eabf
        /// </summary>
        MynauiCalendarDown = 0xEABF,

        /// <summary>
        /// mynaui-calendar-minus
        /// Unicode: U+eac0
        /// </summary>
        MynauiCalendarMinus = 0xEAC0,

        /// <summary>
        /// mynaui-calendar-plus
        /// Unicode: U+eac1
        /// </summary>
        MynauiCalendarPlus = 0xEAC1,

        /// <summary>
        /// mynaui-calendar-slash
        /// Unicode: U+eac2
        /// </summary>
        MynauiCalendarSlash = 0xEAC2,

        /// <summary>
        /// mynaui-calendar-up
        /// Unicode: U+eac3
        /// </summary>
        MynauiCalendarUp = 0xEAC3,

        /// <summary>
        /// mynaui-calendar-x
        /// Unicode: U+eac4
        /// </summary>
        MynauiCalendarX = 0xEAC4,

        /// <summary>
        /// mynaui-camera
        /// Unicode: U+eac7
        /// </summary>
        MynauiCamera = 0xEAC7,

        /// <summary>
        /// mynaui-camera-slash
        /// Unicode: U+eac6
        /// </summary>
        MynauiCameraSlash = 0xEAC6,

        /// <summary>
        /// mynaui-campfire
        /// Unicode: U+eac8
        /// </summary>
        MynauiCampfire = 0xEAC8,

        /// <summary>
        /// mynaui-cannabis
        /// Unicode: U+eac9
        /// </summary>
        MynauiCannabis = 0xEAC9,

        /// <summary>
        /// mynaui-caravan
        /// Unicode: U+eaca
        /// </summary>
        MynauiCaravan = 0xEACA,

        /// <summary>
        /// mynaui-cart
        /// Unicode: U+eacf
        /// </summary>
        MynauiCart = 0xEACF,

        /// <summary>
        /// mynaui-cart-check
        /// Unicode: U+eacb
        /// </summary>
        MynauiCartCheck = 0xEACB,

        /// <summary>
        /// mynaui-cart-minus
        /// Unicode: U+eacc
        /// </summary>
        MynauiCartMinus = 0xEACC,

        /// <summary>
        /// mynaui-cart-plus
        /// Unicode: U+eacd
        /// </summary>
        MynauiCartPlus = 0xEACD,

        /// <summary>
        /// mynaui-cart-x
        /// Unicode: U+eace
        /// </summary>
        MynauiCartX = 0xEACE,

        /// <summary>
        /// mynaui-cast-screen
        /// Unicode: U+ead0
        /// </summary>
        MynauiCastScreen = 0xEAD0,

        /// <summary>
        /// mynaui-center-focus
        /// Unicode: U+ead1
        /// </summary>
        MynauiCenterFocus = 0xEAD1,

        /// <summary>
        /// mynaui-chart-area
        /// Unicode: U+ead2
        /// </summary>
        MynauiChartArea = 0xEAD2,

        /// <summary>
        /// mynaui-chart-bar
        /// Unicode: U+ead8
        /// </summary>
        MynauiChartBar = 0xEAD8,

        /// <summary>
        /// mynaui-chart-bar-big
        /// Unicode: U+ead3
        /// </summary>
        MynauiChartBarBig = 0xEAD3,

        /// <summary>
        /// mynaui-chart-bar-decreasing
        /// Unicode: U+ead4
        /// </summary>
        MynauiChartBarDecreasing = 0xEAD4,

        /// <summary>
        /// mynaui-chart-bar-increasing
        /// Unicode: U+ead5
        /// </summary>
        MynauiChartBarIncreasing = 0xEAD5,

        /// <summary>
        /// mynaui-chart-bar-one
        /// Unicode: U+ead6
        /// </summary>
        MynauiChartBarOne = 0xEAD6,

        /// <summary>
        /// mynaui-chart-bar-stacked
        /// Unicode: U+ead7
        /// </summary>
        MynauiChartBarStacked = 0xEAD7,

        /// <summary>
        /// mynaui-chart-bubble
        /// Unicode: U+ead9
        /// </summary>
        MynauiChartBubble = 0xEAD9,

        /// <summary>
        /// mynaui-chart-candlestick
        /// Unicode: U+eada
        /// </summary>
        MynauiChartCandlestick = 0xEADA,

        /// <summary>
        /// mynaui-chart-column
        /// Unicode: U+eadf
        /// </summary>
        MynauiChartColumn = 0xEADF,

        /// <summary>
        /// mynaui-chart-column-big
        /// Unicode: U+eadb
        /// </summary>
        MynauiChartColumnBig = 0xEADB,

        /// <summary>
        /// mynaui-chart-column-decreasing
        /// Unicode: U+eadc
        /// </summary>
        MynauiChartColumnDecreasing = 0xEADC,

        /// <summary>
        /// mynaui-chart-column-increasing
        /// Unicode: U+eadd
        /// </summary>
        MynauiChartColumnIncreasing = 0xEADD,

        /// <summary>
        /// mynaui-chart-column-stacked
        /// Unicode: U+eade
        /// </summary>
        MynauiChartColumnStacked = 0xEADE,

        /// <summary>
        /// mynaui-chart-gantt
        /// Unicode: U+eae0
        /// </summary>
        MynauiChartGantt = 0xEAE0,

        /// <summary>
        /// mynaui-chart-graph
        /// Unicode: U+eae1
        /// </summary>
        MynauiChartGraph = 0xEAE1,

        /// <summary>
        /// mynaui-chart-line
        /// Unicode: U+eae2
        /// </summary>
        MynauiChartLine = 0xEAE2,

        /// <summary>
        /// mynaui-chart-network
        /// Unicode: U+eae3
        /// </summary>
        MynauiChartNetwork = 0xEAE3,

        /// <summary>
        /// mynaui-chart-no-axes-column
        /// Unicode: U+eae6
        /// </summary>
        MynauiChartNoAxesColumn = 0xEAE6,

        /// <summary>
        /// mynaui-chart-no-axes-column-decreasing
        /// Unicode: U+eae4
        /// </summary>
        MynauiChartNoAxesColumnDecreasing = 0xEAE4,

        /// <summary>
        /// mynaui-chart-no-axes-column-increasing
        /// Unicode: U+eae5
        /// </summary>
        MynauiChartNoAxesColumnIncreasing = 0xEAE5,

        /// <summary>
        /// mynaui-chart-no-axes-combined
        /// Unicode: U+eae7
        /// </summary>
        MynauiChartNoAxesCombined = 0xEAE7,

        /// <summary>
        /// mynaui-chart-no-axes-gantt
        /// Unicode: U+eae8
        /// </summary>
        MynauiChartNoAxesGantt = 0xEAE8,

        /// <summary>
        /// mynaui-chart-pie
        /// Unicode: U+eaeb
        /// </summary>
        MynauiChartPie = 0xEAEB,

        /// <summary>
        /// mynaui-chart-pie-one
        /// Unicode: U+eae9
        /// </summary>
        MynauiChartPieOne = 0xEAE9,

        /// <summary>
        /// mynaui-chart-pie-two
        /// Unicode: U+eaea
        /// </summary>
        MynauiChartPieTwo = 0xEAEA,

        /// <summary>
        /// mynaui-chart-scatter
        /// Unicode: U+eaec
        /// </summary>
        MynauiChartScatter = 0xEAEC,

        /// <summary>
        /// mynaui-chart-spline
        /// Unicode: U+eaed
        /// </summary>
        MynauiChartSpline = 0xEAED,

        /// <summary>
        /// mynaui-chat
        /// Unicode: U+eaf4
        /// </summary>
        MynauiChat = 0xEAF4,

        /// <summary>
        /// mynaui-chat-check
        /// Unicode: U+eaee
        /// </summary>
        MynauiChatCheck = 0xEAEE,

        /// <summary>
        /// mynaui-chat-dots
        /// Unicode: U+eaef
        /// </summary>
        MynauiChatDots = 0xEAEF,

        /// <summary>
        /// mynaui-chat-messages
        /// Unicode: U+eaf0
        /// </summary>
        MynauiChatMessages = 0xEAF0,

        /// <summary>
        /// mynaui-chat-minus
        /// Unicode: U+eaf1
        /// </summary>
        MynauiChatMinus = 0xEAF1,

        /// <summary>
        /// mynaui-chat-plus
        /// Unicode: U+eaf2
        /// </summary>
        MynauiChatPlus = 0xEAF2,

        /// <summary>
        /// mynaui-chat-x
        /// Unicode: U+eaf3
        /// </summary>
        MynauiChatX = 0xEAF3,

        /// <summary>
        /// mynaui-check
        /// Unicode: U+eafd
        /// </summary>
        MynauiCheck = 0xEAFD,

        /// <summary>
        /// mynaui-check-circle
        /// Unicode: U+eaf6
        /// </summary>
        MynauiCheckCircle = 0xEAF6,

        /// <summary>
        /// mynaui-check-circle-one
        /// Unicode: U+eaf5
        /// </summary>
        MynauiCheckCircleOne = 0xEAF5,

        /// <summary>
        /// mynaui-check-diamond
        /// Unicode: U+eaf7
        /// </summary>
        MynauiCheckDiamond = 0xEAF7,

        /// <summary>
        /// mynaui-check-hexagon
        /// Unicode: U+eaf8
        /// </summary>
        MynauiCheckHexagon = 0xEAF8,

        /// <summary>
        /// mynaui-check-octagon
        /// Unicode: U+eaf9
        /// </summary>
        MynauiCheckOctagon = 0xEAF9,

        /// <summary>
        /// mynaui-check-square
        /// Unicode: U+eafb
        /// </summary>
        MynauiCheckSquare = 0xEAFB,

        /// <summary>
        /// mynaui-check-square-one
        /// Unicode: U+eafa
        /// </summary>
        MynauiCheckSquareOne = 0xEAFA,

        /// <summary>
        /// mynaui-check-waves
        /// Unicode: U+eafc
        /// </summary>
        MynauiCheckWaves = 0xEAFC,

        /// <summary>
        /// mynaui-chevron-double-down
        /// Unicode: U+eb00
        /// </summary>
        MynauiChevronDoubleDown = 0xEB00,

        /// <summary>
        /// mynaui-chevron-double-down-left
        /// Unicode: U+eafe
        /// </summary>
        MynauiChevronDoubleDownLeft = 0xEAFE,

        /// <summary>
        /// mynaui-chevron-double-down-right
        /// Unicode: U+eaff
        /// </summary>
        MynauiChevronDoubleDownRight = 0xEAFF,

        /// <summary>
        /// mynaui-chevron-double-left
        /// Unicode: U+eb01
        /// </summary>
        MynauiChevronDoubleLeft = 0xEB01,

        /// <summary>
        /// mynaui-chevron-double-right
        /// Unicode: U+eb02
        /// </summary>
        MynauiChevronDoubleRight = 0xEB02,

        /// <summary>
        /// mynaui-chevron-double-up
        /// Unicode: U+eb05
        /// </summary>
        MynauiChevronDoubleUp = 0xEB05,

        /// <summary>
        /// mynaui-chevron-double-up-left
        /// Unicode: U+eb03
        /// </summary>
        MynauiChevronDoubleUpLeft = 0xEB03,

        /// <summary>
        /// mynaui-chevron-double-up-right
        /// Unicode: U+eb04
        /// </summary>
        MynauiChevronDoubleUpRight = 0xEB04,

        /// <summary>
        /// mynaui-chevron-down
        /// Unicode: U+eb11
        /// </summary>
        MynauiChevronDown = 0xEB11,

        /// <summary>
        /// mynaui-chevron-down-circle
        /// Unicode: U+eb06
        /// </summary>
        MynauiChevronDownCircle = 0xEB06,

        /// <summary>
        /// mynaui-chevron-down-left
        /// Unicode: U+eb0a
        /// </summary>
        MynauiChevronDownLeft = 0xEB0A,

        /// <summary>
        /// mynaui-chevron-down-left-circle
        /// Unicode: U+eb07
        /// </summary>
        MynauiChevronDownLeftCircle = 0xEB07,

        /// <summary>
        /// mynaui-chevron-down-left-square
        /// Unicode: U+eb08
        /// </summary>
        MynauiChevronDownLeftSquare = 0xEB08,

        /// <summary>
        /// mynaui-chevron-down-left-waves
        /// Unicode: U+eb09
        /// </summary>
        MynauiChevronDownLeftWaves = 0xEB09,

        /// <summary>
        /// mynaui-chevron-down-right
        /// Unicode: U+eb0e
        /// </summary>
        MynauiChevronDownRight = 0xEB0E,

        /// <summary>
        /// mynaui-chevron-down-right-circle
        /// Unicode: U+eb0b
        /// </summary>
        MynauiChevronDownRightCircle = 0xEB0B,

        /// <summary>
        /// mynaui-chevron-down-right-square
        /// Unicode: U+eb0c
        /// </summary>
        MynauiChevronDownRightSquare = 0xEB0C,

        /// <summary>
        /// mynaui-chevron-down-right-waves
        /// Unicode: U+eb0d
        /// </summary>
        MynauiChevronDownRightWaves = 0xEB0D,

        /// <summary>
        /// mynaui-chevron-down-square
        /// Unicode: U+eb0f
        /// </summary>
        MynauiChevronDownSquare = 0xEB0F,

        /// <summary>
        /// mynaui-chevron-down-waves
        /// Unicode: U+eb10
        /// </summary>
        MynauiChevronDownWaves = 0xEB10,

        /// <summary>
        /// mynaui-chevron-left
        /// Unicode: U+eb15
        /// </summary>
        MynauiChevronLeft = 0xEB15,

        /// <summary>
        /// mynaui-chevron-left-circle
        /// Unicode: U+eb12
        /// </summary>
        MynauiChevronLeftCircle = 0xEB12,

        /// <summary>
        /// mynaui-chevron-left-square
        /// Unicode: U+eb13
        /// </summary>
        MynauiChevronLeftSquare = 0xEB13,

        /// <summary>
        /// mynaui-chevron-left-waves
        /// Unicode: U+eb14
        /// </summary>
        MynauiChevronLeftWaves = 0xEB14,

        /// <summary>
        /// mynaui-chevron-right
        /// Unicode: U+eb19
        /// </summary>
        MynauiChevronRight = 0xEB19,

        /// <summary>
        /// mynaui-chevron-right-circle
        /// Unicode: U+eb16
        /// </summary>
        MynauiChevronRightCircle = 0xEB16,

        /// <summary>
        /// mynaui-chevron-right-square
        /// Unicode: U+eb17
        /// </summary>
        MynauiChevronRightSquare = 0xEB17,

        /// <summary>
        /// mynaui-chevron-right-waves
        /// Unicode: U+eb18
        /// </summary>
        MynauiChevronRightWaves = 0xEB18,

        /// <summary>
        /// mynaui-chevron-up
        /// Unicode: U+eb26
        /// </summary>
        MynauiChevronUp = 0xEB26,

        /// <summary>
        /// mynaui-chevron-up-circle
        /// Unicode: U+eb1a
        /// </summary>
        MynauiChevronUpCircle = 0xEB1A,

        /// <summary>
        /// mynaui-chevron-up-down
        /// Unicode: U+eb1b
        /// </summary>
        MynauiChevronUpDown = 0xEB1B,

        /// <summary>
        /// mynaui-chevron-up-left
        /// Unicode: U+eb1f
        /// </summary>
        MynauiChevronUpLeft = 0xEB1F,

        /// <summary>
        /// mynaui-chevron-up-left-circle
        /// Unicode: U+eb1c
        /// </summary>
        MynauiChevronUpLeftCircle = 0xEB1C,

        /// <summary>
        /// mynaui-chevron-up-left-square
        /// Unicode: U+eb1d
        /// </summary>
        MynauiChevronUpLeftSquare = 0xEB1D,

        /// <summary>
        /// mynaui-chevron-up-left-waves
        /// Unicode: U+eb1e
        /// </summary>
        MynauiChevronUpLeftWaves = 0xEB1E,

        /// <summary>
        /// mynaui-chevron-up-right
        /// Unicode: U+eb23
        /// </summary>
        MynauiChevronUpRight = 0xEB23,

        /// <summary>
        /// mynaui-chevron-up-right-circle
        /// Unicode: U+eb20
        /// </summary>
        MynauiChevronUpRightCircle = 0xEB20,

        /// <summary>
        /// mynaui-chevron-up-right-square
        /// Unicode: U+eb21
        /// </summary>
        MynauiChevronUpRightSquare = 0xEB21,

        /// <summary>
        /// mynaui-chevron-up-right-waves
        /// Unicode: U+eb22
        /// </summary>
        MynauiChevronUpRightWaves = 0xEB22,

        /// <summary>
        /// mynaui-chevron-up-square
        /// Unicode: U+eb24
        /// </summary>
        MynauiChevronUpSquare = 0xEB24,

        /// <summary>
        /// mynaui-chevron-up-waves
        /// Unicode: U+eb25
        /// </summary>
        MynauiChevronUpWaves = 0xEB25,

        /// <summary>
        /// mynaui-chip
        /// Unicode: U+eb27
        /// </summary>
        MynauiChip = 0xEB27,

        /// <summary>
        /// mynaui-cigarette
        /// Unicode: U+eb29
        /// </summary>
        MynauiCigarette = 0xEB29,

        /// <summary>
        /// mynaui-cigarette-off
        /// Unicode: U+eb28
        /// </summary>
        MynauiCigaretteOff = 0xEB28,

        /// <summary>
        /// mynaui-circle
        /// Unicode: U+eb2e
        /// </summary>
        MynauiCircle = 0xEB2E,

        /// <summary>
        /// mynaui-circle-dashed
        /// Unicode: U+eb2a
        /// </summary>
        MynauiCircleDashed = 0xEB2A,

        /// <summary>
        /// mynaui-circle-half
        /// Unicode: U+eb2c
        /// </summary>
        MynauiCircleHalf = 0xEB2C,

        /// <summary>
        /// mynaui-circle-half-circle
        /// Unicode: U+eb2b
        /// </summary>
        MynauiCircleHalfCircle = 0xEB2B,

        /// <summary>
        /// mynaui-circle-notch
        /// Unicode: U+eb2d
        /// </summary>
        MynauiCircleNotch = 0xEB2D,

        /// <summary>
        /// mynaui-click
        /// Unicode: U+eb2f
        /// </summary>
        MynauiClick = 0xEB2F,

        /// <summary>
        /// mynaui-clipboard
        /// Unicode: U+eb30
        /// </summary>
        MynauiClipboard = 0xEB30,

        /// <summary>
        /// mynaui-clock-circle
        /// Unicode: U+eb31
        /// </summary>
        MynauiClockCircle = 0xEB31,

        /// <summary>
        /// mynaui-clock-diamond
        /// Unicode: U+eb32
        /// </summary>
        MynauiClockDiamond = 0xEB32,

        /// <summary>
        /// mynaui-clock-eight
        /// Unicode: U+eb33
        /// </summary>
        MynauiClockEight = 0xEB33,

        /// <summary>
        /// mynaui-clock-eleven
        /// Unicode: U+eb34
        /// </summary>
        MynauiClockEleven = 0xEB34,

        /// <summary>
        /// mynaui-clock-five
        /// Unicode: U+eb35
        /// </summary>
        MynauiClockFive = 0xEB35,

        /// <summary>
        /// mynaui-clock-four
        /// Unicode: U+eb36
        /// </summary>
        MynauiClockFour = 0xEB36,

        /// <summary>
        /// mynaui-clock-hand
        /// Unicode: U+eb37
        /// </summary>
        MynauiClockHand = 0xEB37,

        /// <summary>
        /// mynaui-clock-hexagon
        /// Unicode: U+eb38
        /// </summary>
        MynauiClockHexagon = 0xEB38,

        /// <summary>
        /// mynaui-clock-nine
        /// Unicode: U+eb39
        /// </summary>
        MynauiClockNine = 0xEB39,

        /// <summary>
        /// mynaui-clock-octagon
        /// Unicode: U+eb3a
        /// </summary>
        MynauiClockOctagon = 0xEB3A,

        /// <summary>
        /// mynaui-clock-one
        /// Unicode: U+eb3b
        /// </summary>
        MynauiClockOne = 0xEB3B,

        /// <summary>
        /// mynaui-clock-seven
        /// Unicode: U+eb3c
        /// </summary>
        MynauiClockSeven = 0xEB3C,

        /// <summary>
        /// mynaui-clock-six
        /// Unicode: U+eb3d
        /// </summary>
        MynauiClockSix = 0xEB3D,

        /// <summary>
        /// mynaui-clock-square
        /// Unicode: U+eb3e
        /// </summary>
        MynauiClockSquare = 0xEB3E,

        /// <summary>
        /// mynaui-clock-ten
        /// Unicode: U+eb3f
        /// </summary>
        MynauiClockTen = 0xEB3F,

        /// <summary>
        /// mynaui-clock-three
        /// Unicode: U+eb40
        /// </summary>
        MynauiClockThree = 0xEB40,

        /// <summary>
        /// mynaui-clock-twelve
        /// Unicode: U+eb41
        /// </summary>
        MynauiClockTwelve = 0xEB41,

        /// <summary>
        /// mynaui-clock-two
        /// Unicode: U+eb42
        /// </summary>
        MynauiClockTwo = 0xEB42,

        /// <summary>
        /// mynaui-clock-waves
        /// Unicode: U+eb43
        /// </summary>
        MynauiClockWaves = 0xEB43,

        /// <summary>
        /// mynaui-cloud
        /// Unicode: U+eb52
        /// </summary>
        MynauiCloud = 0xEB52,

        /// <summary>
        /// mynaui-cloud-download
        /// Unicode: U+eb44
        /// </summary>
        MynauiCloudDownload = 0xEB44,

        /// <summary>
        /// mynaui-cloud-drizzle
        /// Unicode: U+eb45
        /// </summary>
        MynauiCloudDrizzle = 0xEB45,

        /// <summary>
        /// mynaui-cloud-fog
        /// Unicode: U+eb46
        /// </summary>
        MynauiCloudFog = 0xEB46,

        /// <summary>
        /// mynaui-cloud-hail
        /// Unicode: U+eb47
        /// </summary>
        MynauiCloudHail = 0xEB47,

        /// <summary>
        /// mynaui-cloud-lightning
        /// Unicode: U+eb48
        /// </summary>
        MynauiCloudLightning = 0xEB48,

        /// <summary>
        /// mynaui-cloud-moon
        /// Unicode: U+eb4a
        /// </summary>
        MynauiCloudMoon = 0xEB4A,

        /// <summary>
        /// mynaui-cloud-moon-rain
        /// Unicode: U+eb49
        /// </summary>
        MynauiCloudMoonRain = 0xEB49,

        /// <summary>
        /// mynaui-cloud-off
        /// Unicode: U+eb4b
        /// </summary>
        MynauiCloudOff = 0xEB4B,

        /// <summary>
        /// mynaui-cloud-rain
        /// Unicode: U+eb4d
        /// </summary>
        MynauiCloudRain = 0xEB4D,

        /// <summary>
        /// mynaui-cloud-rain-wind
        /// Unicode: U+eb4c
        /// </summary>
        MynauiCloudRainWind = 0xEB4C,

        /// <summary>
        /// mynaui-cloud-snow
        /// Unicode: U+eb4e
        /// </summary>
        MynauiCloudSnow = 0xEB4E,

        /// <summary>
        /// mynaui-cloud-sun
        /// Unicode: U+eb50
        /// </summary>
        MynauiCloudSun = 0xEB50,

        /// <summary>
        /// mynaui-cloud-sun-rain
        /// Unicode: U+eb4f
        /// </summary>
        MynauiCloudSunRain = 0xEB4F,

        /// <summary>
        /// mynaui-cloud-upload
        /// Unicode: U+eb51
        /// </summary>
        MynauiCloudUpload = 0xEB51,

        /// <summary>
        /// mynaui-cloudy
        /// Unicode: U+eb53
        /// </summary>
        MynauiCloudy = 0xEB53,

        /// <summary>
        /// mynaui-cocktail
        /// Unicode: U+eb54
        /// </summary>
        MynauiCocktail = 0xEB54,

        /// <summary>
        /// mynaui-code
        /// Unicode: U+eb5b
        /// </summary>
        MynauiCode = 0xEB5B,

        /// <summary>
        /// mynaui-code-circle
        /// Unicode: U+eb55
        /// </summary>
        MynauiCodeCircle = 0xEB55,

        /// <summary>
        /// mynaui-code-diamond
        /// Unicode: U+eb56
        /// </summary>
        MynauiCodeDiamond = 0xEB56,

        /// <summary>
        /// mynaui-code-hexagon
        /// Unicode: U+eb57
        /// </summary>
        MynauiCodeHexagon = 0xEB57,

        /// <summary>
        /// mynaui-code-octagon
        /// Unicode: U+eb58
        /// </summary>
        MynauiCodeOctagon = 0xEB58,

        /// <summary>
        /// mynaui-code-square
        /// Unicode: U+eb59
        /// </summary>
        MynauiCodeSquare = 0xEB59,

        /// <summary>
        /// mynaui-code-waves
        /// Unicode: U+eb5a
        /// </summary>
        MynauiCodeWaves = 0xEB5A,

        /// <summary>
        /// mynaui-coffee
        /// Unicode: U+eb5c
        /// </summary>
        MynauiCoffee = 0xEB5C,

        /// <summary>
        /// mynaui-cog
        /// Unicode: U+eb61
        /// </summary>
        MynauiCog = 0xEB61,

        /// <summary>
        /// mynaui-cog-four
        /// Unicode: U+eb5d
        /// </summary>
        MynauiCogFour = 0xEB5D,

        /// <summary>
        /// mynaui-cog-one
        /// Unicode: U+eb5e
        /// </summary>
        MynauiCogOne = 0xEB5E,

        /// <summary>
        /// mynaui-cog-three
        /// Unicode: U+eb5f
        /// </summary>
        MynauiCogThree = 0xEB5F,

        /// <summary>
        /// mynaui-cog-two
        /// Unicode: U+eb60
        /// </summary>
        MynauiCogTwo = 0xEB60,

        /// <summary>
        /// mynaui-columns
        /// Unicode: U+eb62
        /// </summary>
        MynauiColumns = 0xEB62,

        /// <summary>
        /// mynaui-command
        /// Unicode: U+eb63
        /// </summary>
        MynauiCommand = 0xEB63,

        /// <summary>
        /// mynaui-compass
        /// Unicode: U+eb64
        /// </summary>
        MynauiCompass = 0xEB64,

        /// <summary>
        /// mynaui-components
        /// Unicode: U+eb65
        /// </summary>
        MynauiComponents = 0xEB65,

        /// <summary>
        /// mynaui-confetti
        /// Unicode: U+eb66
        /// </summary>
        MynauiConfetti = 0xEB66,

        /// <summary>
        /// mynaui-config
        /// Unicode: U+eb68
        /// </summary>
        MynauiConfig = 0xEB68,

        /// <summary>
        /// mynaui-config-vertical
        /// Unicode: U+eb67
        /// </summary>
        MynauiConfigVertical = 0xEB67,

        /// <summary>
        /// mynaui-contactless
        /// Unicode: U+eb6a
        /// </summary>
        MynauiContactless = 0xEB6A,

        /// <summary>
        /// mynaui-contactless-circle
        /// Unicode: U+eb69
        /// </summary>
        MynauiContactlessCircle = 0xEB69,

        /// <summary>
        /// mynaui-controller
        /// Unicode: U+eb6b
        /// </summary>
        MynauiController = 0xEB6B,

        /// <summary>
        /// mynaui-cookie
        /// Unicode: U+eb6c
        /// </summary>
        MynauiCookie = 0xEB6C,

        /// <summary>
        /// mynaui-copy
        /// Unicode: U+eb6d
        /// </summary>
        MynauiCopy = 0xEB6D,

        /// <summary>
        /// mynaui-copyleft
        /// Unicode: U+eb6e
        /// </summary>
        MynauiCopyleft = 0xEB6E,

        /// <summary>
        /// mynaui-copyright
        /// Unicode: U+eb70
        /// </summary>
        MynauiCopyright = 0xEB70,

        /// <summary>
        /// mynaui-copyright-slash
        /// Unicode: U+eb6f
        /// </summary>
        MynauiCopyrightSlash = 0xEB6F,

        /// <summary>
        /// mynaui-corner-down-left
        /// Unicode: U+eb71
        /// </summary>
        MynauiCornerDownLeft = 0xEB71,

        /// <summary>
        /// mynaui-corner-down-right
        /// Unicode: U+eb72
        /// </summary>
        MynauiCornerDownRight = 0xEB72,

        /// <summary>
        /// mynaui-corner-left-down
        /// Unicode: U+eb73
        /// </summary>
        MynauiCornerLeftDown = 0xEB73,

        /// <summary>
        /// mynaui-corner-left-up
        /// Unicode: U+eb74
        /// </summary>
        MynauiCornerLeftUp = 0xEB74,

        /// <summary>
        /// mynaui-corner-right-down
        /// Unicode: U+eb75
        /// </summary>
        MynauiCornerRightDown = 0xEB75,

        /// <summary>
        /// mynaui-corner-right-up
        /// Unicode: U+eb76
        /// </summary>
        MynauiCornerRightUp = 0xEB76,

        /// <summary>
        /// mynaui-corner-up-left
        /// Unicode: U+eb77
        /// </summary>
        MynauiCornerUpLeft = 0xEB77,

        /// <summary>
        /// mynaui-corner-up-right
        /// Unicode: U+eb78
        /// </summary>
        MynauiCornerUpRight = 0xEB78,

        /// <summary>
        /// mynaui-credit-card
        /// Unicode: U+eb7d
        /// </summary>
        MynauiCreditCard = 0xEB7D,

        /// <summary>
        /// mynaui-credit-card-check
        /// Unicode: U+eb79
        /// </summary>
        MynauiCreditCardCheck = 0xEB79,

        /// <summary>
        /// mynaui-credit-card-minus
        /// Unicode: U+eb7a
        /// </summary>
        MynauiCreditCardMinus = 0xEB7A,

        /// <summary>
        /// mynaui-credit-card-plus
        /// Unicode: U+eb7b
        /// </summary>
        MynauiCreditCardPlus = 0xEB7B,

        /// <summary>
        /// mynaui-credit-card-x
        /// Unicode: U+eb7c
        /// </summary>
        MynauiCreditCardX = 0xEB7C,

        /// <summary>
        /// mynaui-croissant
        /// Unicode: U+eb7e
        /// </summary>
        MynauiCroissant = 0xEB7E,

        /// <summary>
        /// mynaui-crop
        /// Unicode: U+eb7f
        /// </summary>
        MynauiCrop = 0xEB7F,

        /// <summary>
        /// mynaui-crosshair
        /// Unicode: U+eb80
        /// </summary>
        MynauiCrosshair = 0xEB80,

        /// <summary>
        /// mynaui-cupcake
        /// Unicode: U+eb81
        /// </summary>
        MynauiCupcake = 0xEB81,

        /// <summary>
        /// mynaui-danger
        /// Unicode: U+eb89
        /// </summary>
        MynauiDanger = 0xEB89,

        /// <summary>
        /// mynaui-danger-circle
        /// Unicode: U+eb82
        /// </summary>
        MynauiDangerCircle = 0xEB82,

        /// <summary>
        /// mynaui-danger-diamond
        /// Unicode: U+eb83
        /// </summary>
        MynauiDangerDiamond = 0xEB83,

        /// <summary>
        /// mynaui-danger-hexagon
        /// Unicode: U+eb84
        /// </summary>
        MynauiDangerHexagon = 0xEB84,

        /// <summary>
        /// mynaui-danger-octagon
        /// Unicode: U+eb85
        /// </summary>
        MynauiDangerOctagon = 0xEB85,

        /// <summary>
        /// mynaui-danger-square
        /// Unicode: U+eb86
        /// </summary>
        MynauiDangerSquare = 0xEB86,

        /// <summary>
        /// mynaui-danger-triangle
        /// Unicode: U+eb87
        /// </summary>
        MynauiDangerTriangle = 0xEB87,

        /// <summary>
        /// mynaui-danger-waves
        /// Unicode: U+eb88
        /// </summary>
        MynauiDangerWaves = 0xEB88,

        /// <summary>
        /// mynaui-database
        /// Unicode: U+eb8a
        /// </summary>
        MynauiDatabase = 0xEB8A,

        /// <summary>
        /// mynaui-daze-circle
        /// Unicode: U+eb8b
        /// </summary>
        MynauiDazeCircle = 0xEB8B,

        /// <summary>
        /// mynaui-daze-ghost
        /// Unicode: U+eb8c
        /// </summary>
        MynauiDazeGhost = 0xEB8C,

        /// <summary>
        /// mynaui-daze-square
        /// Unicode: U+eb8d
        /// </summary>
        MynauiDazeSquare = 0xEB8D,

        /// <summary>
        /// mynaui-delete
        /// Unicode: U+eb8e
        /// </summary>
        MynauiDelete = 0xEB8E,

        /// <summary>
        /// mynaui-desktop
        /// Unicode: U+eb8f
        /// </summary>
        MynauiDesktop = 0xEB8F,

        /// <summary>
        /// mynaui-diamond
        /// Unicode: U+eb90
        /// </summary>
        MynauiDiamond = 0xEB90,

        /// <summary>
        /// mynaui-dice-five
        /// Unicode: U+eb91
        /// </summary>
        MynauiDiceFive = 0xEB91,

        /// <summary>
        /// mynaui-dice-four
        /// Unicode: U+eb92
        /// </summary>
        MynauiDiceFour = 0xEB92,

        /// <summary>
        /// mynaui-dice-one
        /// Unicode: U+eb93
        /// </summary>
        MynauiDiceOne = 0xEB93,

        /// <summary>
        /// mynaui-dice-six
        /// Unicode: U+eb94
        /// </summary>
        MynauiDiceSix = 0xEB94,

        /// <summary>
        /// mynaui-dice-three
        /// Unicode: U+eb95
        /// </summary>
        MynauiDiceThree = 0xEB95,

        /// <summary>
        /// mynaui-dice-two
        /// Unicode: U+eb96
        /// </summary>
        MynauiDiceTwo = 0xEB96,

        /// <summary>
        /// mynaui-dislike
        /// Unicode: U+eb97
        /// </summary>
        MynauiDislike = 0xEB97,

        /// <summary>
        /// mynaui-divide
        /// Unicode: U+eb98
        /// </summary>
        MynauiDivide = 0xEB98,

        /// <summary>
        /// mynaui-dollar
        /// Unicode: U+eb9f
        /// </summary>
        MynauiDollar = 0xEB9F,

        /// <summary>
        /// mynaui-dollar-circle
        /// Unicode: U+eb99
        /// </summary>
        MynauiDollarCircle = 0xEB99,

        /// <summary>
        /// mynaui-dollar-diamond
        /// Unicode: U+eb9a
        /// </summary>
        MynauiDollarDiamond = 0xEB9A,

        /// <summary>
        /// mynaui-dollar-hexagon
        /// Unicode: U+eb9b
        /// </summary>
        MynauiDollarHexagon = 0xEB9B,

        /// <summary>
        /// mynaui-dollar-octagon
        /// Unicode: U+eb9c
        /// </summary>
        MynauiDollarOctagon = 0xEB9C,

        /// <summary>
        /// mynaui-dollar-square
        /// Unicode: U+eb9d
        /// </summary>
        MynauiDollarSquare = 0xEB9D,

        /// <summary>
        /// mynaui-dollar-waves
        /// Unicode: U+eb9e
        /// </summary>
        MynauiDollarWaves = 0xEB9E,

        /// <summary>
        /// mynaui-door-closed
        /// Unicode: U+eba1
        /// </summary>
        MynauiDoorClosed = 0xEBA1,

        /// <summary>
        /// mynaui-door-closed-locked
        /// Unicode: U+eba0
        /// </summary>
        MynauiDoorClosedLocked = 0xEBA0,

        /// <summary>
        /// mynaui-door-open
        /// Unicode: U+eba2
        /// </summary>
        MynauiDoorOpen = 0xEBA2,

        /// <summary>
        /// mynaui-dots
        /// Unicode: U+ebb0
        /// </summary>
        MynauiDots = 0xEBB0,

        /// <summary>
        /// mynaui-dots-circle
        /// Unicode: U+eba3
        /// </summary>
        MynauiDotsCircle = 0xEBA3,

        /// <summary>
        /// mynaui-dots-diamond
        /// Unicode: U+eba4
        /// </summary>
        MynauiDotsDiamond = 0xEBA4,

        /// <summary>
        /// mynaui-dots-hexagon
        /// Unicode: U+eba5
        /// </summary>
        MynauiDotsHexagon = 0xEBA5,

        /// <summary>
        /// mynaui-dots-octagon
        /// Unicode: U+eba6
        /// </summary>
        MynauiDotsOctagon = 0xEBA6,

        /// <summary>
        /// mynaui-dots-square
        /// Unicode: U+eba7
        /// </summary>
        MynauiDotsSquare = 0xEBA7,

        /// <summary>
        /// mynaui-dots-vertical
        /// Unicode: U+ebae
        /// </summary>
        MynauiDotsVertical = 0xEBAE,

        /// <summary>
        /// mynaui-dots-vertical-circle
        /// Unicode: U+eba8
        /// </summary>
        MynauiDotsVerticalCircle = 0xEBA8,

        /// <summary>
        /// mynaui-dots-vertical-diamond
        /// Unicode: U+eba9
        /// </summary>
        MynauiDotsVerticalDiamond = 0xEBA9,

        /// <summary>
        /// mynaui-dots-vertical-hexagon
        /// Unicode: U+ebaa
        /// </summary>
        MynauiDotsVerticalHexagon = 0xEBAA,

        /// <summary>
        /// mynaui-dots-vertical-octagon
        /// Unicode: U+ebab
        /// </summary>
        MynauiDotsVerticalOctagon = 0xEBAB,

        /// <summary>
        /// mynaui-dots-vertical-square
        /// Unicode: U+ebac
        /// </summary>
        MynauiDotsVerticalSquare = 0xEBAC,

        /// <summary>
        /// mynaui-dots-vertical-waves
        /// Unicode: U+ebad
        /// </summary>
        MynauiDotsVerticalWaves = 0xEBAD,

        /// <summary>
        /// mynaui-dots-waves
        /// Unicode: U+ebaf
        /// </summary>
        MynauiDotsWaves = 0xEBAF,

        /// <summary>
        /// mynaui-download
        /// Unicode: U+ebb1
        /// </summary>
        MynauiDownload = 0xEBB1,

        /// <summary>
        /// mynaui-drop
        /// Unicode: U+ebb2
        /// </summary>
        MynauiDrop = 0xEBB2,

        /// <summary>
        /// mynaui-droplet
        /// Unicode: U+ebb4
        /// </summary>
        MynauiDroplet = 0xEBB4,

        /// <summary>
        /// mynaui-droplet-off
        /// Unicode: U+ebb3
        /// </summary>
        MynauiDropletOff = 0xEBB3,

        /// <summary>
        /// mynaui-droplets
        /// Unicode: U+ebb5
        /// </summary>
        MynauiDroplets = 0xEBB5,

        /// <summary>
        /// mynaui-ear
        /// Unicode: U+ebb7
        /// </summary>
        MynauiEar = 0xEBB7,

        /// <summary>
        /// mynaui-ear-slash
        /// Unicode: U+ebb6
        /// </summary>
        MynauiEarSlash = 0xEBB6,

        /// <summary>
        /// mynaui-earth
        /// Unicode: U+ebb8
        /// </summary>
        MynauiEarth = 0xEBB8,

        /// <summary>
        /// mynaui-eclipse
        /// Unicode: U+ebb9
        /// </summary>
        MynauiEclipse = 0xEBB9,

        /// <summary>
        /// mynaui-edit
        /// Unicode: U+ebbb
        /// </summary>
        MynauiEdit = 0xEBBB,

        /// <summary>
        /// mynaui-edit-one
        /// Unicode: U+ebba
        /// </summary>
        MynauiEditOne = 0xEBBA,

        /// <summary>
        /// mynaui-egg
        /// Unicode: U+ebbc
        /// </summary>
        MynauiEgg = 0xEBBC,

        /// <summary>
        /// mynaui-eight
        /// Unicode: U+ebc3
        /// </summary>
        MynauiEight = 0xEBC3,

        /// <summary>
        /// mynaui-eight-circle
        /// Unicode: U+ebbd
        /// </summary>
        MynauiEightCircle = 0xEBBD,

        /// <summary>
        /// mynaui-eight-diamond
        /// Unicode: U+ebbe
        /// </summary>
        MynauiEightDiamond = 0xEBBE,

        /// <summary>
        /// mynaui-eight-hexagon
        /// Unicode: U+ebbf
        /// </summary>
        MynauiEightHexagon = 0xEBBF,

        /// <summary>
        /// mynaui-eight-octagon
        /// Unicode: U+ebc0
        /// </summary>
        MynauiEightOctagon = 0xEBC0,

        /// <summary>
        /// mynaui-eight-square
        /// Unicode: U+ebc1
        /// </summary>
        MynauiEightSquare = 0xEBC1,

        /// <summary>
        /// mynaui-eight-waves
        /// Unicode: U+ebc2
        /// </summary>
        MynauiEightWaves = 0xEBC2,

        /// <summary>
        /// mynaui-elevator
        /// Unicode: U+ebc4
        /// </summary>
        MynauiElevator = 0xEBC4,

        /// <summary>
        /// mynaui-envelope
        /// Unicode: U+ebc6
        /// </summary>
        MynauiEnvelope = 0xEBC6,

        /// <summary>
        /// mynaui-envelope-open
        /// Unicode: U+ebc5
        /// </summary>
        MynauiEnvelopeOpen = 0xEBC5,

        /// <summary>
        /// mynaui-euro
        /// Unicode: U+ebcd
        /// </summary>
        MynauiEuro = 0xEBCD,

        /// <summary>
        /// mynaui-euro-circle
        /// Unicode: U+ebc7
        /// </summary>
        MynauiEuroCircle = 0xEBC7,

        /// <summary>
        /// mynaui-euro-diamond
        /// Unicode: U+ebc8
        /// </summary>
        MynauiEuroDiamond = 0xEBC8,

        /// <summary>
        /// mynaui-euro-hexagon
        /// Unicode: U+ebc9
        /// </summary>
        MynauiEuroHexagon = 0xEBC9,

        /// <summary>
        /// mynaui-euro-octagon
        /// Unicode: U+ebca
        /// </summary>
        MynauiEuroOctagon = 0xEBCA,

        /// <summary>
        /// mynaui-euro-square
        /// Unicode: U+ebcb
        /// </summary>
        MynauiEuroSquare = 0xEBCB,

        /// <summary>
        /// mynaui-euro-waves
        /// Unicode: U+ebcc
        /// </summary>
        MynauiEuroWaves = 0xEBCC,

        /// <summary>
        /// mynaui-exclude
        /// Unicode: U+ebce
        /// </summary>
        MynauiExclude = 0xEBCE,

        /// <summary>
        /// mynaui-external-link
        /// Unicode: U+ebcf
        /// </summary>
        MynauiExternalLink = 0xEBCF,

        /// <summary>
        /// mynaui-eye
        /// Unicode: U+ebd1
        /// </summary>
        MynauiEye = 0xEBD1,

        /// <summary>
        /// mynaui-eye-slash
        /// Unicode: U+ebd0
        /// </summary>
        MynauiEyeSlash = 0xEBD0,

        /// <summary>
        /// mynaui-face-id
        /// Unicode: U+ebd2
        /// </summary>
        MynauiFaceId = 0xEBD2,

        /// <summary>
        /// mynaui-fat-arrow-down
        /// Unicode: U+ebd5
        /// </summary>
        MynauiFatArrowDown = 0xEBD5,

        /// <summary>
        /// mynaui-fat-arrow-down-left
        /// Unicode: U+ebd3
        /// </summary>
        MynauiFatArrowDownLeft = 0xEBD3,

        /// <summary>
        /// mynaui-fat-arrow-down-right
        /// Unicode: U+ebd4
        /// </summary>
        MynauiFatArrowDownRight = 0xEBD4,

        /// <summary>
        /// mynaui-fat-arrow-left
        /// Unicode: U+ebd6
        /// </summary>
        MynauiFatArrowLeft = 0xEBD6,

        /// <summary>
        /// mynaui-fat-arrow-right
        /// Unicode: U+ebd7
        /// </summary>
        MynauiFatArrowRight = 0xEBD7,

        /// <summary>
        /// mynaui-fat-arrow-up
        /// Unicode: U+ebda
        /// </summary>
        MynauiFatArrowUp = 0xEBDA,

        /// <summary>
        /// mynaui-fat-arrow-up-left
        /// Unicode: U+ebd8
        /// </summary>
        MynauiFatArrowUpLeft = 0xEBD8,

        /// <summary>
        /// mynaui-fat-arrow-up-right
        /// Unicode: U+ebd9
        /// </summary>
        MynauiFatArrowUpRight = 0xEBD9,

        /// <summary>
        /// mynaui-fat-corner-down-left
        /// Unicode: U+ebdb
        /// </summary>
        MynauiFatCornerDownLeft = 0xEBDB,

        /// <summary>
        /// mynaui-fat-corner-down-right
        /// Unicode: U+ebdc
        /// </summary>
        MynauiFatCornerDownRight = 0xEBDC,

        /// <summary>
        /// mynaui-fat-corner-left-down
        /// Unicode: U+ebdd
        /// </summary>
        MynauiFatCornerLeftDown = 0xEBDD,

        /// <summary>
        /// mynaui-fat-corner-left-up
        /// Unicode: U+ebde
        /// </summary>
        MynauiFatCornerLeftUp = 0xEBDE,

        /// <summary>
        /// mynaui-fat-corner-right-down
        /// Unicode: U+ebdf
        /// </summary>
        MynauiFatCornerRightDown = 0xEBDF,

        /// <summary>
        /// mynaui-fat-corner-right-up
        /// Unicode: U+ebe0
        /// </summary>
        MynauiFatCornerRightUp = 0xEBE0,

        /// <summary>
        /// mynaui-fat-corner-up-left
        /// Unicode: U+ebe1
        /// </summary>
        MynauiFatCornerUpLeft = 0xEBE1,

        /// <summary>
        /// mynaui-fat-corner-up-right
        /// Unicode: U+ebe2
        /// </summary>
        MynauiFatCornerUpRight = 0xEBE2,

        /// <summary>
        /// mynaui-female
        /// Unicode: U+ebe3
        /// </summary>
        MynauiFemale = 0xEBE3,

        /// <summary>
        /// mynaui-file
        /// Unicode: U+ebe9
        /// </summary>
        MynauiFile = 0xEBE9,

        /// <summary>
        /// mynaui-file-check
        /// Unicode: U+ebe4
        /// </summary>
        MynauiFileCheck = 0xEBE4,

        /// <summary>
        /// mynaui-file-minus
        /// Unicode: U+ebe5
        /// </summary>
        MynauiFileMinus = 0xEBE5,

        /// <summary>
        /// mynaui-file-plus
        /// Unicode: U+ebe6
        /// </summary>
        MynauiFilePlus = 0xEBE6,

        /// <summary>
        /// mynaui-file-text
        /// Unicode: U+ebe7
        /// </summary>
        MynauiFileText = 0xEBE7,

        /// <summary>
        /// mynaui-file-x
        /// Unicode: U+ebe8
        /// </summary>
        MynauiFileX = 0xEBE8,

        /// <summary>
        /// mynaui-film
        /// Unicode: U+ebea
        /// </summary>
        MynauiFilm = 0xEBEA,

        /// <summary>
        /// mynaui-filter
        /// Unicode: U+ebec
        /// </summary>
        MynauiFilter = 0xEBEC,

        /// <summary>
        /// mynaui-filter-one
        /// Unicode: U+ebeb
        /// </summary>
        MynauiFilterOne = 0xEBEB,

        /// <summary>
        /// mynaui-fine-tune
        /// Unicode: U+ebed
        /// </summary>
        MynauiFineTune = 0xEBED,

        /// <summary>
        /// mynaui-fire
        /// Unicode: U+ebee
        /// </summary>
        MynauiFire = 0xEBEE,

        /// <summary>
        /// mynaui-five
        /// Unicode: U+ebf5
        /// </summary>
        MynauiFive = 0xEBF5,

        /// <summary>
        /// mynaui-five-circle
        /// Unicode: U+ebef
        /// </summary>
        MynauiFiveCircle = 0xEBEF,

        /// <summary>
        /// mynaui-five-diamond
        /// Unicode: U+ebf0
        /// </summary>
        MynauiFiveDiamond = 0xEBF0,

        /// <summary>
        /// mynaui-five-hexagon
        /// Unicode: U+ebf1
        /// </summary>
        MynauiFiveHexagon = 0xEBF1,

        /// <summary>
        /// mynaui-five-octagon
        /// Unicode: U+ebf2
        /// </summary>
        MynauiFiveOctagon = 0xEBF2,

        /// <summary>
        /// mynaui-five-square
        /// Unicode: U+ebf3
        /// </summary>
        MynauiFiveSquare = 0xEBF3,

        /// <summary>
        /// mynaui-five-waves
        /// Unicode: U+ebf4
        /// </summary>
        MynauiFiveWaves = 0xEBF4,

        /// <summary>
        /// mynaui-flag
        /// Unicode: U+ebf7
        /// </summary>
        MynauiFlag = 0xEBF7,

        /// <summary>
        /// mynaui-flag-one
        /// Unicode: U+ebf6
        /// </summary>
        MynauiFlagOne = 0xEBF6,

        /// <summary>
        /// mynaui-flame
        /// Unicode: U+ebf9
        /// </summary>
        MynauiFlame = 0xEBF9,

        /// <summary>
        /// mynaui-flame-kindling
        /// Unicode: U+ebf8
        /// </summary>
        MynauiFlameKindling = 0xEBF8,

        /// <summary>
        /// mynaui-flask
        /// Unicode: U+ebfa
        /// </summary>
        MynauiFlask = 0xEBFA,

        /// <summary>
        /// mynaui-flower
        /// Unicode: U+ebfc
        /// </summary>
        MynauiFlower = 0xEBFC,

        /// <summary>
        /// mynaui-flower-2
        /// Unicode: U+ebfb
        /// </summary>
        MynauiFlower2 = 0xEBFB,

        /// <summary>
        /// mynaui-folder
        /// Unicode: U+ec06
        /// </summary>
        MynauiFolder = 0xEC06,

        /// <summary>
        /// mynaui-folder-check
        /// Unicode: U+ebfd
        /// </summary>
        MynauiFolderCheck = 0xEBFD,

        /// <summary>
        /// mynaui-folder-heart
        /// Unicode: U+ebfe
        /// </summary>
        MynauiFolderHeart = 0xEBFE,

        /// <summary>
        /// mynaui-folder-kanban
        /// Unicode: U+ebff
        /// </summary>
        MynauiFolderKanban = 0xEBFF,

        /// <summary>
        /// mynaui-folder-minus
        /// Unicode: U+ec00
        /// </summary>
        MynauiFolderMinus = 0xEC00,

        /// <summary>
        /// mynaui-folder-one
        /// Unicode: U+ec01
        /// </summary>
        MynauiFolderOne = 0xEC01,

        /// <summary>
        /// mynaui-folder-plus
        /// Unicode: U+ec02
        /// </summary>
        MynauiFolderPlus = 0xEC02,

        /// <summary>
        /// mynaui-folder-slash
        /// Unicode: U+ec03
        /// </summary>
        MynauiFolderSlash = 0xEC03,

        /// <summary>
        /// mynaui-folder-two
        /// Unicode: U+ec04
        /// </summary>
        MynauiFolderTwo = 0xEC04,

        /// <summary>
        /// mynaui-folder-x
        /// Unicode: U+ec05
        /// </summary>
        MynauiFolderX = 0xEC05,

        /// <summary>
        /// mynaui-forward
        /// Unicode: U+ec0d
        /// </summary>
        MynauiForward = 0xEC0D,

        /// <summary>
        /// mynaui-forward-circle
        /// Unicode: U+ec07
        /// </summary>
        MynauiForwardCircle = 0xEC07,

        /// <summary>
        /// mynaui-forward-diamond
        /// Unicode: U+ec08
        /// </summary>
        MynauiForwardDiamond = 0xEC08,

        /// <summary>
        /// mynaui-forward-hexagon
        /// Unicode: U+ec09
        /// </summary>
        MynauiForwardHexagon = 0xEC09,

        /// <summary>
        /// mynaui-forward-octagon
        /// Unicode: U+ec0a
        /// </summary>
        MynauiForwardOctagon = 0xEC0A,

        /// <summary>
        /// mynaui-forward-square
        /// Unicode: U+ec0b
        /// </summary>
        MynauiForwardSquare = 0xEC0B,

        /// <summary>
        /// mynaui-forward-waves
        /// Unicode: U+ec0c
        /// </summary>
        MynauiForwardWaves = 0xEC0C,

        /// <summary>
        /// mynaui-four
        /// Unicode: U+ec14
        /// </summary>
        MynauiFour = 0xEC14,

        /// <summary>
        /// mynaui-four-circle
        /// Unicode: U+ec0e
        /// </summary>
        MynauiFourCircle = 0xEC0E,

        /// <summary>
        /// mynaui-four-diamond
        /// Unicode: U+ec0f
        /// </summary>
        MynauiFourDiamond = 0xEC0F,

        /// <summary>
        /// mynaui-four-hexagon
        /// Unicode: U+ec10
        /// </summary>
        MynauiFourHexagon = 0xEC10,

        /// <summary>
        /// mynaui-four-octagon
        /// Unicode: U+ec11
        /// </summary>
        MynauiFourOctagon = 0xEC11,

        /// <summary>
        /// mynaui-four-square
        /// Unicode: U+ec12
        /// </summary>
        MynauiFourSquare = 0xEC12,

        /// <summary>
        /// mynaui-four-waves
        /// Unicode: U+ec13
        /// </summary>
        MynauiFourWaves = 0xEC13,

        /// <summary>
        /// mynaui-frame
        /// Unicode: U+ec15
        /// </summary>
        MynauiFrame = 0xEC15,

        /// <summary>
        /// mynaui-funny-circle
        /// Unicode: U+ec16
        /// </summary>
        MynauiFunnyCircle = 0xEC16,

        /// <summary>
        /// mynaui-funny-ghost
        /// Unicode: U+ec17
        /// </summary>
        MynauiFunnyGhost = 0xEC17,

        /// <summary>
        /// mynaui-funny-square
        /// Unicode: U+ec18
        /// </summary>
        MynauiFunnySquare = 0xEC18,

        /// <summary>
        /// mynaui-gift
        /// Unicode: U+ec19
        /// </summary>
        MynauiGift = 0xEC19,

        /// <summary>
        /// mynaui-git-branch
        /// Unicode: U+ec1a
        /// </summary>
        MynauiGitBranch = 0xEC1A,

        /// <summary>
        /// mynaui-git-circle
        /// Unicode: U+ec1b
        /// </summary>
        MynauiGitCircle = 0xEC1B,

        /// <summary>
        /// mynaui-git-commit
        /// Unicode: U+ec1c
        /// </summary>
        MynauiGitCommit = 0xEC1C,

        /// <summary>
        /// mynaui-git-diamond
        /// Unicode: U+ec1d
        /// </summary>
        MynauiGitDiamond = 0xEC1D,

        /// <summary>
        /// mynaui-git-diff
        /// Unicode: U+ec1e
        /// </summary>
        MynauiGitDiff = 0xEC1E,

        /// <summary>
        /// mynaui-git-hexagon
        /// Unicode: U+ec1f
        /// </summary>
        MynauiGitHexagon = 0xEC1F,

        /// <summary>
        /// mynaui-git-merge
        /// Unicode: U+ec20
        /// </summary>
        MynauiGitMerge = 0xEC20,

        /// <summary>
        /// mynaui-git-octagon
        /// Unicode: U+ec21
        /// </summary>
        MynauiGitOctagon = 0xEC21,

        /// <summary>
        /// mynaui-git-pull-request
        /// Unicode: U+ec22
        /// </summary>
        MynauiGitPullRequest = 0xEC22,

        /// <summary>
        /// mynaui-git-square
        /// Unicode: U+ec23
        /// </summary>
        MynauiGitSquare = 0xEC23,

        /// <summary>
        /// mynaui-git-waves
        /// Unicode: U+ec24
        /// </summary>
        MynauiGitWaves = 0xEC24,

        /// <summary>
        /// mynaui-glasses
        /// Unicode: U+ec25
        /// </summary>
        MynauiGlasses = 0xEC25,

        /// <summary>
        /// mynaui-globe
        /// Unicode: U+ec26
        /// </summary>
        MynauiGlobe = 0xEC26,

        /// <summary>
        /// mynaui-grid
        /// Unicode: U+ec28
        /// </summary>
        MynauiGrid = 0xEC28,

        /// <summary>
        /// mynaui-grid-one
        /// Unicode: U+ec27
        /// </summary>
        MynauiGridOne = 0xEC27,

        /// <summary>
        /// mynaui-hand
        /// Unicode: U+ec29
        /// </summary>
        MynauiHand = 0xEC29,

        /// <summary>
        /// mynaui-hard-drive
        /// Unicode: U+ec2a
        /// </summary>
        MynauiHardDrive = 0xEC2A,

        /// <summary>
        /// mynaui-hash
        /// Unicode: U+ec31
        /// </summary>
        MynauiHash = 0xEC31,

        /// <summary>
        /// mynaui-hash-circle
        /// Unicode: U+ec2b
        /// </summary>
        MynauiHashCircle = 0xEC2B,

        /// <summary>
        /// mynaui-hash-diamond
        /// Unicode: U+ec2c
        /// </summary>
        MynauiHashDiamond = 0xEC2C,

        /// <summary>
        /// mynaui-hash-hexagon
        /// Unicode: U+ec2d
        /// </summary>
        MynauiHashHexagon = 0xEC2D,

        /// <summary>
        /// mynaui-hash-octagon
        /// Unicode: U+ec2e
        /// </summary>
        MynauiHashOctagon = 0xEC2E,

        /// <summary>
        /// mynaui-hash-square
        /// Unicode: U+ec2f
        /// </summary>
        MynauiHashSquare = 0xEC2F,

        /// <summary>
        /// mynaui-hash-waves
        /// Unicode: U+ec30
        /// </summary>
        MynauiHashWaves = 0xEC30,

        /// <summary>
        /// mynaui-haze
        /// Unicode: U+ec32
        /// </summary>
        MynauiHaze = 0xEC32,

        /// <summary>
        /// mynaui-heading
        /// Unicode: U+ec39
        /// </summary>
        MynauiHeading = 0xEC39,

        /// <summary>
        /// mynaui-heading-five
        /// Unicode: U+ec33
        /// </summary>
        MynauiHeadingFive = 0xEC33,

        /// <summary>
        /// mynaui-heading-four
        /// Unicode: U+ec34
        /// </summary>
        MynauiHeadingFour = 0xEC34,

        /// <summary>
        /// mynaui-heading-one
        /// Unicode: U+ec35
        /// </summary>
        MynauiHeadingOne = 0xEC35,

        /// <summary>
        /// mynaui-heading-six
        /// Unicode: U+ec36
        /// </summary>
        MynauiHeadingSix = 0xEC36,

        /// <summary>
        /// mynaui-heading-three
        /// Unicode: U+ec37
        /// </summary>
        MynauiHeadingThree = 0xEC37,

        /// <summary>
        /// mynaui-heading-two
        /// Unicode: U+ec38
        /// </summary>
        MynauiHeadingTwo = 0xEC38,

        /// <summary>
        /// mynaui-headphones
        /// Unicode: U+ec3a
        /// </summary>
        MynauiHeadphones = 0xEC3A,

        /// <summary>
        /// mynaui-heart
        /// Unicode: U+ec4b
        /// </summary>
        MynauiHeart = 0xEC4B,

        /// <summary>
        /// mynaui-heart-broken
        /// Unicode: U+ec3b
        /// </summary>
        MynauiHeartBroken = 0xEC3B,

        /// <summary>
        /// mynaui-heart-check
        /// Unicode: U+ec3c
        /// </summary>
        MynauiHeartCheck = 0xEC3C,

        /// <summary>
        /// mynaui-heart-circle
        /// Unicode: U+ec3d
        /// </summary>
        MynauiHeartCircle = 0xEC3D,

        /// <summary>
        /// mynaui-heart-diamond
        /// Unicode: U+ec3e
        /// </summary>
        MynauiHeartDiamond = 0xEC3E,

        /// <summary>
        /// mynaui-heart-dot
        /// Unicode: U+ec3f
        /// </summary>
        MynauiHeartDot = 0xEC3F,

        /// <summary>
        /// mynaui-heart-hexagon
        /// Unicode: U+ec40
        /// </summary>
        MynauiHeartHexagon = 0xEC40,

        /// <summary>
        /// mynaui-heart-home
        /// Unicode: U+ec41
        /// </summary>
        MynauiHeartHome = 0xEC41,

        /// <summary>
        /// mynaui-heart-minus
        /// Unicode: U+ec42
        /// </summary>
        MynauiHeartMinus = 0xEC42,

        /// <summary>
        /// mynaui-heart-octagon
        /// Unicode: U+ec43
        /// </summary>
        MynauiHeartOctagon = 0xEC43,

        /// <summary>
        /// mynaui-heart-plus
        /// Unicode: U+ec44
        /// </summary>
        MynauiHeartPlus = 0xEC44,

        /// <summary>
        /// mynaui-heart-slash
        /// Unicode: U+ec45
        /// </summary>
        MynauiHeartSlash = 0xEC45,

        /// <summary>
        /// mynaui-heart-snooze
        /// Unicode: U+ec46
        /// </summary>
        MynauiHeartSnooze = 0xEC46,

        /// <summary>
        /// mynaui-heart-square
        /// Unicode: U+ec47
        /// </summary>
        MynauiHeartSquare = 0xEC47,

        /// <summary>
        /// mynaui-heart-user
        /// Unicode: U+ec48
        /// </summary>
        MynauiHeartUser = 0xEC48,

        /// <summary>
        /// mynaui-heart-waves
        /// Unicode: U+ec49
        /// </summary>
        MynauiHeartWaves = 0xEC49,

        /// <summary>
        /// mynaui-heart-x
        /// Unicode: U+ec4a
        /// </summary>
        MynauiHeartX = 0xEC4A,

        /// <summary>
        /// mynaui-hexagon
        /// Unicode: U+ec4c
        /// </summary>
        MynauiHexagon = 0xEC4C,

        /// <summary>
        /// mynaui-home
        /// Unicode: U+ec52
        /// </summary>
        MynauiHome = 0xEC52,

        /// <summary>
        /// mynaui-home-check
        /// Unicode: U+ec4d
        /// </summary>
        MynauiHomeCheck = 0xEC4D,

        /// <summary>
        /// mynaui-home-minus
        /// Unicode: U+ec4e
        /// </summary>
        MynauiHomeMinus = 0xEC4E,

        /// <summary>
        /// mynaui-home-plus
        /// Unicode: U+ec4f
        /// </summary>
        MynauiHomePlus = 0xEC4F,

        /// <summary>
        /// mynaui-home-smile
        /// Unicode: U+ec50
        /// </summary>
        MynauiHomeSmile = 0xEC50,

        /// <summary>
        /// mynaui-home-x
        /// Unicode: U+ec51
        /// </summary>
        MynauiHomeX = 0xEC51,

        /// <summary>
        /// mynaui-image
        /// Unicode: U+ec55
        /// </summary>
        MynauiImage = 0xEC55,

        /// <summary>
        /// mynaui-image-circle
        /// Unicode: U+ec53
        /// </summary>
        MynauiImageCircle = 0xEC53,

        /// <summary>
        /// mynaui-image-rectangle
        /// Unicode: U+ec54
        /// </summary>
        MynauiImageRectangle = 0xEC54,

        /// <summary>
        /// mynaui-inbox
        /// Unicode: U+ec5d
        /// </summary>
        MynauiInbox = 0xEC5D,

        /// <summary>
        /// mynaui-inbox-archive
        /// Unicode: U+ec56
        /// </summary>
        MynauiInboxArchive = 0xEC56,

        /// <summary>
        /// mynaui-inbox-check
        /// Unicode: U+ec57
        /// </summary>
        MynauiInboxCheck = 0xEC57,

        /// <summary>
        /// mynaui-inbox-down
        /// Unicode: U+ec58
        /// </summary>
        MynauiInboxDown = 0xEC58,

        /// <summary>
        /// mynaui-inbox-minus
        /// Unicode: U+ec59
        /// </summary>
        MynauiInboxMinus = 0xEC59,

        /// <summary>
        /// mynaui-inbox-plus
        /// Unicode: U+ec5a
        /// </summary>
        MynauiInboxPlus = 0xEC5A,

        /// <summary>
        /// mynaui-inbox-up
        /// Unicode: U+ec5b
        /// </summary>
        MynauiInboxUp = 0xEC5B,

        /// <summary>
        /// mynaui-inbox-x
        /// Unicode: U+ec5c
        /// </summary>
        MynauiInboxX = 0xEC5C,

        /// <summary>
        /// mynaui-incognito
        /// Unicode: U+ec5e
        /// </summary>
        MynauiIncognito = 0xEC5E,

        /// <summary>
        /// mynaui-indifferent-circle
        /// Unicode: U+ec5f
        /// </summary>
        MynauiIndifferentCircle = 0xEC5F,

        /// <summary>
        /// mynaui-indifferent-ghost
        /// Unicode: U+ec60
        /// </summary>
        MynauiIndifferentGhost = 0xEC60,

        /// <summary>
        /// mynaui-indifferent-square
        /// Unicode: U+ec61
        /// </summary>
        MynauiIndifferentSquare = 0xEC61,

        /// <summary>
        /// mynaui-infinity
        /// Unicode: U+ec62
        /// </summary>
        MynauiInfinity = 0xEC62,

        /// <summary>
        /// mynaui-info
        /// Unicode: U+ec6a
        /// </summary>
        MynauiInfo = 0xEC6A,

        /// <summary>
        /// mynaui-info-circle
        /// Unicode: U+ec63
        /// </summary>
        MynauiInfoCircle = 0xEC63,

        /// <summary>
        /// mynaui-info-diamond
        /// Unicode: U+ec64
        /// </summary>
        MynauiInfoDiamond = 0xEC64,

        /// <summary>
        /// mynaui-info-hexagon
        /// Unicode: U+ec65
        /// </summary>
        MynauiInfoHexagon = 0xEC65,

        /// <summary>
        /// mynaui-info-octagon
        /// Unicode: U+ec66
        /// </summary>
        MynauiInfoOctagon = 0xEC66,

        /// <summary>
        /// mynaui-info-square
        /// Unicode: U+ec67
        /// </summary>
        MynauiInfoSquare = 0xEC67,

        /// <summary>
        /// mynaui-info-triangle
        /// Unicode: U+ec68
        /// </summary>
        MynauiInfoTriangle = 0xEC68,

        /// <summary>
        /// mynaui-info-waves
        /// Unicode: U+ec69
        /// </summary>
        MynauiInfoWaves = 0xEC69,

        /// <summary>
        /// mynaui-intersect
        /// Unicode: U+ec6b
        /// </summary>
        MynauiIntersect = 0xEC6B,

        /// <summary>
        /// mynaui-kanban
        /// Unicode: U+ec6c
        /// </summary>
        MynauiKanban = 0xEC6C,

        /// <summary>
        /// mynaui-key
        /// Unicode: U+ec6d
        /// </summary>
        MynauiKey = 0xEC6D,

        /// <summary>
        /// mynaui-keyboard
        /// Unicode: U+ec70
        /// </summary>
        MynauiKeyboard = 0xEC70,

        /// <summary>
        /// mynaui-keyboard-brightness-high
        /// Unicode: U+ec6e
        /// </summary>
        MynauiKeyboardBrightnessHigh = 0xEC6E,

        /// <summary>
        /// mynaui-keyboard-brightness-low
        /// Unicode: U+ec6f
        /// </summary>
        MynauiKeyboardBrightnessLow = 0xEC6F,

        /// <summary>
        /// mynaui-label
        /// Unicode: U+ec71
        /// </summary>
        MynauiLabel = 0xEC71,

        /// <summary>
        /// mynaui-lamp
        /// Unicode: U+ec72
        /// </summary>
        MynauiLamp = 0xEC72,

        /// <summary>
        /// mynaui-layers-one
        /// Unicode: U+ec73
        /// </summary>
        MynauiLayersOne = 0xEC73,

        /// <summary>
        /// mynaui-layers-three
        /// Unicode: U+ec74
        /// </summary>
        MynauiLayersThree = 0xEC74,

        /// <summary>
        /// mynaui-layers-two
        /// Unicode: U+ec75
        /// </summary>
        MynauiLayersTwo = 0xEC75,

        /// <summary>
        /// mynaui-layout
        /// Unicode: U+ec76
        /// </summary>
        MynauiLayout = 0xEC76,

        /// <summary>
        /// mynaui-leaf
        /// Unicode: U+ec77
        /// </summary>
        MynauiLeaf = 0xEC77,

        /// <summary>
        /// mynaui-leaves
        /// Unicode: U+ec78
        /// </summary>
        MynauiLeaves = 0xEC78,

        /// <summary>
        /// mynaui-letter-a
        /// Unicode: U+ec7f
        /// </summary>
        MynauiLetterA = 0xEC7F,

        /// <summary>
        /// mynaui-letter-a-circle
        /// Unicode: U+ec79
        /// </summary>
        MynauiLetterACircle = 0xEC79,

        /// <summary>
        /// mynaui-letter-a-diamond
        /// Unicode: U+ec7a
        /// </summary>
        MynauiLetterADiamond = 0xEC7A,

        /// <summary>
        /// mynaui-letter-a-hexagon
        /// Unicode: U+ec7b
        /// </summary>
        MynauiLetterAHexagon = 0xEC7B,

        /// <summary>
        /// mynaui-letter-a-octagon
        /// Unicode: U+ec7c
        /// </summary>
        MynauiLetterAOctagon = 0xEC7C,

        /// <summary>
        /// mynaui-letter-a-square
        /// Unicode: U+ec7d
        /// </summary>
        MynauiLetterASquare = 0xEC7D,

        /// <summary>
        /// mynaui-letter-a-waves
        /// Unicode: U+ec7e
        /// </summary>
        MynauiLetterAWaves = 0xEC7E,

        /// <summary>
        /// mynaui-letter-b
        /// Unicode: U+ec86
        /// </summary>
        MynauiLetterB = 0xEC86,

        /// <summary>
        /// mynaui-letter-b-circle
        /// Unicode: U+ec80
        /// </summary>
        MynauiLetterBCircle = 0xEC80,

        /// <summary>
        /// mynaui-letter-b-diamond
        /// Unicode: U+ec81
        /// </summary>
        MynauiLetterBDiamond = 0xEC81,

        /// <summary>
        /// mynaui-letter-b-hexagon
        /// Unicode: U+ec82
        /// </summary>
        MynauiLetterBHexagon = 0xEC82,

        /// <summary>
        /// mynaui-letter-b-octagon
        /// Unicode: U+ec83
        /// </summary>
        MynauiLetterBOctagon = 0xEC83,

        /// <summary>
        /// mynaui-letter-b-square
        /// Unicode: U+ec84
        /// </summary>
        MynauiLetterBSquare = 0xEC84,

        /// <summary>
        /// mynaui-letter-b-waves
        /// Unicode: U+ec85
        /// </summary>
        MynauiLetterBWaves = 0xEC85,

        /// <summary>
        /// mynaui-letter-c
        /// Unicode: U+ec8d
        /// </summary>
        MynauiLetterC = 0xEC8D,

        /// <summary>
        /// mynaui-letter-c-circle
        /// Unicode: U+ec87
        /// </summary>
        MynauiLetterCCircle = 0xEC87,

        /// <summary>
        /// mynaui-letter-c-diamond
        /// Unicode: U+ec88
        /// </summary>
        MynauiLetterCDiamond = 0xEC88,

        /// <summary>
        /// mynaui-letter-c-hexagon
        /// Unicode: U+ec89
        /// </summary>
        MynauiLetterCHexagon = 0xEC89,

        /// <summary>
        /// mynaui-letter-c-octagon
        /// Unicode: U+ec8a
        /// </summary>
        MynauiLetterCOctagon = 0xEC8A,

        /// <summary>
        /// mynaui-letter-c-square
        /// Unicode: U+ec8b
        /// </summary>
        MynauiLetterCSquare = 0xEC8B,

        /// <summary>
        /// mynaui-letter-c-waves
        /// Unicode: U+ec8c
        /// </summary>
        MynauiLetterCWaves = 0xEC8C,

        /// <summary>
        /// mynaui-letter-d
        /// Unicode: U+ec94
        /// </summary>
        MynauiLetterD = 0xEC94,

        /// <summary>
        /// mynaui-letter-d-circle
        /// Unicode: U+ec8e
        /// </summary>
        MynauiLetterDCircle = 0xEC8E,

        /// <summary>
        /// mynaui-letter-d-diamond
        /// Unicode: U+ec8f
        /// </summary>
        MynauiLetterDDiamond = 0xEC8F,

        /// <summary>
        /// mynaui-letter-d-hexagon
        /// Unicode: U+ec90
        /// </summary>
        MynauiLetterDHexagon = 0xEC90,

        /// <summary>
        /// mynaui-letter-d-octagon
        /// Unicode: U+ec91
        /// </summary>
        MynauiLetterDOctagon = 0xEC91,

        /// <summary>
        /// mynaui-letter-d-square
        /// Unicode: U+ec92
        /// </summary>
        MynauiLetterDSquare = 0xEC92,

        /// <summary>
        /// mynaui-letter-d-waves
        /// Unicode: U+ec93
        /// </summary>
        MynauiLetterDWaves = 0xEC93,

        /// <summary>
        /// mynaui-letter-e
        /// Unicode: U+ec9b
        /// </summary>
        MynauiLetterE = 0xEC9B,

        /// <summary>
        /// mynaui-letter-e-circle
        /// Unicode: U+ec95
        /// </summary>
        MynauiLetterECircle = 0xEC95,

        /// <summary>
        /// mynaui-letter-e-diamond
        /// Unicode: U+ec96
        /// </summary>
        MynauiLetterEDiamond = 0xEC96,

        /// <summary>
        /// mynaui-letter-e-hexagon
        /// Unicode: U+ec97
        /// </summary>
        MynauiLetterEHexagon = 0xEC97,

        /// <summary>
        /// mynaui-letter-e-octagon
        /// Unicode: U+ec98
        /// </summary>
        MynauiLetterEOctagon = 0xEC98,

        /// <summary>
        /// mynaui-letter-e-square
        /// Unicode: U+ec99
        /// </summary>
        MynauiLetterESquare = 0xEC99,

        /// <summary>
        /// mynaui-letter-e-waves
        /// Unicode: U+ec9a
        /// </summary>
        MynauiLetterEWaves = 0xEC9A,

        /// <summary>
        /// mynaui-letter-f
        /// Unicode: U+eca2
        /// </summary>
        MynauiLetterF = 0xECA2,

        /// <summary>
        /// mynaui-letter-f-circle
        /// Unicode: U+ec9c
        /// </summary>
        MynauiLetterFCircle = 0xEC9C,

        /// <summary>
        /// mynaui-letter-f-diamond
        /// Unicode: U+ec9d
        /// </summary>
        MynauiLetterFDiamond = 0xEC9D,

        /// <summary>
        /// mynaui-letter-f-hexagon
        /// Unicode: U+ec9e
        /// </summary>
        MynauiLetterFHexagon = 0xEC9E,

        /// <summary>
        /// mynaui-letter-f-octagon
        /// Unicode: U+ec9f
        /// </summary>
        MynauiLetterFOctagon = 0xEC9F,

        /// <summary>
        /// mynaui-letter-f-square
        /// Unicode: U+eca0
        /// </summary>
        MynauiLetterFSquare = 0xECA0,

        /// <summary>
        /// mynaui-letter-f-waves
        /// Unicode: U+eca1
        /// </summary>
        MynauiLetterFWaves = 0xECA1,

        /// <summary>
        /// mynaui-letter-g
        /// Unicode: U+eca9
        /// </summary>
        MynauiLetterG = 0xECA9,

        /// <summary>
        /// mynaui-letter-g-circle
        /// Unicode: U+eca3
        /// </summary>
        MynauiLetterGCircle = 0xECA3,

        /// <summary>
        /// mynaui-letter-g-diamond
        /// Unicode: U+eca4
        /// </summary>
        MynauiLetterGDiamond = 0xECA4,

        /// <summary>
        /// mynaui-letter-g-hexagon
        /// Unicode: U+eca5
        /// </summary>
        MynauiLetterGHexagon = 0xECA5,

        /// <summary>
        /// mynaui-letter-g-octagon
        /// Unicode: U+eca6
        /// </summary>
        MynauiLetterGOctagon = 0xECA6,

        /// <summary>
        /// mynaui-letter-g-square
        /// Unicode: U+eca7
        /// </summary>
        MynauiLetterGSquare = 0xECA7,

        /// <summary>
        /// mynaui-letter-g-waves
        /// Unicode: U+eca8
        /// </summary>
        MynauiLetterGWaves = 0xECA8,

        /// <summary>
        /// mynaui-letter-h
        /// Unicode: U+ecb0
        /// </summary>
        MynauiLetterH = 0xECB0,

        /// <summary>
        /// mynaui-letter-h-circle
        /// Unicode: U+ecaa
        /// </summary>
        MynauiLetterHCircle = 0xECAA,

        /// <summary>
        /// mynaui-letter-h-diamond
        /// Unicode: U+ecab
        /// </summary>
        MynauiLetterHDiamond = 0xECAB,

        /// <summary>
        /// mynaui-letter-h-hexagon
        /// Unicode: U+ecac
        /// </summary>
        MynauiLetterHHexagon = 0xECAC,

        /// <summary>
        /// mynaui-letter-h-octagon
        /// Unicode: U+ecad
        /// </summary>
        MynauiLetterHOctagon = 0xECAD,

        /// <summary>
        /// mynaui-letter-h-square
        /// Unicode: U+ecae
        /// </summary>
        MynauiLetterHSquare = 0xECAE,

        /// <summary>
        /// mynaui-letter-h-waves
        /// Unicode: U+ecaf
        /// </summary>
        MynauiLetterHWaves = 0xECAF,

        /// <summary>
        /// mynaui-letter-i
        /// Unicode: U+ecb7
        /// </summary>
        MynauiLetterI = 0xECB7,

        /// <summary>
        /// mynaui-letter-i-circle
        /// Unicode: U+ecb1
        /// </summary>
        MynauiLetterICircle = 0xECB1,

        /// <summary>
        /// mynaui-letter-i-diamond
        /// Unicode: U+ecb2
        /// </summary>
        MynauiLetterIDiamond = 0xECB2,

        /// <summary>
        /// mynaui-letter-i-hexagon
        /// Unicode: U+ecb3
        /// </summary>
        MynauiLetterIHexagon = 0xECB3,

        /// <summary>
        /// mynaui-letter-i-octagon
        /// Unicode: U+ecb4
        /// </summary>
        MynauiLetterIOctagon = 0xECB4,

        /// <summary>
        /// mynaui-letter-i-square
        /// Unicode: U+ecb5
        /// </summary>
        MynauiLetterISquare = 0xECB5,

        /// <summary>
        /// mynaui-letter-i-waves
        /// Unicode: U+ecb6
        /// </summary>
        MynauiLetterIWaves = 0xECB6,

        /// <summary>
        /// mynaui-letter-j
        /// Unicode: U+ecbe
        /// </summary>
        MynauiLetterJ = 0xECBE,

        /// <summary>
        /// mynaui-letter-j-circle
        /// Unicode: U+ecb8
        /// </summary>
        MynauiLetterJCircle = 0xECB8,

        /// <summary>
        /// mynaui-letter-j-diamond
        /// Unicode: U+ecb9
        /// </summary>
        MynauiLetterJDiamond = 0xECB9,

        /// <summary>
        /// mynaui-letter-j-hexagon
        /// Unicode: U+ecba
        /// </summary>
        MynauiLetterJHexagon = 0xECBA,

        /// <summary>
        /// mynaui-letter-j-octagon
        /// Unicode: U+ecbb
        /// </summary>
        MynauiLetterJOctagon = 0xECBB,

        /// <summary>
        /// mynaui-letter-j-square
        /// Unicode: U+ecbc
        /// </summary>
        MynauiLetterJSquare = 0xECBC,

        /// <summary>
        /// mynaui-letter-j-waves
        /// Unicode: U+ecbd
        /// </summary>
        MynauiLetterJWaves = 0xECBD,

        /// <summary>
        /// mynaui-letter-k
        /// Unicode: U+ecc5
        /// </summary>
        MynauiLetterK = 0xECC5,

        /// <summary>
        /// mynaui-letter-k-circle
        /// Unicode: U+ecbf
        /// </summary>
        MynauiLetterKCircle = 0xECBF,

        /// <summary>
        /// mynaui-letter-k-diamond
        /// Unicode: U+ecc0
        /// </summary>
        MynauiLetterKDiamond = 0xECC0,

        /// <summary>
        /// mynaui-letter-k-hexagon
        /// Unicode: U+ecc1
        /// </summary>
        MynauiLetterKHexagon = 0xECC1,

        /// <summary>
        /// mynaui-letter-k-octagon
        /// Unicode: U+ecc2
        /// </summary>
        MynauiLetterKOctagon = 0xECC2,

        /// <summary>
        /// mynaui-letter-k-square
        /// Unicode: U+ecc3
        /// </summary>
        MynauiLetterKSquare = 0xECC3,

        /// <summary>
        /// mynaui-letter-k-waves
        /// Unicode: U+ecc4
        /// </summary>
        MynauiLetterKWaves = 0xECC4,

        /// <summary>
        /// mynaui-letter-l
        /// Unicode: U+eccc
        /// </summary>
        MynauiLetterL = 0xECCC,

        /// <summary>
        /// mynaui-letter-l-circle
        /// Unicode: U+ecc6
        /// </summary>
        MynauiLetterLCircle = 0xECC6,

        /// <summary>
        /// mynaui-letter-l-diamond
        /// Unicode: U+ecc7
        /// </summary>
        MynauiLetterLDiamond = 0xECC7,

        /// <summary>
        /// mynaui-letter-l-hexagon
        /// Unicode: U+ecc8
        /// </summary>
        MynauiLetterLHexagon = 0xECC8,

        /// <summary>
        /// mynaui-letter-l-octagon
        /// Unicode: U+ecc9
        /// </summary>
        MynauiLetterLOctagon = 0xECC9,

        /// <summary>
        /// mynaui-letter-l-square
        /// Unicode: U+ecca
        /// </summary>
        MynauiLetterLSquare = 0xECCA,

        /// <summary>
        /// mynaui-letter-l-waves
        /// Unicode: U+eccb
        /// </summary>
        MynauiLetterLWaves = 0xECCB,

        /// <summary>
        /// mynaui-letter-m
        /// Unicode: U+ecd3
        /// </summary>
        MynauiLetterM = 0xECD3,

        /// <summary>
        /// mynaui-letter-m-circle
        /// Unicode: U+eccd
        /// </summary>
        MynauiLetterMCircle = 0xECCD,

        /// <summary>
        /// mynaui-letter-m-diamond
        /// Unicode: U+ecce
        /// </summary>
        MynauiLetterMDiamond = 0xECCE,

        /// <summary>
        /// mynaui-letter-m-hexagon
        /// Unicode: U+eccf
        /// </summary>
        MynauiLetterMHexagon = 0xECCF,

        /// <summary>
        /// mynaui-letter-m-octagon
        /// Unicode: U+ecd0
        /// </summary>
        MynauiLetterMOctagon = 0xECD0,

        /// <summary>
        /// mynaui-letter-m-square
        /// Unicode: U+ecd1
        /// </summary>
        MynauiLetterMSquare = 0xECD1,

        /// <summary>
        /// mynaui-letter-m-waves
        /// Unicode: U+ecd2
        /// </summary>
        MynauiLetterMWaves = 0xECD2,

        /// <summary>
        /// mynaui-letter-n
        /// Unicode: U+ecda
        /// </summary>
        MynauiLetterN = 0xECDA,

        /// <summary>
        /// mynaui-letter-n-circle
        /// Unicode: U+ecd4
        /// </summary>
        MynauiLetterNCircle = 0xECD4,

        /// <summary>
        /// mynaui-letter-n-diamond
        /// Unicode: U+ecd5
        /// </summary>
        MynauiLetterNDiamond = 0xECD5,

        /// <summary>
        /// mynaui-letter-n-hexagon
        /// Unicode: U+ecd6
        /// </summary>
        MynauiLetterNHexagon = 0xECD6,

        /// <summary>
        /// mynaui-letter-n-octagon
        /// Unicode: U+ecd7
        /// </summary>
        MynauiLetterNOctagon = 0xECD7,

        /// <summary>
        /// mynaui-letter-n-square
        /// Unicode: U+ecd8
        /// </summary>
        MynauiLetterNSquare = 0xECD8,

        /// <summary>
        /// mynaui-letter-n-waves
        /// Unicode: U+ecd9
        /// </summary>
        MynauiLetterNWaves = 0xECD9,

        /// <summary>
        /// mynaui-letter-o
        /// Unicode: U+ece1
        /// </summary>
        MynauiLetterO = 0xECE1,

        /// <summary>
        /// mynaui-letter-o-circle
        /// Unicode: U+ecdb
        /// </summary>
        MynauiLetterOCircle = 0xECDB,

        /// <summary>
        /// mynaui-letter-o-diamond
        /// Unicode: U+ecdc
        /// </summary>
        MynauiLetterODiamond = 0xECDC,

        /// <summary>
        /// mynaui-letter-o-hexagon
        /// Unicode: U+ecdd
        /// </summary>
        MynauiLetterOHexagon = 0xECDD,

        /// <summary>
        /// mynaui-letter-o-octagon
        /// Unicode: U+ecde
        /// </summary>
        MynauiLetterOOctagon = 0xECDE,

        /// <summary>
        /// mynaui-letter-o-square
        /// Unicode: U+ecdf
        /// </summary>
        MynauiLetterOSquare = 0xECDF,

        /// <summary>
        /// mynaui-letter-o-waves
        /// Unicode: U+ece0
        /// </summary>
        MynauiLetterOWaves = 0xECE0,

        /// <summary>
        /// mynaui-letter-p
        /// Unicode: U+ece8
        /// </summary>
        MynauiLetterP = 0xECE8,

        /// <summary>
        /// mynaui-letter-p-circle
        /// Unicode: U+ece2
        /// </summary>
        MynauiLetterPCircle = 0xECE2,

        /// <summary>
        /// mynaui-letter-p-diamond
        /// Unicode: U+ece3
        /// </summary>
        MynauiLetterPDiamond = 0xECE3,

        /// <summary>
        /// mynaui-letter-p-hexagon
        /// Unicode: U+ece4
        /// </summary>
        MynauiLetterPHexagon = 0xECE4,

        /// <summary>
        /// mynaui-letter-p-octagon
        /// Unicode: U+ece5
        /// </summary>
        MynauiLetterPOctagon = 0xECE5,

        /// <summary>
        /// mynaui-letter-p-square
        /// Unicode: U+ece6
        /// </summary>
        MynauiLetterPSquare = 0xECE6,

        /// <summary>
        /// mynaui-letter-p-waves
        /// Unicode: U+ece7
        /// </summary>
        MynauiLetterPWaves = 0xECE7,

        /// <summary>
        /// mynaui-letter-q
        /// Unicode: U+ecef
        /// </summary>
        MynauiLetterQ = 0xECEF,

        /// <summary>
        /// mynaui-letter-q-circle
        /// Unicode: U+ece9
        /// </summary>
        MynauiLetterQCircle = 0xECE9,

        /// <summary>
        /// mynaui-letter-q-diamond
        /// Unicode: U+ecea
        /// </summary>
        MynauiLetterQDiamond = 0xECEA,

        /// <summary>
        /// mynaui-letter-q-hexagon
        /// Unicode: U+eceb
        /// </summary>
        MynauiLetterQHexagon = 0xECEB,

        /// <summary>
        /// mynaui-letter-q-octagon
        /// Unicode: U+ecec
        /// </summary>
        MynauiLetterQOctagon = 0xECEC,

        /// <summary>
        /// mynaui-letter-q-square
        /// Unicode: U+eced
        /// </summary>
        MynauiLetterQSquare = 0xECED,

        /// <summary>
        /// mynaui-letter-q-waves
        /// Unicode: U+ecee
        /// </summary>
        MynauiLetterQWaves = 0xECEE,

        /// <summary>
        /// mynaui-letter-r
        /// Unicode: U+ecf6
        /// </summary>
        MynauiLetterR = 0xECF6,

        /// <summary>
        /// mynaui-letter-r-circle
        /// Unicode: U+ecf0
        /// </summary>
        MynauiLetterRCircle = 0xECF0,

        /// <summary>
        /// mynaui-letter-r-diamond
        /// Unicode: U+ecf1
        /// </summary>
        MynauiLetterRDiamond = 0xECF1,

        /// <summary>
        /// mynaui-letter-r-hexagon
        /// Unicode: U+ecf2
        /// </summary>
        MynauiLetterRHexagon = 0xECF2,

        /// <summary>
        /// mynaui-letter-r-octagon
        /// Unicode: U+ecf3
        /// </summary>
        MynauiLetterROctagon = 0xECF3,

        /// <summary>
        /// mynaui-letter-r-square
        /// Unicode: U+ecf4
        /// </summary>
        MynauiLetterRSquare = 0xECF4,

        /// <summary>
        /// mynaui-letter-r-waves
        /// Unicode: U+ecf5
        /// </summary>
        MynauiLetterRWaves = 0xECF5,

        /// <summary>
        /// mynaui-letter-s
        /// Unicode: U+ecfd
        /// </summary>
        MynauiLetterS = 0xECFD,

        /// <summary>
        /// mynaui-letter-s-circle
        /// Unicode: U+ecf7
        /// </summary>
        MynauiLetterSCircle = 0xECF7,

        /// <summary>
        /// mynaui-letter-s-diamond
        /// Unicode: U+ecf8
        /// </summary>
        MynauiLetterSDiamond = 0xECF8,

        /// <summary>
        /// mynaui-letter-s-hexagon
        /// Unicode: U+ecf9
        /// </summary>
        MynauiLetterSHexagon = 0xECF9,

        /// <summary>
        /// mynaui-letter-s-octagon
        /// Unicode: U+ecfa
        /// </summary>
        MynauiLetterSOctagon = 0xECFA,

        /// <summary>
        /// mynaui-letter-s-square
        /// Unicode: U+ecfb
        /// </summary>
        MynauiLetterSSquare = 0xECFB,

        /// <summary>
        /// mynaui-letter-s-waves
        /// Unicode: U+ecfc
        /// </summary>
        MynauiLetterSWaves = 0xECFC,

        /// <summary>
        /// mynaui-letter-t
        /// Unicode: U+ed04
        /// </summary>
        MynauiLetterT = 0xED04,

        /// <summary>
        /// mynaui-letter-t-circle
        /// Unicode: U+ecfe
        /// </summary>
        MynauiLetterTCircle = 0xECFE,

        /// <summary>
        /// mynaui-letter-t-diamond
        /// Unicode: U+ecff
        /// </summary>
        MynauiLetterTDiamond = 0xECFF,

        /// <summary>
        /// mynaui-letter-t-hexagon
        /// Unicode: U+ed00
        /// </summary>
        MynauiLetterTHexagon = 0xED00,

        /// <summary>
        /// mynaui-letter-t-octagon
        /// Unicode: U+ed01
        /// </summary>
        MynauiLetterTOctagon = 0xED01,

        /// <summary>
        /// mynaui-letter-t-square
        /// Unicode: U+ed02
        /// </summary>
        MynauiLetterTSquare = 0xED02,

        /// <summary>
        /// mynaui-letter-t-waves
        /// Unicode: U+ed03
        /// </summary>
        MynauiLetterTWaves = 0xED03,

        /// <summary>
        /// mynaui-letter-u
        /// Unicode: U+ed0b
        /// </summary>
        MynauiLetterU = 0xED0B,

        /// <summary>
        /// mynaui-letter-u-circle
        /// Unicode: U+ed05
        /// </summary>
        MynauiLetterUCircle = 0xED05,

        /// <summary>
        /// mynaui-letter-u-diamond
        /// Unicode: U+ed06
        /// </summary>
        MynauiLetterUDiamond = 0xED06,

        /// <summary>
        /// mynaui-letter-u-hexagon
        /// Unicode: U+ed07
        /// </summary>
        MynauiLetterUHexagon = 0xED07,

        /// <summary>
        /// mynaui-letter-u-octagon
        /// Unicode: U+ed08
        /// </summary>
        MynauiLetterUOctagon = 0xED08,

        /// <summary>
        /// mynaui-letter-u-square
        /// Unicode: U+ed09
        /// </summary>
        MynauiLetterUSquare = 0xED09,

        /// <summary>
        /// mynaui-letter-u-waves
        /// Unicode: U+ed0a
        /// </summary>
        MynauiLetterUWaves = 0xED0A,

        /// <summary>
        /// mynaui-letter-v
        /// Unicode: U+ed12
        /// </summary>
        MynauiLetterV = 0xED12,

        /// <summary>
        /// mynaui-letter-v-circle
        /// Unicode: U+ed0c
        /// </summary>
        MynauiLetterVCircle = 0xED0C,

        /// <summary>
        /// mynaui-letter-v-diamond
        /// Unicode: U+ed0d
        /// </summary>
        MynauiLetterVDiamond = 0xED0D,

        /// <summary>
        /// mynaui-letter-v-hexagon
        /// Unicode: U+ed0e
        /// </summary>
        MynauiLetterVHexagon = 0xED0E,

        /// <summary>
        /// mynaui-letter-v-octagon
        /// Unicode: U+ed0f
        /// </summary>
        MynauiLetterVOctagon = 0xED0F,

        /// <summary>
        /// mynaui-letter-v-square
        /// Unicode: U+ed10
        /// </summary>
        MynauiLetterVSquare = 0xED10,

        /// <summary>
        /// mynaui-letter-v-waves
        /// Unicode: U+ed11
        /// </summary>
        MynauiLetterVWaves = 0xED11,

        /// <summary>
        /// mynaui-letter-w
        /// Unicode: U+ed19
        /// </summary>
        MynauiLetterW = 0xED19,

        /// <summary>
        /// mynaui-letter-w-circle
        /// Unicode: U+ed13
        /// </summary>
        MynauiLetterWCircle = 0xED13,

        /// <summary>
        /// mynaui-letter-w-diamond
        /// Unicode: U+ed14
        /// </summary>
        MynauiLetterWDiamond = 0xED14,

        /// <summary>
        /// mynaui-letter-w-hexagon
        /// Unicode: U+ed15
        /// </summary>
        MynauiLetterWHexagon = 0xED15,

        /// <summary>
        /// mynaui-letter-w-octagon
        /// Unicode: U+ed16
        /// </summary>
        MynauiLetterWOctagon = 0xED16,

        /// <summary>
        /// mynaui-letter-w-square
        /// Unicode: U+ed17
        /// </summary>
        MynauiLetterWSquare = 0xED17,

        /// <summary>
        /// mynaui-letter-w-waves
        /// Unicode: U+ed18
        /// </summary>
        MynauiLetterWWaves = 0xED18,

        /// <summary>
        /// mynaui-letter-x
        /// Unicode: U+ed20
        /// </summary>
        MynauiLetterX = 0xED20,

        /// <summary>
        /// mynaui-letter-x-circle
        /// Unicode: U+ed1a
        /// </summary>
        MynauiLetterXCircle = 0xED1A,

        /// <summary>
        /// mynaui-letter-x-diamond
        /// Unicode: U+ed1b
        /// </summary>
        MynauiLetterXDiamond = 0xED1B,

        /// <summary>
        /// mynaui-letter-x-hexagon
        /// Unicode: U+ed1c
        /// </summary>
        MynauiLetterXHexagon = 0xED1C,

        /// <summary>
        /// mynaui-letter-x-octagon
        /// Unicode: U+ed1d
        /// </summary>
        MynauiLetterXOctagon = 0xED1D,

        /// <summary>
        /// mynaui-letter-x-square
        /// Unicode: U+ed1e
        /// </summary>
        MynauiLetterXSquare = 0xED1E,

        /// <summary>
        /// mynaui-letter-x-waves
        /// Unicode: U+ed1f
        /// </summary>
        MynauiLetterXWaves = 0xED1F,

        /// <summary>
        /// mynaui-letter-y
        /// Unicode: U+ed27
        /// </summary>
        MynauiLetterY = 0xED27,

        /// <summary>
        /// mynaui-letter-y-circle
        /// Unicode: U+ed21
        /// </summary>
        MynauiLetterYCircle = 0xED21,

        /// <summary>
        /// mynaui-letter-y-diamond
        /// Unicode: U+ed22
        /// </summary>
        MynauiLetterYDiamond = 0xED22,

        /// <summary>
        /// mynaui-letter-y-hexagon
        /// Unicode: U+ed23
        /// </summary>
        MynauiLetterYHexagon = 0xED23,

        /// <summary>
        /// mynaui-letter-y-octagon
        /// Unicode: U+ed24
        /// </summary>
        MynauiLetterYOctagon = 0xED24,

        /// <summary>
        /// mynaui-letter-y-square
        /// Unicode: U+ed25
        /// </summary>
        MynauiLetterYSquare = 0xED25,

        /// <summary>
        /// mynaui-letter-y-waves
        /// Unicode: U+ed26
        /// </summary>
        MynauiLetterYWaves = 0xED26,

        /// <summary>
        /// mynaui-letter-z
        /// Unicode: U+ed2e
        /// </summary>
        MynauiLetterZ = 0xED2E,

        /// <summary>
        /// mynaui-letter-z-circle
        /// Unicode: U+ed28
        /// </summary>
        MynauiLetterZCircle = 0xED28,

        /// <summary>
        /// mynaui-letter-z-diamond
        /// Unicode: U+ed29
        /// </summary>
        MynauiLetterZDiamond = 0xED29,

        /// <summary>
        /// mynaui-letter-z-hexagon
        /// Unicode: U+ed2a
        /// </summary>
        MynauiLetterZHexagon = 0xED2A,

        /// <summary>
        /// mynaui-letter-z-octagon
        /// Unicode: U+ed2b
        /// </summary>
        MynauiLetterZOctagon = 0xED2B,

        /// <summary>
        /// mynaui-letter-z-square
        /// Unicode: U+ed2c
        /// </summary>
        MynauiLetterZSquare = 0xED2C,

        /// <summary>
        /// mynaui-letter-z-waves
        /// Unicode: U+ed2d
        /// </summary>
        MynauiLetterZWaves = 0xED2D,

        /// <summary>
        /// mynaui-lightning
        /// Unicode: U+ed30
        /// </summary>
        MynauiLightning = 0xED30,

        /// <summary>
        /// mynaui-lightning-off
        /// Unicode: U+ed2f
        /// </summary>
        MynauiLightningOff = 0xED2F,

        /// <summary>
        /// mynaui-like
        /// Unicode: U+ed31
        /// </summary>
        MynauiLike = 0xED31,

        /// <summary>
        /// mynaui-line-chart-circle
        /// Unicode: U+ed32
        /// </summary>
        MynauiLineChartCircle = 0xED32,

        /// <summary>
        /// mynaui-line-chart-diamond
        /// Unicode: U+ed33
        /// </summary>
        MynauiLineChartDiamond = 0xED33,

        /// <summary>
        /// mynaui-line-chart-hexagon
        /// Unicode: U+ed34
        /// </summary>
        MynauiLineChartHexagon = 0xED34,

        /// <summary>
        /// mynaui-line-chart-octagon
        /// Unicode: U+ed35
        /// </summary>
        MynauiLineChartOctagon = 0xED35,

        /// <summary>
        /// mynaui-line-chart-square
        /// Unicode: U+ed36
        /// </summary>
        MynauiLineChartSquare = 0xED36,

        /// <summary>
        /// mynaui-line-chart-waves
        /// Unicode: U+ed37
        /// </summary>
        MynauiLineChartWaves = 0xED37,

        /// <summary>
        /// mynaui-link
        /// Unicode: U+ed3a
        /// </summary>
        MynauiLink = 0xED3A,

        /// <summary>
        /// mynaui-link-one
        /// Unicode: U+ed38
        /// </summary>
        MynauiLinkOne = 0xED38,

        /// <summary>
        /// mynaui-link-two
        /// Unicode: U+ed39
        /// </summary>
        MynauiLinkTwo = 0xED39,

        /// <summary>
        /// mynaui-list
        /// Unicode: U+ed3d
        /// </summary>
        MynauiList = 0xED3D,

        /// <summary>
        /// mynaui-list-check
        /// Unicode: U+ed3b
        /// </summary>
        MynauiListCheck = 0xED3B,

        /// <summary>
        /// mynaui-list-number
        /// Unicode: U+ed3c
        /// </summary>
        MynauiListNumber = 0xED3C,

        /// <summary>
        /// mynaui-location
        /// Unicode: U+ed47
        /// </summary>
        MynauiLocation = 0xED47,

        /// <summary>
        /// mynaui-location-check
        /// Unicode: U+ed3e
        /// </summary>
        MynauiLocationCheck = 0xED3E,

        /// <summary>
        /// mynaui-location-home
        /// Unicode: U+ed3f
        /// </summary>
        MynauiLocationHome = 0xED3F,

        /// <summary>
        /// mynaui-location-minus
        /// Unicode: U+ed40
        /// </summary>
        MynauiLocationMinus = 0xED40,

        /// <summary>
        /// mynaui-location-plus
        /// Unicode: U+ed41
        /// </summary>
        MynauiLocationPlus = 0xED41,

        /// <summary>
        /// mynaui-location-selected
        /// Unicode: U+ed42
        /// </summary>
        MynauiLocationSelected = 0xED42,

        /// <summary>
        /// mynaui-location-slash
        /// Unicode: U+ed43
        /// </summary>
        MynauiLocationSlash = 0xED43,

        /// <summary>
        /// mynaui-location-snooze
        /// Unicode: U+ed44
        /// </summary>
        MynauiLocationSnooze = 0xED44,

        /// <summary>
        /// mynaui-location-user
        /// Unicode: U+ed45
        /// </summary>
        MynauiLocationUser = 0xED45,

        /// <summary>
        /// mynaui-location-x
        /// Unicode: U+ed46
        /// </summary>
        MynauiLocationX = 0xED46,

        /// <summary>
        /// mynaui-lock
        /// Unicode: U+ed53
        /// </summary>
        MynauiLock = 0xED53,

        /// <summary>
        /// mynaui-lock-circle
        /// Unicode: U+ed48
        /// </summary>
        MynauiLockCircle = 0xED48,

        /// <summary>
        /// mynaui-lock-diamond
        /// Unicode: U+ed49
        /// </summary>
        MynauiLockDiamond = 0xED49,

        /// <summary>
        /// mynaui-lock-hexagon
        /// Unicode: U+ed4a
        /// </summary>
        MynauiLockHexagon = 0xED4A,

        /// <summary>
        /// mynaui-lock-keyhole
        /// Unicode: U+ed4b
        /// </summary>
        MynauiLockKeyhole = 0xED4B,

        /// <summary>
        /// mynaui-lock-octagon
        /// Unicode: U+ed4c
        /// </summary>
        MynauiLockOctagon = 0xED4C,

        /// <summary>
        /// mynaui-lock-open
        /// Unicode: U+ed4f
        /// </summary>
        MynauiLockOpen = 0xED4F,

        /// <summary>
        /// mynaui-lock-open-keyhole
        /// Unicode: U+ed4d
        /// </summary>
        MynauiLockOpenKeyhole = 0xED4D,

        /// <summary>
        /// mynaui-lock-open-password
        /// Unicode: U+ed4e
        /// </summary>
        MynauiLockOpenPassword = 0xED4E,

        /// <summary>
        /// mynaui-lock-password
        /// Unicode: U+ed50
        /// </summary>
        MynauiLockPassword = 0xED50,

        /// <summary>
        /// mynaui-lock-square
        /// Unicode: U+ed51
        /// </summary>
        MynauiLockSquare = 0xED51,

        /// <summary>
        /// mynaui-lock-waves
        /// Unicode: U+ed52
        /// </summary>
        MynauiLockWaves = 0xED52,

        /// <summary>
        /// mynaui-login
        /// Unicode: U+ed54
        /// </summary>
        MynauiLogin = 0xED54,

        /// <summary>
        /// mynaui-logout
        /// Unicode: U+ed55
        /// </summary>
        MynauiLogout = 0xED55,

        /// <summary>
        /// mynaui-magnet
        /// Unicode: U+ed56
        /// </summary>
        MynauiMagnet = 0xED56,

        /// <summary>
        /// mynaui-male
        /// Unicode: U+ed57
        /// </summary>
        MynauiMale = 0xED57,

        /// <summary>
        /// mynaui-map
        /// Unicode: U+ed58
        /// </summary>
        MynauiMap = 0xED58,

        /// <summary>
        /// mynaui-mask
        /// Unicode: U+ed59
        /// </summary>
        MynauiMask = 0xED59,

        /// <summary>
        /// mynaui-math
        /// Unicode: U+ed5b
        /// </summary>
        MynauiMath = 0xED5B,

        /// <summary>
        /// mynaui-math-square
        /// Unicode: U+ed5a
        /// </summary>
        MynauiMathSquare = 0xED5A,

        /// <summary>
        /// mynaui-maximize
        /// Unicode: U+ed5d
        /// </summary>
        MynauiMaximize = 0xED5D,

        /// <summary>
        /// mynaui-maximize-one
        /// Unicode: U+ed5c
        /// </summary>
        MynauiMaximizeOne = 0xED5C,

        /// <summary>
        /// mynaui-menu
        /// Unicode: U+ed5e
        /// </summary>
        MynauiMenu = 0xED5E,

        /// <summary>
        /// mynaui-message
        /// Unicode: U+ed65
        /// </summary>
        MynauiMessage = 0xED65,

        /// <summary>
        /// mynaui-message-check
        /// Unicode: U+ed5f
        /// </summary>
        MynauiMessageCheck = 0xED5F,

        /// <summary>
        /// mynaui-message-dots
        /// Unicode: U+ed60
        /// </summary>
        MynauiMessageDots = 0xED60,

        /// <summary>
        /// mynaui-message-minus
        /// Unicode: U+ed61
        /// </summary>
        MynauiMessageMinus = 0xED61,

        /// <summary>
        /// mynaui-message-plus
        /// Unicode: U+ed62
        /// </summary>
        MynauiMessagePlus = 0xED62,

        /// <summary>
        /// mynaui-message-reply
        /// Unicode: U+ed63
        /// </summary>
        MynauiMessageReply = 0xED63,

        /// <summary>
        /// mynaui-message-x
        /// Unicode: U+ed64
        /// </summary>
        MynauiMessageX = 0xED64,

        /// <summary>
        /// mynaui-microphone
        /// Unicode: U+ed67
        /// </summary>
        MynauiMicrophone = 0xED67,

        /// <summary>
        /// mynaui-microphone-slash
        /// Unicode: U+ed66
        /// </summary>
        MynauiMicrophoneSlash = 0xED66,

        /// <summary>
        /// mynaui-minimize
        /// Unicode: U+ed69
        /// </summary>
        MynauiMinimize = 0xED69,

        /// <summary>
        /// mynaui-minimize-one
        /// Unicode: U+ed68
        /// </summary>
        MynauiMinimizeOne = 0xED68,

        /// <summary>
        /// mynaui-minus
        /// Unicode: U+ed70
        /// </summary>
        MynauiMinus = 0xED70,

        /// <summary>
        /// mynaui-minus-circle
        /// Unicode: U+ed6a
        /// </summary>
        MynauiMinusCircle = 0xED6A,

        /// <summary>
        /// mynaui-minus-diamond
        /// Unicode: U+ed6b
        /// </summary>
        MynauiMinusDiamond = 0xED6B,

        /// <summary>
        /// mynaui-minus-hexagon
        /// Unicode: U+ed6c
        /// </summary>
        MynauiMinusHexagon = 0xED6C,

        /// <summary>
        /// mynaui-minus-octagon
        /// Unicode: U+ed6d
        /// </summary>
        MynauiMinusOctagon = 0xED6D,

        /// <summary>
        /// mynaui-minus-square
        /// Unicode: U+ed6e
        /// </summary>
        MynauiMinusSquare = 0xED6E,

        /// <summary>
        /// mynaui-minus-waves
        /// Unicode: U+ed6f
        /// </summary>
        MynauiMinusWaves = 0xED6F,

        /// <summary>
        /// mynaui-mobile
        /// Unicode: U+ed76
        /// </summary>
        MynauiMobile = 0xED76,

        /// <summary>
        /// mynaui-mobile-signal-five
        /// Unicode: U+ed71
        /// </summary>
        MynauiMobileSignalFive = 0xED71,

        /// <summary>
        /// mynaui-mobile-signal-four
        /// Unicode: U+ed72
        /// </summary>
        MynauiMobileSignalFour = 0xED72,

        /// <summary>
        /// mynaui-mobile-signal-one
        /// Unicode: U+ed73
        /// </summary>
        MynauiMobileSignalOne = 0xED73,

        /// <summary>
        /// mynaui-mobile-signal-three
        /// Unicode: U+ed74
        /// </summary>
        MynauiMobileSignalThree = 0xED74,

        /// <summary>
        /// mynaui-mobile-signal-two
        /// Unicode: U+ed75
        /// </summary>
        MynauiMobileSignalTwo = 0xED75,

        /// <summary>
        /// mynaui-moon
        /// Unicode: U+ed78
        /// </summary>
        MynauiMoon = 0xED78,

        /// <summary>
        /// mynaui-moon-star
        /// Unicode: U+ed77
        /// </summary>
        MynauiMoonStar = 0xED77,

        /// <summary>
        /// mynaui-mountain
        /// Unicode: U+ed7a
        /// </summary>
        MynauiMountain = 0xED7A,

        /// <summary>
        /// mynaui-mountain-snow
        /// Unicode: U+ed79
        /// </summary>
        MynauiMountainSnow = 0xED79,

        /// <summary>
        /// mynaui-mouse-pointer
        /// Unicode: U+ed7b
        /// </summary>
        MynauiMousePointer = 0xED7B,

        /// <summary>
        /// mynaui-move
        /// Unicode: U+ed80
        /// </summary>
        MynauiMove = 0xED80,

        /// <summary>
        /// mynaui-move-diagonal
        /// Unicode: U+ed7d
        /// </summary>
        MynauiMoveDiagonal = 0xED7D,

        /// <summary>
        /// mynaui-move-diagonal-one
        /// Unicode: U+ed7c
        /// </summary>
        MynauiMoveDiagonalOne = 0xED7C,

        /// <summary>
        /// mynaui-move-horizontal
        /// Unicode: U+ed7e
        /// </summary>
        MynauiMoveHorizontal = 0xED7E,

        /// <summary>
        /// mynaui-move-vertical
        /// Unicode: U+ed7f
        /// </summary>
        MynauiMoveVertical = 0xED7F,

        /// <summary>
        /// mynaui-music
        /// Unicode: U+ed87
        /// </summary>
        MynauiMusic = 0xED87,

        /// <summary>
        /// mynaui-music-circle
        /// Unicode: U+ed81
        /// </summary>
        MynauiMusicCircle = 0xED81,

        /// <summary>
        /// mynaui-music-diamond
        /// Unicode: U+ed82
        /// </summary>
        MynauiMusicDiamond = 0xED82,

        /// <summary>
        /// mynaui-music-hexagon
        /// Unicode: U+ed83
        /// </summary>
        MynauiMusicHexagon = 0xED83,

        /// <summary>
        /// mynaui-music-octagon
        /// Unicode: U+ed84
        /// </summary>
        MynauiMusicOctagon = 0xED84,

        /// <summary>
        /// mynaui-music-square
        /// Unicode: U+ed85
        /// </summary>
        MynauiMusicSquare = 0xED85,

        /// <summary>
        /// mynaui-music-waves
        /// Unicode: U+ed86
        /// </summary>
        MynauiMusicWaves = 0xED86,

        /// <summary>
        /// mynaui-myna
        /// Unicode: U+ed88
        /// </summary>
        MynauiMyna = 0xED88,

        /// <summary>
        /// mynaui-navigation
        /// Unicode: U+ed8a
        /// </summary>
        MynauiNavigation = 0xED8A,

        /// <summary>
        /// mynaui-navigation-one
        /// Unicode: U+ed89
        /// </summary>
        MynauiNavigationOne = 0xED89,

        /// <summary>
        /// mynaui-nine
        /// Unicode: U+ed91
        /// </summary>
        MynauiNine = 0xED91,

        /// <summary>
        /// mynaui-nine-circle
        /// Unicode: U+ed8b
        /// </summary>
        MynauiNineCircle = 0xED8B,

        /// <summary>
        /// mynaui-nine-diamond
        /// Unicode: U+ed8c
        /// </summary>
        MynauiNineDiamond = 0xED8C,

        /// <summary>
        /// mynaui-nine-hexagon
        /// Unicode: U+ed8d
        /// </summary>
        MynauiNineHexagon = 0xED8D,

        /// <summary>
        /// mynaui-nine-octagon
        /// Unicode: U+ed8e
        /// </summary>
        MynauiNineOctagon = 0xED8E,

        /// <summary>
        /// mynaui-nine-square
        /// Unicode: U+ed8f
        /// </summary>
        MynauiNineSquare = 0xED8F,

        /// <summary>
        /// mynaui-nine-waves
        /// Unicode: U+ed90
        /// </summary>
        MynauiNineWaves = 0xED90,

        /// <summary>
        /// mynaui-notification
        /// Unicode: U+ed92
        /// </summary>
        MynauiNotification = 0xED92,

        /// <summary>
        /// mynaui-octagon
        /// Unicode: U+ed93
        /// </summary>
        MynauiOctagon = 0xED93,

        /// <summary>
        /// mynaui-one
        /// Unicode: U+ed9a
        /// </summary>
        MynauiOne = 0xED9A,

        /// <summary>
        /// mynaui-one-circle
        /// Unicode: U+ed94
        /// </summary>
        MynauiOneCircle = 0xED94,

        /// <summary>
        /// mynaui-one-diamond
        /// Unicode: U+ed95
        /// </summary>
        MynauiOneDiamond = 0xED95,

        /// <summary>
        /// mynaui-one-hexagon
        /// Unicode: U+ed96
        /// </summary>
        MynauiOneHexagon = 0xED96,

        /// <summary>
        /// mynaui-one-octagon
        /// Unicode: U+ed97
        /// </summary>
        MynauiOneOctagon = 0xED97,

        /// <summary>
        /// mynaui-one-square
        /// Unicode: U+ed98
        /// </summary>
        MynauiOneSquare = 0xED98,

        /// <summary>
        /// mynaui-one-waves
        /// Unicode: U+ed99
        /// </summary>
        MynauiOneWaves = 0xED99,

        /// <summary>
        /// mynaui-option
        /// Unicode: U+ed9b
        /// </summary>
        MynauiOption = 0xED9B,

        /// <summary>
        /// mynaui-package
        /// Unicode: U+ed9c
        /// </summary>
        MynauiPackage = 0xED9C,

        /// <summary>
        /// mynaui-paint
        /// Unicode: U+ed9d
        /// </summary>
        MynauiPaint = 0xED9D,

        /// <summary>
        /// mynaui-panel-bottom
        /// Unicode: U+eda1
        /// </summary>
        MynauiPanelBottom = 0xEDA1,

        /// <summary>
        /// mynaui-panel-bottom-close
        /// Unicode: U+ed9e
        /// </summary>
        MynauiPanelBottomClose = 0xED9E,

        /// <summary>
        /// mynaui-panel-bottom-inactive
        /// Unicode: U+ed9f
        /// </summary>
        MynauiPanelBottomInactive = 0xED9F,

        /// <summary>
        /// mynaui-panel-bottom-open
        /// Unicode: U+eda0
        /// </summary>
        MynauiPanelBottomOpen = 0xEDA0,

        /// <summary>
        /// mynaui-panel-left
        /// Unicode: U+eda5
        /// </summary>
        MynauiPanelLeft = 0xEDA5,

        /// <summary>
        /// mynaui-panel-left-close
        /// Unicode: U+eda2
        /// </summary>
        MynauiPanelLeftClose = 0xEDA2,

        /// <summary>
        /// mynaui-panel-left-inactive
        /// Unicode: U+eda3
        /// </summary>
        MynauiPanelLeftInactive = 0xEDA3,

        /// <summary>
        /// mynaui-panel-left-open
        /// Unicode: U+eda4
        /// </summary>
        MynauiPanelLeftOpen = 0xEDA4,

        /// <summary>
        /// mynaui-panel-right
        /// Unicode: U+eda9
        /// </summary>
        MynauiPanelRight = 0xEDA9,

        /// <summary>
        /// mynaui-panel-right-close
        /// Unicode: U+eda6
        /// </summary>
        MynauiPanelRightClose = 0xEDA6,

        /// <summary>
        /// mynaui-panel-right-inactive
        /// Unicode: U+eda7
        /// </summary>
        MynauiPanelRightInactive = 0xEDA7,

        /// <summary>
        /// mynaui-panel-right-open
        /// Unicode: U+eda8
        /// </summary>
        MynauiPanelRightOpen = 0xEDA8,

        /// <summary>
        /// mynaui-panel-top
        /// Unicode: U+edad
        /// </summary>
        MynauiPanelTop = 0xEDAD,

        /// <summary>
        /// mynaui-panel-top-close
        /// Unicode: U+edaa
        /// </summary>
        MynauiPanelTopClose = 0xEDAA,

        /// <summary>
        /// mynaui-panel-top-inactive
        /// Unicode: U+edab
        /// </summary>
        MynauiPanelTopInactive = 0xEDAB,

        /// <summary>
        /// mynaui-panel-top-open
        /// Unicode: U+edac
        /// </summary>
        MynauiPanelTopOpen = 0xEDAC,

        /// <summary>
        /// mynaui-paperclip
        /// Unicode: U+edae
        /// </summary>
        MynauiPaperclip = 0xEDAE,

        /// <summary>
        /// mynaui-parking
        /// Unicode: U+edaf
        /// </summary>
        MynauiParking = 0xEDAF,

        /// <summary>
        /// mynaui-password
        /// Unicode: U+edb0
        /// </summary>
        MynauiPassword = 0xEDB0,

        /// <summary>
        /// mynaui-path
        /// Unicode: U+edb1
        /// </summary>
        MynauiPath = 0xEDB1,

        /// <summary>
        /// mynaui-pause
        /// Unicode: U+edb8
        /// </summary>
        MynauiPause = 0xEDB8,

        /// <summary>
        /// mynaui-pause-circle
        /// Unicode: U+edb2
        /// </summary>
        MynauiPauseCircle = 0xEDB2,

        /// <summary>
        /// mynaui-pause-diamond
        /// Unicode: U+edb3
        /// </summary>
        MynauiPauseDiamond = 0xEDB3,

        /// <summary>
        /// mynaui-pause-hexagon
        /// Unicode: U+edb4
        /// </summary>
        MynauiPauseHexagon = 0xEDB4,

        /// <summary>
        /// mynaui-pause-octagon
        /// Unicode: U+edb5
        /// </summary>
        MynauiPauseOctagon = 0xEDB5,

        /// <summary>
        /// mynaui-pause-square
        /// Unicode: U+edb6
        /// </summary>
        MynauiPauseSquare = 0xEDB6,

        /// <summary>
        /// mynaui-pause-waves
        /// Unicode: U+edb7
        /// </summary>
        MynauiPauseWaves = 0xEDB7,

        /// <summary>
        /// mynaui-pen
        /// Unicode: U+edb9
        /// </summary>
        MynauiPen = 0xEDB9,

        /// <summary>
        /// mynaui-pencil
        /// Unicode: U+edba
        /// </summary>
        MynauiPencil = 0xEDBA,

        /// <summary>
        /// mynaui-percentage
        /// Unicode: U+edc1
        /// </summary>
        MynauiPercentage = 0xEDC1,

        /// <summary>
        /// mynaui-percentage-circle
        /// Unicode: U+edbb
        /// </summary>
        MynauiPercentageCircle = 0xEDBB,

        /// <summary>
        /// mynaui-percentage-diamond
        /// Unicode: U+edbc
        /// </summary>
        MynauiPercentageDiamond = 0xEDBC,

        /// <summary>
        /// mynaui-percentage-hexagon
        /// Unicode: U+edbd
        /// </summary>
        MynauiPercentageHexagon = 0xEDBD,

        /// <summary>
        /// mynaui-percentage-octagon
        /// Unicode: U+edbe
        /// </summary>
        MynauiPercentageOctagon = 0xEDBE,

        /// <summary>
        /// mynaui-percentage-square
        /// Unicode: U+edbf
        /// </summary>
        MynauiPercentageSquare = 0xEDBF,

        /// <summary>
        /// mynaui-percentage-waves
        /// Unicode: U+edc0
        /// </summary>
        MynauiPercentageWaves = 0xEDC0,

        /// <summary>
        /// mynaui-pin
        /// Unicode: U+edc2
        /// </summary>
        MynauiPin = 0xEDC2,

        /// <summary>
        /// mynaui-pizza
        /// Unicode: U+edc3
        /// </summary>
        MynauiPizza = 0xEDC3,

        /// <summary>
        /// mynaui-planet
        /// Unicode: U+edc4
        /// </summary>
        MynauiPlanet = 0xEDC4,

        /// <summary>
        /// mynaui-play
        /// Unicode: U+edcb
        /// </summary>
        MynauiPlay = 0xEDCB,

        /// <summary>
        /// mynaui-play-circle
        /// Unicode: U+edc5
        /// </summary>
        MynauiPlayCircle = 0xEDC5,

        /// <summary>
        /// mynaui-play-diamond
        /// Unicode: U+edc6
        /// </summary>
        MynauiPlayDiamond = 0xEDC6,

        /// <summary>
        /// mynaui-play-hexagon
        /// Unicode: U+edc7
        /// </summary>
        MynauiPlayHexagon = 0xEDC7,

        /// <summary>
        /// mynaui-play-octagon
        /// Unicode: U+edc8
        /// </summary>
        MynauiPlayOctagon = 0xEDC8,

        /// <summary>
        /// mynaui-play-square
        /// Unicode: U+edc9
        /// </summary>
        MynauiPlaySquare = 0xEDC9,

        /// <summary>
        /// mynaui-play-waves
        /// Unicode: U+edca
        /// </summary>
        MynauiPlayWaves = 0xEDCA,

        /// <summary>
        /// mynaui-plus
        /// Unicode: U+edd2
        /// </summary>
        MynauiPlus = 0xEDD2,

        /// <summary>
        /// mynaui-plus-circle
        /// Unicode: U+edcc
        /// </summary>
        MynauiPlusCircle = 0xEDCC,

        /// <summary>
        /// mynaui-plus-diamond
        /// Unicode: U+edcd
        /// </summary>
        MynauiPlusDiamond = 0xEDCD,

        /// <summary>
        /// mynaui-plus-hexagon
        /// Unicode: U+edce
        /// </summary>
        MynauiPlusHexagon = 0xEDCE,

        /// <summary>
        /// mynaui-plus-octagon
        /// Unicode: U+edcf
        /// </summary>
        MynauiPlusOctagon = 0xEDCF,

        /// <summary>
        /// mynaui-plus-square
        /// Unicode: U+edd0
        /// </summary>
        MynauiPlusSquare = 0xEDD0,

        /// <summary>
        /// mynaui-plus-waves
        /// Unicode: U+edd1
        /// </summary>
        MynauiPlusWaves = 0xEDD1,

        /// <summary>
        /// mynaui-pokeball
        /// Unicode: U+edd3
        /// </summary>
        MynauiPokeball = 0xEDD3,

        /// <summary>
        /// mynaui-power
        /// Unicode: U+edd4
        /// </summary>
        MynauiPower = 0xEDD4,

        /// <summary>
        /// mynaui-presentation
        /// Unicode: U+edd5
        /// </summary>
        MynauiPresentation = 0xEDD5,

        /// <summary>
        /// mynaui-printer
        /// Unicode: U+edd6
        /// </summary>
        MynauiPrinter = 0xEDD6,

        /// <summary>
        /// mynaui-puzzle
        /// Unicode: U+edd7
        /// </summary>
        MynauiPuzzle = 0xEDD7,

        /// <summary>
        /// mynaui-question
        /// Unicode: U+edde
        /// </summary>
        MynauiQuestion = 0xEDDE,

        /// <summary>
        /// mynaui-question-circle
        /// Unicode: U+edd8
        /// </summary>
        MynauiQuestionCircle = 0xEDD8,

        /// <summary>
        /// mynaui-question-diamond
        /// Unicode: U+edd9
        /// </summary>
        MynauiQuestionDiamond = 0xEDD9,

        /// <summary>
        /// mynaui-question-hexagon
        /// Unicode: U+edda
        /// </summary>
        MynauiQuestionHexagon = 0xEDDA,

        /// <summary>
        /// mynaui-question-octagon
        /// Unicode: U+eddb
        /// </summary>
        MynauiQuestionOctagon = 0xEDDB,

        /// <summary>
        /// mynaui-question-square
        /// Unicode: U+eddc
        /// </summary>
        MynauiQuestionSquare = 0xEDDC,

        /// <summary>
        /// mynaui-question-waves
        /// Unicode: U+eddd
        /// </summary>
        MynauiQuestionWaves = 0xEDDD,

        /// <summary>
        /// mynaui-radio
        /// Unicode: U+eddf
        /// </summary>
        MynauiRadio = 0xEDDF,

        /// <summary>
        /// mynaui-rainbow
        /// Unicode: U+ede0
        /// </summary>
        MynauiRainbow = 0xEDE0,

        /// <summary>
        /// mynaui-reception-bell
        /// Unicode: U+ede1
        /// </summary>
        MynauiReceptionBell = 0xEDE1,

        /// <summary>
        /// mynaui-record
        /// Unicode: U+ede2
        /// </summary>
        MynauiRecord = 0xEDE2,

        /// <summary>
        /// mynaui-rectangle
        /// Unicode: U+ede4
        /// </summary>
        MynauiRectangle = 0xEDE4,

        /// <summary>
        /// mynaui-rectangle-vertical
        /// Unicode: U+ede3
        /// </summary>
        MynauiRectangleVertical = 0xEDE3,

        /// <summary>
        /// mynaui-redo
        /// Unicode: U+ede5
        /// </summary>
        MynauiRedo = 0xEDE5,

        /// <summary>
        /// mynaui-refresh
        /// Unicode: U+ede7
        /// </summary>
        MynauiRefresh = 0xEDE7,

        /// <summary>
        /// mynaui-refresh-alt
        /// Unicode: U+ede6
        /// </summary>
        MynauiRefreshAlt = 0xEDE6,

        /// <summary>
        /// mynaui-repeat
        /// Unicode: U+ede8
        /// </summary>
        MynauiRepeat = 0xEDE8,

        /// <summary>
        /// mynaui-rewind
        /// Unicode: U+edef
        /// </summary>
        MynauiRewind = 0xEDEF,

        /// <summary>
        /// mynaui-rewind-circle
        /// Unicode: U+ede9
        /// </summary>
        MynauiRewindCircle = 0xEDE9,

        /// <summary>
        /// mynaui-rewind-diamond
        /// Unicode: U+edea
        /// </summary>
        MynauiRewindDiamond = 0xEDEA,

        /// <summary>
        /// mynaui-rewind-hexagon
        /// Unicode: U+edeb
        /// </summary>
        MynauiRewindHexagon = 0xEDEB,

        /// <summary>
        /// mynaui-rewind-octagon
        /// Unicode: U+edec
        /// </summary>
        MynauiRewindOctagon = 0xEDEC,

        /// <summary>
        /// mynaui-rewind-square
        /// Unicode: U+eded
        /// </summary>
        MynauiRewindSquare = 0xEDED,

        /// <summary>
        /// mynaui-rewind-waves
        /// Unicode: U+edee
        /// </summary>
        MynauiRewindWaves = 0xEDEE,

        /// <summary>
        /// mynaui-rhombus
        /// Unicode: U+edf0
        /// </summary>
        MynauiRhombus = 0xEDF0,

        /// <summary>
        /// mynaui-ribbon
        /// Unicode: U+edf1
        /// </summary>
        MynauiRibbon = 0xEDF1,

        /// <summary>
        /// mynaui-rocket
        /// Unicode: U+edf2
        /// </summary>
        MynauiRocket = 0xEDF2,

        /// <summary>
        /// mynaui-room-service
        /// Unicode: U+edf3
        /// </summary>
        MynauiRoomService = 0xEDF3,

        /// <summary>
        /// mynaui-rows
        /// Unicode: U+edf4
        /// </summary>
        MynauiRows = 0xEDF4,

        /// <summary>
        /// mynaui-rss
        /// Unicode: U+edf5
        /// </summary>
        MynauiRss = 0xEDF5,

        /// <summary>
        /// mynaui-ruler
        /// Unicode: U+edf6
        /// </summary>
        MynauiRuler = 0xEDF6,

        /// <summary>
        /// mynaui-rupee
        /// Unicode: U+edfd
        /// </summary>
        MynauiRupee = 0xEDFD,

        /// <summary>
        /// mynaui-rupee-circle
        /// Unicode: U+edf7
        /// </summary>
        MynauiRupeeCircle = 0xEDF7,

        /// <summary>
        /// mynaui-rupee-diamond
        /// Unicode: U+edf8
        /// </summary>
        MynauiRupeeDiamond = 0xEDF8,

        /// <summary>
        /// mynaui-rupee-hexagon
        /// Unicode: U+edf9
        /// </summary>
        MynauiRupeeHexagon = 0xEDF9,

        /// <summary>
        /// mynaui-rupee-octagon
        /// Unicode: U+edfa
        /// </summary>
        MynauiRupeeOctagon = 0xEDFA,

        /// <summary>
        /// mynaui-rupee-square
        /// Unicode: U+edfb
        /// </summary>
        MynauiRupeeSquare = 0xEDFB,

        /// <summary>
        /// mynaui-rupee-waves
        /// Unicode: U+edfc
        /// </summary>
        MynauiRupeeWaves = 0xEDFC,

        /// <summary>
        /// mynaui-sad-circle
        /// Unicode: U+edfe
        /// </summary>
        MynauiSadCircle = 0xEDFE,

        /// <summary>
        /// mynaui-sad-ghost
        /// Unicode: U+edff
        /// </summary>
        MynauiSadGhost = 0xEDFF,

        /// <summary>
        /// mynaui-sad-square
        /// Unicode: U+ee00
        /// </summary>
        MynauiSadSquare = 0xEE00,

        /// <summary>
        /// mynaui-save
        /// Unicode: U+ee01
        /// </summary>
        MynauiSave = 0xEE01,

        /// <summary>
        /// mynaui-scan
        /// Unicode: U+ee02
        /// </summary>
        MynauiScan = 0xEE02,

        /// <summary>
        /// mynaui-scissors
        /// Unicode: U+ee03
        /// </summary>
        MynauiScissors = 0xEE03,

        /// <summary>
        /// mynaui-search
        /// Unicode: U+ee14
        /// </summary>
        MynauiSearch = 0xEE14,

        /// <summary>
        /// mynaui-search-check
        /// Unicode: U+ee05
        /// </summary>
        MynauiSearchCheck = 0xEE05,

        /// <summary>
        /// mynaui-search-circle
        /// Unicode: U+ee06
        /// </summary>
        MynauiSearchCircle = 0xEE06,

        /// <summary>
        /// mynaui-search-diamond
        /// Unicode: U+ee07
        /// </summary>
        MynauiSearchDiamond = 0xEE07,

        /// <summary>
        /// mynaui-search-dot
        /// Unicode: U+ee08
        /// </summary>
        MynauiSearchDot = 0xEE08,

        /// <summary>
        /// mynaui-search-hexagon
        /// Unicode: U+ee09
        /// </summary>
        MynauiSearchHexagon = 0xEE09,

        /// <summary>
        /// mynaui-search-home
        /// Unicode: U+ee0a
        /// </summary>
        MynauiSearchHome = 0xEE0A,

        /// <summary>
        /// mynaui-search-minus
        /// Unicode: U+ee0b
        /// </summary>
        MynauiSearchMinus = 0xEE0B,

        /// <summary>
        /// mynaui-search-octagon
        /// Unicode: U+ee0c
        /// </summary>
        MynauiSearchOctagon = 0xEE0C,

        /// <summary>
        /// mynaui-search-plus
        /// Unicode: U+ee0d
        /// </summary>
        MynauiSearchPlus = 0xEE0D,

        /// <summary>
        /// mynaui-search-slash
        /// Unicode: U+ee0e
        /// </summary>
        MynauiSearchSlash = 0xEE0E,

        /// <summary>
        /// mynaui-search-snooze
        /// Unicode: U+ee0f
        /// </summary>
        MynauiSearchSnooze = 0xEE0F,

        /// <summary>
        /// mynaui-search-square
        /// Unicode: U+ee10
        /// </summary>
        MynauiSearchSquare = 0xEE10,

        /// <summary>
        /// mynaui-search-user
        /// Unicode: U+ee11
        /// </summary>
        MynauiSearchUser = 0xEE11,

        /// <summary>
        /// mynaui-search-waves
        /// Unicode: U+ee12
        /// </summary>
        MynauiSearchWaves = 0xEE12,

        /// <summary>
        /// mynaui-search-x
        /// Unicode: U+ee13
        /// </summary>
        MynauiSearchX = 0xEE13,

        /// <summary>
        /// mynaui-sea-waves
        /// Unicode: U+ee04
        /// </summary>
        MynauiSeaWaves = 0xEE04,

        /// <summary>
        /// mynaui-select-multiple
        /// Unicode: U+ee15
        /// </summary>
        MynauiSelectMultiple = 0xEE15,

        /// <summary>
        /// mynaui-send
        /// Unicode: U+ee16
        /// </summary>
        MynauiSend = 0xEE16,

        /// <summary>
        /// mynaui-servers
        /// Unicode: U+ee17
        /// </summary>
        MynauiServers = 0xEE17,

        /// <summary>
        /// mynaui-seven
        /// Unicode: U+ee1e
        /// </summary>
        MynauiSeven = 0xEE1E,

        /// <summary>
        /// mynaui-seven-circle
        /// Unicode: U+ee18
        /// </summary>
        MynauiSevenCircle = 0xEE18,

        /// <summary>
        /// mynaui-seven-diamond
        /// Unicode: U+ee19
        /// </summary>
        MynauiSevenDiamond = 0xEE19,

        /// <summary>
        /// mynaui-seven-hexagon
        /// Unicode: U+ee1a
        /// </summary>
        MynauiSevenHexagon = 0xEE1A,

        /// <summary>
        /// mynaui-seven-octagon
        /// Unicode: U+ee1b
        /// </summary>
        MynauiSevenOctagon = 0xEE1B,

        /// <summary>
        /// mynaui-seven-square
        /// Unicode: U+ee1c
        /// </summary>
        MynauiSevenSquare = 0xEE1C,

        /// <summary>
        /// mynaui-seven-waves
        /// Unicode: U+ee1d
        /// </summary>
        MynauiSevenWaves = 0xEE1D,

        /// <summary>
        /// mynaui-share
        /// Unicode: U+ee1f
        /// </summary>
        MynauiShare = 0xEE1F,

        /// <summary>
        /// mynaui-shell
        /// Unicode: U+ee20
        /// </summary>
        MynauiShell = 0xEE20,

        /// <summary>
        /// mynaui-shield
        /// Unicode: U+ee29
        /// </summary>
        MynauiShield = 0xEE29,

        /// <summary>
        /// mynaui-shield-check
        /// Unicode: U+ee21
        /// </summary>
        MynauiShieldCheck = 0xEE21,

        /// <summary>
        /// mynaui-shield-crossed
        /// Unicode: U+ee22
        /// </summary>
        MynauiShieldCrossed = 0xEE22,

        /// <summary>
        /// mynaui-shield-minus
        /// Unicode: U+ee23
        /// </summary>
        MynauiShieldMinus = 0xEE23,

        /// <summary>
        /// mynaui-shield-one
        /// Unicode: U+ee24
        /// </summary>
        MynauiShieldOne = 0xEE24,

        /// <summary>
        /// mynaui-shield-plus
        /// Unicode: U+ee25
        /// </summary>
        MynauiShieldPlus = 0xEE25,

        /// <summary>
        /// mynaui-shield-slash
        /// Unicode: U+ee26
        /// </summary>
        MynauiShieldSlash = 0xEE26,

        /// <summary>
        /// mynaui-shield-two
        /// Unicode: U+ee27
        /// </summary>
        MynauiShieldTwo = 0xEE27,

        /// <summary>
        /// mynaui-shield-x
        /// Unicode: U+ee28
        /// </summary>
        MynauiShieldX = 0xEE28,

        /// <summary>
        /// mynaui-shooting-star
        /// Unicode: U+ee2a
        /// </summary>
        MynauiShootingStar = 0xEE2A,

        /// <summary>
        /// mynaui-shopping-bag
        /// Unicode: U+ee2b
        /// </summary>
        MynauiShoppingBag = 0xEE2B,

        /// <summary>
        /// mynaui-shovel
        /// Unicode: U+ee2c
        /// </summary>
        MynauiShovel = 0xEE2C,

        /// <summary>
        /// mynaui-shrub
        /// Unicode: U+ee2d
        /// </summary>
        MynauiShrub = 0xEE2D,

        /// <summary>
        /// mynaui-shuffle
        /// Unicode: U+ee2f
        /// </summary>
        MynauiShuffle = 0xEE2F,

        /// <summary>
        /// mynaui-shuffle-alt
        /// Unicode: U+ee2e
        /// </summary>
        MynauiShuffleAlt = 0xEE2E,

        /// <summary>
        /// mynaui-sidebar
        /// Unicode: U+ee31
        /// </summary>
        MynauiSidebar = 0xEE31,

        /// <summary>
        /// mynaui-sidebar-alt
        /// Unicode: U+ee30
        /// </summary>
        MynauiSidebarAlt = 0xEE30,

        /// <summary>
        /// mynaui-signal
        /// Unicode: U+ee38
        /// </summary>
        MynauiSignal = 0xEE38,

        /// <summary>
        /// mynaui-signal-circle
        /// Unicode: U+ee32
        /// </summary>
        MynauiSignalCircle = 0xEE32,

        /// <summary>
        /// mynaui-signal-diamond
        /// Unicode: U+ee33
        /// </summary>
        MynauiSignalDiamond = 0xEE33,

        /// <summary>
        /// mynaui-signal-hexagon
        /// Unicode: U+ee34
        /// </summary>
        MynauiSignalHexagon = 0xEE34,

        /// <summary>
        /// mynaui-signal-octagon
        /// Unicode: U+ee35
        /// </summary>
        MynauiSignalOctagon = 0xEE35,

        /// <summary>
        /// mynaui-signal-square
        /// Unicode: U+ee36
        /// </summary>
        MynauiSignalSquare = 0xEE36,

        /// <summary>
        /// mynaui-signal-waves
        /// Unicode: U+ee37
        /// </summary>
        MynauiSignalWaves = 0xEE37,

        /// <summary>
        /// mynaui-six
        /// Unicode: U+ee3f
        /// </summary>
        MynauiSix = 0xEE3F,

        /// <summary>
        /// mynaui-six-circle
        /// Unicode: U+ee39
        /// </summary>
        MynauiSixCircle = 0xEE39,

        /// <summary>
        /// mynaui-six-diamond
        /// Unicode: U+ee3a
        /// </summary>
        MynauiSixDiamond = 0xEE3A,

        /// <summary>
        /// mynaui-six-hexagon
        /// Unicode: U+ee3b
        /// </summary>
        MynauiSixHexagon = 0xEE3B,

        /// <summary>
        /// mynaui-six-octagon
        /// Unicode: U+ee3c
        /// </summary>
        MynauiSixOctagon = 0xEE3C,

        /// <summary>
        /// mynaui-six-square
        /// Unicode: U+ee3d
        /// </summary>
        MynauiSixSquare = 0xEE3D,

        /// <summary>
        /// mynaui-six-waves
        /// Unicode: U+ee3e
        /// </summary>
        MynauiSixWaves = 0xEE3E,

        /// <summary>
        /// mynaui-skip-back
        /// Unicode: U+ee40
        /// </summary>
        MynauiSkipBack = 0xEE40,

        /// <summary>
        /// mynaui-skip-forward
        /// Unicode: U+ee41
        /// </summary>
        MynauiSkipForward = 0xEE41,

        /// <summary>
        /// mynaui-slash-circle
        /// Unicode: U+ee42
        /// </summary>
        MynauiSlashCircle = 0xEE42,

        /// <summary>
        /// mynaui-slash-diamond
        /// Unicode: U+ee43
        /// </summary>
        MynauiSlashDiamond = 0xEE43,

        /// <summary>
        /// mynaui-slash-hexagon
        /// Unicode: U+ee44
        /// </summary>
        MynauiSlashHexagon = 0xEE44,

        /// <summary>
        /// mynaui-slash-octagon
        /// Unicode: U+ee45
        /// </summary>
        MynauiSlashOctagon = 0xEE45,

        /// <summary>
        /// mynaui-slash-square
        /// Unicode: U+ee46
        /// </summary>
        MynauiSlashSquare = 0xEE46,

        /// <summary>
        /// mynaui-slash-waves
        /// Unicode: U+ee47
        /// </summary>
        MynauiSlashWaves = 0xEE47,

        /// <summary>
        /// mynaui-smile-circle
        /// Unicode: U+ee48
        /// </summary>
        MynauiSmileCircle = 0xEE48,

        /// <summary>
        /// mynaui-smile-ghost
        /// Unicode: U+ee49
        /// </summary>
        MynauiSmileGhost = 0xEE49,

        /// <summary>
        /// mynaui-smile-square
        /// Unicode: U+ee4a
        /// </summary>
        MynauiSmileSquare = 0xEE4A,

        /// <summary>
        /// mynaui-snow
        /// Unicode: U+ee4b
        /// </summary>
        MynauiSnow = 0xEE4B,

        /// <summary>
        /// mynaui-snowflake
        /// Unicode: U+ee4c
        /// </summary>
        MynauiSnowflake = 0xEE4C,

        /// <summary>
        /// mynaui-sofa
        /// Unicode: U+ee4d
        /// </summary>
        MynauiSofa = 0xEE4D,

        /// <summary>
        /// mynaui-sort
        /// Unicode: U+ee4e
        /// </summary>
        MynauiSort = 0xEE4E,

        /// <summary>
        /// mynaui-sparkles
        /// Unicode: U+ee4f
        /// </summary>
        MynauiSparkles = 0xEE4F,

        /// <summary>
        /// mynaui-speaker
        /// Unicode: U+ee50
        /// </summary>
        MynauiSpeaker = 0xEE50,

        /// <summary>
        /// mynaui-spinner
        /// Unicode: U+ee52
        /// </summary>
        MynauiSpinner = 0xEE52,

        /// <summary>
        /// mynaui-spinner-one
        /// Unicode: U+ee51
        /// </summary>
        MynauiSpinnerOne = 0xEE51,

        /// <summary>
        /// mynaui-sprout
        /// Unicode: U+ee53
        /// </summary>
        MynauiSprout = 0xEE53,

        /// <summary>
        /// mynaui-square
        /// Unicode: U+ee59
        /// </summary>
        MynauiSquare = 0xEE59,

        /// <summary>
        /// mynaui-square-chart-gantt
        /// Unicode: U+ee54
        /// </summary>
        MynauiSquareChartGantt = 0xEE54,

        /// <summary>
        /// mynaui-square-dashed
        /// Unicode: U+ee56
        /// </summary>
        MynauiSquareDashed = 0xEE56,

        /// <summary>
        /// mynaui-square-dashed-kanban
        /// Unicode: U+ee55
        /// </summary>
        MynauiSquareDashedKanban = 0xEE55,

        /// <summary>
        /// mynaui-square-half
        /// Unicode: U+ee57
        /// </summary>
        MynauiSquareHalf = 0xEE57,

        /// <summary>
        /// mynaui-square-kanban
        /// Unicode: U+ee58
        /// </summary>
        MynauiSquareKanban = 0xEE58,

        /// <summary>
        /// mynaui-star
        /// Unicode: U+ee5a
        /// </summary>
        MynauiStar = 0xEE5A,

        /// <summary>
        /// mynaui-stop
        /// Unicode: U+ee61
        /// </summary>
        MynauiStop = 0xEE61,

        /// <summary>
        /// mynaui-stop-circle
        /// Unicode: U+ee5b
        /// </summary>
        MynauiStopCircle = 0xEE5B,

        /// <summary>
        /// mynaui-stop-diamond
        /// Unicode: U+ee5c
        /// </summary>
        MynauiStopDiamond = 0xEE5C,

        /// <summary>
        /// mynaui-stop-hexagon
        /// Unicode: U+ee5d
        /// </summary>
        MynauiStopHexagon = 0xEE5D,

        /// <summary>
        /// mynaui-stop-octagon
        /// Unicode: U+ee5e
        /// </summary>
        MynauiStopOctagon = 0xEE5E,

        /// <summary>
        /// mynaui-stop-square
        /// Unicode: U+ee5f
        /// </summary>
        MynauiStopSquare = 0xEE5F,

        /// <summary>
        /// mynaui-stop-waves
        /// Unicode: U+ee60
        /// </summary>
        MynauiStopWaves = 0xEE60,

        /// <summary>
        /// mynaui-store
        /// Unicode: U+ee62
        /// </summary>
        MynauiStore = 0xEE62,

        /// <summary>
        /// mynaui-subtract
        /// Unicode: U+ee63
        /// </summary>
        MynauiSubtract = 0xEE63,

        /// <summary>
        /// mynaui-sun
        /// Unicode: U+ee67
        /// </summary>
        MynauiSun = 0xEE67,

        /// <summary>
        /// mynaui-sun-dim
        /// Unicode: U+ee64
        /// </summary>
        MynauiSunDim = 0xEE64,

        /// <summary>
        /// mynaui-sun-medium
        /// Unicode: U+ee65
        /// </summary>
        MynauiSunMedium = 0xEE65,

        /// <summary>
        /// mynaui-sunrise
        /// Unicode: U+ee68
        /// </summary>
        MynauiSunrise = 0xEE68,

        /// <summary>
        /// mynaui-sunset
        /// Unicode: U+ee69
        /// </summary>
        MynauiSunset = 0xEE69,

        /// <summary>
        /// mynaui-sun-snow
        /// Unicode: U+ee66
        /// </summary>
        MynauiSunSnow = 0xEE66,

        /// <summary>
        /// mynaui-support
        /// Unicode: U+ee6a
        /// </summary>
        MynauiSupport = 0xEE6A,

        /// <summary>
        /// mynaui-swatches
        /// Unicode: U+ee6b
        /// </summary>
        MynauiSwatches = 0xEE6B,

        /// <summary>
        /// mynaui-table
        /// Unicode: U+ee6c
        /// </summary>
        MynauiTable = 0xEE6C,

        /// <summary>
        /// mynaui-tablet
        /// Unicode: U+ee6d
        /// </summary>
        MynauiTablet = 0xEE6D,

        /// <summary>
        /// mynaui-tag
        /// Unicode: U+ee6f
        /// </summary>
        MynauiTag = 0xEE6F,

        /// <summary>
        /// mynaui-tag-plus
        /// Unicode: U+ee6e
        /// </summary>
        MynauiTagPlus = 0xEE6E,

        /// <summary>
        /// mynaui-tally-five
        /// Unicode: U+ee70
        /// </summary>
        MynauiTallyFive = 0xEE70,

        /// <summary>
        /// mynaui-tally-four
        /// Unicode: U+ee71
        /// </summary>
        MynauiTallyFour = 0xEE71,

        /// <summary>
        /// mynaui-tally-one
        /// Unicode: U+ee72
        /// </summary>
        MynauiTallyOne = 0xEE72,

        /// <summary>
        /// mynaui-tally-three
        /// Unicode: U+ee73
        /// </summary>
        MynauiTallyThree = 0xEE73,

        /// <summary>
        /// mynaui-tally-two
        /// Unicode: U+ee74
        /// </summary>
        MynauiTallyTwo = 0xEE74,

        /// <summary>
        /// mynaui-target
        /// Unicode: U+ee75
        /// </summary>
        MynauiTarget = 0xEE75,

        /// <summary>
        /// mynaui-telephone
        /// Unicode: U+ee7c
        /// </summary>
        MynauiTelephone = 0xEE7C,

        /// <summary>
        /// mynaui-telephone-call
        /// Unicode: U+ee76
        /// </summary>
        MynauiTelephoneCall = 0xEE76,

        /// <summary>
        /// mynaui-telephone-forward
        /// Unicode: U+ee77
        /// </summary>
        MynauiTelephoneForward = 0xEE77,

        /// <summary>
        /// mynaui-telephone-in
        /// Unicode: U+ee78
        /// </summary>
        MynauiTelephoneIn = 0xEE78,

        /// <summary>
        /// mynaui-telephone-missed
        /// Unicode: U+ee79
        /// </summary>
        MynauiTelephoneMissed = 0xEE79,

        /// <summary>
        /// mynaui-telephone-out
        /// Unicode: U+ee7a
        /// </summary>
        MynauiTelephoneOut = 0xEE7A,

        /// <summary>
        /// mynaui-telephone-slash
        /// Unicode: U+ee7b
        /// </summary>
        MynauiTelephoneSlash = 0xEE7B,

        /// <summary>
        /// mynaui-tent
        /// Unicode: U+ee7e
        /// </summary>
        MynauiTent = 0xEE7E,

        /// <summary>
        /// mynaui-tent-tree
        /// Unicode: U+ee7d
        /// </summary>
        MynauiTentTree = 0xEE7D,

        /// <summary>
        /// mynaui-terminal
        /// Unicode: U+ee7f
        /// </summary>
        MynauiTerminal = 0xEE7F,

        /// <summary>
        /// mynaui-text-align-center
        /// Unicode: U+ee80
        /// </summary>
        MynauiTextAlignCenter = 0xEE80,

        /// <summary>
        /// mynaui-text-align-left
        /// Unicode: U+ee81
        /// </summary>
        MynauiTextAlignLeft = 0xEE81,

        /// <summary>
        /// mynaui-text-align-right
        /// Unicode: U+ee82
        /// </summary>
        MynauiTextAlignRight = 0xEE82,

        /// <summary>
        /// mynaui-text-justify
        /// Unicode: U+ee83
        /// </summary>
        MynauiTextJustify = 0xEE83,

        /// <summary>
        /// mynaui-thermometer
        /// Unicode: U+ee86
        /// </summary>
        MynauiThermometer = 0xEE86,

        /// <summary>
        /// mynaui-thermometer-snowflake
        /// Unicode: U+ee84
        /// </summary>
        MynauiThermometerSnowflake = 0xEE84,

        /// <summary>
        /// mynaui-thermometer-sun
        /// Unicode: U+ee85
        /// </summary>
        MynauiThermometerSun = 0xEE85,

        /// <summary>
        /// mynaui-three
        /// Unicode: U+ee8d
        /// </summary>
        MynauiThree = 0xEE8D,

        /// <summary>
        /// mynaui-three-circle
        /// Unicode: U+ee87
        /// </summary>
        MynauiThreeCircle = 0xEE87,

        /// <summary>
        /// mynaui-three-diamond
        /// Unicode: U+ee88
        /// </summary>
        MynauiThreeDiamond = 0xEE88,

        /// <summary>
        /// mynaui-three-hexagon
        /// Unicode: U+ee89
        /// </summary>
        MynauiThreeHexagon = 0xEE89,

        /// <summary>
        /// mynaui-three-octagon
        /// Unicode: U+ee8a
        /// </summary>
        MynauiThreeOctagon = 0xEE8A,

        /// <summary>
        /// mynaui-three-square
        /// Unicode: U+ee8b
        /// </summary>
        MynauiThreeSquare = 0xEE8B,

        /// <summary>
        /// mynaui-three-waves
        /// Unicode: U+ee8c
        /// </summary>
        MynauiThreeWaves = 0xEE8C,

        /// <summary>
        /// mynaui-ticket
        /// Unicode: U+ee8f
        /// </summary>
        MynauiTicket = 0xEE8F,

        /// <summary>
        /// mynaui-ticket-slash
        /// Unicode: U+ee8e
        /// </summary>
        MynauiTicketSlash = 0xEE8E,

        /// <summary>
        /// mynaui-toggle-left
        /// Unicode: U+ee90
        /// </summary>
        MynauiToggleLeft = 0xEE90,

        /// <summary>
        /// mynaui-toggle-right
        /// Unicode: U+ee91
        /// </summary>
        MynauiToggleRight = 0xEE91,

        /// <summary>
        /// mynaui-tool
        /// Unicode: U+ee92
        /// </summary>
        MynauiTool = 0xEE92,

        /// <summary>
        /// mynaui-tornado
        /// Unicode: U+ee93
        /// </summary>
        MynauiTornado = 0xEE93,

        /// <summary>
        /// mynaui-train
        /// Unicode: U+ee94
        /// </summary>
        MynauiTrain = 0xEE94,

        /// <summary>
        /// mynaui-trash
        /// Unicode: U+ee97
        /// </summary>
        MynauiTrash = 0xEE97,

        /// <summary>
        /// mynaui-trash-one
        /// Unicode: U+ee95
        /// </summary>
        MynauiTrashOne = 0xEE95,

        /// <summary>
        /// mynaui-trash-two
        /// Unicode: U+ee96
        /// </summary>
        MynauiTrashTwo = 0xEE96,

        /// <summary>
        /// mynaui-tree
        /// Unicode: U+ee9b
        /// </summary>
        MynauiTree = 0xEE9B,

        /// <summary>
        /// mynaui-tree-deciduous
        /// Unicode: U+ee98
        /// </summary>
        MynauiTreeDeciduous = 0xEE98,

        /// <summary>
        /// mynaui-tree-palm
        /// Unicode: U+ee99
        /// </summary>
        MynauiTreePalm = 0xEE99,

        /// <summary>
        /// mynaui-tree-pine
        /// Unicode: U+ee9a
        /// </summary>
        MynauiTreePine = 0xEE9A,

        /// <summary>
        /// mynaui-trees
        /// Unicode: U+ee9c
        /// </summary>
        MynauiTrees = 0xEE9C,

        /// <summary>
        /// mynaui-trending-down
        /// Unicode: U+ee9d
        /// </summary>
        MynauiTrendingDown = 0xEE9D,

        /// <summary>
        /// mynaui-trending-up
        /// Unicode: U+ee9f
        /// </summary>
        MynauiTrendingUp = 0xEE9F,

        /// <summary>
        /// mynaui-trending-up-down
        /// Unicode: U+ee9e
        /// </summary>
        MynauiTrendingUpDown = 0xEE9E,

        /// <summary>
        /// mynaui-triangle
        /// Unicode: U+eea0
        /// </summary>
        MynauiTriangle = 0xEEA0,

        /// <summary>
        /// mynaui-truck
        /// Unicode: U+eea1
        /// </summary>
        MynauiTruck = 0xEEA1,

        /// <summary>
        /// mynaui-tv
        /// Unicode: U+eea2
        /// </summary>
        MynauiTv = 0xEEA2,

        /// <summary>
        /// mynaui-two
        /// Unicode: U+eea9
        /// </summary>
        MynauiTwo = 0xEEA9,

        /// <summary>
        /// mynaui-two-circle
        /// Unicode: U+eea3
        /// </summary>
        MynauiTwoCircle = 0xEEA3,

        /// <summary>
        /// mynaui-two-diamond
        /// Unicode: U+eea4
        /// </summary>
        MynauiTwoDiamond = 0xEEA4,

        /// <summary>
        /// mynaui-two-hexagon
        /// Unicode: U+eea5
        /// </summary>
        MynauiTwoHexagon = 0xEEA5,

        /// <summary>
        /// mynaui-two-octagon
        /// Unicode: U+eea6
        /// </summary>
        MynauiTwoOctagon = 0xEEA6,

        /// <summary>
        /// mynaui-two-square
        /// Unicode: U+eea7
        /// </summary>
        MynauiTwoSquare = 0xEEA7,

        /// <summary>
        /// mynaui-two-waves
        /// Unicode: U+eea8
        /// </summary>
        MynauiTwoWaves = 0xEEA8,

        /// <summary>
        /// mynaui-type-bold
        /// Unicode: U+eeaa
        /// </summary>
        MynauiTypeBold = 0xEEAA,

        /// <summary>
        /// mynaui-type-italic
        /// Unicode: U+eeab
        /// </summary>
        MynauiTypeItalic = 0xEEAB,

        /// <summary>
        /// mynaui-type-text
        /// Unicode: U+eeac
        /// </summary>
        MynauiTypeText = 0xEEAC,

        /// <summary>
        /// mynaui-type-underline
        /// Unicode: U+eead
        /// </summary>
        MynauiTypeUnderline = 0xEEAD,

        /// <summary>
        /// mynaui-umbrella
        /// Unicode: U+eeaf
        /// </summary>
        MynauiUmbrella = 0xEEAF,

        /// <summary>
        /// mynaui-umbrella-off
        /// Unicode: U+eeae
        /// </summary>
        MynauiUmbrellaOff = 0xEEAE,

        /// <summary>
        /// mynaui-undo
        /// Unicode: U+eeb0
        /// </summary>
        MynauiUndo = 0xEEB0,

        /// <summary>
        /// mynaui-union
        /// Unicode: U+eeb1
        /// </summary>
        MynauiUnion = 0xEEB1,

        /// <summary>
        /// mynaui-unlink
        /// Unicode: U+eeb2
        /// </summary>
        MynauiUnlink = 0xEEB2,

        /// <summary>
        /// mynaui-upload
        /// Unicode: U+eeb3
        /// </summary>
        MynauiUpload = 0xEEB3,

        /// <summary>
        /// mynaui-user
        /// Unicode: U+eebf
        /// </summary>
        MynauiUser = 0xEEBF,

        /// <summary>
        /// mynaui-user-check
        /// Unicode: U+eeb4
        /// </summary>
        MynauiUserCheck = 0xEEB4,

        /// <summary>
        /// mynaui-user-circle
        /// Unicode: U+eeb5
        /// </summary>
        MynauiUserCircle = 0xEEB5,

        /// <summary>
        /// mynaui-user-diamond
        /// Unicode: U+eeb6
        /// </summary>
        MynauiUserDiamond = 0xEEB6,

        /// <summary>
        /// mynaui-user-hexagon
        /// Unicode: U+eeb7
        /// </summary>
        MynauiUserHexagon = 0xEEB7,

        /// <summary>
        /// mynaui-user-minus
        /// Unicode: U+eeb8
        /// </summary>
        MynauiUserMinus = 0xEEB8,

        /// <summary>
        /// mynaui-user-octagon
        /// Unicode: U+eeb9
        /// </summary>
        MynauiUserOctagon = 0xEEB9,

        /// <summary>
        /// mynaui-user-plus
        /// Unicode: U+eeba
        /// </summary>
        MynauiUserPlus = 0xEEBA,

        /// <summary>
        /// mynaui-users
        /// Unicode: U+eec1
        /// </summary>
        MynauiUsers = 0xEEC1,

        /// <summary>
        /// mynaui-user-settings
        /// Unicode: U+eebb
        /// </summary>
        MynauiUserSettings = 0xEEBB,

        /// <summary>
        /// mynaui-users-group
        /// Unicode: U+eec0
        /// </summary>
        MynauiUsersGroup = 0xEEC0,

        /// <summary>
        /// mynaui-user-square
        /// Unicode: U+eebc
        /// </summary>
        MynauiUserSquare = 0xEEBC,

        /// <summary>
        /// mynaui-user-waves
        /// Unicode: U+eebd
        /// </summary>
        MynauiUserWaves = 0xEEBD,

        /// <summary>
        /// mynaui-user-x
        /// Unicode: U+eebe
        /// </summary>
        MynauiUserX = 0xEEBE,

        /// <summary>
        /// mynaui-video
        /// Unicode: U+eec3
        /// </summary>
        MynauiVideo = 0xEEC3,

        /// <summary>
        /// mynaui-video-slash
        /// Unicode: U+eec2
        /// </summary>
        MynauiVideoSlash = 0xEEC2,

        /// <summary>
        /// mynaui-volume-check
        /// Unicode: U+eec4
        /// </summary>
        MynauiVolumeCheck = 0xEEC4,

        /// <summary>
        /// mynaui-volume-high
        /// Unicode: U+eec5
        /// </summary>
        MynauiVolumeHigh = 0xEEC5,

        /// <summary>
        /// mynaui-volume-low
        /// Unicode: U+eec6
        /// </summary>
        MynauiVolumeLow = 0xEEC6,

        /// <summary>
        /// mynaui-volume-minus
        /// Unicode: U+eec7
        /// </summary>
        MynauiVolumeMinus = 0xEEC7,

        /// <summary>
        /// mynaui-volume-none
        /// Unicode: U+eec8
        /// </summary>
        MynauiVolumeNone = 0xEEC8,

        /// <summary>
        /// mynaui-volume-plus
        /// Unicode: U+eec9
        /// </summary>
        MynauiVolumePlus = 0xEEC9,

        /// <summary>
        /// mynaui-volume-slash
        /// Unicode: U+eeca
        /// </summary>
        MynauiVolumeSlash = 0xEECA,

        /// <summary>
        /// mynaui-volume-x
        /// Unicode: U+eecb
        /// </summary>
        MynauiVolumeX = 0xEECB,

        /// <summary>
        /// mynaui-watch
        /// Unicode: U+eecc
        /// </summary>
        MynauiWatch = 0xEECC,

        /// <summary>
        /// mynaui-waves
        /// Unicode: U+eecd
        /// </summary>
        MynauiWaves = 0xEECD,

        /// <summary>
        /// mynaui-webcam
        /// Unicode: U+eece
        /// </summary>
        MynauiWebcam = 0xEECE,

        /// <summary>
        /// mynaui-wheel
        /// Unicode: U+eecf
        /// </summary>
        MynauiWheel = 0xEECF,

        /// <summary>
        /// mynaui-wheelchair
        /// Unicode: U+eed0
        /// </summary>
        MynauiWheelchair = 0xEED0,

        /// <summary>
        /// mynaui-wifi
        /// Unicode: U+eed8
        /// </summary>
        MynauiWifi = 0xEED8,

        /// <summary>
        /// mynaui-wifi-check
        /// Unicode: U+eed1
        /// </summary>
        MynauiWifiCheck = 0xEED1,

        /// <summary>
        /// mynaui-wifi-low
        /// Unicode: U+eed2
        /// </summary>
        MynauiWifiLow = 0xEED2,

        /// <summary>
        /// mynaui-wifi-medium
        /// Unicode: U+eed3
        /// </summary>
        MynauiWifiMedium = 0xEED3,

        /// <summary>
        /// mynaui-wifi-minus
        /// Unicode: U+eed4
        /// </summary>
        MynauiWifiMinus = 0xEED4,

        /// <summary>
        /// mynaui-wifi-plus
        /// Unicode: U+eed5
        /// </summary>
        MynauiWifiPlus = 0xEED5,

        /// <summary>
        /// mynaui-wifi-slash
        /// Unicode: U+eed6
        /// </summary>
        MynauiWifiSlash = 0xEED6,

        /// <summary>
        /// mynaui-wifi-x
        /// Unicode: U+eed7
        /// </summary>
        MynauiWifiX = 0xEED7,

        /// <summary>
        /// mynaui-wind
        /// Unicode: U+eeda
        /// </summary>
        MynauiWind = 0xEEDA,

        /// <summary>
        /// mynaui-wind-arrow-down
        /// Unicode: U+eed9
        /// </summary>
        MynauiWindArrowDown = 0xEED9,

        /// <summary>
        /// mynaui-winds
        /// Unicode: U+eedb
        /// </summary>
        MynauiWinds = 0xEEDB,

        /// <summary>
        /// mynaui-wine
        /// Unicode: U+eedc
        /// </summary>
        MynauiWine = 0xEEDC,

        /// <summary>
        /// mynaui-wink-circle
        /// Unicode: U+eedd
        /// </summary>
        MynauiWinkCircle = 0xEEDD,

        /// <summary>
        /// mynaui-wink-ghost
        /// Unicode: U+eede
        /// </summary>
        MynauiWinkGhost = 0xEEDE,

        /// <summary>
        /// mynaui-wink-square
        /// Unicode: U+eedf
        /// </summary>
        MynauiWinkSquare = 0xEEDF,

        /// <summary>
        /// mynaui-wrench
        /// Unicode: U+eee0
        /// </summary>
        MynauiWrench = 0xEEE0,

        /// <summary>
        /// mynaui-x
        /// Unicode: U+eee8
        /// </summary>
        MynauiX = 0xEEE8,

        /// <summary>
        /// mynaui-x-circle
        /// Unicode: U+eee1
        /// </summary>
        MynauiXCircle = 0xEEE1,

        /// <summary>
        /// mynaui-x-diamond
        /// Unicode: U+eee2
        /// </summary>
        MynauiXDiamond = 0xEEE2,

        /// <summary>
        /// mynaui-x-hexagon
        /// Unicode: U+eee3
        /// </summary>
        MynauiXHexagon = 0xEEE3,

        /// <summary>
        /// mynaui-x-octagon
        /// Unicode: U+eee4
        /// </summary>
        MynauiXOctagon = 0xEEE4,

        /// <summary>
        /// mynaui-x-square
        /// Unicode: U+eee5
        /// </summary>
        MynauiXSquare = 0xEEE5,

        /// <summary>
        /// mynaui-x-triangle
        /// Unicode: U+eee6
        /// </summary>
        MynauiXTriangle = 0xEEE6,

        /// <summary>
        /// mynaui-x-waves
        /// Unicode: U+eee7
        /// </summary>
        MynauiXWaves = 0xEEE7,

        /// <summary>
        /// mynaui-yen
        /// Unicode: U+eeef
        /// </summary>
        MynauiYen = 0xEEEF,

        /// <summary>
        /// mynaui-yen-circle
        /// Unicode: U+eee9
        /// </summary>
        MynauiYenCircle = 0xEEE9,

        /// <summary>
        /// mynaui-yen-diamond
        /// Unicode: U+eeea
        /// </summary>
        MynauiYenDiamond = 0xEEEA,

        /// <summary>
        /// mynaui-yen-hexagon
        /// Unicode: U+eeeb
        /// </summary>
        MynauiYenHexagon = 0xEEEB,

        /// <summary>
        /// mynaui-yen-octagon
        /// Unicode: U+eeec
        /// </summary>
        MynauiYenOctagon = 0xEEEC,

        /// <summary>
        /// mynaui-yen-square
        /// Unicode: U+eeed
        /// </summary>
        MynauiYenSquare = 0xEEED,

        /// <summary>
        /// mynaui-yen-waves
        /// Unicode: U+eeee
        /// </summary>
        MynauiYenWaves = 0xEEEE,

        /// <summary>
        /// mynaui-zap
        /// Unicode: U+eef1
        /// </summary>
        MynauiZap = 0xEEF1,

        /// <summary>
        /// mynaui-zap-off
        /// Unicode: U+eef0
        /// </summary>
        MynauiZapOff = 0xEEF0,

        /// <summary>
        /// mynaui-zero
        /// Unicode: U+eef8
        /// </summary>
        MynauiZero = 0xEEF8,

        /// <summary>
        /// mynaui-zero-circle
        /// Unicode: U+eef2
        /// </summary>
        MynauiZeroCircle = 0xEEF2,

        /// <summary>
        /// mynaui-zero-diamond
        /// Unicode: U+eef3
        /// </summary>
        MynauiZeroDiamond = 0xEEF3,

        /// <summary>
        /// mynaui-zero-hexagon
        /// Unicode: U+eef4
        /// </summary>
        MynauiZeroHexagon = 0xEEF4,

        /// <summary>
        /// mynaui-zero-octagon
        /// Unicode: U+eef5
        /// </summary>
        MynauiZeroOctagon = 0xEEF5,

        /// <summary>
        /// mynaui-zero-square
        /// Unicode: U+eef6
        /// </summary>
        MynauiZeroSquare = 0xEEF6,

        /// <summary>
        /// mynaui-zero-waves
        /// Unicode: U+eef7
        /// </summary>
        MynauiZeroWaves = 0xEEF7,
    }

    /// <summary>
    /// Mynaui-solid 图标枚举
    /// (共 1272 个可用图标)
    /// </summary>
    public enum MynauiSolidIconChar
    {
        /// <summary>
        /// mynaui-solid-a-arrow-down
        /// Unicode: U+ea01
        /// </summary>
        MynauiSolidAArrowDown = 0xEA01,

        /// <summary>
        /// mynaui-solid-a-arrow-up
        /// Unicode: U+ea02
        /// </summary>
        MynauiSolidAArrowUp = 0xEA02,

        /// <summary>
        /// mynaui-solid-academic-hat
        /// Unicode: U+ea03
        /// </summary>
        MynauiSolidAcademicHat = 0xEA03,

        /// <summary>
        /// mynaui-solid-accessibility
        /// Unicode: U+ea04
        /// </summary>
        MynauiSolidAccessibility = 0xEA04,

        /// <summary>
        /// mynaui-solid-activity
        /// Unicode: U+ea06
        /// </summary>
        MynauiSolidActivity = 0xEA06,

        /// <summary>
        /// mynaui-solid-activity-square
        /// Unicode: U+ea05
        /// </summary>
        MynauiSolidActivitySquare = 0xEA05,

        /// <summary>
        /// mynaui-solid-add-queue
        /// Unicode: U+ea07
        /// </summary>
        MynauiSolidAddQueue = 0xEA07,

        /// <summary>
        /// mynaui-solid-aeroplane
        /// Unicode: U+ea08
        /// </summary>
        MynauiSolidAeroplane = 0xEA08,

        /// <summary>
        /// mynaui-solid-air-conditioner
        /// Unicode: U+ea09
        /// </summary>
        MynauiSolidAirConditioner = 0xEA09,

        /// <summary>
        /// mynaui-solid-airplay
        /// Unicode: U+ea0a
        /// </summary>
        MynauiSolidAirplay = 0xEA0A,

        /// <summary>
        /// mynaui-solid-airpods
        /// Unicode: U+ea0b
        /// </summary>
        MynauiSolidAirpods = 0xEA0B,

        /// <summary>
        /// mynaui-solid-alarm
        /// Unicode: U+ea12
        /// </summary>
        MynauiSolidAlarm = 0xEA12,

        /// <summary>
        /// mynaui-solid-alarm-check
        /// Unicode: U+ea0c
        /// </summary>
        MynauiSolidAlarmCheck = 0xEA0C,

        /// <summary>
        /// mynaui-solid-alarm-minus
        /// Unicode: U+ea0d
        /// </summary>
        MynauiSolidAlarmMinus = 0xEA0D,

        /// <summary>
        /// mynaui-solid-alarm-plus
        /// Unicode: U+ea0e
        /// </summary>
        MynauiSolidAlarmPlus = 0xEA0E,

        /// <summary>
        /// mynaui-solid-alarm-smoke
        /// Unicode: U+ea0f
        /// </summary>
        MynauiSolidAlarmSmoke = 0xEA0F,

        /// <summary>
        /// mynaui-solid-alarm-snooze
        /// Unicode: U+ea10
        /// </summary>
        MynauiSolidAlarmSnooze = 0xEA10,

        /// <summary>
        /// mynaui-solid-alarm-x
        /// Unicode: U+ea11
        /// </summary>
        MynauiSolidAlarmX = 0xEA11,

        /// <summary>
        /// mynaui-solid-album
        /// Unicode: U+ea13
        /// </summary>
        MynauiSolidAlbum = 0xEA13,

        /// <summary>
        /// mynaui-solid-align-bottom
        /// Unicode: U+ea14
        /// </summary>
        MynauiSolidAlignBottom = 0xEA14,

        /// <summary>
        /// mynaui-solid-align-horizontal
        /// Unicode: U+ea15
        /// </summary>
        MynauiSolidAlignHorizontal = 0xEA15,

        /// <summary>
        /// mynaui-solid-align-left
        /// Unicode: U+ea16
        /// </summary>
        MynauiSolidAlignLeft = 0xEA16,

        /// <summary>
        /// mynaui-solid-align-right
        /// Unicode: U+ea17
        /// </summary>
        MynauiSolidAlignRight = 0xEA17,

        /// <summary>
        /// mynaui-solid-align-top
        /// Unicode: U+ea18
        /// </summary>
        MynauiSolidAlignTop = 0xEA18,

        /// <summary>
        /// mynaui-solid-align-vertical
        /// Unicode: U+ea19
        /// </summary>
        MynauiSolidAlignVertical = 0xEA19,

        /// <summary>
        /// mynaui-solid-alt
        /// Unicode: U+ea1a
        /// </summary>
        MynauiSolidAlt = 0xEA1A,

        /// <summary>
        /// mynaui-solid-ambulance
        /// Unicode: U+ea1b
        /// </summary>
        MynauiSolidAmbulance = 0xEA1B,

        /// <summary>
        /// mynaui-solid-ampersand
        /// Unicode: U+ea1d
        /// </summary>
        MynauiSolidAmpersand = 0xEA1D,

        /// <summary>
        /// mynaui-solid-ampersands
        /// Unicode: U+ea1e
        /// </summary>
        MynauiSolidAmpersands = 0xEA1E,

        /// <summary>
        /// mynaui-solid-ampersand-square
        /// Unicode: U+ea1c
        /// </summary>
        MynauiSolidAmpersandSquare = 0xEA1C,

        /// <summary>
        /// mynaui-solid-anchor
        /// Unicode: U+ea1f
        /// </summary>
        MynauiSolidAnchor = 0xEA1F,

        /// <summary>
        /// mynaui-solid-angry-circle
        /// Unicode: U+ea20
        /// </summary>
        MynauiSolidAngryCircle = 0xEA20,

        /// <summary>
        /// mynaui-solid-angry-ghost
        /// Unicode: U+ea21
        /// </summary>
        MynauiSolidAngryGhost = 0xEA21,

        /// <summary>
        /// mynaui-solid-angry-square
        /// Unicode: U+ea22
        /// </summary>
        MynauiSolidAngrySquare = 0xEA22,

        /// <summary>
        /// mynaui-solid-annoyed-circle
        /// Unicode: U+ea23
        /// </summary>
        MynauiSolidAnnoyedCircle = 0xEA23,

        /// <summary>
        /// mynaui-solid-annoyed-ghost
        /// Unicode: U+ea24
        /// </summary>
        MynauiSolidAnnoyedGhost = 0xEA24,

        /// <summary>
        /// mynaui-solid-annoyed-square
        /// Unicode: U+ea25
        /// </summary>
        MynauiSolidAnnoyedSquare = 0xEA25,

        /// <summary>
        /// mynaui-solid-aperture
        /// Unicode: U+ea26
        /// </summary>
        MynauiSolidAperture = 0xEA26,

        /// <summary>
        /// mynaui-solid-api
        /// Unicode: U+ea27
        /// </summary>
        MynauiSolidApi = 0xEA27,

        /// <summary>
        /// mynaui-solid-ar
        /// Unicode: U+ea28
        /// </summary>
        MynauiSolidAr = 0xEA28,

        /// <summary>
        /// mynaui-solid-archive
        /// Unicode: U+ea29
        /// </summary>
        MynauiSolidArchive = 0xEA29,

        /// <summary>
        /// mynaui-solid-arrow-diagonal-one
        /// Unicode: U+ea2a
        /// </summary>
        MynauiSolidArrowDiagonalOne = 0xEA2A,

        /// <summary>
        /// mynaui-solid-arrow-diagonal-two
        /// Unicode: U+ea2b
        /// </summary>
        MynauiSolidArrowDiagonalTwo = 0xEA2B,

        /// <summary>
        /// mynaui-solid-arrow-down
        /// Unicode: U+ea37
        /// </summary>
        MynauiSolidArrowDown = 0xEA37,

        /// <summary>
        /// mynaui-solid-arrow-down-circle
        /// Unicode: U+ea2c
        /// </summary>
        MynauiSolidArrowDownCircle = 0xEA2C,

        /// <summary>
        /// mynaui-solid-arrow-down-left
        /// Unicode: U+ea30
        /// </summary>
        MynauiSolidArrowDownLeft = 0xEA30,

        /// <summary>
        /// mynaui-solid-arrow-down-left-circle
        /// Unicode: U+ea2d
        /// </summary>
        MynauiSolidArrowDownLeftCircle = 0xEA2D,

        /// <summary>
        /// mynaui-solid-arrow-down-left-square
        /// Unicode: U+ea2e
        /// </summary>
        MynauiSolidArrowDownLeftSquare = 0xEA2E,

        /// <summary>
        /// mynaui-solid-arrow-down-left-waves
        /// Unicode: U+ea2f
        /// </summary>
        MynauiSolidArrowDownLeftWaves = 0xEA2F,

        /// <summary>
        /// mynaui-solid-arrow-down-right
        /// Unicode: U+ea34
        /// </summary>
        MynauiSolidArrowDownRight = 0xEA34,

        /// <summary>
        /// mynaui-solid-arrow-down-right-circle
        /// Unicode: U+ea31
        /// </summary>
        MynauiSolidArrowDownRightCircle = 0xEA31,

        /// <summary>
        /// mynaui-solid-arrow-down-right-square
        /// Unicode: U+ea32
        /// </summary>
        MynauiSolidArrowDownRightSquare = 0xEA32,

        /// <summary>
        /// mynaui-solid-arrow-down-right-waves
        /// Unicode: U+ea33
        /// </summary>
        MynauiSolidArrowDownRightWaves = 0xEA33,

        /// <summary>
        /// mynaui-solid-arrow-down-square
        /// Unicode: U+ea35
        /// </summary>
        MynauiSolidArrowDownSquare = 0xEA35,

        /// <summary>
        /// mynaui-solid-arrow-down-waves
        /// Unicode: U+ea36
        /// </summary>
        MynauiSolidArrowDownWaves = 0xEA36,

        /// <summary>
        /// mynaui-solid-arrow-left
        /// Unicode: U+ea3c
        /// </summary>
        MynauiSolidArrowLeft = 0xEA3C,

        /// <summary>
        /// mynaui-solid-arrow-left-circle
        /// Unicode: U+ea38
        /// </summary>
        MynauiSolidArrowLeftCircle = 0xEA38,

        /// <summary>
        /// mynaui-solid-arrow-left-right
        /// Unicode: U+ea39
        /// </summary>
        MynauiSolidArrowLeftRight = 0xEA39,

        /// <summary>
        /// mynaui-solid-arrow-left-square
        /// Unicode: U+ea3a
        /// </summary>
        MynauiSolidArrowLeftSquare = 0xEA3A,

        /// <summary>
        /// mynaui-solid-arrow-left-waves
        /// Unicode: U+ea3b
        /// </summary>
        MynauiSolidArrowLeftWaves = 0xEA3B,

        /// <summary>
        /// mynaui-solid-arrow-long-down
        /// Unicode: U+ea3f
        /// </summary>
        MynauiSolidArrowLongDown = 0xEA3F,

        /// <summary>
        /// mynaui-solid-arrow-long-down-left
        /// Unicode: U+ea3d
        /// </summary>
        MynauiSolidArrowLongDownLeft = 0xEA3D,

        /// <summary>
        /// mynaui-solid-arrow-long-down-right
        /// Unicode: U+ea3e
        /// </summary>
        MynauiSolidArrowLongDownRight = 0xEA3E,

        /// <summary>
        /// mynaui-solid-arrow-long-left
        /// Unicode: U+ea40
        /// </summary>
        MynauiSolidArrowLongLeft = 0xEA40,

        /// <summary>
        /// mynaui-solid-arrow-long-right
        /// Unicode: U+ea41
        /// </summary>
        MynauiSolidArrowLongRight = 0xEA41,

        /// <summary>
        /// mynaui-solid-arrow-long-up
        /// Unicode: U+ea44
        /// </summary>
        MynauiSolidArrowLongUp = 0xEA44,

        /// <summary>
        /// mynaui-solid-arrow-long-up-left
        /// Unicode: U+ea42
        /// </summary>
        MynauiSolidArrowLongUpLeft = 0xEA42,

        /// <summary>
        /// mynaui-solid-arrow-long-up-right
        /// Unicode: U+ea43
        /// </summary>
        MynauiSolidArrowLongUpRight = 0xEA43,

        /// <summary>
        /// mynaui-solid-arrow-right
        /// Unicode: U+ea48
        /// </summary>
        MynauiSolidArrowRight = 0xEA48,

        /// <summary>
        /// mynaui-solid-arrow-right-circle
        /// Unicode: U+ea45
        /// </summary>
        MynauiSolidArrowRightCircle = 0xEA45,

        /// <summary>
        /// mynaui-solid-arrow-right-square
        /// Unicode: U+ea46
        /// </summary>
        MynauiSolidArrowRightSquare = 0xEA46,

        /// <summary>
        /// mynaui-solid-arrow-right-waves
        /// Unicode: U+ea47
        /// </summary>
        MynauiSolidArrowRightWaves = 0xEA47,

        /// <summary>
        /// mynaui-solid-arrow-up
        /// Unicode: U+ea55
        /// </summary>
        MynauiSolidArrowUp = 0xEA55,

        /// <summary>
        /// mynaui-solid-arrow-up-circle
        /// Unicode: U+ea49
        /// </summary>
        MynauiSolidArrowUpCircle = 0xEA49,

        /// <summary>
        /// mynaui-solid-arrow-up-down
        /// Unicode: U+ea4a
        /// </summary>
        MynauiSolidArrowUpDown = 0xEA4A,

        /// <summary>
        /// mynaui-solid-arrow-up-left
        /// Unicode: U+ea4e
        /// </summary>
        MynauiSolidArrowUpLeft = 0xEA4E,

        /// <summary>
        /// mynaui-solid-arrow-up-left-circle
        /// Unicode: U+ea4b
        /// </summary>
        MynauiSolidArrowUpLeftCircle = 0xEA4B,

        /// <summary>
        /// mynaui-solid-arrow-up-left-square
        /// Unicode: U+ea4c
        /// </summary>
        MynauiSolidArrowUpLeftSquare = 0xEA4C,

        /// <summary>
        /// mynaui-solid-arrow-up-left-waves
        /// Unicode: U+ea4d
        /// </summary>
        MynauiSolidArrowUpLeftWaves = 0xEA4D,

        /// <summary>
        /// mynaui-solid-arrow-up-right
        /// Unicode: U+ea52
        /// </summary>
        MynauiSolidArrowUpRight = 0xEA52,

        /// <summary>
        /// mynaui-solid-arrow-up-right-circle
        /// Unicode: U+ea4f
        /// </summary>
        MynauiSolidArrowUpRightCircle = 0xEA4F,

        /// <summary>
        /// mynaui-solid-arrow-up-right-square
        /// Unicode: U+ea50
        /// </summary>
        MynauiSolidArrowUpRightSquare = 0xEA50,

        /// <summary>
        /// mynaui-solid-arrow-up-right-waves
        /// Unicode: U+ea51
        /// </summary>
        MynauiSolidArrowUpRightWaves = 0xEA51,

        /// <summary>
        /// mynaui-solid-arrow-up-square
        /// Unicode: U+ea53
        /// </summary>
        MynauiSolidArrowUpSquare = 0xEA53,

        /// <summary>
        /// mynaui-solid-arrow-up-waves
        /// Unicode: U+ea54
        /// </summary>
        MynauiSolidArrowUpWaves = 0xEA54,

        /// <summary>
        /// mynaui-solid-asterisk-circle
        /// Unicode: U+ea56
        /// </summary>
        MynauiSolidAsteriskCircle = 0xEA56,

        /// <summary>
        /// mynaui-solid-asterisk-diamond
        /// Unicode: U+ea57
        /// </summary>
        MynauiSolidAsteriskDiamond = 0xEA57,

        /// <summary>
        /// mynaui-solid-asterisk-hexagon
        /// Unicode: U+ea58
        /// </summary>
        MynauiSolidAsteriskHexagon = 0xEA58,

        /// <summary>
        /// mynaui-solid-asterisk-octagon
        /// Unicode: U+ea59
        /// </summary>
        MynauiSolidAsteriskOctagon = 0xEA59,

        /// <summary>
        /// mynaui-solid-asterisk-square
        /// Unicode: U+ea5a
        /// </summary>
        MynauiSolidAsteriskSquare = 0xEA5A,

        /// <summary>
        /// mynaui-solid-asterisk-waves
        /// Unicode: U+ea5b
        /// </summary>
        MynauiSolidAsteriskWaves = 0xEA5B,

        /// <summary>
        /// mynaui-solid-at
        /// Unicode: U+ea5c
        /// </summary>
        MynauiSolidAt = 0xEA5C,

        /// <summary>
        /// mynaui-solid-atom
        /// Unicode: U+ea5d
        /// </summary>
        MynauiSolidAtom = 0xEA5D,

        /// <summary>
        /// mynaui-solid-baby
        /// Unicode: U+ea5e
        /// </summary>
        MynauiSolidBaby = 0xEA5E,

        /// <summary>
        /// mynaui-solid-backpack
        /// Unicode: U+ea5f
        /// </summary>
        MynauiSolidBackpack = 0xEA5F,

        /// <summary>
        /// mynaui-solid-badge
        /// Unicode: U+ea60
        /// </summary>
        MynauiSolidBadge = 0xEA60,

        /// <summary>
        /// mynaui-solid-baggage-claim
        /// Unicode: U+ea61
        /// </summary>
        MynauiSolidBaggageClaim = 0xEA61,

        /// <summary>
        /// mynaui-solid-ban
        /// Unicode: U+ea62
        /// </summary>
        MynauiSolidBan = 0xEA62,

        /// <summary>
        /// mynaui-solid-bank
        /// Unicode: U+ea63
        /// </summary>
        MynauiSolidBank = 0xEA63,

        /// <summary>
        /// mynaui-solid-baseball
        /// Unicode: U+ea64
        /// </summary>
        MynauiSolidBaseball = 0xEA64,

        /// <summary>
        /// mynaui-solid-bath
        /// Unicode: U+ea65
        /// </summary>
        MynauiSolidBath = 0xEA65,

        /// <summary>
        /// mynaui-solid-battery-charging
        /// Unicode: U+ea6a
        /// </summary>
        MynauiSolidBatteryCharging = 0xEA6A,

        /// <summary>
        /// mynaui-solid-battery-charging-four
        /// Unicode: U+ea66
        /// </summary>
        MynauiSolidBatteryChargingFour = 0xEA66,

        /// <summary>
        /// mynaui-solid-battery-charging-one
        /// Unicode: U+ea67
        /// </summary>
        MynauiSolidBatteryChargingOne = 0xEA67,

        /// <summary>
        /// mynaui-solid-battery-charging-three
        /// Unicode: U+ea68
        /// </summary>
        MynauiSolidBatteryChargingThree = 0xEA68,

        /// <summary>
        /// mynaui-solid-battery-charging-two
        /// Unicode: U+ea69
        /// </summary>
        MynauiSolidBatteryChargingTwo = 0xEA69,

        /// <summary>
        /// mynaui-solid-battery-check
        /// Unicode: U+ea6b
        /// </summary>
        MynauiSolidBatteryCheck = 0xEA6B,

        /// <summary>
        /// mynaui-solid-battery-empty
        /// Unicode: U+ea6c
        /// </summary>
        MynauiSolidBatteryEmpty = 0xEA6C,

        /// <summary>
        /// mynaui-solid-battery-full
        /// Unicode: U+ea6d
        /// </summary>
        MynauiSolidBatteryFull = 0xEA6D,

        /// <summary>
        /// mynaui-solid-battery-minus
        /// Unicode: U+ea6e
        /// </summary>
        MynauiSolidBatteryMinus = 0xEA6E,

        /// <summary>
        /// mynaui-solid-battery-plus
        /// Unicode: U+ea6f
        /// </summary>
        MynauiSolidBatteryPlus = 0xEA6F,

        /// <summary>
        /// mynaui-solid-battery-x
        /// Unicode: U+ea70
        /// </summary>
        MynauiSolidBatteryX = 0xEA70,

        /// <summary>
        /// mynaui-solid-bell
        /// Unicode: U+ea7a
        /// </summary>
        MynauiSolidBell = 0xEA7A,

        /// <summary>
        /// mynaui-solid-bell-check
        /// Unicode: U+ea71
        /// </summary>
        MynauiSolidBellCheck = 0xEA71,

        /// <summary>
        /// mynaui-solid-bell-home
        /// Unicode: U+ea72
        /// </summary>
        MynauiSolidBellHome = 0xEA72,

        /// <summary>
        /// mynaui-solid-bell-minus
        /// Unicode: U+ea73
        /// </summary>
        MynauiSolidBellMinus = 0xEA73,

        /// <summary>
        /// mynaui-solid-bell-on
        /// Unicode: U+ea74
        /// </summary>
        MynauiSolidBellOn = 0xEA74,

        /// <summary>
        /// mynaui-solid-bell-plus
        /// Unicode: U+ea75
        /// </summary>
        MynauiSolidBellPlus = 0xEA75,

        /// <summary>
        /// mynaui-solid-bell-slash
        /// Unicode: U+ea76
        /// </summary>
        MynauiSolidBellSlash = 0xEA76,

        /// <summary>
        /// mynaui-solid-bell-snooze
        /// Unicode: U+ea77
        /// </summary>
        MynauiSolidBellSnooze = 0xEA77,

        /// <summary>
        /// mynaui-solid-bell-user
        /// Unicode: U+ea78
        /// </summary>
        MynauiSolidBellUser = 0xEA78,

        /// <summary>
        /// mynaui-solid-bell-x
        /// Unicode: U+ea79
        /// </summary>
        MynauiSolidBellX = 0xEA79,

        /// <summary>
        /// mynaui-solid-binoculars
        /// Unicode: U+ea7b
        /// </summary>
        MynauiSolidBinoculars = 0xEA7B,

        /// <summary>
        /// mynaui-solid-bitcoin
        /// Unicode: U+ea82
        /// </summary>
        MynauiSolidBitcoin = 0xEA82,

        /// <summary>
        /// mynaui-solid-bitcoin-circle
        /// Unicode: U+ea7c
        /// </summary>
        MynauiSolidBitcoinCircle = 0xEA7C,

        /// <summary>
        /// mynaui-solid-bitcoin-diamond
        /// Unicode: U+ea7d
        /// </summary>
        MynauiSolidBitcoinDiamond = 0xEA7D,

        /// <summary>
        /// mynaui-solid-bitcoin-hexagon
        /// Unicode: U+ea7e
        /// </summary>
        MynauiSolidBitcoinHexagon = 0xEA7E,

        /// <summary>
        /// mynaui-solid-bitcoin-octagon
        /// Unicode: U+ea7f
        /// </summary>
        MynauiSolidBitcoinOctagon = 0xEA7F,

        /// <summary>
        /// mynaui-solid-bitcoin-square
        /// Unicode: U+ea80
        /// </summary>
        MynauiSolidBitcoinSquare = 0xEA80,

        /// <summary>
        /// mynaui-solid-bitcoin-waves
        /// Unicode: U+ea81
        /// </summary>
        MynauiSolidBitcoinWaves = 0xEA81,

        /// <summary>
        /// mynaui-solid-bluetooth
        /// Unicode: U+ea83
        /// </summary>
        MynauiSolidBluetooth = 0xEA83,

        /// <summary>
        /// mynaui-solid-boat
        /// Unicode: U+ea84
        /// </summary>
        MynauiSolidBoat = 0xEA84,

        /// <summary>
        /// mynaui-solid-book
        /// Unicode: U+ea90
        /// </summary>
        MynauiSolidBook = 0xEA90,

        /// <summary>
        /// mynaui-solid-book-check
        /// Unicode: U+ea85
        /// </summary>
        MynauiSolidBookCheck = 0xEA85,

        /// <summary>
        /// mynaui-solid-book-dot
        /// Unicode: U+ea86
        /// </summary>
        MynauiSolidBookDot = 0xEA86,

        /// <summary>
        /// mynaui-solid-book-home
        /// Unicode: U+ea87
        /// </summary>
        MynauiSolidBookHome = 0xEA87,

        /// <summary>
        /// mynaui-solid-book-image
        /// Unicode: U+ea88
        /// </summary>
        MynauiSolidBookImage = 0xEA88,

        /// <summary>
        /// mynaui-solid-bookmark
        /// Unicode: U+ea9a
        /// </summary>
        MynauiSolidBookmark = 0xEA9A,

        /// <summary>
        /// mynaui-solid-bookmark-check
        /// Unicode: U+ea91
        /// </summary>
        MynauiSolidBookmarkCheck = 0xEA91,

        /// <summary>
        /// mynaui-solid-bookmark-dot
        /// Unicode: U+ea92
        /// </summary>
        MynauiSolidBookmarkDot = 0xEA92,

        /// <summary>
        /// mynaui-solid-bookmark-home
        /// Unicode: U+ea93
        /// </summary>
        MynauiSolidBookmarkHome = 0xEA93,

        /// <summary>
        /// mynaui-solid-bookmark-minus
        /// Unicode: U+ea94
        /// </summary>
        MynauiSolidBookmarkMinus = 0xEA94,

        /// <summary>
        /// mynaui-solid-bookmark-plus
        /// Unicode: U+ea95
        /// </summary>
        MynauiSolidBookmarkPlus = 0xEA95,

        /// <summary>
        /// mynaui-solid-bookmark-slash
        /// Unicode: U+ea96
        /// </summary>
        MynauiSolidBookmarkSlash = 0xEA96,

        /// <summary>
        /// mynaui-solid-bookmark-snooze
        /// Unicode: U+ea97
        /// </summary>
        MynauiSolidBookmarkSnooze = 0xEA97,

        /// <summary>
        /// mynaui-solid-bookmark-user
        /// Unicode: U+ea98
        /// </summary>
        MynauiSolidBookmarkUser = 0xEA98,

        /// <summary>
        /// mynaui-solid-bookmark-x
        /// Unicode: U+ea99
        /// </summary>
        MynauiSolidBookmarkX = 0xEA99,

        /// <summary>
        /// mynaui-solid-book-minus
        /// Unicode: U+ea89
        /// </summary>
        MynauiSolidBookMinus = 0xEA89,

        /// <summary>
        /// mynaui-solid-book-open
        /// Unicode: U+ea8a
        /// </summary>
        MynauiSolidBookOpen = 0xEA8A,

        /// <summary>
        /// mynaui-solid-book-plus
        /// Unicode: U+ea8b
        /// </summary>
        MynauiSolidBookPlus = 0xEA8B,

        /// <summary>
        /// mynaui-solid-book-slash
        /// Unicode: U+ea8c
        /// </summary>
        MynauiSolidBookSlash = 0xEA8C,

        /// <summary>
        /// mynaui-solid-book-snooze
        /// Unicode: U+ea8d
        /// </summary>
        MynauiSolidBookSnooze = 0xEA8D,

        /// <summary>
        /// mynaui-solid-book-user
        /// Unicode: U+ea8e
        /// </summary>
        MynauiSolidBookUser = 0xEA8E,

        /// <summary>
        /// mynaui-solid-book-x
        /// Unicode: U+ea8f
        /// </summary>
        MynauiSolidBookX = 0xEA8F,

        /// <summary>
        /// mynaui-solid-bounding-box
        /// Unicode: U+ea9b
        /// </summary>
        MynauiSolidBoundingBox = 0xEA9B,

        /// <summary>
        /// mynaui-solid-bowl
        /// Unicode: U+ea9c
        /// </summary>
        MynauiSolidBowl = 0xEA9C,

        /// <summary>
        /// mynaui-solid-box
        /// Unicode: U+ea9d
        /// </summary>
        MynauiSolidBox = 0xEA9D,

        /// <summary>
        /// mynaui-solid-brand-chrome
        /// Unicode: U+ea9e
        /// </summary>
        MynauiSolidBrandChrome = 0xEA9E,

        /// <summary>
        /// mynaui-solid-brand-codepen
        /// Unicode: U+ea9f
        /// </summary>
        MynauiSolidBrandCodepen = 0xEA9F,

        /// <summary>
        /// mynaui-solid-brand-codesandbox
        /// Unicode: U+eaa0
        /// </summary>
        MynauiSolidBrandCodesandbox = 0xEAA0,

        /// <summary>
        /// mynaui-solid-brand-dribbble
        /// Unicode: U+eaa1
        /// </summary>
        MynauiSolidBrandDribbble = 0xEAA1,

        /// <summary>
        /// mynaui-solid-brand-facebook
        /// Unicode: U+eaa2
        /// </summary>
        MynauiSolidBrandFacebook = 0xEAA2,

        /// <summary>
        /// mynaui-solid-brand-figma
        /// Unicode: U+eaa3
        /// </summary>
        MynauiSolidBrandFigma = 0xEAA3,

        /// <summary>
        /// mynaui-solid-brand-framer
        /// Unicode: U+eaa4
        /// </summary>
        MynauiSolidBrandFramer = 0xEAA4,

        /// <summary>
        /// mynaui-solid-brand-github
        /// Unicode: U+eaa5
        /// </summary>
        MynauiSolidBrandGithub = 0xEAA5,

        /// <summary>
        /// mynaui-solid-brand-gitlab
        /// Unicode: U+eaa6
        /// </summary>
        MynauiSolidBrandGitlab = 0xEAA6,

        /// <summary>
        /// mynaui-solid-brand-google
        /// Unicode: U+eaa7
        /// </summary>
        MynauiSolidBrandGoogle = 0xEAA7,

        /// <summary>
        /// mynaui-solid-brand-instagram
        /// Unicode: U+eaa8
        /// </summary>
        MynauiSolidBrandInstagram = 0xEAA8,

        /// <summary>
        /// mynaui-solid-brand-linkedin
        /// Unicode: U+eaa9
        /// </summary>
        MynauiSolidBrandLinkedin = 0xEAA9,

        /// <summary>
        /// mynaui-solid-brand-pinterest
        /// Unicode: U+eaaa
        /// </summary>
        MynauiSolidBrandPinterest = 0xEAAA,

        /// <summary>
        /// mynaui-solid-brand-pocket
        /// Unicode: U+eaab
        /// </summary>
        MynauiSolidBrandPocket = 0xEAAB,

        /// <summary>
        /// mynaui-solid-brand-slack
        /// Unicode: U+eaac
        /// </summary>
        MynauiSolidBrandSlack = 0xEAAC,

        /// <summary>
        /// mynaui-solid-brand-spotify
        /// Unicode: U+eaad
        /// </summary>
        MynauiSolidBrandSpotify = 0xEAAD,

        /// <summary>
        /// mynaui-solid-brand-telegram
        /// Unicode: U+eaae
        /// </summary>
        MynauiSolidBrandTelegram = 0xEAAE,

        /// <summary>
        /// mynaui-solid-brand-threads
        /// Unicode: U+eaaf
        /// </summary>
        MynauiSolidBrandThreads = 0xEAAF,

        /// <summary>
        /// mynaui-solid-brand-trello
        /// Unicode: U+eab0
        /// </summary>
        MynauiSolidBrandTrello = 0xEAB0,

        /// <summary>
        /// mynaui-solid-brand-twitch
        /// Unicode: U+eab1
        /// </summary>
        MynauiSolidBrandTwitch = 0xEAB1,

        /// <summary>
        /// mynaui-solid-brand-twitter
        /// Unicode: U+eab2
        /// </summary>
        MynauiSolidBrandTwitter = 0xEAB2,

        /// <summary>
        /// mynaui-solid-brand-x
        /// Unicode: U+eab3
        /// </summary>
        MynauiSolidBrandX = 0xEAB3,

        /// <summary>
        /// mynaui-solid-brand-youtube
        /// Unicode: U+eab4
        /// </summary>
        MynauiSolidBrandYoutube = 0xEAB4,

        /// <summary>
        /// mynaui-solid-briefcase
        /// Unicode: U+eab6
        /// </summary>
        MynauiSolidBriefcase = 0xEAB6,

        /// <summary>
        /// mynaui-solid-briefcase-conveyor-belt
        /// Unicode: U+eab5
        /// </summary>
        MynauiSolidBriefcaseConveyorBelt = 0xEAB5,

        /// <summary>
        /// mynaui-solid-brightness-high
        /// Unicode: U+eab7
        /// </summary>
        MynauiSolidBrightnessHigh = 0xEAB7,

        /// <summary>
        /// mynaui-solid-brightness-low
        /// Unicode: U+eab8
        /// </summary>
        MynauiSolidBrightnessLow = 0xEAB8,

        /// <summary>
        /// mynaui-solid-bubbles
        /// Unicode: U+eab9
        /// </summary>
        MynauiSolidBubbles = 0xEAB9,

        /// <summary>
        /// mynaui-solid-building
        /// Unicode: U+eabb
        /// </summary>
        MynauiSolidBuilding = 0xEABB,

        /// <summary>
        /// mynaui-solid-building-one
        /// Unicode: U+eaba
        /// </summary>
        MynauiSolidBuildingOne = 0xEABA,

        /// <summary>
        /// mynaui-solid-cable-car
        /// Unicode: U+eabc
        /// </summary>
        MynauiSolidCableCar = 0xEABC,

        /// <summary>
        /// mynaui-solid-cake
        /// Unicode: U+eabd
        /// </summary>
        MynauiSolidCake = 0xEABD,

        /// <summary>
        /// mynaui-solid-calendar
        /// Unicode: U+eac5
        /// </summary>
        MynauiSolidCalendar = 0xEAC5,

        /// <summary>
        /// mynaui-solid-calendar-check
        /// Unicode: U+eabe
        /// </summary>
        MynauiSolidCalendarCheck = 0xEABE,

        /// <summary>
        /// mynaui-solid-calendar-down
        /// Unicode: U+eabf
        /// </summary>
        MynauiSolidCalendarDown = 0xEABF,

        /// <summary>
        /// mynaui-solid-calendar-minus
        /// Unicode: U+eac0
        /// </summary>
        MynauiSolidCalendarMinus = 0xEAC0,

        /// <summary>
        /// mynaui-solid-calendar-plus
        /// Unicode: U+eac1
        /// </summary>
        MynauiSolidCalendarPlus = 0xEAC1,

        /// <summary>
        /// mynaui-solid-calendar-slash
        /// Unicode: U+eac2
        /// </summary>
        MynauiSolidCalendarSlash = 0xEAC2,

        /// <summary>
        /// mynaui-solid-calendar-up
        /// Unicode: U+eac3
        /// </summary>
        MynauiSolidCalendarUp = 0xEAC3,

        /// <summary>
        /// mynaui-solid-calendar-x
        /// Unicode: U+eac4
        /// </summary>
        MynauiSolidCalendarX = 0xEAC4,

        /// <summary>
        /// mynaui-solid-camera
        /// Unicode: U+eac7
        /// </summary>
        MynauiSolidCamera = 0xEAC7,

        /// <summary>
        /// mynaui-solid-camera-slash
        /// Unicode: U+eac6
        /// </summary>
        MynauiSolidCameraSlash = 0xEAC6,

        /// <summary>
        /// mynaui-solid-campfire
        /// Unicode: U+eac8
        /// </summary>
        MynauiSolidCampfire = 0xEAC8,

        /// <summary>
        /// mynaui-solid-cannabis
        /// Unicode: U+eac9
        /// </summary>
        MynauiSolidCannabis = 0xEAC9,

        /// <summary>
        /// mynaui-solid-caravan
        /// Unicode: U+eaca
        /// </summary>
        MynauiSolidCaravan = 0xEACA,

        /// <summary>
        /// mynaui-solid-cart
        /// Unicode: U+eacf
        /// </summary>
        MynauiSolidCart = 0xEACF,

        /// <summary>
        /// mynaui-solid-cart-check
        /// Unicode: U+eacb
        /// </summary>
        MynauiSolidCartCheck = 0xEACB,

        /// <summary>
        /// mynaui-solid-cart-minus
        /// Unicode: U+eacc
        /// </summary>
        MynauiSolidCartMinus = 0xEACC,

        /// <summary>
        /// mynaui-solid-cart-plus
        /// Unicode: U+eacd
        /// </summary>
        MynauiSolidCartPlus = 0xEACD,

        /// <summary>
        /// mynaui-solid-cart-x
        /// Unicode: U+eace
        /// </summary>
        MynauiSolidCartX = 0xEACE,

        /// <summary>
        /// mynaui-solid-cast-screen
        /// Unicode: U+ead0
        /// </summary>
        MynauiSolidCastScreen = 0xEAD0,

        /// <summary>
        /// mynaui-solid-center-focus
        /// Unicode: U+ead1
        /// </summary>
        MynauiSolidCenterFocus = 0xEAD1,

        /// <summary>
        /// mynaui-solid-chart-area
        /// Unicode: U+ead2
        /// </summary>
        MynauiSolidChartArea = 0xEAD2,

        /// <summary>
        /// mynaui-solid-chart-bar
        /// Unicode: U+ead8
        /// </summary>
        MynauiSolidChartBar = 0xEAD8,

        /// <summary>
        /// mynaui-solid-chart-bar-big
        /// Unicode: U+ead3
        /// </summary>
        MynauiSolidChartBarBig = 0xEAD3,

        /// <summary>
        /// mynaui-solid-chart-bar-decreasing
        /// Unicode: U+ead4
        /// </summary>
        MynauiSolidChartBarDecreasing = 0xEAD4,

        /// <summary>
        /// mynaui-solid-chart-bar-increasing
        /// Unicode: U+ead5
        /// </summary>
        MynauiSolidChartBarIncreasing = 0xEAD5,

        /// <summary>
        /// mynaui-solid-chart-bar-one
        /// Unicode: U+ead6
        /// </summary>
        MynauiSolidChartBarOne = 0xEAD6,

        /// <summary>
        /// mynaui-solid-chart-bar-stacked
        /// Unicode: U+ead7
        /// </summary>
        MynauiSolidChartBarStacked = 0xEAD7,

        /// <summary>
        /// mynaui-solid-chart-bubble
        /// Unicode: U+ead9
        /// </summary>
        MynauiSolidChartBubble = 0xEAD9,

        /// <summary>
        /// mynaui-solid-chart-candlestick
        /// Unicode: U+eada
        /// </summary>
        MynauiSolidChartCandlestick = 0xEADA,

        /// <summary>
        /// mynaui-solid-chart-column
        /// Unicode: U+eadf
        /// </summary>
        MynauiSolidChartColumn = 0xEADF,

        /// <summary>
        /// mynaui-solid-chart-column-big
        /// Unicode: U+eadb
        /// </summary>
        MynauiSolidChartColumnBig = 0xEADB,

        /// <summary>
        /// mynaui-solid-chart-column-decreasing
        /// Unicode: U+eadc
        /// </summary>
        MynauiSolidChartColumnDecreasing = 0xEADC,

        /// <summary>
        /// mynaui-solid-chart-column-increasing
        /// Unicode: U+eadd
        /// </summary>
        MynauiSolidChartColumnIncreasing = 0xEADD,

        /// <summary>
        /// mynaui-solid-chart-column-stacked
        /// Unicode: U+eade
        /// </summary>
        MynauiSolidChartColumnStacked = 0xEADE,

        /// <summary>
        /// mynaui-solid-chart-gantt
        /// Unicode: U+eae0
        /// </summary>
        MynauiSolidChartGantt = 0xEAE0,

        /// <summary>
        /// mynaui-solid-chart-graph
        /// Unicode: U+eae1
        /// </summary>
        MynauiSolidChartGraph = 0xEAE1,

        /// <summary>
        /// mynaui-solid-chart-line
        /// Unicode: U+eae2
        /// </summary>
        MynauiSolidChartLine = 0xEAE2,

        /// <summary>
        /// mynaui-solid-chart-network
        /// Unicode: U+eae3
        /// </summary>
        MynauiSolidChartNetwork = 0xEAE3,

        /// <summary>
        /// mynaui-solid-chart-no-axes-column
        /// Unicode: U+eae6
        /// </summary>
        MynauiSolidChartNoAxesColumn = 0xEAE6,

        /// <summary>
        /// mynaui-solid-chart-no-axes-column-decreasing
        /// Unicode: U+eae4
        /// </summary>
        MynauiSolidChartNoAxesColumnDecreasing = 0xEAE4,

        /// <summary>
        /// mynaui-solid-chart-no-axes-column-increasing
        /// Unicode: U+eae5
        /// </summary>
        MynauiSolidChartNoAxesColumnIncreasing = 0xEAE5,

        /// <summary>
        /// mynaui-solid-chart-no-axes-combined
        /// Unicode: U+eae7
        /// </summary>
        MynauiSolidChartNoAxesCombined = 0xEAE7,

        /// <summary>
        /// mynaui-solid-chart-no-axes-gantt
        /// Unicode: U+eae8
        /// </summary>
        MynauiSolidChartNoAxesGantt = 0xEAE8,

        /// <summary>
        /// mynaui-solid-chart-pie
        /// Unicode: U+eaeb
        /// </summary>
        MynauiSolidChartPie = 0xEAEB,

        /// <summary>
        /// mynaui-solid-chart-pie-one
        /// Unicode: U+eae9
        /// </summary>
        MynauiSolidChartPieOne = 0xEAE9,

        /// <summary>
        /// mynaui-solid-chart-pie-two
        /// Unicode: U+eaea
        /// </summary>
        MynauiSolidChartPieTwo = 0xEAEA,

        /// <summary>
        /// mynaui-solid-chart-scatter
        /// Unicode: U+eaec
        /// </summary>
        MynauiSolidChartScatter = 0xEAEC,

        /// <summary>
        /// mynaui-solid-chart-spline
        /// Unicode: U+eaed
        /// </summary>
        MynauiSolidChartSpline = 0xEAED,

        /// <summary>
        /// mynaui-solid-chat
        /// Unicode: U+eaf4
        /// </summary>
        MynauiSolidChat = 0xEAF4,

        /// <summary>
        /// mynaui-solid-chat-check
        /// Unicode: U+eaee
        /// </summary>
        MynauiSolidChatCheck = 0xEAEE,

        /// <summary>
        /// mynaui-solid-chat-dots
        /// Unicode: U+eaef
        /// </summary>
        MynauiSolidChatDots = 0xEAEF,

        /// <summary>
        /// mynaui-solid-chat-messages
        /// Unicode: U+eaf0
        /// </summary>
        MynauiSolidChatMessages = 0xEAF0,

        /// <summary>
        /// mynaui-solid-chat-minus
        /// Unicode: U+eaf1
        /// </summary>
        MynauiSolidChatMinus = 0xEAF1,

        /// <summary>
        /// mynaui-solid-chat-plus
        /// Unicode: U+eaf2
        /// </summary>
        MynauiSolidChatPlus = 0xEAF2,

        /// <summary>
        /// mynaui-solid-chat-x
        /// Unicode: U+eaf3
        /// </summary>
        MynauiSolidChatX = 0xEAF3,

        /// <summary>
        /// mynaui-solid-check
        /// Unicode: U+eafd
        /// </summary>
        MynauiSolidCheck = 0xEAFD,

        /// <summary>
        /// mynaui-solid-check-circle
        /// Unicode: U+eaf6
        /// </summary>
        MynauiSolidCheckCircle = 0xEAF6,

        /// <summary>
        /// mynaui-solid-check-circle-one
        /// Unicode: U+eaf5
        /// </summary>
        MynauiSolidCheckCircleOne = 0xEAF5,

        /// <summary>
        /// mynaui-solid-check-diamond
        /// Unicode: U+eaf7
        /// </summary>
        MynauiSolidCheckDiamond = 0xEAF7,

        /// <summary>
        /// mynaui-solid-check-hexagon
        /// Unicode: U+eaf8
        /// </summary>
        MynauiSolidCheckHexagon = 0xEAF8,

        /// <summary>
        /// mynaui-solid-check-octagon
        /// Unicode: U+eaf9
        /// </summary>
        MynauiSolidCheckOctagon = 0xEAF9,

        /// <summary>
        /// mynaui-solid-check-square
        /// Unicode: U+eafb
        /// </summary>
        MynauiSolidCheckSquare = 0xEAFB,

        /// <summary>
        /// mynaui-solid-check-square-one
        /// Unicode: U+eafa
        /// </summary>
        MynauiSolidCheckSquareOne = 0xEAFA,

        /// <summary>
        /// mynaui-solid-check-waves
        /// Unicode: U+eafc
        /// </summary>
        MynauiSolidCheckWaves = 0xEAFC,

        /// <summary>
        /// mynaui-solid-chevron-double-down
        /// Unicode: U+eb00
        /// </summary>
        MynauiSolidChevronDoubleDown = 0xEB00,

        /// <summary>
        /// mynaui-solid-chevron-double-down-left
        /// Unicode: U+eafe
        /// </summary>
        MynauiSolidChevronDoubleDownLeft = 0xEAFE,

        /// <summary>
        /// mynaui-solid-chevron-double-down-right
        /// Unicode: U+eaff
        /// </summary>
        MynauiSolidChevronDoubleDownRight = 0xEAFF,

        /// <summary>
        /// mynaui-solid-chevron-double-left
        /// Unicode: U+eb01
        /// </summary>
        MynauiSolidChevronDoubleLeft = 0xEB01,

        /// <summary>
        /// mynaui-solid-chevron-double-right
        /// Unicode: U+eb02
        /// </summary>
        MynauiSolidChevronDoubleRight = 0xEB02,

        /// <summary>
        /// mynaui-solid-chevron-double-up
        /// Unicode: U+eb05
        /// </summary>
        MynauiSolidChevronDoubleUp = 0xEB05,

        /// <summary>
        /// mynaui-solid-chevron-double-up-left
        /// Unicode: U+eb03
        /// </summary>
        MynauiSolidChevronDoubleUpLeft = 0xEB03,

        /// <summary>
        /// mynaui-solid-chevron-double-up-right
        /// Unicode: U+eb04
        /// </summary>
        MynauiSolidChevronDoubleUpRight = 0xEB04,

        /// <summary>
        /// mynaui-solid-chevron-down
        /// Unicode: U+eb11
        /// </summary>
        MynauiSolidChevronDown = 0xEB11,

        /// <summary>
        /// mynaui-solid-chevron-down-circle
        /// Unicode: U+eb06
        /// </summary>
        MynauiSolidChevronDownCircle = 0xEB06,

        /// <summary>
        /// mynaui-solid-chevron-down-left
        /// Unicode: U+eb0a
        /// </summary>
        MynauiSolidChevronDownLeft = 0xEB0A,

        /// <summary>
        /// mynaui-solid-chevron-down-left-circle
        /// Unicode: U+eb07
        /// </summary>
        MynauiSolidChevronDownLeftCircle = 0xEB07,

        /// <summary>
        /// mynaui-solid-chevron-down-left-square
        /// Unicode: U+eb08
        /// </summary>
        MynauiSolidChevronDownLeftSquare = 0xEB08,

        /// <summary>
        /// mynaui-solid-chevron-down-left-waves
        /// Unicode: U+eb09
        /// </summary>
        MynauiSolidChevronDownLeftWaves = 0xEB09,

        /// <summary>
        /// mynaui-solid-chevron-down-right
        /// Unicode: U+eb0e
        /// </summary>
        MynauiSolidChevronDownRight = 0xEB0E,

        /// <summary>
        /// mynaui-solid-chevron-down-right-circle
        /// Unicode: U+eb0b
        /// </summary>
        MynauiSolidChevronDownRightCircle = 0xEB0B,

        /// <summary>
        /// mynaui-solid-chevron-down-right-square
        /// Unicode: U+eb0c
        /// </summary>
        MynauiSolidChevronDownRightSquare = 0xEB0C,

        /// <summary>
        /// mynaui-solid-chevron-down-right-waves
        /// Unicode: U+eb0d
        /// </summary>
        MynauiSolidChevronDownRightWaves = 0xEB0D,

        /// <summary>
        /// mynaui-solid-chevron-down-square
        /// Unicode: U+eb0f
        /// </summary>
        MynauiSolidChevronDownSquare = 0xEB0F,

        /// <summary>
        /// mynaui-solid-chevron-down-waves
        /// Unicode: U+eb10
        /// </summary>
        MynauiSolidChevronDownWaves = 0xEB10,

        /// <summary>
        /// mynaui-solid-chevron-left
        /// Unicode: U+eb15
        /// </summary>
        MynauiSolidChevronLeft = 0xEB15,

        /// <summary>
        /// mynaui-solid-chevron-left-circle
        /// Unicode: U+eb12
        /// </summary>
        MynauiSolidChevronLeftCircle = 0xEB12,

        /// <summary>
        /// mynaui-solid-chevron-left-square
        /// Unicode: U+eb13
        /// </summary>
        MynauiSolidChevronLeftSquare = 0xEB13,

        /// <summary>
        /// mynaui-solid-chevron-left-waves
        /// Unicode: U+eb14
        /// </summary>
        MynauiSolidChevronLeftWaves = 0xEB14,

        /// <summary>
        /// mynaui-solid-chevron-right
        /// Unicode: U+eb19
        /// </summary>
        MynauiSolidChevronRight = 0xEB19,

        /// <summary>
        /// mynaui-solid-chevron-right-circle
        /// Unicode: U+eb16
        /// </summary>
        MynauiSolidChevronRightCircle = 0xEB16,

        /// <summary>
        /// mynaui-solid-chevron-right-square
        /// Unicode: U+eb17
        /// </summary>
        MynauiSolidChevronRightSquare = 0xEB17,

        /// <summary>
        /// mynaui-solid-chevron-right-waves
        /// Unicode: U+eb18
        /// </summary>
        MynauiSolidChevronRightWaves = 0xEB18,

        /// <summary>
        /// mynaui-solid-chevron-up
        /// Unicode: U+eb26
        /// </summary>
        MynauiSolidChevronUp = 0xEB26,

        /// <summary>
        /// mynaui-solid-chevron-up-circle
        /// Unicode: U+eb1a
        /// </summary>
        MynauiSolidChevronUpCircle = 0xEB1A,

        /// <summary>
        /// mynaui-solid-chevron-up-down
        /// Unicode: U+eb1b
        /// </summary>
        MynauiSolidChevronUpDown = 0xEB1B,

        /// <summary>
        /// mynaui-solid-chevron-up-left
        /// Unicode: U+eb1f
        /// </summary>
        MynauiSolidChevronUpLeft = 0xEB1F,

        /// <summary>
        /// mynaui-solid-chevron-up-left-circle
        /// Unicode: U+eb1c
        /// </summary>
        MynauiSolidChevronUpLeftCircle = 0xEB1C,

        /// <summary>
        /// mynaui-solid-chevron-up-left-square
        /// Unicode: U+eb1d
        /// </summary>
        MynauiSolidChevronUpLeftSquare = 0xEB1D,

        /// <summary>
        /// mynaui-solid-chevron-up-left-waves
        /// Unicode: U+eb1e
        /// </summary>
        MynauiSolidChevronUpLeftWaves = 0xEB1E,

        /// <summary>
        /// mynaui-solid-chevron-up-right
        /// Unicode: U+eb23
        /// </summary>
        MynauiSolidChevronUpRight = 0xEB23,

        /// <summary>
        /// mynaui-solid-chevron-up-right-circle
        /// Unicode: U+eb20
        /// </summary>
        MynauiSolidChevronUpRightCircle = 0xEB20,

        /// <summary>
        /// mynaui-solid-chevron-up-right-square
        /// Unicode: U+eb21
        /// </summary>
        MynauiSolidChevronUpRightSquare = 0xEB21,

        /// <summary>
        /// mynaui-solid-chevron-up-right-waves
        /// Unicode: U+eb22
        /// </summary>
        MynauiSolidChevronUpRightWaves = 0xEB22,

        /// <summary>
        /// mynaui-solid-chevron-up-square
        /// Unicode: U+eb24
        /// </summary>
        MynauiSolidChevronUpSquare = 0xEB24,

        /// <summary>
        /// mynaui-solid-chevron-up-waves
        /// Unicode: U+eb25
        /// </summary>
        MynauiSolidChevronUpWaves = 0xEB25,

        /// <summary>
        /// mynaui-solid-chip
        /// Unicode: U+eb27
        /// </summary>
        MynauiSolidChip = 0xEB27,

        /// <summary>
        /// mynaui-solid-cigarette
        /// Unicode: U+eb29
        /// </summary>
        MynauiSolidCigarette = 0xEB29,

        /// <summary>
        /// mynaui-solid-cigarette-off
        /// Unicode: U+eb28
        /// </summary>
        MynauiSolidCigaretteOff = 0xEB28,

        /// <summary>
        /// mynaui-solid-circle
        /// Unicode: U+eb2e
        /// </summary>
        MynauiSolidCircle = 0xEB2E,

        /// <summary>
        /// mynaui-solid-circle-dashed
        /// Unicode: U+eb2a
        /// </summary>
        MynauiSolidCircleDashed = 0xEB2A,

        /// <summary>
        /// mynaui-solid-circle-half
        /// Unicode: U+eb2c
        /// </summary>
        MynauiSolidCircleHalf = 0xEB2C,

        /// <summary>
        /// mynaui-solid-circle-half-circle
        /// Unicode: U+eb2b
        /// </summary>
        MynauiSolidCircleHalfCircle = 0xEB2B,

        /// <summary>
        /// mynaui-solid-circle-notch
        /// Unicode: U+eb2d
        /// </summary>
        MynauiSolidCircleNotch = 0xEB2D,

        /// <summary>
        /// mynaui-solid-click
        /// Unicode: U+eb2f
        /// </summary>
        MynauiSolidClick = 0xEB2F,

        /// <summary>
        /// mynaui-solid-clipboard
        /// Unicode: U+eb30
        /// </summary>
        MynauiSolidClipboard = 0xEB30,

        /// <summary>
        /// mynaui-solid-clock-circle
        /// Unicode: U+eb31
        /// </summary>
        MynauiSolidClockCircle = 0xEB31,

        /// <summary>
        /// mynaui-solid-clock-diamond
        /// Unicode: U+eb32
        /// </summary>
        MynauiSolidClockDiamond = 0xEB32,

        /// <summary>
        /// mynaui-solid-clock-eight
        /// Unicode: U+eb33
        /// </summary>
        MynauiSolidClockEight = 0xEB33,

        /// <summary>
        /// mynaui-solid-clock-eleven
        /// Unicode: U+eb34
        /// </summary>
        MynauiSolidClockEleven = 0xEB34,

        /// <summary>
        /// mynaui-solid-clock-five
        /// Unicode: U+eb35
        /// </summary>
        MynauiSolidClockFive = 0xEB35,

        /// <summary>
        /// mynaui-solid-clock-four
        /// Unicode: U+eb36
        /// </summary>
        MynauiSolidClockFour = 0xEB36,

        /// <summary>
        /// mynaui-solid-clock-hand
        /// Unicode: U+eb37
        /// </summary>
        MynauiSolidClockHand = 0xEB37,

        /// <summary>
        /// mynaui-solid-clock-hexagon
        /// Unicode: U+eb38
        /// </summary>
        MynauiSolidClockHexagon = 0xEB38,

        /// <summary>
        /// mynaui-solid-clock-nine
        /// Unicode: U+eb39
        /// </summary>
        MynauiSolidClockNine = 0xEB39,

        /// <summary>
        /// mynaui-solid-clock-octagon
        /// Unicode: U+eb3a
        /// </summary>
        MynauiSolidClockOctagon = 0xEB3A,

        /// <summary>
        /// mynaui-solid-clock-one
        /// Unicode: U+eb3b
        /// </summary>
        MynauiSolidClockOne = 0xEB3B,

        /// <summary>
        /// mynaui-solid-clock-seven
        /// Unicode: U+eb3c
        /// </summary>
        MynauiSolidClockSeven = 0xEB3C,

        /// <summary>
        /// mynaui-solid-clock-six
        /// Unicode: U+eb3d
        /// </summary>
        MynauiSolidClockSix = 0xEB3D,

        /// <summary>
        /// mynaui-solid-clock-square
        /// Unicode: U+eb3e
        /// </summary>
        MynauiSolidClockSquare = 0xEB3E,

        /// <summary>
        /// mynaui-solid-clock-ten
        /// Unicode: U+eb3f
        /// </summary>
        MynauiSolidClockTen = 0xEB3F,

        /// <summary>
        /// mynaui-solid-clock-three
        /// Unicode: U+eb40
        /// </summary>
        MynauiSolidClockThree = 0xEB40,

        /// <summary>
        /// mynaui-solid-clock-twelve
        /// Unicode: U+eb41
        /// </summary>
        MynauiSolidClockTwelve = 0xEB41,

        /// <summary>
        /// mynaui-solid-clock-two
        /// Unicode: U+eb42
        /// </summary>
        MynauiSolidClockTwo = 0xEB42,

        /// <summary>
        /// mynaui-solid-clock-waves
        /// Unicode: U+eb43
        /// </summary>
        MynauiSolidClockWaves = 0xEB43,

        /// <summary>
        /// mynaui-solid-cloud
        /// Unicode: U+eb52
        /// </summary>
        MynauiSolidCloud = 0xEB52,

        /// <summary>
        /// mynaui-solid-cloud-download
        /// Unicode: U+eb44
        /// </summary>
        MynauiSolidCloudDownload = 0xEB44,

        /// <summary>
        /// mynaui-solid-cloud-drizzle
        /// Unicode: U+eb45
        /// </summary>
        MynauiSolidCloudDrizzle = 0xEB45,

        /// <summary>
        /// mynaui-solid-cloud-fog
        /// Unicode: U+eb46
        /// </summary>
        MynauiSolidCloudFog = 0xEB46,

        /// <summary>
        /// mynaui-solid-cloud-hail
        /// Unicode: U+eb47
        /// </summary>
        MynauiSolidCloudHail = 0xEB47,

        /// <summary>
        /// mynaui-solid-cloud-lightning
        /// Unicode: U+eb48
        /// </summary>
        MynauiSolidCloudLightning = 0xEB48,

        /// <summary>
        /// mynaui-solid-cloud-moon
        /// Unicode: U+eb4a
        /// </summary>
        MynauiSolidCloudMoon = 0xEB4A,

        /// <summary>
        /// mynaui-solid-cloud-moon-rain
        /// Unicode: U+eb49
        /// </summary>
        MynauiSolidCloudMoonRain = 0xEB49,

        /// <summary>
        /// mynaui-solid-cloud-off
        /// Unicode: U+eb4b
        /// </summary>
        MynauiSolidCloudOff = 0xEB4B,

        /// <summary>
        /// mynaui-solid-cloud-rain
        /// Unicode: U+eb4d
        /// </summary>
        MynauiSolidCloudRain = 0xEB4D,

        /// <summary>
        /// mynaui-solid-cloud-rain-wind
        /// Unicode: U+eb4c
        /// </summary>
        MynauiSolidCloudRainWind = 0xEB4C,

        /// <summary>
        /// mynaui-solid-cloud-snow
        /// Unicode: U+eb4e
        /// </summary>
        MynauiSolidCloudSnow = 0xEB4E,

        /// <summary>
        /// mynaui-solid-cloud-sun
        /// Unicode: U+eb50
        /// </summary>
        MynauiSolidCloudSun = 0xEB50,

        /// <summary>
        /// mynaui-solid-cloud-sun-rain
        /// Unicode: U+eb4f
        /// </summary>
        MynauiSolidCloudSunRain = 0xEB4F,

        /// <summary>
        /// mynaui-solid-cloud-upload
        /// Unicode: U+eb51
        /// </summary>
        MynauiSolidCloudUpload = 0xEB51,

        /// <summary>
        /// mynaui-solid-cloudy
        /// Unicode: U+eb53
        /// </summary>
        MynauiSolidCloudy = 0xEB53,

        /// <summary>
        /// mynaui-solid-cocktail
        /// Unicode: U+eb54
        /// </summary>
        MynauiSolidCocktail = 0xEB54,

        /// <summary>
        /// mynaui-solid-code
        /// Unicode: U+eb5b
        /// </summary>
        MynauiSolidCode = 0xEB5B,

        /// <summary>
        /// mynaui-solid-code-circle
        /// Unicode: U+eb55
        /// </summary>
        MynauiSolidCodeCircle = 0xEB55,

        /// <summary>
        /// mynaui-solid-code-diamond
        /// Unicode: U+eb56
        /// </summary>
        MynauiSolidCodeDiamond = 0xEB56,

        /// <summary>
        /// mynaui-solid-code-hexagon
        /// Unicode: U+eb57
        /// </summary>
        MynauiSolidCodeHexagon = 0xEB57,

        /// <summary>
        /// mynaui-solid-code-octagon
        /// Unicode: U+eb58
        /// </summary>
        MynauiSolidCodeOctagon = 0xEB58,

        /// <summary>
        /// mynaui-solid-code-square
        /// Unicode: U+eb59
        /// </summary>
        MynauiSolidCodeSquare = 0xEB59,

        /// <summary>
        /// mynaui-solid-code-waves
        /// Unicode: U+eb5a
        /// </summary>
        MynauiSolidCodeWaves = 0xEB5A,

        /// <summary>
        /// mynaui-solid-coffee
        /// Unicode: U+eb5c
        /// </summary>
        MynauiSolidCoffee = 0xEB5C,

        /// <summary>
        /// mynaui-solid-cog
        /// Unicode: U+eb61
        /// </summary>
        MynauiSolidCog = 0xEB61,

        /// <summary>
        /// mynaui-solid-cog-four
        /// Unicode: U+eb5d
        /// </summary>
        MynauiSolidCogFour = 0xEB5D,

        /// <summary>
        /// mynaui-solid-cog-one
        /// Unicode: U+eb5e
        /// </summary>
        MynauiSolidCogOne = 0xEB5E,

        /// <summary>
        /// mynaui-solid-cog-three
        /// Unicode: U+eb5f
        /// </summary>
        MynauiSolidCogThree = 0xEB5F,

        /// <summary>
        /// mynaui-solid-cog-two
        /// Unicode: U+eb60
        /// </summary>
        MynauiSolidCogTwo = 0xEB60,

        /// <summary>
        /// mynaui-solid-columns
        /// Unicode: U+eb62
        /// </summary>
        MynauiSolidColumns = 0xEB62,

        /// <summary>
        /// mynaui-solid-command
        /// Unicode: U+eb63
        /// </summary>
        MynauiSolidCommand = 0xEB63,

        /// <summary>
        /// mynaui-solid-compass
        /// Unicode: U+eb64
        /// </summary>
        MynauiSolidCompass = 0xEB64,

        /// <summary>
        /// mynaui-solid-components
        /// Unicode: U+eb65
        /// </summary>
        MynauiSolidComponents = 0xEB65,

        /// <summary>
        /// mynaui-solid-confetti
        /// Unicode: U+eb66
        /// </summary>
        MynauiSolidConfetti = 0xEB66,

        /// <summary>
        /// mynaui-solid-config
        /// Unicode: U+eb68
        /// </summary>
        MynauiSolidConfig = 0xEB68,

        /// <summary>
        /// mynaui-solid-config-vertical
        /// Unicode: U+eb67
        /// </summary>
        MynauiSolidConfigVertical = 0xEB67,

        /// <summary>
        /// mynaui-solid-contactless
        /// Unicode: U+eb6a
        /// </summary>
        MynauiSolidContactless = 0xEB6A,

        /// <summary>
        /// mynaui-solid-contactless-circle
        /// Unicode: U+eb69
        /// </summary>
        MynauiSolidContactlessCircle = 0xEB69,

        /// <summary>
        /// mynaui-solid-controller
        /// Unicode: U+eb6b
        /// </summary>
        MynauiSolidController = 0xEB6B,

        /// <summary>
        /// mynaui-solid-cookie
        /// Unicode: U+eb6c
        /// </summary>
        MynauiSolidCookie = 0xEB6C,

        /// <summary>
        /// mynaui-solid-copy
        /// Unicode: U+eb6d
        /// </summary>
        MynauiSolidCopy = 0xEB6D,

        /// <summary>
        /// mynaui-solid-copyleft
        /// Unicode: U+eb6e
        /// </summary>
        MynauiSolidCopyleft = 0xEB6E,

        /// <summary>
        /// mynaui-solid-copyright
        /// Unicode: U+eb70
        /// </summary>
        MynauiSolidCopyright = 0xEB70,

        /// <summary>
        /// mynaui-solid-copyright-slash
        /// Unicode: U+eb6f
        /// </summary>
        MynauiSolidCopyrightSlash = 0xEB6F,

        /// <summary>
        /// mynaui-solid-corner-down-left
        /// Unicode: U+eb71
        /// </summary>
        MynauiSolidCornerDownLeft = 0xEB71,

        /// <summary>
        /// mynaui-solid-corner-down-right
        /// Unicode: U+eb72
        /// </summary>
        MynauiSolidCornerDownRight = 0xEB72,

        /// <summary>
        /// mynaui-solid-corner-left-down
        /// Unicode: U+eb73
        /// </summary>
        MynauiSolidCornerLeftDown = 0xEB73,

        /// <summary>
        /// mynaui-solid-corner-left-up
        /// Unicode: U+eb74
        /// </summary>
        MynauiSolidCornerLeftUp = 0xEB74,

        /// <summary>
        /// mynaui-solid-corner-right-down
        /// Unicode: U+eb75
        /// </summary>
        MynauiSolidCornerRightDown = 0xEB75,

        /// <summary>
        /// mynaui-solid-corner-right-up
        /// Unicode: U+eb76
        /// </summary>
        MynauiSolidCornerRightUp = 0xEB76,

        /// <summary>
        /// mynaui-solid-corner-up-left
        /// Unicode: U+eb77
        /// </summary>
        MynauiSolidCornerUpLeft = 0xEB77,

        /// <summary>
        /// mynaui-solid-corner-up-right
        /// Unicode: U+eb78
        /// </summary>
        MynauiSolidCornerUpRight = 0xEB78,

        /// <summary>
        /// mynaui-solid-credit-card
        /// Unicode: U+eb7d
        /// </summary>
        MynauiSolidCreditCard = 0xEB7D,

        /// <summary>
        /// mynaui-solid-credit-card-check
        /// Unicode: U+eb79
        /// </summary>
        MynauiSolidCreditCardCheck = 0xEB79,

        /// <summary>
        /// mynaui-solid-credit-card-minus
        /// Unicode: U+eb7a
        /// </summary>
        MynauiSolidCreditCardMinus = 0xEB7A,

        /// <summary>
        /// mynaui-solid-credit-card-plus
        /// Unicode: U+eb7b
        /// </summary>
        MynauiSolidCreditCardPlus = 0xEB7B,

        /// <summary>
        /// mynaui-solid-credit-card-x
        /// Unicode: U+eb7c
        /// </summary>
        MynauiSolidCreditCardX = 0xEB7C,

        /// <summary>
        /// mynaui-solid-croissant
        /// Unicode: U+eb7e
        /// </summary>
        MynauiSolidCroissant = 0xEB7E,

        /// <summary>
        /// mynaui-solid-crop
        /// Unicode: U+eb7f
        /// </summary>
        MynauiSolidCrop = 0xEB7F,

        /// <summary>
        /// mynaui-solid-crosshair
        /// Unicode: U+eb80
        /// </summary>
        MynauiSolidCrosshair = 0xEB80,

        /// <summary>
        /// mynaui-solid-cupcake
        /// Unicode: U+eb81
        /// </summary>
        MynauiSolidCupcake = 0xEB81,

        /// <summary>
        /// mynaui-solid-danger
        /// Unicode: U+eb89
        /// </summary>
        MynauiSolidDanger = 0xEB89,

        /// <summary>
        /// mynaui-solid-danger-circle
        /// Unicode: U+eb82
        /// </summary>
        MynauiSolidDangerCircle = 0xEB82,

        /// <summary>
        /// mynaui-solid-danger-diamond
        /// Unicode: U+eb83
        /// </summary>
        MynauiSolidDangerDiamond = 0xEB83,

        /// <summary>
        /// mynaui-solid-danger-hexagon
        /// Unicode: U+eb84
        /// </summary>
        MynauiSolidDangerHexagon = 0xEB84,

        /// <summary>
        /// mynaui-solid-danger-octagon
        /// Unicode: U+eb85
        /// </summary>
        MynauiSolidDangerOctagon = 0xEB85,

        /// <summary>
        /// mynaui-solid-danger-square
        /// Unicode: U+eb86
        /// </summary>
        MynauiSolidDangerSquare = 0xEB86,

        /// <summary>
        /// mynaui-solid-danger-triangle
        /// Unicode: U+eb87
        /// </summary>
        MynauiSolidDangerTriangle = 0xEB87,

        /// <summary>
        /// mynaui-solid-danger-waves
        /// Unicode: U+eb88
        /// </summary>
        MynauiSolidDangerWaves = 0xEB88,

        /// <summary>
        /// mynaui-solid-database
        /// Unicode: U+eb8a
        /// </summary>
        MynauiSolidDatabase = 0xEB8A,

        /// <summary>
        /// mynaui-solid-daze-circle
        /// Unicode: U+eb8b
        /// </summary>
        MynauiSolidDazeCircle = 0xEB8B,

        /// <summary>
        /// mynaui-solid-daze-ghost
        /// Unicode: U+eb8c
        /// </summary>
        MynauiSolidDazeGhost = 0xEB8C,

        /// <summary>
        /// mynaui-solid-daze-square
        /// Unicode: U+eb8d
        /// </summary>
        MynauiSolidDazeSquare = 0xEB8D,

        /// <summary>
        /// mynaui-solid-delete
        /// Unicode: U+eb8e
        /// </summary>
        MynauiSolidDelete = 0xEB8E,

        /// <summary>
        /// mynaui-solid-desktop
        /// Unicode: U+eb8f
        /// </summary>
        MynauiSolidDesktop = 0xEB8F,

        /// <summary>
        /// mynaui-solid-diamond
        /// Unicode: U+eb90
        /// </summary>
        MynauiSolidDiamond = 0xEB90,

        /// <summary>
        /// mynaui-solid-dice-five
        /// Unicode: U+eb91
        /// </summary>
        MynauiSolidDiceFive = 0xEB91,

        /// <summary>
        /// mynaui-solid-dice-four
        /// Unicode: U+eb92
        /// </summary>
        MynauiSolidDiceFour = 0xEB92,

        /// <summary>
        /// mynaui-solid-dice-one
        /// Unicode: U+eb93
        /// </summary>
        MynauiSolidDiceOne = 0xEB93,

        /// <summary>
        /// mynaui-solid-dice-six
        /// Unicode: U+eb94
        /// </summary>
        MynauiSolidDiceSix = 0xEB94,

        /// <summary>
        /// mynaui-solid-dice-three
        /// Unicode: U+eb95
        /// </summary>
        MynauiSolidDiceThree = 0xEB95,

        /// <summary>
        /// mynaui-solid-dice-two
        /// Unicode: U+eb96
        /// </summary>
        MynauiSolidDiceTwo = 0xEB96,

        /// <summary>
        /// mynaui-solid-dislike
        /// Unicode: U+eb97
        /// </summary>
        MynauiSolidDislike = 0xEB97,

        /// <summary>
        /// mynaui-solid-divide
        /// Unicode: U+eb98
        /// </summary>
        MynauiSolidDivide = 0xEB98,

        /// <summary>
        /// mynaui-solid-dollar
        /// Unicode: U+eb9f
        /// </summary>
        MynauiSolidDollar = 0xEB9F,

        /// <summary>
        /// mynaui-solid-dollar-circle
        /// Unicode: U+eb99
        /// </summary>
        MynauiSolidDollarCircle = 0xEB99,

        /// <summary>
        /// mynaui-solid-dollar-diamond
        /// Unicode: U+eb9a
        /// </summary>
        MynauiSolidDollarDiamond = 0xEB9A,

        /// <summary>
        /// mynaui-solid-dollar-hexagon
        /// Unicode: U+eb9b
        /// </summary>
        MynauiSolidDollarHexagon = 0xEB9B,

        /// <summary>
        /// mynaui-solid-dollar-octagon
        /// Unicode: U+eb9c
        /// </summary>
        MynauiSolidDollarOctagon = 0xEB9C,

        /// <summary>
        /// mynaui-solid-dollar-square
        /// Unicode: U+eb9d
        /// </summary>
        MynauiSolidDollarSquare = 0xEB9D,

        /// <summary>
        /// mynaui-solid-dollar-waves
        /// Unicode: U+eb9e
        /// </summary>
        MynauiSolidDollarWaves = 0xEB9E,

        /// <summary>
        /// mynaui-solid-door-closed
        /// Unicode: U+eba1
        /// </summary>
        MynauiSolidDoorClosed = 0xEBA1,

        /// <summary>
        /// mynaui-solid-door-closed-locked
        /// Unicode: U+eba0
        /// </summary>
        MynauiSolidDoorClosedLocked = 0xEBA0,

        /// <summary>
        /// mynaui-solid-door-open
        /// Unicode: U+eba2
        /// </summary>
        MynauiSolidDoorOpen = 0xEBA2,

        /// <summary>
        /// mynaui-solid-dots
        /// Unicode: U+ebb0
        /// </summary>
        MynauiSolidDots = 0xEBB0,

        /// <summary>
        /// mynaui-solid-dots-circle
        /// Unicode: U+eba3
        /// </summary>
        MynauiSolidDotsCircle = 0xEBA3,

        /// <summary>
        /// mynaui-solid-dots-diamond
        /// Unicode: U+eba4
        /// </summary>
        MynauiSolidDotsDiamond = 0xEBA4,

        /// <summary>
        /// mynaui-solid-dots-hexagon
        /// Unicode: U+eba5
        /// </summary>
        MynauiSolidDotsHexagon = 0xEBA5,

        /// <summary>
        /// mynaui-solid-dots-octagon
        /// Unicode: U+eba6
        /// </summary>
        MynauiSolidDotsOctagon = 0xEBA6,

        /// <summary>
        /// mynaui-solid-dots-square
        /// Unicode: U+eba7
        /// </summary>
        MynauiSolidDotsSquare = 0xEBA7,

        /// <summary>
        /// mynaui-solid-dots-vertical
        /// Unicode: U+ebae
        /// </summary>
        MynauiSolidDotsVertical = 0xEBAE,

        /// <summary>
        /// mynaui-solid-dots-vertical-circle
        /// Unicode: U+eba8
        /// </summary>
        MynauiSolidDotsVerticalCircle = 0xEBA8,

        /// <summary>
        /// mynaui-solid-dots-vertical-diamond
        /// Unicode: U+eba9
        /// </summary>
        MynauiSolidDotsVerticalDiamond = 0xEBA9,

        /// <summary>
        /// mynaui-solid-dots-vertical-hexagon
        /// Unicode: U+ebaa
        /// </summary>
        MynauiSolidDotsVerticalHexagon = 0xEBAA,

        /// <summary>
        /// mynaui-solid-dots-vertical-octagon
        /// Unicode: U+ebab
        /// </summary>
        MynauiSolidDotsVerticalOctagon = 0xEBAB,

        /// <summary>
        /// mynaui-solid-dots-vertical-square
        /// Unicode: U+ebac
        /// </summary>
        MynauiSolidDotsVerticalSquare = 0xEBAC,

        /// <summary>
        /// mynaui-solid-dots-vertical-waves
        /// Unicode: U+ebad
        /// </summary>
        MynauiSolidDotsVerticalWaves = 0xEBAD,

        /// <summary>
        /// mynaui-solid-dots-waves
        /// Unicode: U+ebaf
        /// </summary>
        MynauiSolidDotsWaves = 0xEBAF,

        /// <summary>
        /// mynaui-solid-download
        /// Unicode: U+ebb1
        /// </summary>
        MynauiSolidDownload = 0xEBB1,

        /// <summary>
        /// mynaui-solid-drop
        /// Unicode: U+ebb2
        /// </summary>
        MynauiSolidDrop = 0xEBB2,

        /// <summary>
        /// mynaui-solid-droplet
        /// Unicode: U+ebb4
        /// </summary>
        MynauiSolidDroplet = 0xEBB4,

        /// <summary>
        /// mynaui-solid-droplet-off
        /// Unicode: U+ebb3
        /// </summary>
        MynauiSolidDropletOff = 0xEBB3,

        /// <summary>
        /// mynaui-solid-droplets
        /// Unicode: U+ebb5
        /// </summary>
        MynauiSolidDroplets = 0xEBB5,

        /// <summary>
        /// mynaui-solid-ear
        /// Unicode: U+ebb7
        /// </summary>
        MynauiSolidEar = 0xEBB7,

        /// <summary>
        /// mynaui-solid-ear-slash
        /// Unicode: U+ebb6
        /// </summary>
        MynauiSolidEarSlash = 0xEBB6,

        /// <summary>
        /// mynaui-solid-earth
        /// Unicode: U+ebb8
        /// </summary>
        MynauiSolidEarth = 0xEBB8,

        /// <summary>
        /// mynaui-solid-eclipse
        /// Unicode: U+ebb9
        /// </summary>
        MynauiSolidEclipse = 0xEBB9,

        /// <summary>
        /// mynaui-solid-edit
        /// Unicode: U+ebbb
        /// </summary>
        MynauiSolidEdit = 0xEBBB,

        /// <summary>
        /// mynaui-solid-edit-one
        /// Unicode: U+ebba
        /// </summary>
        MynauiSolidEditOne = 0xEBBA,

        /// <summary>
        /// mynaui-solid-egg
        /// Unicode: U+ebbc
        /// </summary>
        MynauiSolidEgg = 0xEBBC,

        /// <summary>
        /// mynaui-solid-eight
        /// Unicode: U+ebc3
        /// </summary>
        MynauiSolidEight = 0xEBC3,

        /// <summary>
        /// mynaui-solid-eight-circle
        /// Unicode: U+ebbd
        /// </summary>
        MynauiSolidEightCircle = 0xEBBD,

        /// <summary>
        /// mynaui-solid-eight-diamond
        /// Unicode: U+ebbe
        /// </summary>
        MynauiSolidEightDiamond = 0xEBBE,

        /// <summary>
        /// mynaui-solid-eight-hexagon
        /// Unicode: U+ebbf
        /// </summary>
        MynauiSolidEightHexagon = 0xEBBF,

        /// <summary>
        /// mynaui-solid-eight-octagon
        /// Unicode: U+ebc0
        /// </summary>
        MynauiSolidEightOctagon = 0xEBC0,

        /// <summary>
        /// mynaui-solid-eight-square
        /// Unicode: U+ebc1
        /// </summary>
        MynauiSolidEightSquare = 0xEBC1,

        /// <summary>
        /// mynaui-solid-eight-waves
        /// Unicode: U+ebc2
        /// </summary>
        MynauiSolidEightWaves = 0xEBC2,

        /// <summary>
        /// mynaui-solid-elevator
        /// Unicode: U+ebc4
        /// </summary>
        MynauiSolidElevator = 0xEBC4,

        /// <summary>
        /// mynaui-solid-envelope
        /// Unicode: U+ebc6
        /// </summary>
        MynauiSolidEnvelope = 0xEBC6,

        /// <summary>
        /// mynaui-solid-envelope-open
        /// Unicode: U+ebc5
        /// </summary>
        MynauiSolidEnvelopeOpen = 0xEBC5,

        /// <summary>
        /// mynaui-solid-euro
        /// Unicode: U+ebcd
        /// </summary>
        MynauiSolidEuro = 0xEBCD,

        /// <summary>
        /// mynaui-solid-euro-circle
        /// Unicode: U+ebc7
        /// </summary>
        MynauiSolidEuroCircle = 0xEBC7,

        /// <summary>
        /// mynaui-solid-euro-diamond
        /// Unicode: U+ebc8
        /// </summary>
        MynauiSolidEuroDiamond = 0xEBC8,

        /// <summary>
        /// mynaui-solid-euro-hexagon
        /// Unicode: U+ebc9
        /// </summary>
        MynauiSolidEuroHexagon = 0xEBC9,

        /// <summary>
        /// mynaui-solid-euro-octagon
        /// Unicode: U+ebca
        /// </summary>
        MynauiSolidEuroOctagon = 0xEBCA,

        /// <summary>
        /// mynaui-solid-euro-square
        /// Unicode: U+ebcb
        /// </summary>
        MynauiSolidEuroSquare = 0xEBCB,

        /// <summary>
        /// mynaui-solid-euro-waves
        /// Unicode: U+ebcc
        /// </summary>
        MynauiSolidEuroWaves = 0xEBCC,

        /// <summary>
        /// mynaui-solid-exclude
        /// Unicode: U+ebce
        /// </summary>
        MynauiSolidExclude = 0xEBCE,

        /// <summary>
        /// mynaui-solid-external-link
        /// Unicode: U+ebcf
        /// </summary>
        MynauiSolidExternalLink = 0xEBCF,

        /// <summary>
        /// mynaui-solid-eye
        /// Unicode: U+ebd1
        /// </summary>
        MynauiSolidEye = 0xEBD1,

        /// <summary>
        /// mynaui-solid-eye-slash
        /// Unicode: U+ebd0
        /// </summary>
        MynauiSolidEyeSlash = 0xEBD0,

        /// <summary>
        /// mynaui-solid-face-id
        /// Unicode: U+ebd2
        /// </summary>
        MynauiSolidFaceId = 0xEBD2,

        /// <summary>
        /// mynaui-solid-fat-arrow-down
        /// Unicode: U+ebd5
        /// </summary>
        MynauiSolidFatArrowDown = 0xEBD5,

        /// <summary>
        /// mynaui-solid-fat-arrow-down-left
        /// Unicode: U+ebd3
        /// </summary>
        MynauiSolidFatArrowDownLeft = 0xEBD3,

        /// <summary>
        /// mynaui-solid-fat-arrow-down-right
        /// Unicode: U+ebd4
        /// </summary>
        MynauiSolidFatArrowDownRight = 0xEBD4,

        /// <summary>
        /// mynaui-solid-fat-arrow-left
        /// Unicode: U+ebd6
        /// </summary>
        MynauiSolidFatArrowLeft = 0xEBD6,

        /// <summary>
        /// mynaui-solid-fat-arrow-right
        /// Unicode: U+ebd7
        /// </summary>
        MynauiSolidFatArrowRight = 0xEBD7,

        /// <summary>
        /// mynaui-solid-fat-arrow-up
        /// Unicode: U+ebda
        /// </summary>
        MynauiSolidFatArrowUp = 0xEBDA,

        /// <summary>
        /// mynaui-solid-fat-arrow-up-left
        /// Unicode: U+ebd8
        /// </summary>
        MynauiSolidFatArrowUpLeft = 0xEBD8,

        /// <summary>
        /// mynaui-solid-fat-arrow-up-right
        /// Unicode: U+ebd9
        /// </summary>
        MynauiSolidFatArrowUpRight = 0xEBD9,

        /// <summary>
        /// mynaui-solid-fat-corner-down-left
        /// Unicode: U+ebdb
        /// </summary>
        MynauiSolidFatCornerDownLeft = 0xEBDB,

        /// <summary>
        /// mynaui-solid-fat-corner-down-right
        /// Unicode: U+ebdc
        /// </summary>
        MynauiSolidFatCornerDownRight = 0xEBDC,

        /// <summary>
        /// mynaui-solid-fat-corner-left-down
        /// Unicode: U+ebdd
        /// </summary>
        MynauiSolidFatCornerLeftDown = 0xEBDD,

        /// <summary>
        /// mynaui-solid-fat-corner-left-up
        /// Unicode: U+ebde
        /// </summary>
        MynauiSolidFatCornerLeftUp = 0xEBDE,

        /// <summary>
        /// mynaui-solid-fat-corner-right-down
        /// Unicode: U+ebdf
        /// </summary>
        MynauiSolidFatCornerRightDown = 0xEBDF,

        /// <summary>
        /// mynaui-solid-fat-corner-right-up
        /// Unicode: U+ebe0
        /// </summary>
        MynauiSolidFatCornerRightUp = 0xEBE0,

        /// <summary>
        /// mynaui-solid-fat-corner-up-left
        /// Unicode: U+ebe1
        /// </summary>
        MynauiSolidFatCornerUpLeft = 0xEBE1,

        /// <summary>
        /// mynaui-solid-fat-corner-up-right
        /// Unicode: U+ebe2
        /// </summary>
        MynauiSolidFatCornerUpRight = 0xEBE2,

        /// <summary>
        /// mynaui-solid-female
        /// Unicode: U+ebe3
        /// </summary>
        MynauiSolidFemale = 0xEBE3,

        /// <summary>
        /// mynaui-solid-file
        /// Unicode: U+ebe9
        /// </summary>
        MynauiSolidFile = 0xEBE9,

        /// <summary>
        /// mynaui-solid-file-check
        /// Unicode: U+ebe4
        /// </summary>
        MynauiSolidFileCheck = 0xEBE4,

        /// <summary>
        /// mynaui-solid-file-minus
        /// Unicode: U+ebe5
        /// </summary>
        MynauiSolidFileMinus = 0xEBE5,

        /// <summary>
        /// mynaui-solid-file-plus
        /// Unicode: U+ebe6
        /// </summary>
        MynauiSolidFilePlus = 0xEBE6,

        /// <summary>
        /// mynaui-solid-file-text
        /// Unicode: U+ebe7
        /// </summary>
        MynauiSolidFileText = 0xEBE7,

        /// <summary>
        /// mynaui-solid-file-x
        /// Unicode: U+ebe8
        /// </summary>
        MynauiSolidFileX = 0xEBE8,

        /// <summary>
        /// mynaui-solid-film
        /// Unicode: U+ebea
        /// </summary>
        MynauiSolidFilm = 0xEBEA,

        /// <summary>
        /// mynaui-solid-filter
        /// Unicode: U+ebec
        /// </summary>
        MynauiSolidFilter = 0xEBEC,

        /// <summary>
        /// mynaui-solid-filter-one
        /// Unicode: U+ebeb
        /// </summary>
        MynauiSolidFilterOne = 0xEBEB,

        /// <summary>
        /// mynaui-solid-fine-tune
        /// Unicode: U+ebed
        /// </summary>
        MynauiSolidFineTune = 0xEBED,

        /// <summary>
        /// mynaui-solid-fire
        /// Unicode: U+ebee
        /// </summary>
        MynauiSolidFire = 0xEBEE,

        /// <summary>
        /// mynaui-solid-five
        /// Unicode: U+ebf5
        /// </summary>
        MynauiSolidFive = 0xEBF5,

        /// <summary>
        /// mynaui-solid-five-circle
        /// Unicode: U+ebef
        /// </summary>
        MynauiSolidFiveCircle = 0xEBEF,

        /// <summary>
        /// mynaui-solid-five-diamond
        /// Unicode: U+ebf0
        /// </summary>
        MynauiSolidFiveDiamond = 0xEBF0,

        /// <summary>
        /// mynaui-solid-five-hexagon
        /// Unicode: U+ebf1
        /// </summary>
        MynauiSolidFiveHexagon = 0xEBF1,

        /// <summary>
        /// mynaui-solid-five-octagon
        /// Unicode: U+ebf2
        /// </summary>
        MynauiSolidFiveOctagon = 0xEBF2,

        /// <summary>
        /// mynaui-solid-five-square
        /// Unicode: U+ebf3
        /// </summary>
        MynauiSolidFiveSquare = 0xEBF3,

        /// <summary>
        /// mynaui-solid-five-waves
        /// Unicode: U+ebf4
        /// </summary>
        MynauiSolidFiveWaves = 0xEBF4,

        /// <summary>
        /// mynaui-solid-flag
        /// Unicode: U+ebf7
        /// </summary>
        MynauiSolidFlag = 0xEBF7,

        /// <summary>
        /// mynaui-solid-flag-one
        /// Unicode: U+ebf6
        /// </summary>
        MynauiSolidFlagOne = 0xEBF6,

        /// <summary>
        /// mynaui-solid-flame
        /// Unicode: U+ebf9
        /// </summary>
        MynauiSolidFlame = 0xEBF9,

        /// <summary>
        /// mynaui-solid-flame-kindling
        /// Unicode: U+ebf8
        /// </summary>
        MynauiSolidFlameKindling = 0xEBF8,

        /// <summary>
        /// mynaui-solid-flask
        /// Unicode: U+ebfa
        /// </summary>
        MynauiSolidFlask = 0xEBFA,

        /// <summary>
        /// mynaui-solid-flower
        /// Unicode: U+ebfc
        /// </summary>
        MynauiSolidFlower = 0xEBFC,

        /// <summary>
        /// mynaui-solid-flower-2
        /// Unicode: U+ebfb
        /// </summary>
        MynauiSolidFlower2 = 0xEBFB,

        /// <summary>
        /// mynaui-solid-folder
        /// Unicode: U+ec06
        /// </summary>
        MynauiSolidFolder = 0xEC06,

        /// <summary>
        /// mynaui-solid-folder-check
        /// Unicode: U+ebfd
        /// </summary>
        MynauiSolidFolderCheck = 0xEBFD,

        /// <summary>
        /// mynaui-solid-folder-heart
        /// Unicode: U+ebfe
        /// </summary>
        MynauiSolidFolderHeart = 0xEBFE,

        /// <summary>
        /// mynaui-solid-folder-kanban
        /// Unicode: U+ebff
        /// </summary>
        MynauiSolidFolderKanban = 0xEBFF,

        /// <summary>
        /// mynaui-solid-folder-minus
        /// Unicode: U+ec00
        /// </summary>
        MynauiSolidFolderMinus = 0xEC00,

        /// <summary>
        /// mynaui-solid-folder-one
        /// Unicode: U+ec01
        /// </summary>
        MynauiSolidFolderOne = 0xEC01,

        /// <summary>
        /// mynaui-solid-folder-plus
        /// Unicode: U+ec02
        /// </summary>
        MynauiSolidFolderPlus = 0xEC02,

        /// <summary>
        /// mynaui-solid-folder-slash
        /// Unicode: U+ec03
        /// </summary>
        MynauiSolidFolderSlash = 0xEC03,

        /// <summary>
        /// mynaui-solid-folder-two
        /// Unicode: U+ec04
        /// </summary>
        MynauiSolidFolderTwo = 0xEC04,

        /// <summary>
        /// mynaui-solid-folder-x
        /// Unicode: U+ec05
        /// </summary>
        MynauiSolidFolderX = 0xEC05,

        /// <summary>
        /// mynaui-solid-forward
        /// Unicode: U+ec0d
        /// </summary>
        MynauiSolidForward = 0xEC0D,

        /// <summary>
        /// mynaui-solid-forward-circle
        /// Unicode: U+ec07
        /// </summary>
        MynauiSolidForwardCircle = 0xEC07,

        /// <summary>
        /// mynaui-solid-forward-diamond
        /// Unicode: U+ec08
        /// </summary>
        MynauiSolidForwardDiamond = 0xEC08,

        /// <summary>
        /// mynaui-solid-forward-hexagon
        /// Unicode: U+ec09
        /// </summary>
        MynauiSolidForwardHexagon = 0xEC09,

        /// <summary>
        /// mynaui-solid-forward-octagon
        /// Unicode: U+ec0a
        /// </summary>
        MynauiSolidForwardOctagon = 0xEC0A,

        /// <summary>
        /// mynaui-solid-forward-square
        /// Unicode: U+ec0b
        /// </summary>
        MynauiSolidForwardSquare = 0xEC0B,

        /// <summary>
        /// mynaui-solid-forward-waves
        /// Unicode: U+ec0c
        /// </summary>
        MynauiSolidForwardWaves = 0xEC0C,

        /// <summary>
        /// mynaui-solid-four
        /// Unicode: U+ec14
        /// </summary>
        MynauiSolidFour = 0xEC14,

        /// <summary>
        /// mynaui-solid-four-circle
        /// Unicode: U+ec0e
        /// </summary>
        MynauiSolidFourCircle = 0xEC0E,

        /// <summary>
        /// mynaui-solid-four-diamond
        /// Unicode: U+ec0f
        /// </summary>
        MynauiSolidFourDiamond = 0xEC0F,

        /// <summary>
        /// mynaui-solid-four-hexagon
        /// Unicode: U+ec10
        /// </summary>
        MynauiSolidFourHexagon = 0xEC10,

        /// <summary>
        /// mynaui-solid-four-octagon
        /// Unicode: U+ec11
        /// </summary>
        MynauiSolidFourOctagon = 0xEC11,

        /// <summary>
        /// mynaui-solid-four-square
        /// Unicode: U+ec12
        /// </summary>
        MynauiSolidFourSquare = 0xEC12,

        /// <summary>
        /// mynaui-solid-four-waves
        /// Unicode: U+ec13
        /// </summary>
        MynauiSolidFourWaves = 0xEC13,

        /// <summary>
        /// mynaui-solid-frame
        /// Unicode: U+ec15
        /// </summary>
        MynauiSolidFrame = 0xEC15,

        /// <summary>
        /// mynaui-solid-funny-circle
        /// Unicode: U+ec16
        /// </summary>
        MynauiSolidFunnyCircle = 0xEC16,

        /// <summary>
        /// mynaui-solid-funny-ghost
        /// Unicode: U+ec17
        /// </summary>
        MynauiSolidFunnyGhost = 0xEC17,

        /// <summary>
        /// mynaui-solid-funny-square
        /// Unicode: U+ec18
        /// </summary>
        MynauiSolidFunnySquare = 0xEC18,

        /// <summary>
        /// mynaui-solid-gift
        /// Unicode: U+ec19
        /// </summary>
        MynauiSolidGift = 0xEC19,

        /// <summary>
        /// mynaui-solid-git-branch
        /// Unicode: U+ec1a
        /// </summary>
        MynauiSolidGitBranch = 0xEC1A,

        /// <summary>
        /// mynaui-solid-git-circle
        /// Unicode: U+ec1b
        /// </summary>
        MynauiSolidGitCircle = 0xEC1B,

        /// <summary>
        /// mynaui-solid-git-commit
        /// Unicode: U+ec1c
        /// </summary>
        MynauiSolidGitCommit = 0xEC1C,

        /// <summary>
        /// mynaui-solid-git-diamond
        /// Unicode: U+ec1d
        /// </summary>
        MynauiSolidGitDiamond = 0xEC1D,

        /// <summary>
        /// mynaui-solid-git-diff
        /// Unicode: U+ec1e
        /// </summary>
        MynauiSolidGitDiff = 0xEC1E,

        /// <summary>
        /// mynaui-solid-git-hexagon
        /// Unicode: U+ec1f
        /// </summary>
        MynauiSolidGitHexagon = 0xEC1F,

        /// <summary>
        /// mynaui-solid-git-merge
        /// Unicode: U+ec20
        /// </summary>
        MynauiSolidGitMerge = 0xEC20,

        /// <summary>
        /// mynaui-solid-git-octagon
        /// Unicode: U+ec21
        /// </summary>
        MynauiSolidGitOctagon = 0xEC21,

        /// <summary>
        /// mynaui-solid-git-pull-request
        /// Unicode: U+ec22
        /// </summary>
        MynauiSolidGitPullRequest = 0xEC22,

        /// <summary>
        /// mynaui-solid-git-square
        /// Unicode: U+ec23
        /// </summary>
        MynauiSolidGitSquare = 0xEC23,

        /// <summary>
        /// mynaui-solid-git-waves
        /// Unicode: U+ec24
        /// </summary>
        MynauiSolidGitWaves = 0xEC24,

        /// <summary>
        /// mynaui-solid-glasses
        /// Unicode: U+ec25
        /// </summary>
        MynauiSolidGlasses = 0xEC25,

        /// <summary>
        /// mynaui-solid-globe
        /// Unicode: U+ec26
        /// </summary>
        MynauiSolidGlobe = 0xEC26,

        /// <summary>
        /// mynaui-solid-grid
        /// Unicode: U+ec28
        /// </summary>
        MynauiSolidGrid = 0xEC28,

        /// <summary>
        /// mynaui-solid-grid-one
        /// Unicode: U+ec27
        /// </summary>
        MynauiSolidGridOne = 0xEC27,

        /// <summary>
        /// mynaui-solid-hand
        /// Unicode: U+ec29
        /// </summary>
        MynauiSolidHand = 0xEC29,

        /// <summary>
        /// mynaui-solid-hard-drive
        /// Unicode: U+ec2a
        /// </summary>
        MynauiSolidHardDrive = 0xEC2A,

        /// <summary>
        /// mynaui-solid-hash
        /// Unicode: U+ec31
        /// </summary>
        MynauiSolidHash = 0xEC31,

        /// <summary>
        /// mynaui-solid-hash-circle
        /// Unicode: U+ec2b
        /// </summary>
        MynauiSolidHashCircle = 0xEC2B,

        /// <summary>
        /// mynaui-solid-hash-diamond
        /// Unicode: U+ec2c
        /// </summary>
        MynauiSolidHashDiamond = 0xEC2C,

        /// <summary>
        /// mynaui-solid-hash-hexagon
        /// Unicode: U+ec2d
        /// </summary>
        MynauiSolidHashHexagon = 0xEC2D,

        /// <summary>
        /// mynaui-solid-hash-octagon
        /// Unicode: U+ec2e
        /// </summary>
        MynauiSolidHashOctagon = 0xEC2E,

        /// <summary>
        /// mynaui-solid-hash-square
        /// Unicode: U+ec2f
        /// </summary>
        MynauiSolidHashSquare = 0xEC2F,

        /// <summary>
        /// mynaui-solid-hash-waves
        /// Unicode: U+ec30
        /// </summary>
        MynauiSolidHashWaves = 0xEC30,

        /// <summary>
        /// mynaui-solid-haze
        /// Unicode: U+ec32
        /// </summary>
        MynauiSolidHaze = 0xEC32,

        /// <summary>
        /// mynaui-solid-heading
        /// Unicode: U+ec39
        /// </summary>
        MynauiSolidHeading = 0xEC39,

        /// <summary>
        /// mynaui-solid-heading-five
        /// Unicode: U+ec33
        /// </summary>
        MynauiSolidHeadingFive = 0xEC33,

        /// <summary>
        /// mynaui-solid-heading-four
        /// Unicode: U+ec34
        /// </summary>
        MynauiSolidHeadingFour = 0xEC34,

        /// <summary>
        /// mynaui-solid-heading-one
        /// Unicode: U+ec35
        /// </summary>
        MynauiSolidHeadingOne = 0xEC35,

        /// <summary>
        /// mynaui-solid-heading-six
        /// Unicode: U+ec36
        /// </summary>
        MynauiSolidHeadingSix = 0xEC36,

        /// <summary>
        /// mynaui-solid-heading-three
        /// Unicode: U+ec37
        /// </summary>
        MynauiSolidHeadingThree = 0xEC37,

        /// <summary>
        /// mynaui-solid-heading-two
        /// Unicode: U+ec38
        /// </summary>
        MynauiSolidHeadingTwo = 0xEC38,

        /// <summary>
        /// mynaui-solid-headphones
        /// Unicode: U+ec3a
        /// </summary>
        MynauiSolidHeadphones = 0xEC3A,

        /// <summary>
        /// mynaui-solid-heart
        /// Unicode: U+ec4b
        /// </summary>
        MynauiSolidHeart = 0xEC4B,

        /// <summary>
        /// mynaui-solid-heart-broken
        /// Unicode: U+ec3b
        /// </summary>
        MynauiSolidHeartBroken = 0xEC3B,

        /// <summary>
        /// mynaui-solid-heart-check
        /// Unicode: U+ec3c
        /// </summary>
        MynauiSolidHeartCheck = 0xEC3C,

        /// <summary>
        /// mynaui-solid-heart-circle
        /// Unicode: U+ec3d
        /// </summary>
        MynauiSolidHeartCircle = 0xEC3D,

        /// <summary>
        /// mynaui-solid-heart-diamond
        /// Unicode: U+ec3e
        /// </summary>
        MynauiSolidHeartDiamond = 0xEC3E,

        /// <summary>
        /// mynaui-solid-heart-dot
        /// Unicode: U+ec3f
        /// </summary>
        MynauiSolidHeartDot = 0xEC3F,

        /// <summary>
        /// mynaui-solid-heart-hexagon
        /// Unicode: U+ec40
        /// </summary>
        MynauiSolidHeartHexagon = 0xEC40,

        /// <summary>
        /// mynaui-solid-heart-home
        /// Unicode: U+ec41
        /// </summary>
        MynauiSolidHeartHome = 0xEC41,

        /// <summary>
        /// mynaui-solid-heart-minus
        /// Unicode: U+ec42
        /// </summary>
        MynauiSolidHeartMinus = 0xEC42,

        /// <summary>
        /// mynaui-solid-heart-octagon
        /// Unicode: U+ec43
        /// </summary>
        MynauiSolidHeartOctagon = 0xEC43,

        /// <summary>
        /// mynaui-solid-heart-plus
        /// Unicode: U+ec44
        /// </summary>
        MynauiSolidHeartPlus = 0xEC44,

        /// <summary>
        /// mynaui-solid-heart-slash
        /// Unicode: U+ec45
        /// </summary>
        MynauiSolidHeartSlash = 0xEC45,

        /// <summary>
        /// mynaui-solid-heart-snooze
        /// Unicode: U+ec46
        /// </summary>
        MynauiSolidHeartSnooze = 0xEC46,

        /// <summary>
        /// mynaui-solid-heart-square
        /// Unicode: U+ec47
        /// </summary>
        MynauiSolidHeartSquare = 0xEC47,

        /// <summary>
        /// mynaui-solid-heart-user
        /// Unicode: U+ec48
        /// </summary>
        MynauiSolidHeartUser = 0xEC48,

        /// <summary>
        /// mynaui-solid-heart-waves
        /// Unicode: U+ec49
        /// </summary>
        MynauiSolidHeartWaves = 0xEC49,

        /// <summary>
        /// mynaui-solid-heart-x
        /// Unicode: U+ec4a
        /// </summary>
        MynauiSolidHeartX = 0xEC4A,

        /// <summary>
        /// mynaui-solid-hexagon
        /// Unicode: U+ec4c
        /// </summary>
        MynauiSolidHexagon = 0xEC4C,

        /// <summary>
        /// mynaui-solid-home
        /// Unicode: U+ec52
        /// </summary>
        MynauiSolidHome = 0xEC52,

        /// <summary>
        /// mynaui-solid-home-check
        /// Unicode: U+ec4d
        /// </summary>
        MynauiSolidHomeCheck = 0xEC4D,

        /// <summary>
        /// mynaui-solid-home-minus
        /// Unicode: U+ec4e
        /// </summary>
        MynauiSolidHomeMinus = 0xEC4E,

        /// <summary>
        /// mynaui-solid-home-plus
        /// Unicode: U+ec4f
        /// </summary>
        MynauiSolidHomePlus = 0xEC4F,

        /// <summary>
        /// mynaui-solid-home-smile
        /// Unicode: U+ec50
        /// </summary>
        MynauiSolidHomeSmile = 0xEC50,

        /// <summary>
        /// mynaui-solid-home-x
        /// Unicode: U+ec51
        /// </summary>
        MynauiSolidHomeX = 0xEC51,

        /// <summary>
        /// mynaui-solid-image
        /// Unicode: U+ec55
        /// </summary>
        MynauiSolidImage = 0xEC55,

        /// <summary>
        /// mynaui-solid-image-circle
        /// Unicode: U+ec53
        /// </summary>
        MynauiSolidImageCircle = 0xEC53,

        /// <summary>
        /// mynaui-solid-image-rectangle
        /// Unicode: U+ec54
        /// </summary>
        MynauiSolidImageRectangle = 0xEC54,

        /// <summary>
        /// mynaui-solid-inbox
        /// Unicode: U+ec5d
        /// </summary>
        MynauiSolidInbox = 0xEC5D,

        /// <summary>
        /// mynaui-solid-inbox-archive
        /// Unicode: U+ec56
        /// </summary>
        MynauiSolidInboxArchive = 0xEC56,

        /// <summary>
        /// mynaui-solid-inbox-check
        /// Unicode: U+ec57
        /// </summary>
        MynauiSolidInboxCheck = 0xEC57,

        /// <summary>
        /// mynaui-solid-inbox-down
        /// Unicode: U+ec58
        /// </summary>
        MynauiSolidInboxDown = 0xEC58,

        /// <summary>
        /// mynaui-solid-inbox-minus
        /// Unicode: U+ec59
        /// </summary>
        MynauiSolidInboxMinus = 0xEC59,

        /// <summary>
        /// mynaui-solid-inbox-plus
        /// Unicode: U+ec5a
        /// </summary>
        MynauiSolidInboxPlus = 0xEC5A,

        /// <summary>
        /// mynaui-solid-inbox-up
        /// Unicode: U+ec5b
        /// </summary>
        MynauiSolidInboxUp = 0xEC5B,

        /// <summary>
        /// mynaui-solid-inbox-x
        /// Unicode: U+ec5c
        /// </summary>
        MynauiSolidInboxX = 0xEC5C,

        /// <summary>
        /// mynaui-solid-incognito
        /// Unicode: U+ec5e
        /// </summary>
        MynauiSolidIncognito = 0xEC5E,

        /// <summary>
        /// mynaui-solid-indifferent-circle
        /// Unicode: U+ec5f
        /// </summary>
        MynauiSolidIndifferentCircle = 0xEC5F,

        /// <summary>
        /// mynaui-solid-indifferent-ghost
        /// Unicode: U+ec60
        /// </summary>
        MynauiSolidIndifferentGhost = 0xEC60,

        /// <summary>
        /// mynaui-solid-indifferent-square
        /// Unicode: U+ec61
        /// </summary>
        MynauiSolidIndifferentSquare = 0xEC61,

        /// <summary>
        /// mynaui-solid-infinity
        /// Unicode: U+ec62
        /// </summary>
        MynauiSolidInfinity = 0xEC62,

        /// <summary>
        /// mynaui-solid-info
        /// Unicode: U+ec6a
        /// </summary>
        MynauiSolidInfo = 0xEC6A,

        /// <summary>
        /// mynaui-solid-info-circle
        /// Unicode: U+ec63
        /// </summary>
        MynauiSolidInfoCircle = 0xEC63,

        /// <summary>
        /// mynaui-solid-info-diamond
        /// Unicode: U+ec64
        /// </summary>
        MynauiSolidInfoDiamond = 0xEC64,

        /// <summary>
        /// mynaui-solid-info-hexagon
        /// Unicode: U+ec65
        /// </summary>
        MynauiSolidInfoHexagon = 0xEC65,

        /// <summary>
        /// mynaui-solid-info-octagon
        /// Unicode: U+ec66
        /// </summary>
        MynauiSolidInfoOctagon = 0xEC66,

        /// <summary>
        /// mynaui-solid-info-square
        /// Unicode: U+ec67
        /// </summary>
        MynauiSolidInfoSquare = 0xEC67,

        /// <summary>
        /// mynaui-solid-info-triangle
        /// Unicode: U+ec68
        /// </summary>
        MynauiSolidInfoTriangle = 0xEC68,

        /// <summary>
        /// mynaui-solid-info-waves
        /// Unicode: U+ec69
        /// </summary>
        MynauiSolidInfoWaves = 0xEC69,

        /// <summary>
        /// mynaui-solid-intersect
        /// Unicode: U+ec6b
        /// </summary>
        MynauiSolidIntersect = 0xEC6B,

        /// <summary>
        /// mynaui-solid-kanban
        /// Unicode: U+ec6c
        /// </summary>
        MynauiSolidKanban = 0xEC6C,

        /// <summary>
        /// mynaui-solid-key
        /// Unicode: U+ec6d
        /// </summary>
        MynauiSolidKey = 0xEC6D,

        /// <summary>
        /// mynaui-solid-keyboard
        /// Unicode: U+ec70
        /// </summary>
        MynauiSolidKeyboard = 0xEC70,

        /// <summary>
        /// mynaui-solid-keyboard-brightness-high
        /// Unicode: U+ec6e
        /// </summary>
        MynauiSolidKeyboardBrightnessHigh = 0xEC6E,

        /// <summary>
        /// mynaui-solid-keyboard-brightness-low
        /// Unicode: U+ec6f
        /// </summary>
        MynauiSolidKeyboardBrightnessLow = 0xEC6F,

        /// <summary>
        /// mynaui-solid-label
        /// Unicode: U+ec71
        /// </summary>
        MynauiSolidLabel = 0xEC71,

        /// <summary>
        /// mynaui-solid-lamp
        /// Unicode: U+ec72
        /// </summary>
        MynauiSolidLamp = 0xEC72,

        /// <summary>
        /// mynaui-solid-layers-one
        /// Unicode: U+ec73
        /// </summary>
        MynauiSolidLayersOne = 0xEC73,

        /// <summary>
        /// mynaui-solid-layers-three
        /// Unicode: U+ec74
        /// </summary>
        MynauiSolidLayersThree = 0xEC74,

        /// <summary>
        /// mynaui-solid-layers-two
        /// Unicode: U+ec75
        /// </summary>
        MynauiSolidLayersTwo = 0xEC75,

        /// <summary>
        /// mynaui-solid-layout
        /// Unicode: U+ec76
        /// </summary>
        MynauiSolidLayout = 0xEC76,

        /// <summary>
        /// mynaui-solid-leaf
        /// Unicode: U+ec77
        /// </summary>
        MynauiSolidLeaf = 0xEC77,

        /// <summary>
        /// mynaui-solid-leaves
        /// Unicode: U+ec78
        /// </summary>
        MynauiSolidLeaves = 0xEC78,

        /// <summary>
        /// mynaui-solid-letter-a
        /// Unicode: U+ec7f
        /// </summary>
        MynauiSolidLetterA = 0xEC7F,

        /// <summary>
        /// mynaui-solid-letter-a-circle
        /// Unicode: U+ec79
        /// </summary>
        MynauiSolidLetterACircle = 0xEC79,

        /// <summary>
        /// mynaui-solid-letter-a-diamond
        /// Unicode: U+ec7a
        /// </summary>
        MynauiSolidLetterADiamond = 0xEC7A,

        /// <summary>
        /// mynaui-solid-letter-a-hexagon
        /// Unicode: U+ec7b
        /// </summary>
        MynauiSolidLetterAHexagon = 0xEC7B,

        /// <summary>
        /// mynaui-solid-letter-a-octagon
        /// Unicode: U+ec7c
        /// </summary>
        MynauiSolidLetterAOctagon = 0xEC7C,

        /// <summary>
        /// mynaui-solid-letter-a-square
        /// Unicode: U+ec7d
        /// </summary>
        MynauiSolidLetterASquare = 0xEC7D,

        /// <summary>
        /// mynaui-solid-letter-a-waves
        /// Unicode: U+ec7e
        /// </summary>
        MynauiSolidLetterAWaves = 0xEC7E,

        /// <summary>
        /// mynaui-solid-letter-b
        /// Unicode: U+ec86
        /// </summary>
        MynauiSolidLetterB = 0xEC86,

        /// <summary>
        /// mynaui-solid-letter-b-circle
        /// Unicode: U+ec80
        /// </summary>
        MynauiSolidLetterBCircle = 0xEC80,

        /// <summary>
        /// mynaui-solid-letter-b-diamond
        /// Unicode: U+ec81
        /// </summary>
        MynauiSolidLetterBDiamond = 0xEC81,

        /// <summary>
        /// mynaui-solid-letter-b-hexagon
        /// Unicode: U+ec82
        /// </summary>
        MynauiSolidLetterBHexagon = 0xEC82,

        /// <summary>
        /// mynaui-solid-letter-b-octagon
        /// Unicode: U+ec83
        /// </summary>
        MynauiSolidLetterBOctagon = 0xEC83,

        /// <summary>
        /// mynaui-solid-letter-b-square
        /// Unicode: U+ec84
        /// </summary>
        MynauiSolidLetterBSquare = 0xEC84,

        /// <summary>
        /// mynaui-solid-letter-b-waves
        /// Unicode: U+ec85
        /// </summary>
        MynauiSolidLetterBWaves = 0xEC85,

        /// <summary>
        /// mynaui-solid-letter-c
        /// Unicode: U+ec8d
        /// </summary>
        MynauiSolidLetterC = 0xEC8D,

        /// <summary>
        /// mynaui-solid-letter-c-circle
        /// Unicode: U+ec87
        /// </summary>
        MynauiSolidLetterCCircle = 0xEC87,

        /// <summary>
        /// mynaui-solid-letter-c-diamond
        /// Unicode: U+ec88
        /// </summary>
        MynauiSolidLetterCDiamond = 0xEC88,

        /// <summary>
        /// mynaui-solid-letter-c-hexagon
        /// Unicode: U+ec89
        /// </summary>
        MynauiSolidLetterCHexagon = 0xEC89,

        /// <summary>
        /// mynaui-solid-letter-c-octagon
        /// Unicode: U+ec8a
        /// </summary>
        MynauiSolidLetterCOctagon = 0xEC8A,

        /// <summary>
        /// mynaui-solid-letter-c-square
        /// Unicode: U+ec8b
        /// </summary>
        MynauiSolidLetterCSquare = 0xEC8B,

        /// <summary>
        /// mynaui-solid-letter-c-waves
        /// Unicode: U+ec8c
        /// </summary>
        MynauiSolidLetterCWaves = 0xEC8C,

        /// <summary>
        /// mynaui-solid-letter-d
        /// Unicode: U+ec94
        /// </summary>
        MynauiSolidLetterD = 0xEC94,

        /// <summary>
        /// mynaui-solid-letter-d-circle
        /// Unicode: U+ec8e
        /// </summary>
        MynauiSolidLetterDCircle = 0xEC8E,

        /// <summary>
        /// mynaui-solid-letter-d-diamond
        /// Unicode: U+ec8f
        /// </summary>
        MynauiSolidLetterDDiamond = 0xEC8F,

        /// <summary>
        /// mynaui-solid-letter-d-hexagon
        /// Unicode: U+ec90
        /// </summary>
        MynauiSolidLetterDHexagon = 0xEC90,

        /// <summary>
        /// mynaui-solid-letter-d-octagon
        /// Unicode: U+ec91
        /// </summary>
        MynauiSolidLetterDOctagon = 0xEC91,

        /// <summary>
        /// mynaui-solid-letter-d-square
        /// Unicode: U+ec92
        /// </summary>
        MynauiSolidLetterDSquare = 0xEC92,

        /// <summary>
        /// mynaui-solid-letter-d-waves
        /// Unicode: U+ec93
        /// </summary>
        MynauiSolidLetterDWaves = 0xEC93,

        /// <summary>
        /// mynaui-solid-letter-e
        /// Unicode: U+ec9b
        /// </summary>
        MynauiSolidLetterE = 0xEC9B,

        /// <summary>
        /// mynaui-solid-letter-e-circle
        /// Unicode: U+ec95
        /// </summary>
        MynauiSolidLetterECircle = 0xEC95,

        /// <summary>
        /// mynaui-solid-letter-e-diamond
        /// Unicode: U+ec96
        /// </summary>
        MynauiSolidLetterEDiamond = 0xEC96,

        /// <summary>
        /// mynaui-solid-letter-e-hexagon
        /// Unicode: U+ec97
        /// </summary>
        MynauiSolidLetterEHexagon = 0xEC97,

        /// <summary>
        /// mynaui-solid-letter-e-octagon
        /// Unicode: U+ec98
        /// </summary>
        MynauiSolidLetterEOctagon = 0xEC98,

        /// <summary>
        /// mynaui-solid-letter-e-square
        /// Unicode: U+ec99
        /// </summary>
        MynauiSolidLetterESquare = 0xEC99,

        /// <summary>
        /// mynaui-solid-letter-e-waves
        /// Unicode: U+ec9a
        /// </summary>
        MynauiSolidLetterEWaves = 0xEC9A,

        /// <summary>
        /// mynaui-solid-letter-f
        /// Unicode: U+eca2
        /// </summary>
        MynauiSolidLetterF = 0xECA2,

        /// <summary>
        /// mynaui-solid-letter-f-circle
        /// Unicode: U+ec9c
        /// </summary>
        MynauiSolidLetterFCircle = 0xEC9C,

        /// <summary>
        /// mynaui-solid-letter-f-diamond
        /// Unicode: U+ec9d
        /// </summary>
        MynauiSolidLetterFDiamond = 0xEC9D,

        /// <summary>
        /// mynaui-solid-letter-f-hexagon
        /// Unicode: U+ec9e
        /// </summary>
        MynauiSolidLetterFHexagon = 0xEC9E,

        /// <summary>
        /// mynaui-solid-letter-f-octagon
        /// Unicode: U+ec9f
        /// </summary>
        MynauiSolidLetterFOctagon = 0xEC9F,

        /// <summary>
        /// mynaui-solid-letter-f-square
        /// Unicode: U+eca0
        /// </summary>
        MynauiSolidLetterFSquare = 0xECA0,

        /// <summary>
        /// mynaui-solid-letter-f-waves
        /// Unicode: U+eca1
        /// </summary>
        MynauiSolidLetterFWaves = 0xECA1,

        /// <summary>
        /// mynaui-solid-letter-g
        /// Unicode: U+eca9
        /// </summary>
        MynauiSolidLetterG = 0xECA9,

        /// <summary>
        /// mynaui-solid-letter-g-circle
        /// Unicode: U+eca3
        /// </summary>
        MynauiSolidLetterGCircle = 0xECA3,

        /// <summary>
        /// mynaui-solid-letter-g-diamond
        /// Unicode: U+eca4
        /// </summary>
        MynauiSolidLetterGDiamond = 0xECA4,

        /// <summary>
        /// mynaui-solid-letter-g-hexagon
        /// Unicode: U+eca5
        /// </summary>
        MynauiSolidLetterGHexagon = 0xECA5,

        /// <summary>
        /// mynaui-solid-letter-g-octagon
        /// Unicode: U+eca6
        /// </summary>
        MynauiSolidLetterGOctagon = 0xECA6,

        /// <summary>
        /// mynaui-solid-letter-g-square
        /// Unicode: U+eca7
        /// </summary>
        MynauiSolidLetterGSquare = 0xECA7,

        /// <summary>
        /// mynaui-solid-letter-g-waves
        /// Unicode: U+eca8
        /// </summary>
        MynauiSolidLetterGWaves = 0xECA8,

        /// <summary>
        /// mynaui-solid-letter-h
        /// Unicode: U+ecb0
        /// </summary>
        MynauiSolidLetterH = 0xECB0,

        /// <summary>
        /// mynaui-solid-letter-h-circle
        /// Unicode: U+ecaa
        /// </summary>
        MynauiSolidLetterHCircle = 0xECAA,

        /// <summary>
        /// mynaui-solid-letter-h-diamond
        /// Unicode: U+ecab
        /// </summary>
        MynauiSolidLetterHDiamond = 0xECAB,

        /// <summary>
        /// mynaui-solid-letter-h-hexagon
        /// Unicode: U+ecac
        /// </summary>
        MynauiSolidLetterHHexagon = 0xECAC,

        /// <summary>
        /// mynaui-solid-letter-h-octagon
        /// Unicode: U+ecad
        /// </summary>
        MynauiSolidLetterHOctagon = 0xECAD,

        /// <summary>
        /// mynaui-solid-letter-h-square
        /// Unicode: U+ecae
        /// </summary>
        MynauiSolidLetterHSquare = 0xECAE,

        /// <summary>
        /// mynaui-solid-letter-h-waves
        /// Unicode: U+ecaf
        /// </summary>
        MynauiSolidLetterHWaves = 0xECAF,

        /// <summary>
        /// mynaui-solid-letter-i
        /// Unicode: U+ecb7
        /// </summary>
        MynauiSolidLetterI = 0xECB7,

        /// <summary>
        /// mynaui-solid-letter-i-circle
        /// Unicode: U+ecb1
        /// </summary>
        MynauiSolidLetterICircle = 0xECB1,

        /// <summary>
        /// mynaui-solid-letter-i-diamond
        /// Unicode: U+ecb2
        /// </summary>
        MynauiSolidLetterIDiamond = 0xECB2,

        /// <summary>
        /// mynaui-solid-letter-i-hexagon
        /// Unicode: U+ecb3
        /// </summary>
        MynauiSolidLetterIHexagon = 0xECB3,

        /// <summary>
        /// mynaui-solid-letter-i-octagon
        /// Unicode: U+ecb4
        /// </summary>
        MynauiSolidLetterIOctagon = 0xECB4,

        /// <summary>
        /// mynaui-solid-letter-i-square
        /// Unicode: U+ecb5
        /// </summary>
        MynauiSolidLetterISquare = 0xECB5,

        /// <summary>
        /// mynaui-solid-letter-i-waves
        /// Unicode: U+ecb6
        /// </summary>
        MynauiSolidLetterIWaves = 0xECB6,

        /// <summary>
        /// mynaui-solid-letter-j
        /// Unicode: U+ecbe
        /// </summary>
        MynauiSolidLetterJ = 0xECBE,

        /// <summary>
        /// mynaui-solid-letter-j-circle
        /// Unicode: U+ecb8
        /// </summary>
        MynauiSolidLetterJCircle = 0xECB8,

        /// <summary>
        /// mynaui-solid-letter-j-diamond
        /// Unicode: U+ecb9
        /// </summary>
        MynauiSolidLetterJDiamond = 0xECB9,

        /// <summary>
        /// mynaui-solid-letter-j-hexagon
        /// Unicode: U+ecba
        /// </summary>
        MynauiSolidLetterJHexagon = 0xECBA,

        /// <summary>
        /// mynaui-solid-letter-j-octagon
        /// Unicode: U+ecbb
        /// </summary>
        MynauiSolidLetterJOctagon = 0xECBB,

        /// <summary>
        /// mynaui-solid-letter-j-square
        /// Unicode: U+ecbc
        /// </summary>
        MynauiSolidLetterJSquare = 0xECBC,

        /// <summary>
        /// mynaui-solid-letter-j-waves
        /// Unicode: U+ecbd
        /// </summary>
        MynauiSolidLetterJWaves = 0xECBD,

        /// <summary>
        /// mynaui-solid-letter-k
        /// Unicode: U+ecc5
        /// </summary>
        MynauiSolidLetterK = 0xECC5,

        /// <summary>
        /// mynaui-solid-letter-k-circle
        /// Unicode: U+ecbf
        /// </summary>
        MynauiSolidLetterKCircle = 0xECBF,

        /// <summary>
        /// mynaui-solid-letter-k-diamond
        /// Unicode: U+ecc0
        /// </summary>
        MynauiSolidLetterKDiamond = 0xECC0,

        /// <summary>
        /// mynaui-solid-letter-k-hexagon
        /// Unicode: U+ecc1
        /// </summary>
        MynauiSolidLetterKHexagon = 0xECC1,

        /// <summary>
        /// mynaui-solid-letter-k-octagon
        /// Unicode: U+ecc2
        /// </summary>
        MynauiSolidLetterKOctagon = 0xECC2,

        /// <summary>
        /// mynaui-solid-letter-k-square
        /// Unicode: U+ecc3
        /// </summary>
        MynauiSolidLetterKSquare = 0xECC3,

        /// <summary>
        /// mynaui-solid-letter-k-waves
        /// Unicode: U+ecc4
        /// </summary>
        MynauiSolidLetterKWaves = 0xECC4,

        /// <summary>
        /// mynaui-solid-letter-l
        /// Unicode: U+eccc
        /// </summary>
        MynauiSolidLetterL = 0xECCC,

        /// <summary>
        /// mynaui-solid-letter-l-circle
        /// Unicode: U+ecc6
        /// </summary>
        MynauiSolidLetterLCircle = 0xECC6,

        /// <summary>
        /// mynaui-solid-letter-l-diamond
        /// Unicode: U+ecc7
        /// </summary>
        MynauiSolidLetterLDiamond = 0xECC7,

        /// <summary>
        /// mynaui-solid-letter-l-hexagon
        /// Unicode: U+ecc8
        /// </summary>
        MynauiSolidLetterLHexagon = 0xECC8,

        /// <summary>
        /// mynaui-solid-letter-l-octagon
        /// Unicode: U+ecc9
        /// </summary>
        MynauiSolidLetterLOctagon = 0xECC9,

        /// <summary>
        /// mynaui-solid-letter-l-square
        /// Unicode: U+ecca
        /// </summary>
        MynauiSolidLetterLSquare = 0xECCA,

        /// <summary>
        /// mynaui-solid-letter-l-waves
        /// Unicode: U+eccb
        /// </summary>
        MynauiSolidLetterLWaves = 0xECCB,

        /// <summary>
        /// mynaui-solid-letter-m
        /// Unicode: U+ecd3
        /// </summary>
        MynauiSolidLetterM = 0xECD3,

        /// <summary>
        /// mynaui-solid-letter-m-circle
        /// Unicode: U+eccd
        /// </summary>
        MynauiSolidLetterMCircle = 0xECCD,

        /// <summary>
        /// mynaui-solid-letter-m-diamond
        /// Unicode: U+ecce
        /// </summary>
        MynauiSolidLetterMDiamond = 0xECCE,

        /// <summary>
        /// mynaui-solid-letter-m-hexagon
        /// Unicode: U+eccf
        /// </summary>
        MynauiSolidLetterMHexagon = 0xECCF,

        /// <summary>
        /// mynaui-solid-letter-m-octagon
        /// Unicode: U+ecd0
        /// </summary>
        MynauiSolidLetterMOctagon = 0xECD0,

        /// <summary>
        /// mynaui-solid-letter-m-square
        /// Unicode: U+ecd1
        /// </summary>
        MynauiSolidLetterMSquare = 0xECD1,

        /// <summary>
        /// mynaui-solid-letter-m-waves
        /// Unicode: U+ecd2
        /// </summary>
        MynauiSolidLetterMWaves = 0xECD2,

        /// <summary>
        /// mynaui-solid-letter-n
        /// Unicode: U+ecda
        /// </summary>
        MynauiSolidLetterN = 0xECDA,

        /// <summary>
        /// mynaui-solid-letter-n-circle
        /// Unicode: U+ecd4
        /// </summary>
        MynauiSolidLetterNCircle = 0xECD4,

        /// <summary>
        /// mynaui-solid-letter-n-diamond
        /// Unicode: U+ecd5
        /// </summary>
        MynauiSolidLetterNDiamond = 0xECD5,

        /// <summary>
        /// mynaui-solid-letter-n-hexagon
        /// Unicode: U+ecd6
        /// </summary>
        MynauiSolidLetterNHexagon = 0xECD6,

        /// <summary>
        /// mynaui-solid-letter-n-octagon
        /// Unicode: U+ecd7
        /// </summary>
        MynauiSolidLetterNOctagon = 0xECD7,

        /// <summary>
        /// mynaui-solid-letter-n-square
        /// Unicode: U+ecd8
        /// </summary>
        MynauiSolidLetterNSquare = 0xECD8,

        /// <summary>
        /// mynaui-solid-letter-n-waves
        /// Unicode: U+ecd9
        /// </summary>
        MynauiSolidLetterNWaves = 0xECD9,

        /// <summary>
        /// mynaui-solid-letter-o
        /// Unicode: U+ece1
        /// </summary>
        MynauiSolidLetterO = 0xECE1,

        /// <summary>
        /// mynaui-solid-letter-o-circle
        /// Unicode: U+ecdb
        /// </summary>
        MynauiSolidLetterOCircle = 0xECDB,

        /// <summary>
        /// mynaui-solid-letter-o-diamond
        /// Unicode: U+ecdc
        /// </summary>
        MynauiSolidLetterODiamond = 0xECDC,

        /// <summary>
        /// mynaui-solid-letter-o-hexagon
        /// Unicode: U+ecdd
        /// </summary>
        MynauiSolidLetterOHexagon = 0xECDD,

        /// <summary>
        /// mynaui-solid-letter-o-octagon
        /// Unicode: U+ecde
        /// </summary>
        MynauiSolidLetterOOctagon = 0xECDE,

        /// <summary>
        /// mynaui-solid-letter-o-square
        /// Unicode: U+ecdf
        /// </summary>
        MynauiSolidLetterOSquare = 0xECDF,

        /// <summary>
        /// mynaui-solid-letter-o-waves
        /// Unicode: U+ece0
        /// </summary>
        MynauiSolidLetterOWaves = 0xECE0,

        /// <summary>
        /// mynaui-solid-letter-p
        /// Unicode: U+ece8
        /// </summary>
        MynauiSolidLetterP = 0xECE8,

        /// <summary>
        /// mynaui-solid-letter-p-circle
        /// Unicode: U+ece2
        /// </summary>
        MynauiSolidLetterPCircle = 0xECE2,

        /// <summary>
        /// mynaui-solid-letter-p-diamond
        /// Unicode: U+ece3
        /// </summary>
        MynauiSolidLetterPDiamond = 0xECE3,

        /// <summary>
        /// mynaui-solid-letter-p-hexagon
        /// Unicode: U+ece4
        /// </summary>
        MynauiSolidLetterPHexagon = 0xECE4,

        /// <summary>
        /// mynaui-solid-letter-p-octagon
        /// Unicode: U+ece5
        /// </summary>
        MynauiSolidLetterPOctagon = 0xECE5,

        /// <summary>
        /// mynaui-solid-letter-p-square
        /// Unicode: U+ece6
        /// </summary>
        MynauiSolidLetterPSquare = 0xECE6,

        /// <summary>
        /// mynaui-solid-letter-p-waves
        /// Unicode: U+ece7
        /// </summary>
        MynauiSolidLetterPWaves = 0xECE7,

        /// <summary>
        /// mynaui-solid-letter-q
        /// Unicode: U+ecef
        /// </summary>
        MynauiSolidLetterQ = 0xECEF,

        /// <summary>
        /// mynaui-solid-letter-q-circle
        /// Unicode: U+ece9
        /// </summary>
        MynauiSolidLetterQCircle = 0xECE9,

        /// <summary>
        /// mynaui-solid-letter-q-diamond
        /// Unicode: U+ecea
        /// </summary>
        MynauiSolidLetterQDiamond = 0xECEA,

        /// <summary>
        /// mynaui-solid-letter-q-hexagon
        /// Unicode: U+eceb
        /// </summary>
        MynauiSolidLetterQHexagon = 0xECEB,

        /// <summary>
        /// mynaui-solid-letter-q-octagon
        /// Unicode: U+ecec
        /// </summary>
        MynauiSolidLetterQOctagon = 0xECEC,

        /// <summary>
        /// mynaui-solid-letter-q-square
        /// Unicode: U+eced
        /// </summary>
        MynauiSolidLetterQSquare = 0xECED,

        /// <summary>
        /// mynaui-solid-letter-q-waves
        /// Unicode: U+ecee
        /// </summary>
        MynauiSolidLetterQWaves = 0xECEE,

        /// <summary>
        /// mynaui-solid-letter-r
        /// Unicode: U+ecf6
        /// </summary>
        MynauiSolidLetterR = 0xECF6,

        /// <summary>
        /// mynaui-solid-letter-r-circle
        /// Unicode: U+ecf0
        /// </summary>
        MynauiSolidLetterRCircle = 0xECF0,

        /// <summary>
        /// mynaui-solid-letter-r-diamond
        /// Unicode: U+ecf1
        /// </summary>
        MynauiSolidLetterRDiamond = 0xECF1,

        /// <summary>
        /// mynaui-solid-letter-r-hexagon
        /// Unicode: U+ecf2
        /// </summary>
        MynauiSolidLetterRHexagon = 0xECF2,

        /// <summary>
        /// mynaui-solid-letter-r-octagon
        /// Unicode: U+ecf3
        /// </summary>
        MynauiSolidLetterROctagon = 0xECF3,

        /// <summary>
        /// mynaui-solid-letter-r-square
        /// Unicode: U+ecf4
        /// </summary>
        MynauiSolidLetterRSquare = 0xECF4,

        /// <summary>
        /// mynaui-solid-letter-r-waves
        /// Unicode: U+ecf5
        /// </summary>
        MynauiSolidLetterRWaves = 0xECF5,

        /// <summary>
        /// mynaui-solid-letter-s
        /// Unicode: U+ecfd
        /// </summary>
        MynauiSolidLetterS = 0xECFD,

        /// <summary>
        /// mynaui-solid-letter-s-circle
        /// Unicode: U+ecf7
        /// </summary>
        MynauiSolidLetterSCircle = 0xECF7,

        /// <summary>
        /// mynaui-solid-letter-s-diamond
        /// Unicode: U+ecf8
        /// </summary>
        MynauiSolidLetterSDiamond = 0xECF8,

        /// <summary>
        /// mynaui-solid-letter-s-hexagon
        /// Unicode: U+ecf9
        /// </summary>
        MynauiSolidLetterSHexagon = 0xECF9,

        /// <summary>
        /// mynaui-solid-letter-s-octagon
        /// Unicode: U+ecfa
        /// </summary>
        MynauiSolidLetterSOctagon = 0xECFA,

        /// <summary>
        /// mynaui-solid-letter-s-square
        /// Unicode: U+ecfb
        /// </summary>
        MynauiSolidLetterSSquare = 0xECFB,

        /// <summary>
        /// mynaui-solid-letter-s-waves
        /// Unicode: U+ecfc
        /// </summary>
        MynauiSolidLetterSWaves = 0xECFC,

        /// <summary>
        /// mynaui-solid-letter-t
        /// Unicode: U+ed04
        /// </summary>
        MynauiSolidLetterT = 0xED04,

        /// <summary>
        /// mynaui-solid-letter-t-circle
        /// Unicode: U+ecfe
        /// </summary>
        MynauiSolidLetterTCircle = 0xECFE,

        /// <summary>
        /// mynaui-solid-letter-t-diamond
        /// Unicode: U+ecff
        /// </summary>
        MynauiSolidLetterTDiamond = 0xECFF,

        /// <summary>
        /// mynaui-solid-letter-t-hexagon
        /// Unicode: U+ed00
        /// </summary>
        MynauiSolidLetterTHexagon = 0xED00,

        /// <summary>
        /// mynaui-solid-letter-t-octagon
        /// Unicode: U+ed01
        /// </summary>
        MynauiSolidLetterTOctagon = 0xED01,

        /// <summary>
        /// mynaui-solid-letter-t-square
        /// Unicode: U+ed02
        /// </summary>
        MynauiSolidLetterTSquare = 0xED02,

        /// <summary>
        /// mynaui-solid-letter-t-waves
        /// Unicode: U+ed03
        /// </summary>
        MynauiSolidLetterTWaves = 0xED03,

        /// <summary>
        /// mynaui-solid-letter-u
        /// Unicode: U+ed0b
        /// </summary>
        MynauiSolidLetterU = 0xED0B,

        /// <summary>
        /// mynaui-solid-letter-u-circle
        /// Unicode: U+ed05
        /// </summary>
        MynauiSolidLetterUCircle = 0xED05,

        /// <summary>
        /// mynaui-solid-letter-u-diamond
        /// Unicode: U+ed06
        /// </summary>
        MynauiSolidLetterUDiamond = 0xED06,

        /// <summary>
        /// mynaui-solid-letter-u-hexagon
        /// Unicode: U+ed07
        /// </summary>
        MynauiSolidLetterUHexagon = 0xED07,

        /// <summary>
        /// mynaui-solid-letter-u-octagon
        /// Unicode: U+ed08
        /// </summary>
        MynauiSolidLetterUOctagon = 0xED08,

        /// <summary>
        /// mynaui-solid-letter-u-square
        /// Unicode: U+ed09
        /// </summary>
        MynauiSolidLetterUSquare = 0xED09,

        /// <summary>
        /// mynaui-solid-letter-u-waves
        /// Unicode: U+ed0a
        /// </summary>
        MynauiSolidLetterUWaves = 0xED0A,

        /// <summary>
        /// mynaui-solid-letter-v
        /// Unicode: U+ed12
        /// </summary>
        MynauiSolidLetterV = 0xED12,

        /// <summary>
        /// mynaui-solid-letter-v-circle
        /// Unicode: U+ed0c
        /// </summary>
        MynauiSolidLetterVCircle = 0xED0C,

        /// <summary>
        /// mynaui-solid-letter-v-diamond
        /// Unicode: U+ed0d
        /// </summary>
        MynauiSolidLetterVDiamond = 0xED0D,

        /// <summary>
        /// mynaui-solid-letter-v-hexagon
        /// Unicode: U+ed0e
        /// </summary>
        MynauiSolidLetterVHexagon = 0xED0E,

        /// <summary>
        /// mynaui-solid-letter-v-octagon
        /// Unicode: U+ed0f
        /// </summary>
        MynauiSolidLetterVOctagon = 0xED0F,

        /// <summary>
        /// mynaui-solid-letter-v-square
        /// Unicode: U+ed10
        /// </summary>
        MynauiSolidLetterVSquare = 0xED10,

        /// <summary>
        /// mynaui-solid-letter-v-waves
        /// Unicode: U+ed11
        /// </summary>
        MynauiSolidLetterVWaves = 0xED11,

        /// <summary>
        /// mynaui-solid-letter-w
        /// Unicode: U+ed19
        /// </summary>
        MynauiSolidLetterW = 0xED19,

        /// <summary>
        /// mynaui-solid-letter-w-circle
        /// Unicode: U+ed13
        /// </summary>
        MynauiSolidLetterWCircle = 0xED13,

        /// <summary>
        /// mynaui-solid-letter-w-diamond
        /// Unicode: U+ed14
        /// </summary>
        MynauiSolidLetterWDiamond = 0xED14,

        /// <summary>
        /// mynaui-solid-letter-w-hexagon
        /// Unicode: U+ed15
        /// </summary>
        MynauiSolidLetterWHexagon = 0xED15,

        /// <summary>
        /// mynaui-solid-letter-w-octagon
        /// Unicode: U+ed16
        /// </summary>
        MynauiSolidLetterWOctagon = 0xED16,

        /// <summary>
        /// mynaui-solid-letter-w-square
        /// Unicode: U+ed17
        /// </summary>
        MynauiSolidLetterWSquare = 0xED17,

        /// <summary>
        /// mynaui-solid-letter-w-waves
        /// Unicode: U+ed18
        /// </summary>
        MynauiSolidLetterWWaves = 0xED18,

        /// <summary>
        /// mynaui-solid-letter-x
        /// Unicode: U+ed20
        /// </summary>
        MynauiSolidLetterX = 0xED20,

        /// <summary>
        /// mynaui-solid-letter-x-circle
        /// Unicode: U+ed1a
        /// </summary>
        MynauiSolidLetterXCircle = 0xED1A,

        /// <summary>
        /// mynaui-solid-letter-x-diamond
        /// Unicode: U+ed1b
        /// </summary>
        MynauiSolidLetterXDiamond = 0xED1B,

        /// <summary>
        /// mynaui-solid-letter-x-hexagon
        /// Unicode: U+ed1c
        /// </summary>
        MynauiSolidLetterXHexagon = 0xED1C,

        /// <summary>
        /// mynaui-solid-letter-x-octagon
        /// Unicode: U+ed1d
        /// </summary>
        MynauiSolidLetterXOctagon = 0xED1D,

        /// <summary>
        /// mynaui-solid-letter-x-square
        /// Unicode: U+ed1e
        /// </summary>
        MynauiSolidLetterXSquare = 0xED1E,

        /// <summary>
        /// mynaui-solid-letter-x-waves
        /// Unicode: U+ed1f
        /// </summary>
        MynauiSolidLetterXWaves = 0xED1F,

        /// <summary>
        /// mynaui-solid-letter-y
        /// Unicode: U+ed27
        /// </summary>
        MynauiSolidLetterY = 0xED27,

        /// <summary>
        /// mynaui-solid-letter-y-circle
        /// Unicode: U+ed21
        /// </summary>
        MynauiSolidLetterYCircle = 0xED21,

        /// <summary>
        /// mynaui-solid-letter-y-diamond
        /// Unicode: U+ed22
        /// </summary>
        MynauiSolidLetterYDiamond = 0xED22,

        /// <summary>
        /// mynaui-solid-letter-y-hexagon
        /// Unicode: U+ed23
        /// </summary>
        MynauiSolidLetterYHexagon = 0xED23,

        /// <summary>
        /// mynaui-solid-letter-y-octagon
        /// Unicode: U+ed24
        /// </summary>
        MynauiSolidLetterYOctagon = 0xED24,

        /// <summary>
        /// mynaui-solid-letter-y-square
        /// Unicode: U+ed25
        /// </summary>
        MynauiSolidLetterYSquare = 0xED25,

        /// <summary>
        /// mynaui-solid-letter-y-waves
        /// Unicode: U+ed26
        /// </summary>
        MynauiSolidLetterYWaves = 0xED26,

        /// <summary>
        /// mynaui-solid-letter-z
        /// Unicode: U+ed2e
        /// </summary>
        MynauiSolidLetterZ = 0xED2E,

        /// <summary>
        /// mynaui-solid-letter-z-circle
        /// Unicode: U+ed28
        /// </summary>
        MynauiSolidLetterZCircle = 0xED28,

        /// <summary>
        /// mynaui-solid-letter-z-diamond
        /// Unicode: U+ed29
        /// </summary>
        MynauiSolidLetterZDiamond = 0xED29,

        /// <summary>
        /// mynaui-solid-letter-z-hexagon
        /// Unicode: U+ed2a
        /// </summary>
        MynauiSolidLetterZHexagon = 0xED2A,

        /// <summary>
        /// mynaui-solid-letter-z-octagon
        /// Unicode: U+ed2b
        /// </summary>
        MynauiSolidLetterZOctagon = 0xED2B,

        /// <summary>
        /// mynaui-solid-letter-z-square
        /// Unicode: U+ed2c
        /// </summary>
        MynauiSolidLetterZSquare = 0xED2C,

        /// <summary>
        /// mynaui-solid-letter-z-waves
        /// Unicode: U+ed2d
        /// </summary>
        MynauiSolidLetterZWaves = 0xED2D,

        /// <summary>
        /// mynaui-solid-lightning
        /// Unicode: U+ed30
        /// </summary>
        MynauiSolidLightning = 0xED30,

        /// <summary>
        /// mynaui-solid-lightning-off
        /// Unicode: U+ed2f
        /// </summary>
        MynauiSolidLightningOff = 0xED2F,

        /// <summary>
        /// mynaui-solid-like
        /// Unicode: U+ed31
        /// </summary>
        MynauiSolidLike = 0xED31,

        /// <summary>
        /// mynaui-solid-line-chart-circle
        /// Unicode: U+ed32
        /// </summary>
        MynauiSolidLineChartCircle = 0xED32,

        /// <summary>
        /// mynaui-solid-line-chart-diamond
        /// Unicode: U+ed33
        /// </summary>
        MynauiSolidLineChartDiamond = 0xED33,

        /// <summary>
        /// mynaui-solid-line-chart-hexagon
        /// Unicode: U+ed34
        /// </summary>
        MynauiSolidLineChartHexagon = 0xED34,

        /// <summary>
        /// mynaui-solid-line-chart-octagon
        /// Unicode: U+ed35
        /// </summary>
        MynauiSolidLineChartOctagon = 0xED35,

        /// <summary>
        /// mynaui-solid-line-chart-square
        /// Unicode: U+ed36
        /// </summary>
        MynauiSolidLineChartSquare = 0xED36,

        /// <summary>
        /// mynaui-solid-line-chart-waves
        /// Unicode: U+ed37
        /// </summary>
        MynauiSolidLineChartWaves = 0xED37,

        /// <summary>
        /// mynaui-solid-link
        /// Unicode: U+ed3a
        /// </summary>
        MynauiSolidLink = 0xED3A,

        /// <summary>
        /// mynaui-solid-link-one
        /// Unicode: U+ed38
        /// </summary>
        MynauiSolidLinkOne = 0xED38,

        /// <summary>
        /// mynaui-solid-link-two
        /// Unicode: U+ed39
        /// </summary>
        MynauiSolidLinkTwo = 0xED39,

        /// <summary>
        /// mynaui-solid-list
        /// Unicode: U+ed3d
        /// </summary>
        MynauiSolidList = 0xED3D,

        /// <summary>
        /// mynaui-solid-list-check
        /// Unicode: U+ed3b
        /// </summary>
        MynauiSolidListCheck = 0xED3B,

        /// <summary>
        /// mynaui-solid-list-number
        /// Unicode: U+ed3c
        /// </summary>
        MynauiSolidListNumber = 0xED3C,

        /// <summary>
        /// mynaui-solid-location
        /// Unicode: U+ed47
        /// </summary>
        MynauiSolidLocation = 0xED47,

        /// <summary>
        /// mynaui-solid-location-check
        /// Unicode: U+ed3e
        /// </summary>
        MynauiSolidLocationCheck = 0xED3E,

        /// <summary>
        /// mynaui-solid-location-home
        /// Unicode: U+ed3f
        /// </summary>
        MynauiSolidLocationHome = 0xED3F,

        /// <summary>
        /// mynaui-solid-location-minus
        /// Unicode: U+ed40
        /// </summary>
        MynauiSolidLocationMinus = 0xED40,

        /// <summary>
        /// mynaui-solid-location-plus
        /// Unicode: U+ed41
        /// </summary>
        MynauiSolidLocationPlus = 0xED41,

        /// <summary>
        /// mynaui-solid-location-selected
        /// Unicode: U+ed42
        /// </summary>
        MynauiSolidLocationSelected = 0xED42,

        /// <summary>
        /// mynaui-solid-location-slash
        /// Unicode: U+ed43
        /// </summary>
        MynauiSolidLocationSlash = 0xED43,

        /// <summary>
        /// mynaui-solid-location-snooze
        /// Unicode: U+ed44
        /// </summary>
        MynauiSolidLocationSnooze = 0xED44,

        /// <summary>
        /// mynaui-solid-location-user
        /// Unicode: U+ed45
        /// </summary>
        MynauiSolidLocationUser = 0xED45,

        /// <summary>
        /// mynaui-solid-location-x
        /// Unicode: U+ed46
        /// </summary>
        MynauiSolidLocationX = 0xED46,

        /// <summary>
        /// mynaui-solid-lock
        /// Unicode: U+ed53
        /// </summary>
        MynauiSolidLock = 0xED53,

        /// <summary>
        /// mynaui-solid-lock-circle
        /// Unicode: U+ed48
        /// </summary>
        MynauiSolidLockCircle = 0xED48,

        /// <summary>
        /// mynaui-solid-lock-diamond
        /// Unicode: U+ed49
        /// </summary>
        MynauiSolidLockDiamond = 0xED49,

        /// <summary>
        /// mynaui-solid-lock-hexagon
        /// Unicode: U+ed4a
        /// </summary>
        MynauiSolidLockHexagon = 0xED4A,

        /// <summary>
        /// mynaui-solid-lock-keyhole
        /// Unicode: U+ed4b
        /// </summary>
        MynauiSolidLockKeyhole = 0xED4B,

        /// <summary>
        /// mynaui-solid-lock-octagon
        /// Unicode: U+ed4c
        /// </summary>
        MynauiSolidLockOctagon = 0xED4C,

        /// <summary>
        /// mynaui-solid-lock-open
        /// Unicode: U+ed4f
        /// </summary>
        MynauiSolidLockOpen = 0xED4F,

        /// <summary>
        /// mynaui-solid-lock-open-keyhole
        /// Unicode: U+ed4d
        /// </summary>
        MynauiSolidLockOpenKeyhole = 0xED4D,

        /// <summary>
        /// mynaui-solid-lock-open-password
        /// Unicode: U+ed4e
        /// </summary>
        MynauiSolidLockOpenPassword = 0xED4E,

        /// <summary>
        /// mynaui-solid-lock-password
        /// Unicode: U+ed50
        /// </summary>
        MynauiSolidLockPassword = 0xED50,

        /// <summary>
        /// mynaui-solid-lock-square
        /// Unicode: U+ed51
        /// </summary>
        MynauiSolidLockSquare = 0xED51,

        /// <summary>
        /// mynaui-solid-lock-waves
        /// Unicode: U+ed52
        /// </summary>
        MynauiSolidLockWaves = 0xED52,

        /// <summary>
        /// mynaui-solid-login
        /// Unicode: U+ed54
        /// </summary>
        MynauiSolidLogin = 0xED54,

        /// <summary>
        /// mynaui-solid-logout
        /// Unicode: U+ed55
        /// </summary>
        MynauiSolidLogout = 0xED55,

        /// <summary>
        /// mynaui-solid-magnet
        /// Unicode: U+ed56
        /// </summary>
        MynauiSolidMagnet = 0xED56,

        /// <summary>
        /// mynaui-solid-male
        /// Unicode: U+ed57
        /// </summary>
        MynauiSolidMale = 0xED57,

        /// <summary>
        /// mynaui-solid-map
        /// Unicode: U+ed58
        /// </summary>
        MynauiSolidMap = 0xED58,

        /// <summary>
        /// mynaui-solid-mask
        /// Unicode: U+ed59
        /// </summary>
        MynauiSolidMask = 0xED59,

        /// <summary>
        /// mynaui-solid-math
        /// Unicode: U+ed5b
        /// </summary>
        MynauiSolidMath = 0xED5B,

        /// <summary>
        /// mynaui-solid-math-square
        /// Unicode: U+ed5a
        /// </summary>
        MynauiSolidMathSquare = 0xED5A,

        /// <summary>
        /// mynaui-solid-maximize
        /// Unicode: U+ed5d
        /// </summary>
        MynauiSolidMaximize = 0xED5D,

        /// <summary>
        /// mynaui-solid-maximize-one
        /// Unicode: U+ed5c
        /// </summary>
        MynauiSolidMaximizeOne = 0xED5C,

        /// <summary>
        /// mynaui-solid-menu
        /// Unicode: U+ed5e
        /// </summary>
        MynauiSolidMenu = 0xED5E,

        /// <summary>
        /// mynaui-solid-message
        /// Unicode: U+ed65
        /// </summary>
        MynauiSolidMessage = 0xED65,

        /// <summary>
        /// mynaui-solid-message-check
        /// Unicode: U+ed5f
        /// </summary>
        MynauiSolidMessageCheck = 0xED5F,

        /// <summary>
        /// mynaui-solid-message-dots
        /// Unicode: U+ed60
        /// </summary>
        MynauiSolidMessageDots = 0xED60,

        /// <summary>
        /// mynaui-solid-message-minus
        /// Unicode: U+ed61
        /// </summary>
        MynauiSolidMessageMinus = 0xED61,

        /// <summary>
        /// mynaui-solid-message-plus
        /// Unicode: U+ed62
        /// </summary>
        MynauiSolidMessagePlus = 0xED62,

        /// <summary>
        /// mynaui-solid-message-reply
        /// Unicode: U+ed63
        /// </summary>
        MynauiSolidMessageReply = 0xED63,

        /// <summary>
        /// mynaui-solid-message-x
        /// Unicode: U+ed64
        /// </summary>
        MynauiSolidMessageX = 0xED64,

        /// <summary>
        /// mynaui-solid-microphone
        /// Unicode: U+ed67
        /// </summary>
        MynauiSolidMicrophone = 0xED67,

        /// <summary>
        /// mynaui-solid-microphone-slash
        /// Unicode: U+ed66
        /// </summary>
        MynauiSolidMicrophoneSlash = 0xED66,

        /// <summary>
        /// mynaui-solid-minimize
        /// Unicode: U+ed69
        /// </summary>
        MynauiSolidMinimize = 0xED69,

        /// <summary>
        /// mynaui-solid-minimize-one
        /// Unicode: U+ed68
        /// </summary>
        MynauiSolidMinimizeOne = 0xED68,

        /// <summary>
        /// mynaui-solid-minus
        /// Unicode: U+ed70
        /// </summary>
        MynauiSolidMinus = 0xED70,

        /// <summary>
        /// mynaui-solid-minus-circle
        /// Unicode: U+ed6a
        /// </summary>
        MynauiSolidMinusCircle = 0xED6A,

        /// <summary>
        /// mynaui-solid-minus-diamond
        /// Unicode: U+ed6b
        /// </summary>
        MynauiSolidMinusDiamond = 0xED6B,

        /// <summary>
        /// mynaui-solid-minus-hexagon
        /// Unicode: U+ed6c
        /// </summary>
        MynauiSolidMinusHexagon = 0xED6C,

        /// <summary>
        /// mynaui-solid-minus-octagon
        /// Unicode: U+ed6d
        /// </summary>
        MynauiSolidMinusOctagon = 0xED6D,

        /// <summary>
        /// mynaui-solid-minus-square
        /// Unicode: U+ed6e
        /// </summary>
        MynauiSolidMinusSquare = 0xED6E,

        /// <summary>
        /// mynaui-solid-minus-waves
        /// Unicode: U+ed6f
        /// </summary>
        MynauiSolidMinusWaves = 0xED6F,

        /// <summary>
        /// mynaui-solid-mobile
        /// Unicode: U+ed76
        /// </summary>
        MynauiSolidMobile = 0xED76,

        /// <summary>
        /// mynaui-solid-mobile-signal-five
        /// Unicode: U+ed71
        /// </summary>
        MynauiSolidMobileSignalFive = 0xED71,

        /// <summary>
        /// mynaui-solid-mobile-signal-four
        /// Unicode: U+ed72
        /// </summary>
        MynauiSolidMobileSignalFour = 0xED72,

        /// <summary>
        /// mynaui-solid-mobile-signal-one
        /// Unicode: U+ed73
        /// </summary>
        MynauiSolidMobileSignalOne = 0xED73,

        /// <summary>
        /// mynaui-solid-mobile-signal-three
        /// Unicode: U+ed74
        /// </summary>
        MynauiSolidMobileSignalThree = 0xED74,

        /// <summary>
        /// mynaui-solid-mobile-signal-two
        /// Unicode: U+ed75
        /// </summary>
        MynauiSolidMobileSignalTwo = 0xED75,

        /// <summary>
        /// mynaui-solid-moon
        /// Unicode: U+ed78
        /// </summary>
        MynauiSolidMoon = 0xED78,

        /// <summary>
        /// mynaui-solid-moon-star
        /// Unicode: U+ed77
        /// </summary>
        MynauiSolidMoonStar = 0xED77,

        /// <summary>
        /// mynaui-solid-mountain
        /// Unicode: U+ed7a
        /// </summary>
        MynauiSolidMountain = 0xED7A,

        /// <summary>
        /// mynaui-solid-mountain-snow
        /// Unicode: U+ed79
        /// </summary>
        MynauiSolidMountainSnow = 0xED79,

        /// <summary>
        /// mynaui-solid-mouse-pointer
        /// Unicode: U+ed7b
        /// </summary>
        MynauiSolidMousePointer = 0xED7B,

        /// <summary>
        /// mynaui-solid-move
        /// Unicode: U+ed80
        /// </summary>
        MynauiSolidMove = 0xED80,

        /// <summary>
        /// mynaui-solid-move-diagonal
        /// Unicode: U+ed7d
        /// </summary>
        MynauiSolidMoveDiagonal = 0xED7D,

        /// <summary>
        /// mynaui-solid-move-diagonal-one
        /// Unicode: U+ed7c
        /// </summary>
        MynauiSolidMoveDiagonalOne = 0xED7C,

        /// <summary>
        /// mynaui-solid-move-horizontal
        /// Unicode: U+ed7e
        /// </summary>
        MynauiSolidMoveHorizontal = 0xED7E,

        /// <summary>
        /// mynaui-solid-move-vertical
        /// Unicode: U+ed7f
        /// </summary>
        MynauiSolidMoveVertical = 0xED7F,

        /// <summary>
        /// mynaui-solid-music
        /// Unicode: U+ed87
        /// </summary>
        MynauiSolidMusic = 0xED87,

        /// <summary>
        /// mynaui-solid-music-circle
        /// Unicode: U+ed81
        /// </summary>
        MynauiSolidMusicCircle = 0xED81,

        /// <summary>
        /// mynaui-solid-music-diamond
        /// Unicode: U+ed82
        /// </summary>
        MynauiSolidMusicDiamond = 0xED82,

        /// <summary>
        /// mynaui-solid-music-hexagon
        /// Unicode: U+ed83
        /// </summary>
        MynauiSolidMusicHexagon = 0xED83,

        /// <summary>
        /// mynaui-solid-music-octagon
        /// Unicode: U+ed84
        /// </summary>
        MynauiSolidMusicOctagon = 0xED84,

        /// <summary>
        /// mynaui-solid-music-square
        /// Unicode: U+ed85
        /// </summary>
        MynauiSolidMusicSquare = 0xED85,

        /// <summary>
        /// mynaui-solid-music-waves
        /// Unicode: U+ed86
        /// </summary>
        MynauiSolidMusicWaves = 0xED86,

        /// <summary>
        /// mynaui-solid-myna
        /// Unicode: U+ed88
        /// </summary>
        MynauiSolidMyna = 0xED88,

        /// <summary>
        /// mynaui-solid-navigation
        /// Unicode: U+ed8a
        /// </summary>
        MynauiSolidNavigation = 0xED8A,

        /// <summary>
        /// mynaui-solid-navigation-one
        /// Unicode: U+ed89
        /// </summary>
        MynauiSolidNavigationOne = 0xED89,

        /// <summary>
        /// mynaui-solid-nine
        /// Unicode: U+ed91
        /// </summary>
        MynauiSolidNine = 0xED91,

        /// <summary>
        /// mynaui-solid-nine-circle
        /// Unicode: U+ed8b
        /// </summary>
        MynauiSolidNineCircle = 0xED8B,

        /// <summary>
        /// mynaui-solid-nine-diamond
        /// Unicode: U+ed8c
        /// </summary>
        MynauiSolidNineDiamond = 0xED8C,

        /// <summary>
        /// mynaui-solid-nine-hexagon
        /// Unicode: U+ed8d
        /// </summary>
        MynauiSolidNineHexagon = 0xED8D,

        /// <summary>
        /// mynaui-solid-nine-octagon
        /// Unicode: U+ed8e
        /// </summary>
        MynauiSolidNineOctagon = 0xED8E,

        /// <summary>
        /// mynaui-solid-nine-square
        /// Unicode: U+ed8f
        /// </summary>
        MynauiSolidNineSquare = 0xED8F,

        /// <summary>
        /// mynaui-solid-nine-waves
        /// Unicode: U+ed90
        /// </summary>
        MynauiSolidNineWaves = 0xED90,

        /// <summary>
        /// mynaui-solid-notification
        /// Unicode: U+ed92
        /// </summary>
        MynauiSolidNotification = 0xED92,

        /// <summary>
        /// mynaui-solid-octagon
        /// Unicode: U+ed93
        /// </summary>
        MynauiSolidOctagon = 0xED93,

        /// <summary>
        /// mynaui-solid-one
        /// Unicode: U+ed9a
        /// </summary>
        MynauiSolidOne = 0xED9A,

        /// <summary>
        /// mynaui-solid-one-circle
        /// Unicode: U+ed94
        /// </summary>
        MynauiSolidOneCircle = 0xED94,

        /// <summary>
        /// mynaui-solid-one-diamond
        /// Unicode: U+ed95
        /// </summary>
        MynauiSolidOneDiamond = 0xED95,

        /// <summary>
        /// mynaui-solid-one-hexagon
        /// Unicode: U+ed96
        /// </summary>
        MynauiSolidOneHexagon = 0xED96,

        /// <summary>
        /// mynaui-solid-one-octagon
        /// Unicode: U+ed97
        /// </summary>
        MynauiSolidOneOctagon = 0xED97,

        /// <summary>
        /// mynaui-solid-one-square
        /// Unicode: U+ed98
        /// </summary>
        MynauiSolidOneSquare = 0xED98,

        /// <summary>
        /// mynaui-solid-one-waves
        /// Unicode: U+ed99
        /// </summary>
        MynauiSolidOneWaves = 0xED99,

        /// <summary>
        /// mynaui-solid-option
        /// Unicode: U+ed9b
        /// </summary>
        MynauiSolidOption = 0xED9B,

        /// <summary>
        /// mynaui-solid-package
        /// Unicode: U+ed9c
        /// </summary>
        MynauiSolidPackage = 0xED9C,

        /// <summary>
        /// mynaui-solid-paint
        /// Unicode: U+ed9d
        /// </summary>
        MynauiSolidPaint = 0xED9D,

        /// <summary>
        /// mynaui-solid-panel-bottom
        /// Unicode: U+eda1
        /// </summary>
        MynauiSolidPanelBottom = 0xEDA1,

        /// <summary>
        /// mynaui-solid-panel-bottom-close
        /// Unicode: U+ed9e
        /// </summary>
        MynauiSolidPanelBottomClose = 0xED9E,

        /// <summary>
        /// mynaui-solid-panel-bottom-inactive
        /// Unicode: U+ed9f
        /// </summary>
        MynauiSolidPanelBottomInactive = 0xED9F,

        /// <summary>
        /// mynaui-solid-panel-bottom-open
        /// Unicode: U+eda0
        /// </summary>
        MynauiSolidPanelBottomOpen = 0xEDA0,

        /// <summary>
        /// mynaui-solid-panel-left
        /// Unicode: U+eda5
        /// </summary>
        MynauiSolidPanelLeft = 0xEDA5,

        /// <summary>
        /// mynaui-solid-panel-left-close
        /// Unicode: U+eda2
        /// </summary>
        MynauiSolidPanelLeftClose = 0xEDA2,

        /// <summary>
        /// mynaui-solid-panel-left-inactive
        /// Unicode: U+eda3
        /// </summary>
        MynauiSolidPanelLeftInactive = 0xEDA3,

        /// <summary>
        /// mynaui-solid-panel-left-open
        /// Unicode: U+eda4
        /// </summary>
        MynauiSolidPanelLeftOpen = 0xEDA4,

        /// <summary>
        /// mynaui-solid-panel-right
        /// Unicode: U+eda9
        /// </summary>
        MynauiSolidPanelRight = 0xEDA9,

        /// <summary>
        /// mynaui-solid-panel-right-close
        /// Unicode: U+eda6
        /// </summary>
        MynauiSolidPanelRightClose = 0xEDA6,

        /// <summary>
        /// mynaui-solid-panel-right-inactive
        /// Unicode: U+eda7
        /// </summary>
        MynauiSolidPanelRightInactive = 0xEDA7,

        /// <summary>
        /// mynaui-solid-panel-right-open
        /// Unicode: U+eda8
        /// </summary>
        MynauiSolidPanelRightOpen = 0xEDA8,

        /// <summary>
        /// mynaui-solid-panel-top
        /// Unicode: U+edad
        /// </summary>
        MynauiSolidPanelTop = 0xEDAD,

        /// <summary>
        /// mynaui-solid-panel-top-close
        /// Unicode: U+edaa
        /// </summary>
        MynauiSolidPanelTopClose = 0xEDAA,

        /// <summary>
        /// mynaui-solid-panel-top-inactive
        /// Unicode: U+edab
        /// </summary>
        MynauiSolidPanelTopInactive = 0xEDAB,

        /// <summary>
        /// mynaui-solid-panel-top-open
        /// Unicode: U+edac
        /// </summary>
        MynauiSolidPanelTopOpen = 0xEDAC,

        /// <summary>
        /// mynaui-solid-paperclip
        /// Unicode: U+edae
        /// </summary>
        MynauiSolidPaperclip = 0xEDAE,

        /// <summary>
        /// mynaui-solid-parking
        /// Unicode: U+edaf
        /// </summary>
        MynauiSolidParking = 0xEDAF,

        /// <summary>
        /// mynaui-solid-password
        /// Unicode: U+edb0
        /// </summary>
        MynauiSolidPassword = 0xEDB0,

        /// <summary>
        /// mynaui-solid-path
        /// Unicode: U+edb1
        /// </summary>
        MynauiSolidPath = 0xEDB1,

        /// <summary>
        /// mynaui-solid-pause
        /// Unicode: U+edb8
        /// </summary>
        MynauiSolidPause = 0xEDB8,

        /// <summary>
        /// mynaui-solid-pause-circle
        /// Unicode: U+edb2
        /// </summary>
        MynauiSolidPauseCircle = 0xEDB2,

        /// <summary>
        /// mynaui-solid-pause-diamond
        /// Unicode: U+edb3
        /// </summary>
        MynauiSolidPauseDiamond = 0xEDB3,

        /// <summary>
        /// mynaui-solid-pause-hexagon
        /// Unicode: U+edb4
        /// </summary>
        MynauiSolidPauseHexagon = 0xEDB4,

        /// <summary>
        /// mynaui-solid-pause-octagon
        /// Unicode: U+edb5
        /// </summary>
        MynauiSolidPauseOctagon = 0xEDB5,

        /// <summary>
        /// mynaui-solid-pause-square
        /// Unicode: U+edb6
        /// </summary>
        MynauiSolidPauseSquare = 0xEDB6,

        /// <summary>
        /// mynaui-solid-pause-waves
        /// Unicode: U+edb7
        /// </summary>
        MynauiSolidPauseWaves = 0xEDB7,

        /// <summary>
        /// mynaui-solid-pen
        /// Unicode: U+edb9
        /// </summary>
        MynauiSolidPen = 0xEDB9,

        /// <summary>
        /// mynaui-solid-pencil
        /// Unicode: U+edba
        /// </summary>
        MynauiSolidPencil = 0xEDBA,

        /// <summary>
        /// mynaui-solid-percentage
        /// Unicode: U+edc1
        /// </summary>
        MynauiSolidPercentage = 0xEDC1,

        /// <summary>
        /// mynaui-solid-percentage-circle
        /// Unicode: U+edbb
        /// </summary>
        MynauiSolidPercentageCircle = 0xEDBB,

        /// <summary>
        /// mynaui-solid-percentage-diamond
        /// Unicode: U+edbc
        /// </summary>
        MynauiSolidPercentageDiamond = 0xEDBC,

        /// <summary>
        /// mynaui-solid-percentage-hexagon
        /// Unicode: U+edbd
        /// </summary>
        MynauiSolidPercentageHexagon = 0xEDBD,

        /// <summary>
        /// mynaui-solid-percentage-octagon
        /// Unicode: U+edbe
        /// </summary>
        MynauiSolidPercentageOctagon = 0xEDBE,

        /// <summary>
        /// mynaui-solid-percentage-square
        /// Unicode: U+edbf
        /// </summary>
        MynauiSolidPercentageSquare = 0xEDBF,

        /// <summary>
        /// mynaui-solid-percentage-waves
        /// Unicode: U+edc0
        /// </summary>
        MynauiSolidPercentageWaves = 0xEDC0,

        /// <summary>
        /// mynaui-solid-pin
        /// Unicode: U+edc2
        /// </summary>
        MynauiSolidPin = 0xEDC2,

        /// <summary>
        /// mynaui-solid-pizza
        /// Unicode: U+edc3
        /// </summary>
        MynauiSolidPizza = 0xEDC3,

        /// <summary>
        /// mynaui-solid-planet
        /// Unicode: U+edc4
        /// </summary>
        MynauiSolidPlanet = 0xEDC4,

        /// <summary>
        /// mynaui-solid-play
        /// Unicode: U+edcb
        /// </summary>
        MynauiSolidPlay = 0xEDCB,

        /// <summary>
        /// mynaui-solid-play-circle
        /// Unicode: U+edc5
        /// </summary>
        MynauiSolidPlayCircle = 0xEDC5,

        /// <summary>
        /// mynaui-solid-play-diamond
        /// Unicode: U+edc6
        /// </summary>
        MynauiSolidPlayDiamond = 0xEDC6,

        /// <summary>
        /// mynaui-solid-play-hexagon
        /// Unicode: U+edc7
        /// </summary>
        MynauiSolidPlayHexagon = 0xEDC7,

        /// <summary>
        /// mynaui-solid-play-octagon
        /// Unicode: U+edc8
        /// </summary>
        MynauiSolidPlayOctagon = 0xEDC8,

        /// <summary>
        /// mynaui-solid-play-square
        /// Unicode: U+edc9
        /// </summary>
        MynauiSolidPlaySquare = 0xEDC9,

        /// <summary>
        /// mynaui-solid-play-waves
        /// Unicode: U+edca
        /// </summary>
        MynauiSolidPlayWaves = 0xEDCA,

        /// <summary>
        /// mynaui-solid-plus
        /// Unicode: U+edd2
        /// </summary>
        MynauiSolidPlus = 0xEDD2,

        /// <summary>
        /// mynaui-solid-plus-circle
        /// Unicode: U+edcc
        /// </summary>
        MynauiSolidPlusCircle = 0xEDCC,

        /// <summary>
        /// mynaui-solid-plus-diamond
        /// Unicode: U+edcd
        /// </summary>
        MynauiSolidPlusDiamond = 0xEDCD,

        /// <summary>
        /// mynaui-solid-plus-hexagon
        /// Unicode: U+edce
        /// </summary>
        MynauiSolidPlusHexagon = 0xEDCE,

        /// <summary>
        /// mynaui-solid-plus-octagon
        /// Unicode: U+edcf
        /// </summary>
        MynauiSolidPlusOctagon = 0xEDCF,

        /// <summary>
        /// mynaui-solid-plus-square
        /// Unicode: U+edd0
        /// </summary>
        MynauiSolidPlusSquare = 0xEDD0,

        /// <summary>
        /// mynaui-solid-plus-waves
        /// Unicode: U+edd1
        /// </summary>
        MynauiSolidPlusWaves = 0xEDD1,

        /// <summary>
        /// mynaui-solid-pokeball
        /// Unicode: U+edd3
        /// </summary>
        MynauiSolidPokeball = 0xEDD3,

        /// <summary>
        /// mynaui-solid-power
        /// Unicode: U+edd4
        /// </summary>
        MynauiSolidPower = 0xEDD4,

        /// <summary>
        /// mynaui-solid-presentation
        /// Unicode: U+edd5
        /// </summary>
        MynauiSolidPresentation = 0xEDD5,

        /// <summary>
        /// mynaui-solid-printer
        /// Unicode: U+edd6
        /// </summary>
        MynauiSolidPrinter = 0xEDD6,

        /// <summary>
        /// mynaui-solid-puzzle
        /// Unicode: U+edd7
        /// </summary>
        MynauiSolidPuzzle = 0xEDD7,

        /// <summary>
        /// mynaui-solid-question
        /// Unicode: U+edde
        /// </summary>
        MynauiSolidQuestion = 0xEDDE,

        /// <summary>
        /// mynaui-solid-question-circle
        /// Unicode: U+edd8
        /// </summary>
        MynauiSolidQuestionCircle = 0xEDD8,

        /// <summary>
        /// mynaui-solid-question-diamond
        /// Unicode: U+edd9
        /// </summary>
        MynauiSolidQuestionDiamond = 0xEDD9,

        /// <summary>
        /// mynaui-solid-question-hexagon
        /// Unicode: U+edda
        /// </summary>
        MynauiSolidQuestionHexagon = 0xEDDA,

        /// <summary>
        /// mynaui-solid-question-octagon
        /// Unicode: U+eddb
        /// </summary>
        MynauiSolidQuestionOctagon = 0xEDDB,

        /// <summary>
        /// mynaui-solid-question-square
        /// Unicode: U+eddc
        /// </summary>
        MynauiSolidQuestionSquare = 0xEDDC,

        /// <summary>
        /// mynaui-solid-question-waves
        /// Unicode: U+eddd
        /// </summary>
        MynauiSolidQuestionWaves = 0xEDDD,

        /// <summary>
        /// mynaui-solid-radio
        /// Unicode: U+eddf
        /// </summary>
        MynauiSolidRadio = 0xEDDF,

        /// <summary>
        /// mynaui-solid-rainbow
        /// Unicode: U+ede0
        /// </summary>
        MynauiSolidRainbow = 0xEDE0,

        /// <summary>
        /// mynaui-solid-reception-bell
        /// Unicode: U+ede1
        /// </summary>
        MynauiSolidReceptionBell = 0xEDE1,

        /// <summary>
        /// mynaui-solid-record
        /// Unicode: U+ede2
        /// </summary>
        MynauiSolidRecord = 0xEDE2,

        /// <summary>
        /// mynaui-solid-rectangle
        /// Unicode: U+ede4
        /// </summary>
        MynauiSolidRectangle = 0xEDE4,

        /// <summary>
        /// mynaui-solid-rectangle-vertical
        /// Unicode: U+ede3
        /// </summary>
        MynauiSolidRectangleVertical = 0xEDE3,

        /// <summary>
        /// mynaui-solid-redo
        /// Unicode: U+ede5
        /// </summary>
        MynauiSolidRedo = 0xEDE5,

        /// <summary>
        /// mynaui-solid-refresh
        /// Unicode: U+ede7
        /// </summary>
        MynauiSolidRefresh = 0xEDE7,

        /// <summary>
        /// mynaui-solid-refresh-alt
        /// Unicode: U+ede6
        /// </summary>
        MynauiSolidRefreshAlt = 0xEDE6,

        /// <summary>
        /// mynaui-solid-repeat
        /// Unicode: U+ede8
        /// </summary>
        MynauiSolidRepeat = 0xEDE8,

        /// <summary>
        /// mynaui-solid-rewind
        /// Unicode: U+edef
        /// </summary>
        MynauiSolidRewind = 0xEDEF,

        /// <summary>
        /// mynaui-solid-rewind-circle
        /// Unicode: U+ede9
        /// </summary>
        MynauiSolidRewindCircle = 0xEDE9,

        /// <summary>
        /// mynaui-solid-rewind-diamond
        /// Unicode: U+edea
        /// </summary>
        MynauiSolidRewindDiamond = 0xEDEA,

        /// <summary>
        /// mynaui-solid-rewind-hexagon
        /// Unicode: U+edeb
        /// </summary>
        MynauiSolidRewindHexagon = 0xEDEB,

        /// <summary>
        /// mynaui-solid-rewind-octagon
        /// Unicode: U+edec
        /// </summary>
        MynauiSolidRewindOctagon = 0xEDEC,

        /// <summary>
        /// mynaui-solid-rewind-square
        /// Unicode: U+eded
        /// </summary>
        MynauiSolidRewindSquare = 0xEDED,

        /// <summary>
        /// mynaui-solid-rewind-waves
        /// Unicode: U+edee
        /// </summary>
        MynauiSolidRewindWaves = 0xEDEE,

        /// <summary>
        /// mynaui-solid-rhombus
        /// Unicode: U+edf0
        /// </summary>
        MynauiSolidRhombus = 0xEDF0,

        /// <summary>
        /// mynaui-solid-ribbon
        /// Unicode: U+edf1
        /// </summary>
        MynauiSolidRibbon = 0xEDF1,

        /// <summary>
        /// mynaui-solid-rocket
        /// Unicode: U+edf2
        /// </summary>
        MynauiSolidRocket = 0xEDF2,

        /// <summary>
        /// mynaui-solid-room-service
        /// Unicode: U+edf3
        /// </summary>
        MynauiSolidRoomService = 0xEDF3,

        /// <summary>
        /// mynaui-solid-rows
        /// Unicode: U+edf4
        /// </summary>
        MynauiSolidRows = 0xEDF4,

        /// <summary>
        /// mynaui-solid-rss
        /// Unicode: U+edf5
        /// </summary>
        MynauiSolidRss = 0xEDF5,

        /// <summary>
        /// mynaui-solid-ruler
        /// Unicode: U+edf6
        /// </summary>
        MynauiSolidRuler = 0xEDF6,

        /// <summary>
        /// mynaui-solid-rupee
        /// Unicode: U+edfd
        /// </summary>
        MynauiSolidRupee = 0xEDFD,

        /// <summary>
        /// mynaui-solid-rupee-circle
        /// Unicode: U+edf7
        /// </summary>
        MynauiSolidRupeeCircle = 0xEDF7,

        /// <summary>
        /// mynaui-solid-rupee-diamond
        /// Unicode: U+edf8
        /// </summary>
        MynauiSolidRupeeDiamond = 0xEDF8,

        /// <summary>
        /// mynaui-solid-rupee-hexagon
        /// Unicode: U+edf9
        /// </summary>
        MynauiSolidRupeeHexagon = 0xEDF9,

        /// <summary>
        /// mynaui-solid-rupee-octagon
        /// Unicode: U+edfa
        /// </summary>
        MynauiSolidRupeeOctagon = 0xEDFA,

        /// <summary>
        /// mynaui-solid-rupee-square
        /// Unicode: U+edfb
        /// </summary>
        MynauiSolidRupeeSquare = 0xEDFB,

        /// <summary>
        /// mynaui-solid-rupee-waves
        /// Unicode: U+edfc
        /// </summary>
        MynauiSolidRupeeWaves = 0xEDFC,

        /// <summary>
        /// mynaui-solid-sad-circle
        /// Unicode: U+edfe
        /// </summary>
        MynauiSolidSadCircle = 0xEDFE,

        /// <summary>
        /// mynaui-solid-sad-ghost
        /// Unicode: U+edff
        /// </summary>
        MynauiSolidSadGhost = 0xEDFF,

        /// <summary>
        /// mynaui-solid-sad-square
        /// Unicode: U+ee00
        /// </summary>
        MynauiSolidSadSquare = 0xEE00,

        /// <summary>
        /// mynaui-solid-save
        /// Unicode: U+ee01
        /// </summary>
        MynauiSolidSave = 0xEE01,

        /// <summary>
        /// mynaui-solid-scan
        /// Unicode: U+ee02
        /// </summary>
        MynauiSolidScan = 0xEE02,

        /// <summary>
        /// mynaui-solid-scissors
        /// Unicode: U+ee03
        /// </summary>
        MynauiSolidScissors = 0xEE03,

        /// <summary>
        /// mynaui-solid-search
        /// Unicode: U+ee14
        /// </summary>
        MynauiSolidSearch = 0xEE14,

        /// <summary>
        /// mynaui-solid-search-check
        /// Unicode: U+ee05
        /// </summary>
        MynauiSolidSearchCheck = 0xEE05,

        /// <summary>
        /// mynaui-solid-search-circle
        /// Unicode: U+ee06
        /// </summary>
        MynauiSolidSearchCircle = 0xEE06,

        /// <summary>
        /// mynaui-solid-search-diamond
        /// Unicode: U+ee07
        /// </summary>
        MynauiSolidSearchDiamond = 0xEE07,

        /// <summary>
        /// mynaui-solid-search-dot
        /// Unicode: U+ee08
        /// </summary>
        MynauiSolidSearchDot = 0xEE08,

        /// <summary>
        /// mynaui-solid-search-hexagon
        /// Unicode: U+ee09
        /// </summary>
        MynauiSolidSearchHexagon = 0xEE09,

        /// <summary>
        /// mynaui-solid-search-home
        /// Unicode: U+ee0a
        /// </summary>
        MynauiSolidSearchHome = 0xEE0A,

        /// <summary>
        /// mynaui-solid-search-minus
        /// Unicode: U+ee0b
        /// </summary>
        MynauiSolidSearchMinus = 0xEE0B,

        /// <summary>
        /// mynaui-solid-search-octagon
        /// Unicode: U+ee0c
        /// </summary>
        MynauiSolidSearchOctagon = 0xEE0C,

        /// <summary>
        /// mynaui-solid-search-plus
        /// Unicode: U+ee0d
        /// </summary>
        MynauiSolidSearchPlus = 0xEE0D,

        /// <summary>
        /// mynaui-solid-search-slash
        /// Unicode: U+ee0e
        /// </summary>
        MynauiSolidSearchSlash = 0xEE0E,

        /// <summary>
        /// mynaui-solid-search-snooze
        /// Unicode: U+ee0f
        /// </summary>
        MynauiSolidSearchSnooze = 0xEE0F,

        /// <summary>
        /// mynaui-solid-search-square
        /// Unicode: U+ee10
        /// </summary>
        MynauiSolidSearchSquare = 0xEE10,

        /// <summary>
        /// mynaui-solid-search-user
        /// Unicode: U+ee11
        /// </summary>
        MynauiSolidSearchUser = 0xEE11,

        /// <summary>
        /// mynaui-solid-search-waves
        /// Unicode: U+ee12
        /// </summary>
        MynauiSolidSearchWaves = 0xEE12,

        /// <summary>
        /// mynaui-solid-search-x
        /// Unicode: U+ee13
        /// </summary>
        MynauiSolidSearchX = 0xEE13,

        /// <summary>
        /// mynaui-solid-sea-waves
        /// Unicode: U+ee04
        /// </summary>
        MynauiSolidSeaWaves = 0xEE04,

        /// <summary>
        /// mynaui-solid-select-multiple
        /// Unicode: U+ee15
        /// </summary>
        MynauiSolidSelectMultiple = 0xEE15,

        /// <summary>
        /// mynaui-solid-send
        /// Unicode: U+ee16
        /// </summary>
        MynauiSolidSend = 0xEE16,

        /// <summary>
        /// mynaui-solid-servers
        /// Unicode: U+ee17
        /// </summary>
        MynauiSolidServers = 0xEE17,

        /// <summary>
        /// mynaui-solid-seven
        /// Unicode: U+ee1e
        /// </summary>
        MynauiSolidSeven = 0xEE1E,

        /// <summary>
        /// mynaui-solid-seven-circle
        /// Unicode: U+ee18
        /// </summary>
        MynauiSolidSevenCircle = 0xEE18,

        /// <summary>
        /// mynaui-solid-seven-diamond
        /// Unicode: U+ee19
        /// </summary>
        MynauiSolidSevenDiamond = 0xEE19,

        /// <summary>
        /// mynaui-solid-seven-hexagon
        /// Unicode: U+ee1a
        /// </summary>
        MynauiSolidSevenHexagon = 0xEE1A,

        /// <summary>
        /// mynaui-solid-seven-octagon
        /// Unicode: U+ee1b
        /// </summary>
        MynauiSolidSevenOctagon = 0xEE1B,

        /// <summary>
        /// mynaui-solid-seven-square
        /// Unicode: U+ee1c
        /// </summary>
        MynauiSolidSevenSquare = 0xEE1C,

        /// <summary>
        /// mynaui-solid-seven-waves
        /// Unicode: U+ee1d
        /// </summary>
        MynauiSolidSevenWaves = 0xEE1D,

        /// <summary>
        /// mynaui-solid-share
        /// Unicode: U+ee1f
        /// </summary>
        MynauiSolidShare = 0xEE1F,

        /// <summary>
        /// mynaui-solid-shell
        /// Unicode: U+ee20
        /// </summary>
        MynauiSolidShell = 0xEE20,

        /// <summary>
        /// mynaui-solid-shield
        /// Unicode: U+ee29
        /// </summary>
        MynauiSolidShield = 0xEE29,

        /// <summary>
        /// mynaui-solid-shield-check
        /// Unicode: U+ee21
        /// </summary>
        MynauiSolidShieldCheck = 0xEE21,

        /// <summary>
        /// mynaui-solid-shield-crossed
        /// Unicode: U+ee22
        /// </summary>
        MynauiSolidShieldCrossed = 0xEE22,

        /// <summary>
        /// mynaui-solid-shield-minus
        /// Unicode: U+ee23
        /// </summary>
        MynauiSolidShieldMinus = 0xEE23,

        /// <summary>
        /// mynaui-solid-shield-one
        /// Unicode: U+ee24
        /// </summary>
        MynauiSolidShieldOne = 0xEE24,

        /// <summary>
        /// mynaui-solid-shield-plus
        /// Unicode: U+ee25
        /// </summary>
        MynauiSolidShieldPlus = 0xEE25,

        /// <summary>
        /// mynaui-solid-shield-slash
        /// Unicode: U+ee26
        /// </summary>
        MynauiSolidShieldSlash = 0xEE26,

        /// <summary>
        /// mynaui-solid-shield-two
        /// Unicode: U+ee27
        /// </summary>
        MynauiSolidShieldTwo = 0xEE27,

        /// <summary>
        /// mynaui-solid-shield-x
        /// Unicode: U+ee28
        /// </summary>
        MynauiSolidShieldX = 0xEE28,

        /// <summary>
        /// mynaui-solid-shooting-star
        /// Unicode: U+ee2a
        /// </summary>
        MynauiSolidShootingStar = 0xEE2A,

        /// <summary>
        /// mynaui-solid-shopping-bag
        /// Unicode: U+ee2b
        /// </summary>
        MynauiSolidShoppingBag = 0xEE2B,

        /// <summary>
        /// mynaui-solid-shovel
        /// Unicode: U+ee2c
        /// </summary>
        MynauiSolidShovel = 0xEE2C,

        /// <summary>
        /// mynaui-solid-shrub
        /// Unicode: U+ee2d
        /// </summary>
        MynauiSolidShrub = 0xEE2D,

        /// <summary>
        /// mynaui-solid-shuffle
        /// Unicode: U+ee2f
        /// </summary>
        MynauiSolidShuffle = 0xEE2F,

        /// <summary>
        /// mynaui-solid-shuffle-alt
        /// Unicode: U+ee2e
        /// </summary>
        MynauiSolidShuffleAlt = 0xEE2E,

        /// <summary>
        /// mynaui-solid-sidebar
        /// Unicode: U+ee31
        /// </summary>
        MynauiSolidSidebar = 0xEE31,

        /// <summary>
        /// mynaui-solid-sidebar-alt
        /// Unicode: U+ee30
        /// </summary>
        MynauiSolidSidebarAlt = 0xEE30,

        /// <summary>
        /// mynaui-solid-signal
        /// Unicode: U+ee38
        /// </summary>
        MynauiSolidSignal = 0xEE38,

        /// <summary>
        /// mynaui-solid-signal-circle
        /// Unicode: U+ee32
        /// </summary>
        MynauiSolidSignalCircle = 0xEE32,

        /// <summary>
        /// mynaui-solid-signal-diamond
        /// Unicode: U+ee33
        /// </summary>
        MynauiSolidSignalDiamond = 0xEE33,

        /// <summary>
        /// mynaui-solid-signal-hexagon
        /// Unicode: U+ee34
        /// </summary>
        MynauiSolidSignalHexagon = 0xEE34,

        /// <summary>
        /// mynaui-solid-signal-octagon
        /// Unicode: U+ee35
        /// </summary>
        MynauiSolidSignalOctagon = 0xEE35,

        /// <summary>
        /// mynaui-solid-signal-square
        /// Unicode: U+ee36
        /// </summary>
        MynauiSolidSignalSquare = 0xEE36,

        /// <summary>
        /// mynaui-solid-signal-waves
        /// Unicode: U+ee37
        /// </summary>
        MynauiSolidSignalWaves = 0xEE37,

        /// <summary>
        /// mynaui-solid-six
        /// Unicode: U+ee3f
        /// </summary>
        MynauiSolidSix = 0xEE3F,

        /// <summary>
        /// mynaui-solid-six-circle
        /// Unicode: U+ee39
        /// </summary>
        MynauiSolidSixCircle = 0xEE39,

        /// <summary>
        /// mynaui-solid-six-diamond
        /// Unicode: U+ee3a
        /// </summary>
        MynauiSolidSixDiamond = 0xEE3A,

        /// <summary>
        /// mynaui-solid-six-hexagon
        /// Unicode: U+ee3b
        /// </summary>
        MynauiSolidSixHexagon = 0xEE3B,

        /// <summary>
        /// mynaui-solid-six-octagon
        /// Unicode: U+ee3c
        /// </summary>
        MynauiSolidSixOctagon = 0xEE3C,

        /// <summary>
        /// mynaui-solid-six-square
        /// Unicode: U+ee3d
        /// </summary>
        MynauiSolidSixSquare = 0xEE3D,

        /// <summary>
        /// mynaui-solid-six-waves
        /// Unicode: U+ee3e
        /// </summary>
        MynauiSolidSixWaves = 0xEE3E,

        /// <summary>
        /// mynaui-solid-skip-back
        /// Unicode: U+ee40
        /// </summary>
        MynauiSolidSkipBack = 0xEE40,

        /// <summary>
        /// mynaui-solid-skip-forward
        /// Unicode: U+ee41
        /// </summary>
        MynauiSolidSkipForward = 0xEE41,

        /// <summary>
        /// mynaui-solid-slash-circle
        /// Unicode: U+ee42
        /// </summary>
        MynauiSolidSlashCircle = 0xEE42,

        /// <summary>
        /// mynaui-solid-slash-diamond
        /// Unicode: U+ee43
        /// </summary>
        MynauiSolidSlashDiamond = 0xEE43,

        /// <summary>
        /// mynaui-solid-slash-hexagon
        /// Unicode: U+ee44
        /// </summary>
        MynauiSolidSlashHexagon = 0xEE44,

        /// <summary>
        /// mynaui-solid-slash-octagon
        /// Unicode: U+ee45
        /// </summary>
        MynauiSolidSlashOctagon = 0xEE45,

        /// <summary>
        /// mynaui-solid-slash-square
        /// Unicode: U+ee46
        /// </summary>
        MynauiSolidSlashSquare = 0xEE46,

        /// <summary>
        /// mynaui-solid-slash-waves
        /// Unicode: U+ee47
        /// </summary>
        MynauiSolidSlashWaves = 0xEE47,

        /// <summary>
        /// mynaui-solid-smile-circle
        /// Unicode: U+ee48
        /// </summary>
        MynauiSolidSmileCircle = 0xEE48,

        /// <summary>
        /// mynaui-solid-smile-ghost
        /// Unicode: U+ee49
        /// </summary>
        MynauiSolidSmileGhost = 0xEE49,

        /// <summary>
        /// mynaui-solid-smile-square
        /// Unicode: U+ee4a
        /// </summary>
        MynauiSolidSmileSquare = 0xEE4A,

        /// <summary>
        /// mynaui-solid-snow
        /// Unicode: U+ee4b
        /// </summary>
        MynauiSolidSnow = 0xEE4B,

        /// <summary>
        /// mynaui-solid-snowflake
        /// Unicode: U+ee4c
        /// </summary>
        MynauiSolidSnowflake = 0xEE4C,

        /// <summary>
        /// mynaui-solid-sofa
        /// Unicode: U+ee4d
        /// </summary>
        MynauiSolidSofa = 0xEE4D,

        /// <summary>
        /// mynaui-solid-sort
        /// Unicode: U+ee4e
        /// </summary>
        MynauiSolidSort = 0xEE4E,

        /// <summary>
        /// mynaui-solid-sparkles
        /// Unicode: U+ee4f
        /// </summary>
        MynauiSolidSparkles = 0xEE4F,

        /// <summary>
        /// mynaui-solid-speaker
        /// Unicode: U+ee50
        /// </summary>
        MynauiSolidSpeaker = 0xEE50,

        /// <summary>
        /// mynaui-solid-spinner
        /// Unicode: U+ee52
        /// </summary>
        MynauiSolidSpinner = 0xEE52,

        /// <summary>
        /// mynaui-solid-spinner-one
        /// Unicode: U+ee51
        /// </summary>
        MynauiSolidSpinnerOne = 0xEE51,

        /// <summary>
        /// mynaui-solid-sprout
        /// Unicode: U+ee53
        /// </summary>
        MynauiSolidSprout = 0xEE53,

        /// <summary>
        /// mynaui-solid-square
        /// Unicode: U+ee59
        /// </summary>
        MynauiSolidSquare = 0xEE59,

        /// <summary>
        /// mynaui-solid-square-chart-gantt
        /// Unicode: U+ee54
        /// </summary>
        MynauiSolidSquareChartGantt = 0xEE54,

        /// <summary>
        /// mynaui-solid-square-dashed
        /// Unicode: U+ee56
        /// </summary>
        MynauiSolidSquareDashed = 0xEE56,

        /// <summary>
        /// mynaui-solid-square-dashed-kanban
        /// Unicode: U+ee55
        /// </summary>
        MynauiSolidSquareDashedKanban = 0xEE55,

        /// <summary>
        /// mynaui-solid-square-half
        /// Unicode: U+ee57
        /// </summary>
        MynauiSolidSquareHalf = 0xEE57,

        /// <summary>
        /// mynaui-solid-square-kanban
        /// Unicode: U+ee58
        /// </summary>
        MynauiSolidSquareKanban = 0xEE58,

        /// <summary>
        /// mynaui-solid-star
        /// Unicode: U+ee5a
        /// </summary>
        MynauiSolidStar = 0xEE5A,

        /// <summary>
        /// mynaui-solid-stop
        /// Unicode: U+ee61
        /// </summary>
        MynauiSolidStop = 0xEE61,

        /// <summary>
        /// mynaui-solid-stop-circle
        /// Unicode: U+ee5b
        /// </summary>
        MynauiSolidStopCircle = 0xEE5B,

        /// <summary>
        /// mynaui-solid-stop-diamond
        /// Unicode: U+ee5c
        /// </summary>
        MynauiSolidStopDiamond = 0xEE5C,

        /// <summary>
        /// mynaui-solid-stop-hexagon
        /// Unicode: U+ee5d
        /// </summary>
        MynauiSolidStopHexagon = 0xEE5D,

        /// <summary>
        /// mynaui-solid-stop-octagon
        /// Unicode: U+ee5e
        /// </summary>
        MynauiSolidStopOctagon = 0xEE5E,

        /// <summary>
        /// mynaui-solid-stop-square
        /// Unicode: U+ee5f
        /// </summary>
        MynauiSolidStopSquare = 0xEE5F,

        /// <summary>
        /// mynaui-solid-stop-waves
        /// Unicode: U+ee60
        /// </summary>
        MynauiSolidStopWaves = 0xEE60,

        /// <summary>
        /// mynaui-solid-store
        /// Unicode: U+ee62
        /// </summary>
        MynauiSolidStore = 0xEE62,

        /// <summary>
        /// mynaui-solid-subtract
        /// Unicode: U+ee63
        /// </summary>
        MynauiSolidSubtract = 0xEE63,

        /// <summary>
        /// mynaui-solid-sun
        /// Unicode: U+ee67
        /// </summary>
        MynauiSolidSun = 0xEE67,

        /// <summary>
        /// mynaui-solid-sun-dim
        /// Unicode: U+ee64
        /// </summary>
        MynauiSolidSunDim = 0xEE64,

        /// <summary>
        /// mynaui-solid-sun-medium
        /// Unicode: U+ee65
        /// </summary>
        MynauiSolidSunMedium = 0xEE65,

        /// <summary>
        /// mynaui-solid-sunrise
        /// Unicode: U+ee68
        /// </summary>
        MynauiSolidSunrise = 0xEE68,

        /// <summary>
        /// mynaui-solid-sunset
        /// Unicode: U+ee69
        /// </summary>
        MynauiSolidSunset = 0xEE69,

        /// <summary>
        /// mynaui-solid-sun-snow
        /// Unicode: U+ee66
        /// </summary>
        MynauiSolidSunSnow = 0xEE66,

        /// <summary>
        /// mynaui-solid-support
        /// Unicode: U+ee6a
        /// </summary>
        MynauiSolidSupport = 0xEE6A,

        /// <summary>
        /// mynaui-solid-swatches
        /// Unicode: U+ee6b
        /// </summary>
        MynauiSolidSwatches = 0xEE6B,

        /// <summary>
        /// mynaui-solid-table
        /// Unicode: U+ee6c
        /// </summary>
        MynauiSolidTable = 0xEE6C,

        /// <summary>
        /// mynaui-solid-tablet
        /// Unicode: U+ee6d
        /// </summary>
        MynauiSolidTablet = 0xEE6D,

        /// <summary>
        /// mynaui-solid-tag
        /// Unicode: U+ee6f
        /// </summary>
        MynauiSolidTag = 0xEE6F,

        /// <summary>
        /// mynaui-solid-tag-plus
        /// Unicode: U+ee6e
        /// </summary>
        MynauiSolidTagPlus = 0xEE6E,

        /// <summary>
        /// mynaui-solid-tally-five
        /// Unicode: U+ee70
        /// </summary>
        MynauiSolidTallyFive = 0xEE70,

        /// <summary>
        /// mynaui-solid-tally-four
        /// Unicode: U+ee71
        /// </summary>
        MynauiSolidTallyFour = 0xEE71,

        /// <summary>
        /// mynaui-solid-tally-one
        /// Unicode: U+ee72
        /// </summary>
        MynauiSolidTallyOne = 0xEE72,

        /// <summary>
        /// mynaui-solid-tally-three
        /// Unicode: U+ee73
        /// </summary>
        MynauiSolidTallyThree = 0xEE73,

        /// <summary>
        /// mynaui-solid-tally-two
        /// Unicode: U+ee74
        /// </summary>
        MynauiSolidTallyTwo = 0xEE74,

        /// <summary>
        /// mynaui-solid-target
        /// Unicode: U+ee75
        /// </summary>
        MynauiSolidTarget = 0xEE75,

        /// <summary>
        /// mynaui-solid-telephone
        /// Unicode: U+ee7c
        /// </summary>
        MynauiSolidTelephone = 0xEE7C,

        /// <summary>
        /// mynaui-solid-telephone-call
        /// Unicode: U+ee76
        /// </summary>
        MynauiSolidTelephoneCall = 0xEE76,

        /// <summary>
        /// mynaui-solid-telephone-forward
        /// Unicode: U+ee77
        /// </summary>
        MynauiSolidTelephoneForward = 0xEE77,

        /// <summary>
        /// mynaui-solid-telephone-in
        /// Unicode: U+ee78
        /// </summary>
        MynauiSolidTelephoneIn = 0xEE78,

        /// <summary>
        /// mynaui-solid-telephone-missed
        /// Unicode: U+ee79
        /// </summary>
        MynauiSolidTelephoneMissed = 0xEE79,

        /// <summary>
        /// mynaui-solid-telephone-out
        /// Unicode: U+ee7a
        /// </summary>
        MynauiSolidTelephoneOut = 0xEE7A,

        /// <summary>
        /// mynaui-solid-telephone-slash
        /// Unicode: U+ee7b
        /// </summary>
        MynauiSolidTelephoneSlash = 0xEE7B,

        /// <summary>
        /// mynaui-solid-tent
        /// Unicode: U+ee7e
        /// </summary>
        MynauiSolidTent = 0xEE7E,

        /// <summary>
        /// mynaui-solid-tent-tree
        /// Unicode: U+ee7d
        /// </summary>
        MynauiSolidTentTree = 0xEE7D,

        /// <summary>
        /// mynaui-solid-terminal
        /// Unicode: U+ee7f
        /// </summary>
        MynauiSolidTerminal = 0xEE7F,

        /// <summary>
        /// mynaui-solid-text-align-center
        /// Unicode: U+ee80
        /// </summary>
        MynauiSolidTextAlignCenter = 0xEE80,

        /// <summary>
        /// mynaui-solid-text-align-left
        /// Unicode: U+ee81
        /// </summary>
        MynauiSolidTextAlignLeft = 0xEE81,

        /// <summary>
        /// mynaui-solid-text-align-right
        /// Unicode: U+ee82
        /// </summary>
        MynauiSolidTextAlignRight = 0xEE82,

        /// <summary>
        /// mynaui-solid-text-justify
        /// Unicode: U+ee83
        /// </summary>
        MynauiSolidTextJustify = 0xEE83,

        /// <summary>
        /// mynaui-solid-thermometer
        /// Unicode: U+ee86
        /// </summary>
        MynauiSolidThermometer = 0xEE86,

        /// <summary>
        /// mynaui-solid-thermometer-snowflake
        /// Unicode: U+ee84
        /// </summary>
        MynauiSolidThermometerSnowflake = 0xEE84,

        /// <summary>
        /// mynaui-solid-thermometer-sun
        /// Unicode: U+ee85
        /// </summary>
        MynauiSolidThermometerSun = 0xEE85,

        /// <summary>
        /// mynaui-solid-three
        /// Unicode: U+ee8d
        /// </summary>
        MynauiSolidThree = 0xEE8D,

        /// <summary>
        /// mynaui-solid-three-circle
        /// Unicode: U+ee87
        /// </summary>
        MynauiSolidThreeCircle = 0xEE87,

        /// <summary>
        /// mynaui-solid-three-diamond
        /// Unicode: U+ee88
        /// </summary>
        MynauiSolidThreeDiamond = 0xEE88,

        /// <summary>
        /// mynaui-solid-three-hexagon
        /// Unicode: U+ee89
        /// </summary>
        MynauiSolidThreeHexagon = 0xEE89,

        /// <summary>
        /// mynaui-solid-three-octagon
        /// Unicode: U+ee8a
        /// </summary>
        MynauiSolidThreeOctagon = 0xEE8A,

        /// <summary>
        /// mynaui-solid-three-square
        /// Unicode: U+ee8b
        /// </summary>
        MynauiSolidThreeSquare = 0xEE8B,

        /// <summary>
        /// mynaui-solid-three-waves
        /// Unicode: U+ee8c
        /// </summary>
        MynauiSolidThreeWaves = 0xEE8C,

        /// <summary>
        /// mynaui-solid-ticket
        /// Unicode: U+ee8f
        /// </summary>
        MynauiSolidTicket = 0xEE8F,

        /// <summary>
        /// mynaui-solid-ticket-slash
        /// Unicode: U+ee8e
        /// </summary>
        MynauiSolidTicketSlash = 0xEE8E,

        /// <summary>
        /// mynaui-solid-toggle-left
        /// Unicode: U+ee90
        /// </summary>
        MynauiSolidToggleLeft = 0xEE90,

        /// <summary>
        /// mynaui-solid-toggle-right
        /// Unicode: U+ee91
        /// </summary>
        MynauiSolidToggleRight = 0xEE91,

        /// <summary>
        /// mynaui-solid-tool
        /// Unicode: U+ee92
        /// </summary>
        MynauiSolidTool = 0xEE92,

        /// <summary>
        /// mynaui-solid-tornado
        /// Unicode: U+ee93
        /// </summary>
        MynauiSolidTornado = 0xEE93,

        /// <summary>
        /// mynaui-solid-train
        /// Unicode: U+ee94
        /// </summary>
        MynauiSolidTrain = 0xEE94,

        /// <summary>
        /// mynaui-solid-trash
        /// Unicode: U+ee97
        /// </summary>
        MynauiSolidTrash = 0xEE97,

        /// <summary>
        /// mynaui-solid-trash-one
        /// Unicode: U+ee95
        /// </summary>
        MynauiSolidTrashOne = 0xEE95,

        /// <summary>
        /// mynaui-solid-trash-two
        /// Unicode: U+ee96
        /// </summary>
        MynauiSolidTrashTwo = 0xEE96,

        /// <summary>
        /// mynaui-solid-tree
        /// Unicode: U+ee9b
        /// </summary>
        MynauiSolidTree = 0xEE9B,

        /// <summary>
        /// mynaui-solid-tree-deciduous
        /// Unicode: U+ee98
        /// </summary>
        MynauiSolidTreeDeciduous = 0xEE98,

        /// <summary>
        /// mynaui-solid-tree-palm
        /// Unicode: U+ee99
        /// </summary>
        MynauiSolidTreePalm = 0xEE99,

        /// <summary>
        /// mynaui-solid-tree-pine
        /// Unicode: U+ee9a
        /// </summary>
        MynauiSolidTreePine = 0xEE9A,

        /// <summary>
        /// mynaui-solid-trees
        /// Unicode: U+ee9c
        /// </summary>
        MynauiSolidTrees = 0xEE9C,

        /// <summary>
        /// mynaui-solid-trending-down
        /// Unicode: U+ee9d
        /// </summary>
        MynauiSolidTrendingDown = 0xEE9D,

        /// <summary>
        /// mynaui-solid-trending-up
        /// Unicode: U+ee9f
        /// </summary>
        MynauiSolidTrendingUp = 0xEE9F,

        /// <summary>
        /// mynaui-solid-trending-up-down
        /// Unicode: U+ee9e
        /// </summary>
        MynauiSolidTrendingUpDown = 0xEE9E,

        /// <summary>
        /// mynaui-solid-triangle
        /// Unicode: U+eea0
        /// </summary>
        MynauiSolidTriangle = 0xEEA0,

        /// <summary>
        /// mynaui-solid-truck
        /// Unicode: U+eea1
        /// </summary>
        MynauiSolidTruck = 0xEEA1,

        /// <summary>
        /// mynaui-solid-tv
        /// Unicode: U+eea2
        /// </summary>
        MynauiSolidTv = 0xEEA2,

        /// <summary>
        /// mynaui-solid-two
        /// Unicode: U+eea9
        /// </summary>
        MynauiSolidTwo = 0xEEA9,

        /// <summary>
        /// mynaui-solid-two-circle
        /// Unicode: U+eea3
        /// </summary>
        MynauiSolidTwoCircle = 0xEEA3,

        /// <summary>
        /// mynaui-solid-two-diamond
        /// Unicode: U+eea4
        /// </summary>
        MynauiSolidTwoDiamond = 0xEEA4,

        /// <summary>
        /// mynaui-solid-two-hexagon
        /// Unicode: U+eea5
        /// </summary>
        MynauiSolidTwoHexagon = 0xEEA5,

        /// <summary>
        /// mynaui-solid-two-octagon
        /// Unicode: U+eea6
        /// </summary>
        MynauiSolidTwoOctagon = 0xEEA6,

        /// <summary>
        /// mynaui-solid-two-square
        /// Unicode: U+eea7
        /// </summary>
        MynauiSolidTwoSquare = 0xEEA7,

        /// <summary>
        /// mynaui-solid-two-waves
        /// Unicode: U+eea8
        /// </summary>
        MynauiSolidTwoWaves = 0xEEA8,

        /// <summary>
        /// mynaui-solid-type-bold
        /// Unicode: U+eeaa
        /// </summary>
        MynauiSolidTypeBold = 0xEEAA,

        /// <summary>
        /// mynaui-solid-type-italic
        /// Unicode: U+eeab
        /// </summary>
        MynauiSolidTypeItalic = 0xEEAB,

        /// <summary>
        /// mynaui-solid-type-text
        /// Unicode: U+eeac
        /// </summary>
        MynauiSolidTypeText = 0xEEAC,

        /// <summary>
        /// mynaui-solid-type-underline
        /// Unicode: U+eead
        /// </summary>
        MynauiSolidTypeUnderline = 0xEEAD,

        /// <summary>
        /// mynaui-solid-umbrella
        /// Unicode: U+eeaf
        /// </summary>
        MynauiSolidUmbrella = 0xEEAF,

        /// <summary>
        /// mynaui-solid-umbrella-off
        /// Unicode: U+eeae
        /// </summary>
        MynauiSolidUmbrellaOff = 0xEEAE,

        /// <summary>
        /// mynaui-solid-undo
        /// Unicode: U+eeb0
        /// </summary>
        MynauiSolidUndo = 0xEEB0,

        /// <summary>
        /// mynaui-solid-union
        /// Unicode: U+eeb1
        /// </summary>
        MynauiSolidUnion = 0xEEB1,

        /// <summary>
        /// mynaui-solid-unlink
        /// Unicode: U+eeb2
        /// </summary>
        MynauiSolidUnlink = 0xEEB2,

        /// <summary>
        /// mynaui-solid-upload
        /// Unicode: U+eeb3
        /// </summary>
        MynauiSolidUpload = 0xEEB3,

        /// <summary>
        /// mynaui-solid-user
        /// Unicode: U+eebf
        /// </summary>
        MynauiSolidUser = 0xEEBF,

        /// <summary>
        /// mynaui-solid-user-check
        /// Unicode: U+eeb4
        /// </summary>
        MynauiSolidUserCheck = 0xEEB4,

        /// <summary>
        /// mynaui-solid-user-circle
        /// Unicode: U+eeb5
        /// </summary>
        MynauiSolidUserCircle = 0xEEB5,

        /// <summary>
        /// mynaui-solid-user-diamond
        /// Unicode: U+eeb6
        /// </summary>
        MynauiSolidUserDiamond = 0xEEB6,

        /// <summary>
        /// mynaui-solid-user-hexagon
        /// Unicode: U+eeb7
        /// </summary>
        MynauiSolidUserHexagon = 0xEEB7,

        /// <summary>
        /// mynaui-solid-user-minus
        /// Unicode: U+eeb8
        /// </summary>
        MynauiSolidUserMinus = 0xEEB8,

        /// <summary>
        /// mynaui-solid-user-octagon
        /// Unicode: U+eeb9
        /// </summary>
        MynauiSolidUserOctagon = 0xEEB9,

        /// <summary>
        /// mynaui-solid-user-plus
        /// Unicode: U+eeba
        /// </summary>
        MynauiSolidUserPlus = 0xEEBA,

        /// <summary>
        /// mynaui-solid-users
        /// Unicode: U+eec1
        /// </summary>
        MynauiSolidUsers = 0xEEC1,

        /// <summary>
        /// mynaui-solid-user-settings
        /// Unicode: U+eebb
        /// </summary>
        MynauiSolidUserSettings = 0xEEBB,

        /// <summary>
        /// mynaui-solid-users-group
        /// Unicode: U+eec0
        /// </summary>
        MynauiSolidUsersGroup = 0xEEC0,

        /// <summary>
        /// mynaui-solid-user-square
        /// Unicode: U+eebc
        /// </summary>
        MynauiSolidUserSquare = 0xEEBC,

        /// <summary>
        /// mynaui-solid-user-waves
        /// Unicode: U+eebd
        /// </summary>
        MynauiSolidUserWaves = 0xEEBD,

        /// <summary>
        /// mynaui-solid-user-x
        /// Unicode: U+eebe
        /// </summary>
        MynauiSolidUserX = 0xEEBE,

        /// <summary>
        /// mynaui-solid-video
        /// Unicode: U+eec3
        /// </summary>
        MynauiSolidVideo = 0xEEC3,

        /// <summary>
        /// mynaui-solid-video-slash
        /// Unicode: U+eec2
        /// </summary>
        MynauiSolidVideoSlash = 0xEEC2,

        /// <summary>
        /// mynaui-solid-volume-check
        /// Unicode: U+eec4
        /// </summary>
        MynauiSolidVolumeCheck = 0xEEC4,

        /// <summary>
        /// mynaui-solid-volume-high
        /// Unicode: U+eec5
        /// </summary>
        MynauiSolidVolumeHigh = 0xEEC5,

        /// <summary>
        /// mynaui-solid-volume-low
        /// Unicode: U+eec6
        /// </summary>
        MynauiSolidVolumeLow = 0xEEC6,

        /// <summary>
        /// mynaui-solid-volume-minus
        /// Unicode: U+eec7
        /// </summary>
        MynauiSolidVolumeMinus = 0xEEC7,

        /// <summary>
        /// mynaui-solid-volume-none
        /// Unicode: U+eec8
        /// </summary>
        MynauiSolidVolumeNone = 0xEEC8,

        /// <summary>
        /// mynaui-solid-volume-plus
        /// Unicode: U+eec9
        /// </summary>
        MynauiSolidVolumePlus = 0xEEC9,

        /// <summary>
        /// mynaui-solid-volume-slash
        /// Unicode: U+eeca
        /// </summary>
        MynauiSolidVolumeSlash = 0xEECA,

        /// <summary>
        /// mynaui-solid-volume-x
        /// Unicode: U+eecb
        /// </summary>
        MynauiSolidVolumeX = 0xEECB,

        /// <summary>
        /// mynaui-solid-watch
        /// Unicode: U+eecc
        /// </summary>
        MynauiSolidWatch = 0xEECC,

        /// <summary>
        /// mynaui-solid-waves
        /// Unicode: U+eecd
        /// </summary>
        MynauiSolidWaves = 0xEECD,

        /// <summary>
        /// mynaui-solid-webcam
        /// Unicode: U+eece
        /// </summary>
        MynauiSolidWebcam = 0xEECE,

        /// <summary>
        /// mynaui-solid-wheel
        /// Unicode: U+eecf
        /// </summary>
        MynauiSolidWheel = 0xEECF,

        /// <summary>
        /// mynaui-solid-wheelchair
        /// Unicode: U+eed0
        /// </summary>
        MynauiSolidWheelchair = 0xEED0,

        /// <summary>
        /// mynaui-solid-wifi
        /// Unicode: U+eed8
        /// </summary>
        MynauiSolidWifi = 0xEED8,

        /// <summary>
        /// mynaui-solid-wifi-check
        /// Unicode: U+eed1
        /// </summary>
        MynauiSolidWifiCheck = 0xEED1,

        /// <summary>
        /// mynaui-solid-wifi-low
        /// Unicode: U+eed2
        /// </summary>
        MynauiSolidWifiLow = 0xEED2,

        /// <summary>
        /// mynaui-solid-wifi-medium
        /// Unicode: U+eed3
        /// </summary>
        MynauiSolidWifiMedium = 0xEED3,

        /// <summary>
        /// mynaui-solid-wifi-minus
        /// Unicode: U+eed4
        /// </summary>
        MynauiSolidWifiMinus = 0xEED4,

        /// <summary>
        /// mynaui-solid-wifi-plus
        /// Unicode: U+eed5
        /// </summary>
        MynauiSolidWifiPlus = 0xEED5,

        /// <summary>
        /// mynaui-solid-wifi-slash
        /// Unicode: U+eed6
        /// </summary>
        MynauiSolidWifiSlash = 0xEED6,

        /// <summary>
        /// mynaui-solid-wifi-x
        /// Unicode: U+eed7
        /// </summary>
        MynauiSolidWifiX = 0xEED7,

        /// <summary>
        /// mynaui-solid-wind
        /// Unicode: U+eeda
        /// </summary>
        MynauiSolidWind = 0xEEDA,

        /// <summary>
        /// mynaui-solid-wind-arrow-down
        /// Unicode: U+eed9
        /// </summary>
        MynauiSolidWindArrowDown = 0xEED9,

        /// <summary>
        /// mynaui-solid-winds
        /// Unicode: U+eedb
        /// </summary>
        MynauiSolidWinds = 0xEEDB,

        /// <summary>
        /// mynaui-solid-wine
        /// Unicode: U+eedc
        /// </summary>
        MynauiSolidWine = 0xEEDC,

        /// <summary>
        /// mynaui-solid-wink-circle
        /// Unicode: U+eedd
        /// </summary>
        MynauiSolidWinkCircle = 0xEEDD,

        /// <summary>
        /// mynaui-solid-wink-ghost
        /// Unicode: U+eede
        /// </summary>
        MynauiSolidWinkGhost = 0xEEDE,

        /// <summary>
        /// mynaui-solid-wink-square
        /// Unicode: U+eedf
        /// </summary>
        MynauiSolidWinkSquare = 0xEEDF,

        /// <summary>
        /// mynaui-solid-wrench
        /// Unicode: U+eee0
        /// </summary>
        MynauiSolidWrench = 0xEEE0,

        /// <summary>
        /// mynaui-solid-x
        /// Unicode: U+eee8
        /// </summary>
        MynauiSolidX = 0xEEE8,

        /// <summary>
        /// mynaui-solid-x-circle
        /// Unicode: U+eee1
        /// </summary>
        MynauiSolidXCircle = 0xEEE1,

        /// <summary>
        /// mynaui-solid-x-diamond
        /// Unicode: U+eee2
        /// </summary>
        MynauiSolidXDiamond = 0xEEE2,

        /// <summary>
        /// mynaui-solid-x-hexagon
        /// Unicode: U+eee3
        /// </summary>
        MynauiSolidXHexagon = 0xEEE3,

        /// <summary>
        /// mynaui-solid-x-octagon
        /// Unicode: U+eee4
        /// </summary>
        MynauiSolidXOctagon = 0xEEE4,

        /// <summary>
        /// mynaui-solid-x-square
        /// Unicode: U+eee5
        /// </summary>
        MynauiSolidXSquare = 0xEEE5,

        /// <summary>
        /// mynaui-solid-x-triangle
        /// Unicode: U+eee6
        /// </summary>
        MynauiSolidXTriangle = 0xEEE6,

        /// <summary>
        /// mynaui-solid-x-waves
        /// Unicode: U+eee7
        /// </summary>
        MynauiSolidXWaves = 0xEEE7,

        /// <summary>
        /// mynaui-solid-yen
        /// Unicode: U+eeef
        /// </summary>
        MynauiSolidYen = 0xEEEF,

        /// <summary>
        /// mynaui-solid-yen-circle
        /// Unicode: U+eee9
        /// </summary>
        MynauiSolidYenCircle = 0xEEE9,

        /// <summary>
        /// mynaui-solid-yen-diamond
        /// Unicode: U+eeea
        /// </summary>
        MynauiSolidYenDiamond = 0xEEEA,

        /// <summary>
        /// mynaui-solid-yen-hexagon
        /// Unicode: U+eeeb
        /// </summary>
        MynauiSolidYenHexagon = 0xEEEB,

        /// <summary>
        /// mynaui-solid-yen-octagon
        /// Unicode: U+eeec
        /// </summary>
        MynauiSolidYenOctagon = 0xEEEC,

        /// <summary>
        /// mynaui-solid-yen-square
        /// Unicode: U+eeed
        /// </summary>
        MynauiSolidYenSquare = 0xEEED,

        /// <summary>
        /// mynaui-solid-yen-waves
        /// Unicode: U+eeee
        /// </summary>
        MynauiSolidYenWaves = 0xEEEE,

        /// <summary>
        /// mynaui-solid-zap
        /// Unicode: U+eef1
        /// </summary>
        MynauiSolidZap = 0xEEF1,

        /// <summary>
        /// mynaui-solid-zap-off
        /// Unicode: U+eef0
        /// </summary>
        MynauiSolidZapOff = 0xEEF0,

        /// <summary>
        /// mynaui-solid-zero
        /// Unicode: U+eef8
        /// </summary>
        MynauiSolidZero = 0xEEF8,

        /// <summary>
        /// mynaui-solid-zero-circle
        /// Unicode: U+eef2
        /// </summary>
        MynauiSolidZeroCircle = 0xEEF2,

        /// <summary>
        /// mynaui-solid-zero-diamond
        /// Unicode: U+eef3
        /// </summary>
        MynauiSolidZeroDiamond = 0xEEF3,

        /// <summary>
        /// mynaui-solid-zero-hexagon
        /// Unicode: U+eef4
        /// </summary>
        MynauiSolidZeroHexagon = 0xEEF4,

        /// <summary>
        /// mynaui-solid-zero-octagon
        /// Unicode: U+eef5
        /// </summary>
        MynauiSolidZeroOctagon = 0xEEF5,

        /// <summary>
        /// mynaui-solid-zero-square
        /// Unicode: U+eef6
        /// </summary>
        MynauiSolidZeroSquare = 0xEEF6,

        /// <summary>
        /// mynaui-solid-zero-waves
        /// Unicode: U+eef7
        /// </summary>
        MynauiSolidZeroWaves = 0xEEF7,
    }

}
