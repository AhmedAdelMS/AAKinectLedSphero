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
using System.ComponentModel;



namespace AAKinectLedSphero
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        SpheroConnector spheroConnector;
        Sphero sphero = null;
        string currentMessage;

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker.DoWork +=              backgroundWorker_DoWork;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerCompleted +=  backgroundWorker_RunWorkerCompleted;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TBLog.Text = currentMessage; 
        }
        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!(e.Error == null))
            {
                updateStatus("Error: " + e.Error.Message);
            }
        }

        
        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            // Do the heavy lifting here
            worker.ReportProgress(1);
            updateStatus(" Initializing the Sphero Connector");
            spheroConnector = new SpheroConnector(updateStatus);
            updateStatus("Scanning for Sphero");
            spheroConnector.Scan(updateStatus);
            int spheroIndex = 0;
            bool spheroFound = false;

            try
            {
                var deviceNames = spheroConnector.DeviceNames;
                for (int i = 0; i < deviceNames.Count; i++)
                {
                    updateStatus("Attempt " + i + " to connect to Sphero");
                    if (deviceNames[i].Contains("Sphero"))
                    {
                        updateStatus("\n Sphero Found");
                        spheroIndex = i;
                        spheroFound = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                updateStatus(" Exception:\n" + ex.Message);
            }
            if (spheroFound)
            {
                try
                {
                    sphero = spheroConnector.Connect(spheroIndex);
                    updateStatus("\n Sphero Connected"); 
                }
                catch (Exception ex)
                {
                    updateStatus(" Exception:\n" + ex.Message);
                }
            }

        }
        void updateStatus(string text)
        {
            currentMessage = text;
            backgroundWorker.ReportProgress(1);
        }
        


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (backgroundWorker.IsBusy != true)
            {
                backgroundWorker.RunWorkerAsync();
            }


        }
        string updateUI(string text)
        {
            currentMessage = text;
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
