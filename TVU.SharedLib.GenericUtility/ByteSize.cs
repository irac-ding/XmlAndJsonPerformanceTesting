/* =============================================
 * Copyright 2013 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: ByteSize.cs
 * Purpose:  Translate size in bits/bytes to KB/MB/GB/TB
 *           Refer to github: https://github.com/omar/ByteSize
 * Author:   MikkoXU (mikkoxu@tvunetworks.com) added on Mar.31th, 2014
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;

namespace TVU.SharedLib.GenericUtility
{
    /// <summary>
    /// Represents a byte size value.
    /// </summary>
    public struct ByteSize : IComparable<ByteSize>, IEquatable<ByteSize>
    {
        public static readonly ByteSize MinValue = ByteSize.FromBits(0);
        public static readonly ByteSize MaxValue = ByteSize.FromBits(long.MaxValue);

        public const long BitsInByte = 8;
        public const long BytesInKiloByte = 1024;
        public const long BytesInMegaByte = 1048576;
        public const long BytesInGigaByte = 1073741824;
        public const long BytesInTeraByte = 1099511627776;

        public const string BitSymbol = "b";
        public const string ByteSymbol = "B";
        public const string KiloByteSymbol = "KB";
        public const string MegaByteSymbol = "MB";
        public const string GigaByteSymbol = "GB";
        public const string TeraByteSymbol = "TB";

        public long Bits { get; private set; }
        public double Bytes { get; private set; }
        public double KiloBytes { get; private set; }
        public double MegaBytes { get; private set; }
        public double GigaBytes { get; private set; }
        public double TeraBytes { get; private set; }

        public string LargestWholeNumberSymbol
        {
            get
            {
                // Absolute value is used to deal with negative values
                if (Math.Abs(this.TeraBytes) >= 1)
                    return ByteSize.TeraByteSymbol;

                if (Math.Abs(this.GigaBytes) >= 1)
                    return ByteSize.GigaByteSymbol;

                if (Math.Abs(this.MegaBytes) >= 1)
                    return ByteSize.MegaByteSymbol;

                if (Math.Abs(this.KiloBytes) >= 1)
                    return ByteSize.KiloByteSymbol;

                if (Math.Abs(this.Bytes) >= 1)
                    return ByteSize.ByteSymbol;

                return ByteSize.BitSymbol;
            }
        }
        public double LargestWholeNumberValue
        {
            get
            {
                // Absolute value is used to deal with negative values
                if (Math.Abs(this.TeraBytes) >= 1)
                    return this.TeraBytes;

                if (Math.Abs(this.GigaBytes) >= 1)
                    return this.GigaBytes;

                if (Math.Abs(this.MegaBytes) >= 1)
                    return this.MegaBytes;

                if (Math.Abs(this.KiloBytes) >= 1)
                    return this.KiloBytes;

                if (Math.Abs(this.Bytes) >= 1)
                    return this.Bytes;

                return this.Bits;
            }
        }

        public ByteSize(double byteSize)
            : this()
        {
            // Get ceiling because bis are whole units
            Bits = (long)Math.Ceiling(byteSize * BitsInByte);

            Bytes = byteSize;
            KiloBytes = byteSize / BytesInKiloByte;
            MegaBytes = byteSize / BytesInMegaByte;
            GigaBytes = byteSize / BytesInGigaByte;
            TeraBytes = byteSize / BytesInTeraByte;
        }

        public static ByteSize FromBits(long value)
        {
            return new ByteSize(value / (double)BitsInByte);
        }

        public static ByteSize FromBytes(double value)
        {
            return new ByteSize(value);
        }

        public static ByteSize FromKiloBytes(double value)
        {
            return new ByteSize(value * BytesInKiloByte);
        }

        public static ByteSize FromMegaBytes(double value)
        {
            return new ByteSize(value * BytesInMegaByte);
        }

        public static ByteSize FromGigaBytes(double value)
        {
            return new ByteSize(value * BytesInGigaByte);
        }

        public static ByteSize FromTeraBytes(double value)
        {
            return new ByteSize(value * BytesInTeraByte);
        }

        /// <summary>
        /// Converts the value of the current ByteSize object to a string.
        /// The metric prefix symbol (bit, byte, kilo, mega, giga, tera) used is
        /// the largest metric prefix such that the corresponding value is greater
        //  than or equal to one.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} {1}", this.LargestWholeNumberValue, this.LargestWholeNumberSymbol);
        }

        public string ToString(string format)
        {
            if (!format.Contains("#") && !format.Contains("0"))
                format = "#.## " + format;

            Func<string, bool> has = s => format.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) != -1;
            Func<double, string> output = n => n.ToString(format);

            if (has("TB"))
                return output(this.TeraBytes);
            if (has("GB"))
                return output(this.GigaBytes);
            if (has("MB"))
                return output(this.MegaBytes);
            if (has("KB"))
                return output(this.KiloBytes);

            // Byte and Bit symbol look must be case-sensitive
            if (format.IndexOf(ByteSize.ByteSymbol) != -1)
                return output(this.Bytes);

            if (format.IndexOf(ByteSize.BitSymbol) != -1)
                return output(this.Bits);

            return string.Format("{0} {1}", this.LargestWholeNumberValue.ToString(format), this.LargestWholeNumberSymbol);
        }

        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            ByteSize other;
            if (value is ByteSize)
                other = (ByteSize)value;
            else
                return false;

            return Equals(other);
        }

        public bool Equals(ByteSize value)
        {
            return this.Bits == value.Bits;
        }

        public override int GetHashCode()
        {
            return this.Bits.GetHashCode();
        }

        public int CompareTo(ByteSize other)
        {
            return this.Bits.CompareTo(other.Bits);
        }

        public ByteSize Add(ByteSize bs)
        {
            return new ByteSize(this.Bits + bs.Bits);
        }

        public ByteSize AddBits(long value)
        {
            return new ByteSize(this.Bits + value);
        }

        public ByteSize AddBytes(double value)
        {
            return this + ByteSize.FromBytes(value);
        }

        public ByteSize AddKiloBytes(double value)
        {
            return this + ByteSize.FromKiloBytes(value);
        }

        public ByteSize AddMegaBytes(double value)
        {
            return this + ByteSize.FromMegaBytes(value);
        }

        public ByteSize AddGigaBytes(double value)
        {
            return this + ByteSize.FromGigaBytes(value);
        }

        public ByteSize AddTeraBytes(double value)
        {
            return this + ByteSize.FromTeraBytes(value);
        }

        public ByteSize Subtract(ByteSize bs)
        {
            return new ByteSize(this.Bits - bs.Bits);
        }

        public static ByteSize operator +(ByteSize b1, ByteSize b2)
        {
            return new ByteSize(b1.Bits + b2.Bits);
        }

        public static ByteSize operator ++(ByteSize b)
        {
            return new ByteSize(b.Bits++);
        }

        public static ByteSize operator -(ByteSize b)
        {
            return new ByteSize(-b.Bits);
        }

        public static ByteSize operator --(ByteSize b)
        {
            return new ByteSize(b.Bits--);
        }

        public static bool operator ==(ByteSize b1, ByteSize b2)
        {
            return b1.Bits == b2.Bits;
        }

        public static bool operator !=(ByteSize b1, ByteSize b2)
        {
            return b1.Bits != b2.Bits;
        }

        public static bool operator <(ByteSize b1, ByteSize b2)
        {
            return b1.Bits < b2.Bits;
        }

        public static bool operator <=(ByteSize b1, ByteSize b2)
        {
            return b1.Bits <= b2.Bits;
        }

        public static bool operator >(ByteSize b1, ByteSize b2)
        {
            return b1.Bits > b2.Bits;
        }

        public static bool operator >=(ByteSize b1, ByteSize b2)
        {
            return b1.Bits >= b2.Bits;
        }

        public static bool TryParse(string s, out ByteSize result)
        {
            // Arg checking
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s", "String is null or whitespace");

            // Setup the result
            result = new ByteSize();

            // Get the index of the first non-digit character
            s = s.TrimStart(); // Protect against leading spaces

            var num = 0;
            var found = false;

            // Pick first non-digit number
            for (num = 0; num < s.Length; num++)
                if (!(char.IsDigit(s[num]) || s[num] == '.'))
                {
                    found = true;
                    break;
                }

            if (found == false)
                return false;

            int lastNumber = num;

            // Cut the input string in half
            string numberPart = s.Substring(0, lastNumber).Trim();
            string sizePart = s.Substring(lastNumber, s.Length - lastNumber).Trim();

            // Get the numeric part
            double number;
            if (!double.TryParse(numberPart, out number))
                return false;

            // Get the magnitude part
            switch (sizePart)
            {
                case "b":
                    if (number % 1 != 0) // Can't have partial bits
                        return false;

                    result = FromBits((long)number);
                    break;

                case "B":
                    result = FromBytes(number);
                    break;

                case "KB":
                case "kB":
                case "kb":
                case "K":
                case "k":
                    result = FromKiloBytes(number);
                    break;

                case "MB":
                case "mB":
                case "mb":
                case "M":
                case "m":
                    result = FromMegaBytes(number);
                    break;

                case "GB":
                case "gB":
                case "gb":
                case "G":
                case "g":
                    result = FromGigaBytes(number);
                    break;

                case "TB":
                case "tB":
                case "tb":
                case "T":
                case "t":
                    result = FromTeraBytes(number);
                    break;
            }

            return true;
        }

        public static ByteSize Parse(string s)
        {
            ByteSize result;

            if (TryParse(s, out result))
                return result;

            throw new FormatException("Value is not in the correct format");
        }
    }
}
