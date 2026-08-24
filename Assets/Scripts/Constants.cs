using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    public static string STORY_PATH = "Assets/Resources/story/";
    public static string DEFAULT_STORY_FILE_NAME = "1.xlsx";
    public static int DEFAULT_START_LINE = 1;

    public static string AVATAR_PATH = "image/avatar/";
    public static string VOCAL_PATH = "audio/vocal";
    public static string AUDIO_LOAD_FAILED = "Failed to load audio: ";
    public static string IMAGE_LOAD_FAILED = "Failed to load image: ";

    public static string BACKGROUND_PATH = "image/background/";
    public static string MUSIC_PATH = "audio/music";
    public static string MUSIC_LOAD_FAILED = "Failed to load music";

    public static string NO_DATA_FOUND = "No data found";
    public static string END_OD_STORY = "End of story";
    public static float DEFAULT_WAITING_SECONDS = 0.05f;
}
