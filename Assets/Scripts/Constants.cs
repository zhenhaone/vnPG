using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    public static string STORY_PATH = "Assets/Resources/story/";
    public static string EXCEL_FILE_EXTENSION = ".xlsx";
    public static string DEFAULT_STORY_FILE_NAME = "1";
    public static int DEFAULT_START_LINE = 1;

    public static string AVATAR_PATH = "image/avatar/";
    public static string BACKGROUND_PATH = "image/background/";
    public static string BUTTON_PATH = "image/UI/";
    public static string CHARACTOR_PATH = "image/charactor/";
    public static string IMAGE_LOAD_FAILED = "Failed to load image: ";

    public static float DEFAULT_TYPING_SPEED = 0.05f;
    public static float SKIP_MODE_TYPING_SPEED = 0.01f;

    public static string AUTO_ON = "auto";
    public static string AUTO_OFF = "stop";
    public static float DEFAULT_AUTO_WAITING_SECONDS = 0.1f;

    public static string SKIP_ON = "1";
    public static string SKIP_OFF = "2";
    public static float DEFAULT_SKIP_WAITTING_SECONDS = 0.02f;

    public static string VOCAL_PATH = "audio/vocal/";
    public static string MUSIC_PATH = "audio/music/";
    public static string AUDIO_LOAD_FAILED = "Failed to load audio: ";
    public static string MUSIC_LOAD_FAILED = "Failed to load music";

    

    public static string NO_DATA_FOUND = "No data found";
    public static string END_OD_STORY = "EOF";
    public static string CHOICE = "choice";
    public static float DEFAULT_WAITING_SECONDS = 0.1f;

    public static string charactorActionAppearAt = "appearAt";
    public static string charactorActionDisappear = "disappear";
    public static string charactorActionMoveTo = "moveTo";
    public static int DURATION_TIME = 1;
    public static string COORDINATE_MISSING = "Coordinate missing";

    public static int DEFAULT_START_INDEX = 0;
    public static int SLOTS_PER_PAGE = 8;
    public static int TOTAL_SLOTS = 40;
    public static string COLON = ": ";
    public static string SAVE_GAME = "save_game";
    public static string LOAD_GAME = "load_game";
    public static string EMPTY_SLOT = "empty_slot";

    public static string CAMERA_NOT_FOUND = "Main camera not found";
    public static string SAVE_FILE_PATH = "saves";
    public static string SAVE_FILE_EXTENSION = ".json";

    public static string X = "x";

    public static string GOTO = "goto";
    public static string APPEAR_AT_INSTANTLY = "appearAtInstantly";
    public static string NEW_STORY_FILE_NAME = "11";
    public static int MAX_LENGTH = 50;

    public static char ChoiceDelimeter = '\n';
}
