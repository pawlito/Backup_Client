using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
namespace NASClientTCP
{
    public partial class Form1 : Form, IbytesConvertable, IhashGenerator
    {
        private TcpClient _client;

        private StreamReader _sReader;
        private StreamWriter _sWriter;
        private NetworkStream _ns;

        private Boolean _isConnected;
        private FileInfo file;
        private FileStream fileStream;
        private FileStream fileStream2;
        private FileStream fileStream3;
        int portNum = 5555;
        string ipAddress = "127.0.0.1";
        private DbWrapper db;
        List<string> toBackup = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CompareItems ci = new CompareItems();
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            db = new DbWrapper(@"C:\Users\paweł\Documents\Visual Studio 2015\Projects\NASClientTCP\NASClientTCP\bin\filesInfo.sqlite");
            DirectoryInfo di = new DirectoryInfo(@"C:\Users\paweł\Desktop\test");
            FileInfo[] files = di.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                string hash = "";
                Boolean state = true;
                int compResult = int.MinValue;
                Hashtable row = new Hashtable();
                row = db.SelectRow(files[i].Name.ToString());
      
                if (row.ContainsKey("fileName") && row.ContainsValue(files[i].Name.ToString()))
                {
                    fileStream3 = new FileStream(@"C:\Users\paweł\Desktop\test\" + files[i].Name.ToString(), FileMode.OpenOrCreate,
                    FileAccess.Read);
                    hash = GetChecksumBuffered(fileStream3);
                    fileStream3.Close();
                    state = ci.CompareStrings(row["checksum"].ToString(), hash);
                    compResult = ci.CompareDates(files[i].LastWriteTime.ToString(), row["modified"].ToString());
                    if (!state && compResult == 1)
                    {
                        toBackup.Add(files[i].Name.ToString());
                    }
                }
                else
                {
                    toBackup.Add(files[i].Name.ToString());
                }
            }
            if (toBackup.Count > 0)
            {
                string message = Helpers.BuildMessage(toBackup);
                MessageBox.Show("pliki wymagajace wykonania kopii: \n" + message);
            }
            else
            {
                DialogResult result = MessageBox.Show(" brak zmian w plikach\n kopia zapasowa nie jest wymagana\n Czy chcesz ją wykonać ręcznie?\n", "komunikat",
                   MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    RunManualBackup();
                }
                else if (result == DialogResult.No)
                {
                    MessageBox.Show(" kopia nie została wykonana\n anulowano", "komunikat",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (result == DialogResult.Cancel)
                {
                    //code for Cancel
                }
                //Run();
            }

        }
        public void Run()
        {
            if (toBackup.Count > 0)
            {
                for (int i = 0; i < toBackup.Count; i++)
                {
                    string directory = @"C:\Users\paweł\Desktop\test\";
                    _client = new TcpClient();
                    _client.Connect(ipAddress, portNum);
                    HandleCommunication(toBackup[i].ToString(), directory);
                    _client.Close();
                }
            }
            /*DirectoryInfo di = new DirectoryInfo(@"C:\Users\paweł\Desktop\test");
            FileInfo[] files = di.GetFiles();
            for (int i = 0; i < 5; i++)
            {
                string directory = @"C:\Users\paweł\Desktop\test";
                _client = new TcpClient();
                _client.Connect(ipAddress, portNum);
                HandleCommunication(files[i].Name.ToString(), directory);
                _client.Close();
            }*/


        }

        public void RunManualBackup()
        {
            DirectoryInfo di = new DirectoryInfo(@"C:\Users\paweł\Desktop\test");
            FileInfo[] files = di.GetFiles();
            for (int i = 0; i < 5; i++)
            {
                string directory = @"C:\Users\paweł\Desktop\test";
                _client = new TcpClient();
                _client.Connect(ipAddress, portNum);
                HandleCommunication(files[i].Name.ToString(), directory);
                _client.Close();
            }


        }
        public void HandleCommunication(string fileName2, string directory)
        {
            string hash = "";
            try
            {
                file = new FileInfo(directory + fileName2);
                fileStream = file.OpenRead();
            }
            catch
            {
                //MessageBox.Show("Error opening file");
                //resetControls();
                return;
            }
            _sReader = new StreamReader(_client.GetStream(), Encoding.ASCII);
            _sWriter = new StreamWriter(_client.GetStream(), Encoding.ASCII);
            //_ns = new NetworkStream(_client);
            _isConnected = true;

            NetworkStream ns = _client.GetStream();
            byte[] fileName = ASCIIEncoding.ASCII.GetBytes(file.Name);
            byte[] fileNameLength = BitConverter.GetBytes(fileName.Length);
            byte[] fileLength = BitConverter.GetBytes(file.Length);
            ns.Write(fileLength, 0, fileLength.Length);
            ns.Write(fileNameLength, 0, fileNameLength.Length);
            ns.Write(fileName, 0, fileName.Length);
            int read;
            int totalWritten = 0;
            byte[] buffer = new byte[32 * 1024]; // 32k chunks
            while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ns.Write(buffer, 0, read);
                totalWritten += read;
                //progressBar1.Value = (int)((100d * totalWritten) / file.Length);
            }
            fileStream.Close();
            fileStream2 = new FileStream(@"C:\Users\paweł\Desktop\test\" + fileName2, FileMode.OpenOrCreate,
                FileAccess.Read);
            hash = GetChecksumBuffered(fileStream2);
            fileStream2.Close();
            db.UpInsert("Files", fileName2, hash.ToString(), 0, file.LastWriteTime.ToString());
            ////////////////////////////////////////////////////////////////////////////////////////
            //str = "";
            
            _client.Close();
        }
        public byte[] GetBytesFromString(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public string GetString(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
        public string GetChecksumBuffered(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(bufferedStream);
                stream.Flush();
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Run();
        }

        private async void pingserver(object sender, EventArgs e)
        {
            Progress<string> progress = new Progress<string>(text => listView1.Items.Add(text));
            PingServer ps = new PingServer(progress);
            await ps.TestPing();
        }
    }
}
