using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;

namespace SRTprotocols
{
    [TestFixture]
    public class TestOfSRTProtocols
    {
        static void Main(string[] args)
        {
        }
        [Test]
        public void TestDecoderGetinstance()
        {
            string url = "srt://10.12.23.22:2?Mode=LISTENER&Listen_timeout=2015&Rw_timeout=2&TsbpdDelay=6654&TlpktDrop=True";
            SRTDecodingParameter ret = SRTDecodingParameter.GetInstance(url);
            SRTDecodingParameter instance = new SRTDecodingParameter("10.12.23.22", 2, 2015, 2, 6654, true, EnumModeType.LISTENER);
            PropertyValuesAreEquals<SRTDecodingParameter>(ret, instance);
        }

        [Test]
        public void TestEncoderGetinstance()
        {
            string url = "srt://:9000?Send_buffer_size=0&TsbpdDelay=0&TlpktDrop=False";
            SRTEncodingParameter ret = SRTEncodingParameter.GetInstance(url);
            SRTEncodingParameter instance = new SRTEncodingParameter("", 9000, 0, 0, false, "");
            PropertyValuesAreEquals<SRTEncodingParameter>(ret, instance);
        }

        [Test]
        public void TestEncoderGetURL()
        {
            string ret = "srt://:9000?Send_buffer_size=0&TsbpdDelay=0&TlpktDrop=False";
            SRTEncodingParameter encoderMode = new SRTEncodingParameter("", 9000, 0, 0, false, "");
            string url = encoderMode.GetUrl();
            Assert.AreEqual(ret, url);
        }

        [Test]
        public void TestDecoderGetURL()
        {
            string ret = "srt://10.12.23.22:2?Mode=LISTENER&Listen_timeout=2015&Rw_timeout=2&TsbpdDelay=6654&TlpktDrop=True";
            SRTDecodingParameter decoder = new SRTDecodingParameter("10.12.23.22", 2, 2015, 2, 6654, true, EnumModeType.LISTENER);
            string url = decoder.GetUrl();
            Assert.AreEqual(ret, url);
        }

        public static void PropertyValuesAreEquals<T>(object actual, object expected)
        {
            PropertyInfo[] properties = expected.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object expectedValue = property.GetValue(expected, null);
                object actualValue = property.GetValue(actual, null);

                if (actualValue is IList)
                    AssertListsAreEquals(property, (IList)actualValue, (IList)expectedValue);
                else if (!Equals(expectedValue, actualValue))
                    Assert.Fail("Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue, actualValue);
            }

        }
        private static void AssertListsAreEquals(PropertyInfo property, IList actualList, IList expectedList)
        {
            if (actualList.Count != expectedList.Count)
                Assert.Fail("Property {0}.{1} does not match. Expected IList containing {2} elements but was IList containing {3} elements", property.PropertyType.Name, property.Name, expectedList.Count, actualList.Count);

            for (int i = 0; i < actualList.Count; i++)
                if (!Equals(actualList[i], expectedList[i]))
                    Assert.Fail("Property {0}.{1} does not match. Expected IList with element {1} equals to {2} but was IList with element {1} equals to {3}", property.PropertyType.Name, property.Name, expectedList[i], actualList[i]);
        }
    }
}
