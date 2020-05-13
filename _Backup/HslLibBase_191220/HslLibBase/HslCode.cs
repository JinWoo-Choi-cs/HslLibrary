using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Drawing;
using System.Data;
using MySql.Data.MySqlClient;

namespace HslBase
{
    namespace HDatabase
    {
        public class HslMySql
        {
            private string ipAddress { get; set; } = "127.0.0.1";
            private string Database { get; set; }
            private string Uid { get; set; }
            private string Pwd { get; set; }
            private string Charset { get; set; } = "UTF8";

            private string BasestrConn = "Server={0};Database={1};Uid={2};Pwd={3};Charset={4}";

            private string strConn = "";

            public HslMySql() { }

            public HslMySql(string _ipAddress, string _Database, string _Uid, string _Pwd, string _Charset = "UTF8")
            {
                ipAddress = _ipAddress;
                Database = _Database;
                Uid = _Uid;
                Pwd = _Pwd;
                Charset = _Charset;

                InitializeDB();
            }

            public bool InitializeDB()
            {
                if ((ipAddress == "") || (Database == "") || (Uid == ""))
                {
                    return false;
                }

                strConn = string.Format(BasestrConn, ipAddress, Database, Uid, Pwd, Charset);

                return true;
            }

            public bool InitializeDB(string _ipAddress, string _Database, string _Uid, string _Pwd, string _Charset = "UTF8")
            {
                ipAddress = _ipAddress;
                Database = _Database;
                Uid = _Uid;
                Pwd = _Pwd;
                Charset = _Charset;

                if ((ipAddress == "") || (Database == "") || (Uid == ""))
                {
                    return false;
                }

                strConn = string.Format(BasestrConn, ipAddress, Database, Uid, Pwd, Charset);

                return true;
            }

            public ConnectionState GetConnectionState()
            {
                MySqlConnection conn = new MySqlConnection(strConn);
                return conn.State;
            }

            /// <summary>
            /// 테이블 이름으로 조회한 내용을 전부 반환한다. Offset, Limit 설정 옵션 
            /// </summary>
            /// <param name="_table_name">조회할 테이블 이름</param>
            /// <param name="_offset">조회할 행 Offset 설정</param>
            /// <param name="_limitNum">조회할 행 갯수 설정</param>
            /// <returns></returns>
            public DataTable Extract_Table(string _table_name, int _offset = - 1, int _limitNum = - 1)
            {
                DataTable _dt = new DataTable();

                string _limitOffset = "";

                if ( (_offset == -1 || _limitNum == -1) == false)
                {
                    _limitOffset = $"limit {_offset} , {_limitNum}";
                }

                string sql = $"select * from {_table_name} {_limitOffset} ;";

                using (MySqlConnection _conn = new MySqlConnection(strConn))
                {
                    using (MySqlDataAdapter adpt = new MySqlDataAdapter(sql, _conn))
                    {
                        try
                        {
                            _conn.Open();
                            adpt.Fill(_dt);
                            _conn.Close();

                            return _dt;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            return null;
                        }
                    }
                }
            }

            // 쿼리문 실행
            public void Execute_NonQuery(string _query)
            {
                using (MySqlConnection _conn = new MySqlConnection(strConn))
                {
                    using (MySqlCommand _cmd = new MySqlCommand(_query, _conn))
                    {
                        try
                        {
                            _conn.Open();
                            _cmd.ExecuteNonQuery();
                            _conn.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }

            // 쿼리문 전체로 DataTable 추출
            public DataTable Extract_Data(string _query)
            {
                DataTable _dt = new DataTable();
                string sql = _query;

                using (MySqlConnection _conn = new MySqlConnection(strConn))
                {
                    using (MySqlDataAdapter adpt = new MySqlDataAdapter(sql, _conn))
                    {
                        try
                        {
                            _conn.Open();
                            adpt.Fill(_dt);
                            _conn.Close();

                            return _dt;
                        }

                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            _conn.Dispose();

                            return null;
                        }
                    }
                }

            }
        }

        

    
    }

    namespace HFormTool
    {
        // 경계선 없는 버튼
        public class HslBorderlessButton : Button
        {
            protected override void OnPaint(PaintEventArgs pevent)
            {
                base.OnPaint(pevent);
                pevent.Graphics.DrawRectangle(new Pen(this.BackColor, 10), this.ClientRectangle);
            }
        }

        public class HslForm
        {
            // 탭 컨트롤 탭부분 보이지 않게 하기
            private static void HideAllTabsOnTabControl(TabControl theTabControl)
            {
                theTabControl.Appearance = TabAppearance.FlatButtons;
                theTabControl.ItemSize = new Size(0, 1);
                theTabControl.SizeMode = TabSizeMode.Fixed;
            }

            // Find Control from Parent
            public static Control FindControl(Control parent, string ctlName)
            {
                foreach (Control ctl in parent.Controls)
                {
                    if (ctl.Name.Equals(ctlName))
                    {
                        return ctl;
                    }

                    FindControl(ctl, ctlName);
                }
                return null;
            }
        }


    }

    namespace HDatetime
    {
        public class HslTimeSpan
        {
            // Calculate TimeSpan 
            public static int TimeSpanToSecond(TimeSpan _ts)
            {
                return _ts.Seconds + _ts.Minutes * 60 + _ts.Hours * 60 * 60 + _ts.Days * 60 * 60 * 24;
            }
        }
    }

    namespace HData
    {
        public class HslData
        {
            // ListView to CSV Export
            public static void ListViewToCSV(ListView _listView, string _path, string _fileName, bool _includeHidden, Encoding _encoding = null)
            {
                if (!_path.EndsWith("\\"))
                {
                    _path = _path + "\\";
                }

                if (_encoding == null)
                    _encoding = Encoding.UTF8;

                string _filePath = _path + "\\" + _fileName;

                CreateFolderIfNotExists(_path);

                //make header string
                StringBuilder result = new StringBuilder();

                WriteCSVRow(result, _listView.Columns.Count, i => _includeHidden || _listView.Columns[i].Width > 0, i => _listView.Columns[i].Text);

                //export data rows
                foreach (ListViewItem listItem in _listView.Items)
                    WriteCSVRow(result, _listView.Columns.Count, i => _includeHidden || _listView.Columns[i].Width > 0, i => listItem.SubItems[i].Text);

                File.WriteAllText(_filePath, result.ToString(), Encoding.UTF8);
            }

            // Write Stream Row
            private static void WriteCSVRow(StringBuilder result, int itemsCount, Func<int, bool> isColumnNeeded, Func<int, string> columnValue)
            {
                bool isFirstTime = true;
                for (int i = 0; i < itemsCount; i++)
                {
                    if (!isColumnNeeded(i))
                        continue;

                    if (!isFirstTime)
                        result.Append(",");
                    isFirstTime = false;

                    result.Append(String.Format("\"{0}\"", columnValue(i)));
                }

                result.AppendLine();
            }

            // Get Configfile value by name
            public static string GetAppConfig(string key)
            {
                return ConfigurationManager.AppSettings[key];
            }

            // Set Configfile value
            public static void SetAppConfig(string key, string value)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection cfgCollection = config.AppSettings.Settings;

                cfgCollection.Remove(key);
                cfgCollection.Add(key, value);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }

            // Add Configfile Key, Value
            public static void AddAppConfig(string key, string value)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection cfgCollection = config.AppSettings.Settings;

                cfgCollection.Add(key, value);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }

            // Remove Configfile by key
            public static void RemoveAppConfig(string key)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection cfgCollection = config.AppSettings.Settings;

                try
                {
                    cfgCollection.Remove(key);

                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                }
                catch { }
            }

            // Create Folder If Not Exists
            public static void CreateFolderIfNotExists(string _filePath)
            {
                DirectoryInfo di = new DirectoryInfo(_filePath);  //Create Directoryinfo value by sDirPath  

                if (di.Exists == false)   //If New Folder not exits  
                {
                    di.Create();             //create Folder  
                }
            }
        }
    }

    namespace HNet
    {
        public class HslNet
        {
            // Check IP Address(String) Format
            public static bool isIPAddressFormat(string _str)
            {
                string _pattern = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

                if (Regex.IsMatch(_str, _pattern))
                {
                    return true;
                }
                return false;
            }
        }
    }

    public class HslCode
    {
        // Make Int Array
        public static int[] CreateNumericArray(int min, int max)
        {
            int[] tmpArr = new int[max - min + 1];

            int k = 0;
            for (int i = min; i <= max; i++)
            {
                tmpArr[k] = i;
                k++;
            }

            return tmpArr;
        }

        // GetUniCode String
        public static string GetUnicodeString(string input, int _codePageNumber = 60001)
        {
            Encoding encoding = Encoding.GetEncoding(_codePageNumber);

            var bytest = encoding.GetBytes(input);
            var output = encoding.GetString(bytest);
            List<string> unicodes = new List<string>();
            string result = String.Empty;

            if (input != output)
            {
                for (int i = 0; i < input.Length; i += char.IsSurrogatePair(input, i) ? 2 : 1)
                {
                    int codepoint = char.ConvertToUtf32(input, i);
                    unicodes.Add(String.Format("&#{0}", codepoint));
                }
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i].ToString() != output[i].ToString())
                    {
                        result += unicodes[i];
                    }
                    else
                    {
                        result += input[i];
                    }
                }
            }
            else result = input;

            return result;
        }
        
        // Trim '0' char from number(string)
        public static string TrimZero(string _tmpString, char _deli)
        {
            int _tmpInt = 0;

            string[] _arrStr = _tmpString.Split(_deli);
            string _retVal = "";

            int cnt = _arrStr.Count();
            for (int i = 0; i < cnt; i++)
            {
                if (int.TryParse(_arrStr[i], out _tmpInt))
                {
                    _retVal += _tmpInt.ToString();
                }
                else
                {
                    _retVal += _arrStr[i];
                }

                if (i < cnt - 1)
                {
                    _retVal += _deli.ToString();
                }
            }
            return _retVal;
        }

        // Trim '0' char from number(string) left
        public static string TrimZero_Left(string _tmpString, char _deli)
        {
            int _tmpInt = 0;

            string[] _arrStr = _tmpString.Split(_deli);
            string _retVal = "";

            int cnt = _arrStr.Count();
            for (int i = 0; i < cnt; i++)
            {
                if (int.TryParse(_arrStr[i], out _tmpInt))
                {
                    if (i == 0)
                        _retVal += _tmpInt.ToString();
                    else
                        _retVal += _arrStr[i];
                }
                else
                {
                    _retVal += _arrStr[i];
                }

                if (i < cnt - 1)
                {
                    _retVal += _deli.ToString();
                }
            }
            return _retVal;
        }

        //개발 중
        public static void Browsing()
        {
            using (FolderBrowserDialog _fbd = new FolderBrowserDialog())
            {
                _fbd.SelectedPath = "";
                DialogResult _result = _fbd.ShowDialog();

                if (_result == DialogResult.OK && !string.IsNullOrWhiteSpace(_fbd.SelectedPath))
                {
                    string _path = _fbd.SelectedPath;
                }
            }
        }

    }
}
