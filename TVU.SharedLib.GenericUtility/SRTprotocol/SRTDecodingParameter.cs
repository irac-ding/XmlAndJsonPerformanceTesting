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
    public class SRTEncodingParameter
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
                else if (IPAddress.TryParse(value, out IP) || string.IsNullOrEmpty(value))
                {
                    if (_host != value) _host = value;
                }
                else
                {
                    logger.Error($"Current value of host is invalid. ");
                    throw new Exception("Value is invalid.");
                }
            }
        }

        public int Port { get; set; }

        [DefaultValue(typeof(EnumSRTMode), "LISTENER")]
        public EnumSRTMode Mode { get; set; } = EnumSRTMode.LISTENER;

        [DefaultValue(typeof(long), "")]
        public long? Listen_timeout { get; set; }

        [DefaultValue(typeof(long), "")]
        public long? Rw_timeout { get; set; }

        [DefaultValue(typeof(int), "")]
        public int? Send_buffer_size { get; set; }

        [DefaultValue(typeof(int), "")]
        public int? TsbpdDelay { get; set; }

        [DefaultValue(typeof(bool), "")]
        public bool? TlpktDrop { get; set; }

        [DefaultValue(typeof(long), "0")]
        public long? Inputbw { get; set; } = 0;

        [DefaultValue(typeof(int), "1316")]
        public int? Pkt_size { get; set; } = 1316;

        [DefaultValue(typeof(int), "64")]
        public int? IpTtl { get; set; } = 64;

        [DefaultValue(typeof(int), "0xB8")]
        public int? IpTos { get; set; } = 0xB8;

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

        private string _passphrase = string.Empty;
        [DefaultValue(typeof(string), "string.Empty")]
        public string Passphrase
        {
            get { return _passphrase; }
            set
            {
                if (Pbkeylen != 0)
                {
                    if (value.Length >= 10 && value.Length <= 79)
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
                else
                {
                    _passphrase = null;
                }
            }
        }

        private int? _pbkeylen = 0;
        [DefaultValue(typeof(int), "0")]
        public int? Pbkeylen
        {
            get { return _pbkeylen; }
            set
            {
                if (value == 0 || value == 16 || value == 24 || value == 32)
                {
                    if (_pbkeylen != value)
                    {
                        _pbkeylen = value;
                    }
                }
                else
                {
                    logger.Error($"The value of Pbkeylen is invalid which should be as 0/16/24/32.");
                    throw new Exception("Value is invalid.");
                }
            }
        }

        #endregion

        #region Const

        private const string PROTOCOL_SRT_FORMAT = "srt://{0}:{1}?{2}";

        #endregion

        #region Constructor

        public SRTEncodingParameter()
        {
        }

        public SRTEncodingParameter(string host, int port, int? send_buffer_size, int? tsbpddelay, bool? tlpktdrop, string passhrase, EnumSRTMode mode = EnumSRTMode.LISTENER, long inputbw = 0, int connect_timeout = 3000, int pbkeylen = 0, int mss = 1500, int ipttl = 64, int iptos = 0xB8, int pkt_size = 1316)
        {
            _host = host;
            Port = port;
            Inputbw = inputbw;
            Mode = mode;
            _connect_Timeout = connect_timeout;
            Send_buffer_size = send_buffer_size;
            _mss = mss;
            IpTtl = ipttl;
            IpTos = iptos;
            TsbpdDelay = tsbpddelay;
            TlpktDrop = tlpktdrop;
            Pkt_size = pkt_size;
            _passphrase = passhrase;
            Pbkeylen = pbkeylen;
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

        public static SRTEncodingParameter GetInstance(string url)
        {
            SRTEncodingParameter instance = new SRTEncodingParameter();

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
                    instance = JsonConvert.DeserializeObject<SRTEncodingParameter>(json);
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
