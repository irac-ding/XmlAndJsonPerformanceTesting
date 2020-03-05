/* =============================================
 * Copyright 2018 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: SRTParameter2.cs
 * Purpose:  Serialize/deserialize class from/to url for SRT.
 *           Works with options in uri.Query, without authority.
 *           Will remove all options with default value, so technically it is not reversible.
 * Author:   MikkoXU copied from the confusing SRTEncodingParameter.cs, some day August 2018.
 * Todo:     1) Make AdditionalOptions work for all the unrecognized options.
 *           2) Enable all the comment-out options, and add more options.
 *           3) Check if it works for both encoding and decoding.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    /// <summary>
    /// Generate URL query options only.
    /// <see cref="http://ffmpeg.org/ffmpeg-all.html#srt" />
    /// <seealso cref="https://github.com/FFmpeg/FFmpeg/blob/master/libavformat/libsrt.c" />
    /// </summary>
    public class SRTParameter2
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

        // Names equals to options defined in libsrt (see above), to make serialization/deserialization easier.
        // No Nullable since we don't need it.

        [DefaultValue(typeof(EnumSRTMode), "CALLER")]
        public EnumSRTMode mode { get; set; } = EnumSRTMode.CALLER;

        // TODO: Confirm default value.
        //[DefaultValue(typeof(long), "")]
        //public long listen_timeout { get; set; }

        [DefaultValue(typeof(long), "3000")]
        public long rw_timeout { get; set; } = 3000;

        [DefaultValue(typeof(int), "65536")]
        public int recv_buffer_size { get; set; } = 65536;

        // TODO: Confirm default value.
        //[DefaultValue(typeof(int), "")]
        //public int tsbpddelay { get; set; }

        [DefaultValue(typeof(bool), "false")]
        public bool tlpktdrop { get; set; } = false;

        //[DefaultValue(typeof(long), "0")]
        //public long maxbw { get; set; } = 0;

        //[DefaultValue(typeof(int), "25600")]
        //public int ffs { get; set; } = 25600;

        //[DefaultValue(typeof(int), "1316")]
        //public int pkt_size { get; set; } = 1316;

        [DefaultValue(typeof(bool), "true")]
        public bool nakreport { get; set; } = true;

        [DefaultValue(typeof(int), "3000")]
        public int connect_timeout { get; set; } = 3000;

        [DefaultValue(typeof(int), "1500")]
        public int mss { get; set; } = 1500;

        //[DefaultValue(typeof(int), "25")]
        //public int oheadbw { get; set; } = 25;

        [DefaultValue(typeof(string), "")]
        public string passphrase { get; set; } = string.Empty;

        #endregion

        #region Additional options

        /// <summary>
        /// Unrecognized options.
        /// </summary>
        [DefaultValue(typeof(Dictionary<string, string>), null)]
        public Dictionary<string, string> AdditionalOptions { get; set; }

        #endregion

        #region Constructor

        public SRTParameter2()
        {
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize an <c>SRTParameter2</c> instance to url (actually the query part).
        /// </summary>
        public string GetUrl(string authority)
        {
            string ret = $"srt://{authority}";

            try
            {
                NameValueCollection nvc = new NameValueCollection();

                IEnumerable<PropertyInfo> properties = GetNonInheritedProperties(this);
                foreach (PropertyInfo pi in properties)
                {
                    if (pi.PropertyType == typeof(bool))
                        nvc.Add(pi.Name, Convert.ToInt32(pi.GetValue(this)).ToString());
                    else
                        nvc.Add(pi.Name, pi.GetValue(this).ToString());
                }

                nvc.Remove(nameof(AdditionalOptions));

                if (AdditionalOptions != null && AdditionalOptions.Count > 0)
                {
                    foreach (string key in AdditionalOptions.Keys)
                    {
                        nvc.Add(key, AdditionalOptions[key]);
                    }
                }

                logger.Info($"GerUrl() nvc.Count is {nvc.Count}.");

                string options = ToQueryString(nvc);
                if (!string.IsNullOrWhiteSpace(options))
                    ret = $"{ret}?{options}";
            }
            catch (Exception ex)
            {
                logger.Error($"GetUrl() error, {ex.Message}.");
            }

            logger.Info($"GetUrl() ret [{ret}].");
            return ret;
        }

        // TODO: The following two methods are separated from ReflectionGetPropertiesUtility.cs.

        private IEnumerable<PropertyInfo> GetNonInheritedProperties(object model)
        {
            try
            {
                IEnumerable<PropertyInfo> ret = model.GetType()
                    .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetValue(model, null) != null && !p.GetValue(model, null).Equals((p.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute)?.Value));

                logger.Info($"GetNonInheritedProperties() ret.Count is {ret.Count()}.");
                return ret;
            }
            catch (Exception ex)
            {
                logger.Error($"GetNonInheritedProperties() error, {ex.Message}.");
                return null;
            }
        }

        private string ToQueryString(NameValueCollection nvc)
        {
            string[] array = (from key in nvc.AllKeys
                              from value in nvc.GetValues(key)
                              select $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(value)}")
                              .ToArray();
            return string.Join("&", array);
        }

        #endregion

        #region Deserialization

        /// <summary>
        /// Deserialize url to an <c>SRTParameter2</c> instance.
        /// </summary>
        public static SRTParameter2 GetInstance(string url)
        {
            SRTParameter2 ret = null;

            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    ret = new SRTParameter2();

                    Dictionary<string, string> options = GetOptions(url);

                    IEnumerable<PropertyInfo> recognizedProperties = ret.GetType().GetProperties().Where(p => options.ContainsKey(p.Name));
                    logger.Info($"GetInstance() {recognizedProperties.Count()} properties recognized.");

                    foreach (PropertyInfo pi in recognizedProperties)
                    {
                        try
                        {
                            if (pi.PropertyType == typeof(bool))
                            {
                                string value = options[pi.Name];
                                long num;
                                if (long.TryParse(value, out num))
                                    pi.SetValue(ret, Convert.ToBoolean(num));
                                else
                                    pi.SetValue(ret, Convert.ToBoolean(value));
                            }
                            else if (pi.PropertyType == typeof(int))
                            {
                                string value = options[pi.Name];
                                int num;
                                if (int.TryParse(value, out num))
                                    pi.SetValue(ret, num);
                                else
                                    logger.Warn($"GetInstance() cannot parse {pi.Name} to int.");
                            }
                            else if (pi.PropertyType == typeof(long))
                            {
                                string value = options[pi.Name];
                                long num;
                                if (long.TryParse(value, out num))
                                    pi.SetValue(ret, num);
                                else
                                    logger.Warn($"GetInstance() cannot parse {pi.Name} to long.");
                            }
                            else
                                pi.SetValue(ret, options[pi.Name]);

                            options.Remove(pi.Name);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"GetInstance() for {pi.Name} error, {ex.Message}.");
                        }
                    }

                    ret.AdditionalOptions = options;
                }
                catch (Exception ex)
                {
                    logger.Error($"GetInstance() error, {ex.Message}.");
                }
            }
            else
            {
                logger.Warn("GetInstance() given url is null or empty.");
            }

            return ret;
        }

        /// <summary>
        /// Extract all options in <c>uri.Query</c> to a dictionary.
        /// </summary>
        private static Dictionary<string, string> GetOptions(string url)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            try
            {
                Uri uri = new Uri(url);
                string query = uri.Query;
                NameValueCollection options = HttpUtility.ParseQueryString(query);

                foreach (string key in options.AllKeys)
                    ret.Add(key, options[key]);

                logger.Info($"GetOptions() from {url} to {options.Count} options.");
            }
            catch (Exception ex)
            {
                logger.Error($"GetOptions() error, {ex.Message}.");
            }

            return ret;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Verify if all options are in the correct range, or fire an <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        public bool Verify()
        {
            bool ret = true;

            if (connect_timeout < 3000)
                throw new ArgumentOutOfRangeException("connect_timeout");

            if (mss > 1500)
                throw new ArgumentOutOfRangeException("mss");

            if (!string.IsNullOrWhiteSpace(passphrase) && (passphrase.Length < 10 || passphrase.Length > 79))
                throw new ArgumentOutOfRangeException("passphrase");

            return ret;
        }

        #endregion

        #region Helper Methods

        public static string Dict2AdditionalString(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count <= 0)
                return string.Empty;

            string[] ret = dict.Select(kv => $"{kv.Key}={kv.Value}").ToArray();
            return string.Join("&", ret);
        }

        public static Dictionary<string, string> AdditionalString2Dict(string aString)
        {
            if (string.IsNullOrWhiteSpace(aString))
                return null;

            Dictionary<string, string> ret;

            try
            {
                string[] strArr = aString.Split('&');
                ret = strArr.Select(str =>
                {
                    string[] kv = str.Split('=');
                    if (kv.Length == 2)
                        return new Tuple<string, string>(kv[0], kv[1]);
                    else
                        return null;
                }).Where(itm => itm != null).ToDictionary(itm => itm.Item1, itm => itm.Item2);
            }
            catch (Exception ex)
            {
                logger.Warn($"AdditionalString2Dict() Err msg: {ex.Message}");
                ret = null;
            }

            return ret;
        }

        #endregion
    }
}
