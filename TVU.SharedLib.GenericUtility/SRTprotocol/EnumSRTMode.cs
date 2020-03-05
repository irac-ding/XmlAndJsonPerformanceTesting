namespace TVU.SharedLib.GenericUtility
{
    /// <summary>
    /// <see cref="https://github.com/FFmpeg/FFmpeg/blob/master/libavformat/libsrt.c" />
    /// </summary>
    public enum EnumSRTMode
    {
        FAILURE = -1,
        CALLER = 0,
        LISTENER = 1,
        RENDEZVOUS = 2,
    }
}
