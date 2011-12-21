using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Net.Sockets;
using System.Text;

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        // プレビューフレームの解析を行うタイマー
        System.Windows.Threading.DispatcherTimer readTimer = null;

        UdpAnySourceMulticastClient myClient = new UdpAnySourceMulticastClient( IPAddress.Parse( "224.100.0.1" ), 9050 );

        // コンストラクター
        public MainPage()
        {
            InitializeComponent();

            // 100ミリ秒毎に満了する様にタイマーを開始
            readTimer = new System.Windows.Threading.DispatcherTimer();
            readTimer.Tick += new EventHandler( readTimer_Tick );
            readTimer.Interval = TimeSpan.FromMilliseconds( 100 );
            //readTimer.Start();

            try {
                canvas1.Width = width;
                canvas1.Height = height;

                myClient.BeginJoinGroup(
                    result =>
                    {
                        try {
                            myClient.EndJoinGroup( result );
                            myClient.MulticastLoopback = true;
                        }
                        catch ( Exception ex ) {
                            MessageBox.Show( "Join succeeded. but something wrong. " + ex.Message );
                        }
                    }, null
                );

                Receive();
            }
            catch ( Exception ex ) {
                MessageBox.Show( "Join failed. " + ex.Message );
            }
        }

        int count = 0;
        //const int width = 640 / 8;
        //const int height = 480 / 8;
        //byte[] receiveBuffer = new byte[width * height * 4];
        const int width = 640;
        const int height = 480;
        byte[] receiveBuffer = new byte[20 * 2 * 4];
        int[] skeleton = new int[20 * 2];

        private void Receive()
        {
            myClient.BeginReceiveFromGroup( receiveBuffer, 0, receiveBuffer.Length,
                result =>
                {
                    try {
                        IPEndPoint source;
                        myClient.EndReceiveFromGroup( result, out source );
                        string sourceIPAddress = source.Address.ToString();

                        Dispatcher.BeginInvoke(() =>
                            {
                                count++;
                                textBlock1.Text = count.ToString();

                                //var b = new WriteableBitmap( width, height );
                                //for ( int i = 0; i < b.Pixels.Length; i++ ) {
                                //    int index = i * 4;
                                //    b.Pixels[i] = receiveBuffer[index + 0] << 24 | 
                                //                  receiveBuffer[index + 1] << 16 |
                                //                  receiveBuffer[index + 2] << 8 |
                                //                  receiveBuffer[index + 3];
                                //}
                                //image1.Source = b;

                                var b = new WriteableBitmap( width, height );

                                for ( int i = 0; i < skeleton.Length; i++ ) {
                                    int index = i * 4;
                                    skeleton[i] = receiveBuffer[index + 3] << 24 | 
                                                  receiveBuffer[index + 2] << 16 |
                                                  receiveBuffer[index + 1] << 8 |
                                                  receiveBuffer[index + 0];
                                }

                                canvas1.Children.Clear();
                                for ( int i = 0; i < skeleton.Length; i += 2 ) {
                                    int x = i;
                                    int y = i + 1;

                                    //b.Pixels[y * width + x] = 0xFFFFFFF;
                                    var e = new Ellipse()
                                    {
                                        Fill = new SolidColorBrush()
                                        {
                                            Color = Colors.Red
                                        },
                                        Width = 10,
                                        Height = 10,
                                    };

                                    Canvas.SetLeft( e, skeleton[x] );
                                    Canvas.SetTop( e, skeleton[y] );
                                    canvas1.Children.Add( e );
                                }
                            } );

                        Receive();
                    }
                    catch ( Exception ex ) {
                        Receive();

                        Dispatcher.BeginInvoke( () =>
                        {
                            MessageBox.Show( "recv error " + ex.Message );
                        } );
                    }
                },
                null
            );
        }

        void readTimer_Tick( object sender, EventArgs e )
        {
            var wb = new WriteableBitmap( 640, 480 );
            int[] pixel = new int[640*480];
            pixel.CopyTo( wb.Pixels, 0 );
            //image1.Source = wb;
        }
    }
}