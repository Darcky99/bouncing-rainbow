using System;

namespace KobGamesSDKSlim
{
    public enum eAudioSource
    {
        Music,
        Main,
        Secondary
    }

    [Flags]
    public enum eMessageBoxButtons
    {
        None = 1,
        All = 2,
        Ok = 4,
        Yes = 8,
        No = 16,
        Next = 32
    }

    public enum Language
    {
        English = 1,
        Spanish = 2,
        German = 3
    }

    public enum eMouseState
    {
        Up,
        Down
    }
}