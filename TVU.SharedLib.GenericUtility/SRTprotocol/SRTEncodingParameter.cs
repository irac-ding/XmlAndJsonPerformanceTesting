using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public class SRTDecodingParameter
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields and Properties

        private string _host = string.Empty;
        public string Host
        {
            get { return _host; }
            set
            {
                IPAddress IP;
                if (string.IsNullOrEmpty(value) && Mode != EnumSRTMode.LISTENER)
                {
                    logger.Error($"Current value of host is invalid. ");
                    throw new Exception("Value is invalid.");
                }
                else if (IPAddress.TryParse(value, out IP) || (string.IsNullOrEmpty(value) && Mode == EnumSRTMode.LISTENER))
                {
                    if (_host != value) _host = value;
                }
            }
        }

        public int Port { get; set; }

        [DefaultValue(typeof(EnumSRTMode), "CALLER")]
        public EnumSRTMode Mode { get; set; } = EnumSRTMode.CALLER;

        [DefaultValue(typeof(long), "")]
        public long? Listen_timeout { get; set; }

        [DefaultValue(typeof(long), "")]
        public long? Rw_timeout { get; set; }

        [DefaultValue(typeof(int), "65536")]
        public int? Recv_buffer_size { get; set; } = 65536;

        [DefaultValue(typeof(int), "")]
        public int? TsbpdDelay { get; set; }

        [DefaultValue(typeof(bool), "")]
        public bool? TlpktDrop { get; set; }

        [DefaultValue(typeof(long), "0")]
        public long? Maxbw { get; set; } = 0;

        [DefaultValue(typeof(int), "25600")]
        public int? FFS { get; set; } = 25600;

        [DefaultValue(typeof(int), "1316")]
        public int? Pkt_size { get; set; } = 1316;

        [DefaultValue(typeof(bool), "true")]
        public bool? NakReport { get; set; } = true;

        private int? _connect_Timeout = null;
        [DefaultValue(typeof(int), "3000")]
        public int? Connect_Timeout
        {
            get { return _connect_Timeout; }
            set
            {
                if (value < 3000)
                {
                    logger.Error($"The value of connect_Timeout(int) is invalid which should >3000 and the mode can't be listener.");
                    throw new Exception("Value is invalid.");
                }
                else if (Mode == EnumSRTMode.LISTENER)
                {
                    _connect_Timeout = null;
                }
                else if (_connect_Timeout != value) _connect_Timeout = value;
            }
        }

        private int? _mss = 1500;
        [DefaultValue(typeof(int), "1500")]
        public int? Mss
        {
            get { return _mss; }
            set
            {
                if (value > 1500)
                {
                    logger.Error($"The value of mss(int) is out of range which shoould <1500");
                    throw new Exception("Value is invalid.");
                }
                else if (_mss != value)
                {
                    _mss = value;
                }
            }
        }

        [DefaultValue(typeof(int), "25")]
        public int? Oheadbw { get; set; } = 25;

        private string _passphrase = "";
        [DefaultValue(typeof(string), "")]
        public string Passphrase
        {
            get { return _passphrase; }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length >= 10 && value.Length <= 79)
                {
                    if (_passphrase != value)
                    {
                        _passphrase = value;
                    }
                }
                else
                {
                    logger.Error($"The length of Passphrase is out of range which should between 10 and 79. ");
                    throw new Exception("Value is invalid.");
                }
            }
        }

        #endregion

        #region Const

        private const string PROTOCOL_SRT_FORMAT = "srt://{0}:{1}?{2}";

        #endregion

        #region Constructor

        public SRTDecodingParameter()
        {
        }

        public SRTDecodingParameter(string host, int port, long? listen_timeout, long? rw_timeout, int? tsbpdDelay, bool? tlpktDrop, EnumSRTMode mode = EnumSRTMode.CALLER, long maxbw = 0, int ffs = 25600, int oheadbw = 25, int pkt_size = 1316, int recv_buffer_size = 65536, bool nakReport = true, int mss = 1500, int connect_Timeout = 3000, string passhrase = "")
        {
            _host = host;
            Port = port;
            Listen_timeout = listen_timeout;
            Rw_timeout = rw_timeout;
            TsbpdDelay = tsbpdDelay;
            TlpktDrop = tlpktDrop;
            Mode = mode;
            _connect_Timeout = connect_Timeout;
            Recv_buffer_size = recv_buffer_size;
            Maxbw = maxbw;
            FFS = ffs;
            Oheadbw = oheadbw;
            NakReport = nakReport;
            Pkt_size = pkt_size;
            _mss = mss;
            _passphrase = passhrase;
        }

        #endregion

        #region Method

        public string GetUrl()
        {
            string url = string.Empty;
            try
            {
                string options = string.Empty;
                List<string> optionValueList = ReflectionGetPropertiesUtility.GetNonInheritedProperties(this);
                if (optionValueList != null)
                {
                    optionValueList.RemoveRange(0, 2);
                    if (optionValueList.Count != 0) options = string.Join("&", optionValueList);
                }
                else logger.Warn("GetNonInheritedProperties is null.");

                url = string.Format(PROTOCOL_SRT_FORMAT, Host, Port, options);

                logger.Info($"GetUrl(): {url}");
            }
            catch (Exception ex)
            {
                logger.Error($"GetUrl error,{ex.Message}");
            }
            return url;
        }

        public static SRTDecodingParameter GetInstance(string url)
        {
            SRTDecodingParameter instance = new SRTDecodingParameter();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Dictionary<string, string> keyValueD;
                    string host;
                    int port;

                    ParseUrl(url, out host, out port, out keyValueD);

                    var ret = instance.GetType().GetProperties().Select(p => new { p.Name, (p.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute)?.Value });

                    foreach (var item in ret)
                    {
                        if (!keyValueD.ContainsKey(item.Name.ToString()))
                        {
                            if (item.Value == null)
                            {
                                keyValueD.Add(item.Name, "");
                            }
                            else keyValueD.Add(item.Name, item.Value.ToString());
                        }
                    }
                    string json = JsonConvert.SerializeObject(keyValueD);
                    instance = JsonConvert.DeserializeObject<SRTDecodingParameter>(json);
                    logger.Info("GetInstance success.");
                }
                catch (Exception ex)
                {
                    logger.Error($"ParseUrl() error: {ex.Message}");
                }
            }
            else
            {
                logger.Warn("Url is null or empty");
                return null;
            }

            return instance;
        }

        private static void ParseUrl(string url, out string host, out int port, out Dictionary<string, string> keyValueD)
        {
            host = string.Empty;
            port = 0;
            keyValueD = null;
            string baseUrl = "";
            try
            {
                keyValueD = new Dictionary<string, string>();

                int questionMarkIndex = url.IndexOf('?');

                if (questionMarkIndex == -1)
                {
                    baseUrl = url.Substring(url.IndexOf('/') + 2, url.Length - url.IndexOf('/') - 2);
                }
                else
                {
                    baseUrl = url.Substring(url.IndexOf('/') + 2, questionMarkIndex - url.IndexOf('/') - 2);
                }

                host = baseUrl.Substring(0, baseUrl.IndexOf(':'));
                port = int.Parse(baseUrl.Substring(baseUrl.IndexOf(':') + 1, baseUrl.Length - baseUrl.IndexOf(':') - 1));
                keyValueD.Add("Host", host);
                keyValueD.Add("Port", port.ToString());

                if (questionMarkIndex != -1)
                {
                    if (questionMarkIndex == url.Length - 1)
                        return;
                    string ps = url.Substring(questionMarkIndex + 1);

                    //Parse options
                    Regex re = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
                    MatchCollection mc = re.Matches(ps);
                    foreach (Match m in mc)
                    {
                        keyValueD.Add(m.Result("$2"), m.Result("$3").ToLower());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ParseUrl() error, {ex.Message}");
            }
        }

        #endregion
    }
}
