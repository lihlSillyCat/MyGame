using System;

namespace War.Base
{
    public class SocketMessageReceived : EventArgs
    {
        public UInt16 PacketID
        {
            get;
            private set;
        }

        public byte[] Message
        {
            get;
            private set;
        }

        public UInt16 Length
        {
            get;
            private set;
        }

        public SocketMessageReceived(UInt16 packetId, byte[] data, UInt16 dataSize)
        {
            PacketID = packetId;
            Message = data;
            Length = dataSize;
        }
    }
}