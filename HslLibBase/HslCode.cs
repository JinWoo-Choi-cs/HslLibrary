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
using System.Xml;

namespace HslBase
{
    /// <summary>
    /// Database 통합 관리 (MySQL, ...
    /// </summary>
    namespace HDatabase
    {
        /// <summary>
        /// MySQL 클래스
        /// </summary>
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

            /// <summary>
            /// Connection String 초기화
            /// </summary>
            /// <returns>retval : 정상적으로 초기화 되었는지 체크</returns>
            public bool InitializeDB()
            {
                if ((ipAddress == "") || (Database == "") || (Uid == ""))
                {
                    return false;
                }

                strConn = string.Format(BasestrConn, ipAddress, Database, Uid, Pwd, Charset);

                return true;
            }

            /// <summary>
            /// Connection String 초기화
            /// </summary>
            /// <param name="_ipAddress">ipaddress</param>
            /// <param name="_Database"></param>
            /// <param name="_Uid"></param>
            /// <param name="_Pwd"></param>
            /// <param name="_Charset"></param>
            /// <returns>retval : 정상적으로 초기화 되었는지 체크</returns>
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

            /// <summary>
            /// 서버 접속체크
            /// </summary>
            /// <returns>retval : 서버 접속체크 결과</returns>
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
            /// <returns>조회 된 DataTable. 조회되지 않았다면 null</returns>
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

    /// <summary>
    /// WindowForm의 사용자 오브젝트
    /// </summary>
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
            /// <summary>
            ///  탭 컨트롤 탭부분 보이지 않게 하기
            /// </summary>
            /// <param name="theTabControl">제어할 TabControl</param>
            private static void HideAllTabsOnTabControl(TabControl theTabControl)
            {
                theTabControl.Appearance = TabAppearance.FlatButtons;
                theTabControl.ItemSize = new Size(0, 1);
                theTabControl.SizeMode = TabSizeMode.Fixed;
            }

            /// <summary>
            /// 상위 Control에서 이름으로 하위 Control 검색
            /// </summary>
            /// <param name="parent">상위 Control</param>
            /// <param name="ctlName">검색할 Control 이름</param>
            /// <returns></returns>
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

    /// <summary>
    /// DateTime 함수
    /// </summary>
    namespace HDatetime
    {
        public class HslTimeSpan
        {
            /// <summary>
            /// TimeSpan '초' 계산
            /// </summary>
            /// <param name="_ts"></param>
            /// <returns></returns>
            public static int TimeSpanToSecond(TimeSpan _ts)
            {
                return _ts.Seconds + _ts.Minutes * 60 + _ts.Hours * 60 * 60 + _ts.Days * 60 * 60 * 24;
            }
        }
    }

    /// <summary>
    /// 파일 입출력, Config 파일 관리 라이브러리
    /// </summary>
    namespace HData
    {
        public class HslData
        {
            /// <summary>
            /// Listview를 csv로 내보냄
            /// </summary>
            /// <param name="_listView">Listview Control</param>
            /// <param name="_path">csv 내보낼 경로</param>
            /// <param name="_fileName">csv 내보낼 파일 이름</param>
            /// <param name="_includeHidden">Column Width가 0이거나 false인 열을 포함시킬지 여부. default = false</param>
            /// <param name="_encoding">csv 내보낼 Encoding. default = UTF8</param>
            public static void ListViewToCSV(ListView _listView, string _path, string _fileName, bool _includeHidden = false, Encoding _encoding = null)
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

            /// <summary>
            /// Get Configfile value by name(key)
            /// </summary>
            /// <param name="key">config key</param>
            /// <returns>config value</returns>
            public static string GetAppConfig(string key)
            {
                return ConfigurationManager.AppSettings[key];
            }

            /// <summary>
            /// Set Config value
            /// </summary>
            /// <param name="key">config key</param>
            /// <param name="value">config value</param>
            public static void SetAppConfig(string key, string value)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection cfgCollection = config.AppSettings.Settings;

                cfgCollection.Remove(key);
                cfgCollection.Add(key, value);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }

            /// <summary>
            /// Add Config Key, Value
            /// </summary>
            /// <param name="key">new onfig key</param>
            /// <param name="value">new config value</param>
            public static void AddAppConfig(string key, string value)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection cfgCollection = config.AppSettings.Settings;

                cfgCollection.Add(key, value);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }

            /// <summary>
            /// Remove config by key
            /// </summary>
            /// <param name="key">삭제할 config key</param>
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

            /// <summary>
            /// Create Folder If Not Exists
            /// </summary>
            /// <param name="_filePath">해당 경로가 없다면 폴더 생성</param>
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

    namespace HXml
    {
        public class HslXml
        {
            public XmlDocument documnet { get; set; }
            public XmlNode root { get; set; }
            public XmlDeclaration declare { get; set; }
            public string version { get; set; }
            public string encoding{ get; set; }
            public bool standalone { get; set; }

            public HslXml(string path)
            {
                documnet.Load("C:\\Users\\Hasel\\Desktop\\xml\\pages\\page001.xml");

                root = documnet.FirstChild;

                // documnet.

                // version = declare.Attributes["version"].ToString();
                // encoding = declare.Attributes["encoding"].ToString();
                // standalone = (declare.Attributes["standalone"].ToString()) == "yes" ? true : false;
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
