using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;
using System.Net.Sockets;
using InTheHand.Net.Sockets;
using System.Threading;
using SpheroNET;



namespace AAKinectLedSphero
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SpheroConnector spheroConnector = new SpheroConnector();
        Sphero sphero = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TBLog.Text += "\n Scanning for Sphero";
            spheroConnector.Scan();
            int spheroIndex = 0;
            bool spheroFound = false;

            try
            {
                var deviceNames = spheroConnector.DeviceNames;
                for (int i = 0; i < deviceNames.Count; i++)
                {
                    if(deviceNames[i].Contains("Sphero"))
                    {
                        TBLog.Text += "\n Sphero FOund";
                        spheroIndex = i;
                        spheroFound = true;
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                TBLog.Text = " Exception:\n" + ex.Message;
            }
            if(spheroFound)
            {
                sphero = spheroConnector.Connect(spheroIndex);
                TBLog.Text += "\n Sphero COnnected";

            }


            /*
            String logEntry = "";
            logEntry += "\n Creating a BlueTooth client";
            BluetoothClient client = new BluetoothClient();
            List<BluetoothDeviceInfo> devices = new List<BluetoothDeviceInfo>();
            logEntry += "\n Getting the devices' info";
            devices.AddRange(client.DiscoverDevices());
            
            foreach (BluetoothDeviceInfo device in devices)
            {
                logEntry += "\n Processing " + device.DeviceName;
                if (device.DeviceName.Contains("Sphero"))
                {
                    logEntry += "\n Getting Address";
                    BluetoothAddress addr = device.DeviceAddress;
                    Guid serviceClass = BluetoothService.SerialPort;
                    logEntry += "\n Creating End Point";
                    var ep = new BluetoothEndPoint(addr, serviceClass);
                    try
                    {
                        int retries = 5;
                        for (int i = 0; i < retries; i++)
                        {
                            try
                            {
                                logEntry += "\n Trying to connect to " + device.DeviceName;
                                client.Connect(ep);
                                break;
                            }
                            catch (Exception ex)
                            {
                                logEntry += "\n Caught an exception" + ex.Message;
                                if (i == (retries-1))
                                    throw new Exception(string.Format("Could not connect after {0} retries", retries), ex);
                            }
                        }
                        logEntry += "\n and we have a connection :-) ";
                        NetworkStream stream = client.GetStream();
                        stream.ReadTimeout = 100;
                        logEntry += "\n GOt the stream :-) ";
                        
                    }
                    catch (Exception ex)
                    {
                        logEntry += "\n Exception " + ex.Message;
                        TBLog.Text = logEntry;
                    }
                }
                TBLog.Text = logEntry;
            }
            */
        }
        string updateUI(string text)
        {
            TBLog.Text = text;
            return text;
        }

        private void BSetColor_Click(object sender, RoutedEventArgs e)
        {
            sphero.SetRGBLEDOutput((byte)SLDRRed.Value,
                                   (byte)SLDRGreen.Value,
                                   (byte)SLDRBlue.Value);
        }

        private void BDisconnect_Click(object sender, RoutedEventArgs e)
        {
            spheroConnector.Close();
        }

        private void BRoll_Click(object sender, RoutedEventArgs e)
        {
            
            bool flag = false;
            bool isStop = false;
            SpheroCommandPacket rollPacket =  
                new SpheroCommandPacket(0x02, 
                                        0x30, 
                                        0x01, 
                                        new byte[] { (byte) SLDRVelocity.Value,        // Velocity 
                                                     (byte)((byte)(SLDRHeading.Value) >> 8),   // Heading
                                                     (byte) SLDRHeading.Value,
                                                     (byte) (isStop ? 0 : 1),
                                                     (flag ? (byte)0x01 : (byte)0x00) });
            sphero.SendPacket(rollPacket);
        }

        private void BStop_Click(object sender, RoutedEventArgs e)
        {
            bool flag = false;
            bool isStop = true;
            SpheroCommandPacket rollPacket =
                new SpheroCommandPacket(0x02,
                                        0x30,
                                        0x01,
                                        new byte[] { (byte) SLDRVelocity.Value,        // Velocity 
                                                     (byte)((byte)(SLDRHeading.Value) >> 8),   // Heading
                                                     (byte) SLDRHeading.Value,
                                                     (byte) (isStop ? 0 : 1),
                                                     (flag ? (byte)0x01 : (byte)0x00) });
            sphero.SendPacket(rollPacket);
        }
        /*private IBuffer GetBufferFromByteArray(byte[] package)
        {
            using (DataWriter dw = new DataWriter())
            {
                dw.WriteBytes(package);
                return dw.DetachBuffer();
            }
        }*/
    }
}
