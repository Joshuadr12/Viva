using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static void MuteMusic() { MusicSync.sync.ToggleMute(); }
    public static void QuitGame() { Application.Quit(); }
}