using System;
using System.Windows.Media;

namespace CorpSphero
{
    public abstract class CorpSpheroCommand
    {
        /* Static indicies */
        private readonly byte CommandPrefix = 255;
        private const int ChecksumLength = 1,
                IndexStart1 = 0,
                IndexStart2 = 1,
                IndexDeviceId = 2,
                IndexCommand = 3,
                IndexCommandSequenceNo = 4,
                IndexCommandDataLength = 5,
                CommandHeaderLength = 6;


        public byte[] ToPacket()
        {
            byte[] data = PacketData;

            int dataLength = data != null ? data.Length : 0;
            int packetLength = dataLength + CommandHeaderLength + ChecksumLength;

            byte[] buffer = new byte[packetLength];
            byte checksum = 0;

            buffer[IndexStart1] = CommandPrefix;
            buffer[IndexStart2] = CommandPrefix;

            byte device_id = DeviceId;
            checksum = (byte)(checksum + device_id);
            buffer[IndexDeviceId] = device_id;

            byte cmd = CommandId;
            checksum = (byte)(checksum + cmd);
            buffer[IndexCommand] = cmd;

            int sequenceNumber = SequenceNumber;
            checksum = (byte)(checksum + sequenceNumber);
            buffer[IndexCommandSequenceNo] = (byte)(sequenceNumber);

            byte response_length = (byte)(dataLength + 1);
            checksum = (byte)(checksum + response_length);
            buffer[IndexCommandDataLength] = response_length;

            // Check if we need to calculate the checksum for the data we have added
            if (data != null)
            {
                // Calculate the checksum for the data (also add the data to the array)
                for (int i = 0; i < dataLength; i++)
                {
                    buffer[(i + CommandHeaderLength)] = data[i];
                    checksum = (byte)(checksum + data[i]);
                }
            }

            buffer[(packetLength - ChecksumLength)] = (byte)(checksum ^ 0xFFFFFFFF);

            return buffer;
        }

        public abstract byte[] PacketData { get; }

        public byte DeviceId { get; set; }
        public byte CommandId { get; set; }


        private static int _nextSequanceNumber = 0;
        private int? _sequenceNumber;
        public int SequenceNumber
        {
            get
            {

                if (_sequenceNumber.HasValue == false)
                {
                    _sequenceNumber = _nextSequanceNumber;
                    _nextSequanceNumber += 1;
                }
                return _sequenceNumber.Value;
            }
        }

        protected CorpSpheroCommand(byte deviceId, byte commandId)
        {
            DeviceId = deviceId;
            CommandId = commandId;
        }
    }

    public class FrontLedCommand : CorpSpheroCommand
    {
        public FrontLedCommand(int brightness)
            : base(2, 33)
        {
            Brightness = brightness;
        }


        public int Brightness { get; set; }

        public override byte[] PacketData
        {
            get
            {
                if (Brightness > 255 || Brightness < 0)
                    throw new ArgumentOutOfRangeException("Brightness must be between 255 and 0");

                byte[] data = new byte[1];
                data[0] = (byte)Brightness;

                return data;
            }
        }
    }

    public class ColorLedCommand : CorpSpheroCommand
    {
        public ColorLedCommand(System.Windows.Media.Color color)
            : base(2, 32)
        {
            Color = color;
        }

        public System.Windows.Media.Color Color { get; set; }


        public override byte[] PacketData
        {
            get
            {
                var data = new byte[3];

                data[0] = this.Color.R;
                data[1] = this.Color.G;
                data[2] = this.Color.B;

                return data;
            }
        }
    }

    public class RollCommand : CorpSpheroCommand
    {
        public RollCommand(int velocity, int heading, bool isStop)
            : base(2, 48)
        {
            Velocity = velocity;
            Heading = heading;
            IsStop = isStop;
        }

        public int Velocity { get; set; }

        public int Heading { get; set; }

        public bool IsStop { get; set; }

        public override byte[] PacketData
        {
            get
            {
                if (Velocity < 0 || Velocity > 255)
                    throw new ArgumentOutOfRangeException("Velocity must be between 0-255");
                if (Heading < 0 || Heading > 359)
                    throw new ArgumentOutOfRangeException("Heading must be between 0-359");

                byte[] data = new byte[4];

                data[0] = (byte)(int)(this.Velocity);
                data[1] = (byte)((int) (this.Heading >> 8));
                data[2] = (byte)(int)this.Heading;
                data[3] = (byte)(this.IsStop ? 0 : 1);

                return data;

            }
        }
    }

    public class HeadingCommand : CorpSpheroCommand
    {
        public HeadingCommand(int heading) : base(2, 1)
        {
            Heading = heading;
        }

        public int Heading { get; set; }


        public override byte[] PacketData
        {
            get { 
                if (Heading < 0 || Heading > 359)
                    throw new ArgumentOutOfRangeException("Heading must be between 1-359");

                byte[] data = new byte[2];
                data[0] = (byte)((int)this.Heading >> 8);
                data[1] = (byte)(int)this.Heading;

                return data;

            }
        }
    }

    public class RawMotorCommand : CorpSpheroCommand
    {
        public RawMotorCommand(RawMotorDirection leftMotorDirection, int leftMotorSpeed, RawMotorDirection rightMotorDirection, int rightMotorSpeed) : base(2, 51)
        {
            LeftMotorDirection = leftMotorDirection;
            LeftMotorSpeed = leftMotorSpeed;
            RightMotorDirection = rightMotorDirection;
            RightMotorSpeed = rightMotorSpeed;
        }

        public RawMotorDirection LeftMotorDirection { get; set; }
        public int LeftMotorSpeed { get; set; }
        public RawMotorDirection RightMotorDirection { get; set; }
        public int RightMotorSpeed { get; set; }

        public enum RawMotorDirection
        {
            Forward = 1,
            Reverse = 2
        }

        public override byte[] PacketData
        {
            get 
            {
                byte[] data = new byte[4];

                data[0] = (byte) (int) this.LeftMotorDirection;
                data[1] = (byte)this.LeftMotorSpeed;
                data[2] = (byte) (int) this.RightMotorDirection;
                data[3] = (byte)this.RightMotorSpeed;

                return data;
            }
        }
    }

    public class SpinRightCommand : RawMotorCommand
    {
        public SpinRightCommand(int spinSpeed) 
            : base(RawMotorDirection.Reverse, spinSpeed, RawMotorDirection.Forward, spinSpeed)
        {
        }
    }

    public class SpinLeftCommand : RawMotorCommand
    {
        public SpinLeftCommand(int spinSpeed)
            : base(RawMotorDirection.Forward, spinSpeed, RawMotorDirection.Reverse, spinSpeed)
        {
        }
    }
}
