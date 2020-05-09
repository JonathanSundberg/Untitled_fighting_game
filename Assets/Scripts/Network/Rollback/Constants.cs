namespace Network.Rollback
{
    public class Constants
    {
        public const int NULL_FRAME = -1;
        public const float MS_PER_FRAME = 1000 / 60f;
        public const float HIGHEST_REMOTE_PING = 333f;
        
        public const int   FRAME_BUFFER_SIZE  = (int) (HIGHEST_REMOTE_PING / MS_PER_FRAME) + 2;
        public const float FRAME_DELAY_FACTOR = MS_PER_FRAME / (1000 / MS_PER_FRAME);
    }
}