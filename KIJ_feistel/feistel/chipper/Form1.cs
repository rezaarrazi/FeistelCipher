using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace feistel
{
    public partial class Form1 : Form
    {
        int mode = 0;       //Variabel mode encode/decode
        public Form1()
        {
            InitializeComponent();
        }

        #region Feistel

        private static byte Encrypt(byte msg, Func<byte, byte, byte> FunctionF, byte[] keys)
        {
            byte step = msg;
            for (int i = 0; i < keys.Length; i++)
            {
                step = FeistelStep(step, keys[i], FunctionF);
            }

            return step;
        }

        private static byte Decrypt(byte msg, Func<byte, byte, byte> FunctionF, byte[] keys)
        {
            byte step = msg;
            step = InversionLR(step);
            for (int i = keys.Length - 1; i >= 0; i--)
            {
                step = FeistelStep(step, keys[i], FunctionF);
            }
            step = InversionLR(step);

            return step;
        }

        private static byte FunctionF(byte x, byte key)
        {
            return Xor(x, key);
        }

        private static byte FeistelStep(byte msg, byte key, Func<byte, byte, byte> FunctionF)
        {
            var R = GetR(msg);
            var L = GetL(msg);

            var funcResult = OperateR(R, key, FunctionF);

            var xorResult = OperateL(L, funcResult, Xor);

            var finalResult = InversionLR(xorResult, R);

            return finalResult;
        }

        #endregion

        #region Bit Manipulation

        private static byte Xor(byte x, byte y)
        {
            return (byte)((int)x ^ (int)y);
        }

        private static byte GetR(byte x)
        {
            var temp = (byte)(((int)x) << 4);
            return (byte)(((int)temp) >> 4);
        }

        private static byte GetL(byte x)
        {
            var temp = (byte)(((int)x) >> 4);
            return (byte)(((int)temp) << 4);
        }

        private static byte InversionLR(byte l, byte r)
        {
            l = (byte)(((int)l) >> 4);
            r = (byte)(((int)r) << 4);
            return Xor(r, l);
        }

        private static byte InversionLR(byte msg)
        {
            var R = GetR(msg);
            var L = GetL(msg);
            return InversionLR(L, R);
        }

        private static byte OperateL(byte l, byte key, Func<byte, byte, byte> function)
        {
            key = (byte)(((int)key) << 4);
            return function(l, key);
        }

        private static byte OperateR(byte r, byte key, Func<byte, byte, byte> function)
        {
            return function(r, key);
        }

        #endregion

        #region Util

        private static void PrintOutByte(string initialText, byte x)
        {
            var temp = (int)x;
            int bits = 0;
            int factor = 1;
            for (int i = 7; i >= 0; i--)
            {
                bits += (temp % 2) * factor;
                factor *= 10;
                temp /= 2;
            }
            Console.WriteLine(initialText + "\t" + bits.ToString("00000000") + "\n");
        }

        private static byte EnsureKeyHas4Bits(byte key)
        {
            return (byte)((int)key % 16);
        }

        #endregion

        #region Files

        private static void EncryptFile(string fileInPath, Func<byte, byte, byte> FunctionF, byte[] keys)
        {
            byte[] file = File.ReadAllBytes(fileInPath);
            byte[] encFile = new byte[file.Length];
            for (int i = 0; i < file.Length; i++)
            {
                encFile[i] = Encrypt(file[i], FunctionF, keys);
            }
            //string someString = Encoding.ASCII.GetString(encFile);
            //Console.WriteLine("encrypt: " + someString);
            MessageBox.Show("Encode complete!\nSelect directory to save file.");
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.FilterIndex = 1;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, encFile);
                    //File.WriteAllText(sfd.FileName, "");
                }
            }
        }

        private static void DecryptFile(string fileInPath, Func<byte, byte, byte> FunctionF, byte[] keys)
        {
            byte[] file = File.ReadAllBytes(fileInPath);
            byte[] decFile = new byte[file.Length];
            for (int i = 0; i < file.Length; i++)
            {
                decFile[i] = Decrypt(file[i], FunctionF, keys);
            }
            MessageBox.Show("Decode complete!\nSelect directory to save file.");
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.FilterIndex = 1;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, decFile);
                    //File.WriteAllText(sfd.FileName, "");
                }
            }
        }

        #endregion


        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
                MessageBox.Show("Fill the key!");
            else if (string.IsNullOrWhiteSpace(textBox1.Text))
                MessageBox.Show("No string to encode!");
            else
            {
                string key = textBox2.Text;   //Variabel kata kunci
                byte[] keys = Encoding.ASCII.GetBytes(key);
                
                if (mode == 0)
                {
                    EncryptFile(textBox1.Text, FunctionF, keys);
                }
                else
                {
                    DecryptFile(textBox1.Text, FunctionF, keys);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (mode == 0)
            {
                mode = 1;
                button4.Text = "Chiper To Plain Text";
                label1.Text = "Chiper text";
                //label3.Text = "Plain text";
                button3.Text = "Decode";
            }
            else
            {
                mode = 0;
                button4.Text = "Plain Text To Chiper";
                label1.Text = "Plain text";
                //label3.Text = "Chiper text";
                button3.Text = "Encode";
            }
            //richTextBox1.Text = "";
            //richTextBox2.Text = "";
        }

    }
}
