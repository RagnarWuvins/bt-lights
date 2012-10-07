using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Text;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;

namespace BTLights
{
    public class CommandHandler
    {
        public event BTEvent ChannelRequest, GlobalRequest, ExternalRequest;
        public static int CommandCounter = 0;

        //|CLA |MOD |       ADR         |   VAL   |   CRC   | 
        //|0001|0101|0000|0000|0000|0001|0000|0000|0000|0006|

        private byte[] mRawCommand;
        private byte mClass;
        private byte mMode;
        private long mGlobalCommand;
        private int mAddress;
        private int mValue;
        private int mChecksum;
        private bool mInternalCommand = false;

        public CommandHandler()
        {
            this.mClass = 0;
            this.mMode = 0;
            this.mAddress = 0;
            this.mGlobalCommand = 0;
            this.mValue = 0;
            this.mChecksum = 0;
        }

        public void SplitCommand(object sender, BTEventArgs e)
        {
            mRawCommand = e.CommandRaw;
            Thread sc = new Thread(new ThreadStart(SplitCommand));
            sc.Start();
        }

        private void SplitCommand()
        {
            if (mRawCommand == null)
            {
                Program.THROW_ERROR(Constants.FW_ERRORS.CMD_ASSERT_FAIL);
                return;
            }
            BTEventArgs e = new BTEventArgs();
            if (mRawCommand.Length == Constants.C_LENGTH)
            {
                this.mClass = (byte)(mRawCommand[0] >> 4);
                this.mMode = (byte)(mRawCommand[0] & 0x0F);
                this.mAddress = ((int)mRawCommand[1] << 8) | (int)mRawCommand[2];
                this.mGlobalCommand = this.mAddress;
                this.mValue = mRawCommand[3];
                this.mChecksum = mRawCommand[0] + mRawCommand[3];
                // check if the command comes from the BT board or user. Unfortunately the
                // CRC can be bigger than a byte, that's why we add 0x100 for the checksum
                mInternalCommand = (mRawCommand[4] == this.mChecksum || (mRawCommand[4] + 0x100) == this.mChecksum) ? true : false;
            }
            if (!mInternalCommand)
            {
                Program.THROW_ERROR(Constants.FW_ERRORS.CMD_CORRUPT);
                e.CommandRaw = mRawCommand;
                ExternalRequest(this, e);
            }
            else
            {
                Debug.Print("SplitCommand:\nClass: " + mClass + "\nMode: " + mMode + "\nAddress: " +mAddress + "\nValue: " + mValue);
                
                CommandCounter++;                
                e.CommandClass = mClass;
                e.CommandAddress = mAddress;
                e.CommandMode = mMode;
                e.CommandRaw = mRawCommand;
                e.CommandValue = mValue;
                e.CommandChecksum = mChecksum;
                switch (mClass)
                {
                    case (int)Constants.CLASS.CC_CMD:
                        ChannelRequest(this, e);
                        break;
                    case (int)Constants.CLASS.GC_CMD:
                        GlobalRequest(this, e);
                        break;
                    case (int)Constants.CLASS.AT_CMD:
                        break;
                    case (int)Constants.CLASS.DP_CMD:
                        break;
                    default:
                        break;
                }
            }//if (!mInternalCommand)
        }//SplitCommand()
    }//public class CommandHandler
}
