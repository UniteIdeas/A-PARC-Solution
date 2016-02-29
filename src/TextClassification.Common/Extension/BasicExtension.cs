using System;
using System.Globalization;
using System.Text;

namespace TextClassification.Common.Extension
{
    public static class BasicExtensions
    {
        #region Private properties

        private static short errCode { get; set; }
        private static bool isError
        {
            get { return errCode != 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts and object to an int 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="withException">if true then if an error is trapped, this is set into the error code
        /// </param>
        /// <returns>the int of the object ( else a zero 0</returns>
        public static int ToInt(this object item, bool withException = false)
        {
            errCode = 0;
            string asText;
            if (item == null)
            {
                errCode = 3;
                asText = string.Empty;
            }
            else
            {
                asText = item.ToString();
            }

            if (!isError)
            {
                if (string.IsNullOrEmpty(asText) && errCode == 0)
                {
                    errCode = 1;
                }
                else
                {
                    int i;
                    if (int.TryParse(asText, out i))
                    {
                        return i;
                    }
                    errCode = 2;
                }
            }
            if (withException)
            {
                ThrowError();
            }
            return 0;
        }

        /// <summary>
        /// Convert Int to Double
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static double ToDouble(this int i)
        {
            try
            {
                return Convert.ToDouble(i);
            }
            catch
            {
                return 0.00;
            }
        }

        /// <summary>
        /// Convert int to Byte
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static byte ToByte(this string i)
        {
            try
            {
                return Convert.ToByte(i);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// creates a string with the invariant culture
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string AsString(this int obj)
        {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// creates a string with the invariant culture
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string AsString(this short obj)
        {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// creates a string with the invariant culture
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string AsString(this short? obj)
        {
            return obj != null ? obj.Value.AsString() : string.Empty;
        }

        /// <summary>
        /// creates a string with the invariant culture
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string AsString(this byte obj)
        {
            return obj.ToInt().AsString();
        }

        /// <summary>
        /// creates a string with the invariant culture
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string AsString(this byte? obj)
        {
            return obj != null ? obj.Value.AsString() : string.Empty;
        }

        /// <summary>
        /// Max Length
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string MaxLength(this string str, int length)
        {
            return str.Length <= length ? str : str.Substring(0, length);
        }

        /// <summary>
        /// Converts object to boolean
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ToBoolean(this object obj)
        {
            if (obj == null) return false;
            int i;
            if (int.TryParse(obj.ToString(), out i))
            {
                return i == 1;
            }

            return obj.ToString().ToLower() == "true";
        }

        /// <summary>
        /// Returns object as Short(int16)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static short ToShort(this object obj)
        {
            short i;
            return short.TryParse(obj.ToString(), out i) ? i : 0.ToShort();
        }

        /// <summary>
        /// Tries to parse string as int
        /// </summary>
        /// <param name="str"></param>
        /// <param name="integer"></param>
        /// <returns></returns>
        public static bool ParseInt(this String str, out int integer)
        {
            int i;
            if (int.TryParse(str, out i))
            {
                integer = i;
                return true;
            }
            integer = 0;
            return false;
        }

        public static string Replace(this String str, string oldValue, string newValue, StringComparison comparison)
        {
            var sb = new StringBuilder();

            var previousIndex = 0;
            var index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        public static bool IsNumeric(this String str)
        {
            double result;
            return double.TryParse(str, out result);
        }

        public static string UppercaseFirst(this String str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            var a = str.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static double RoundUp(this double input, int places = 2)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            return Math.Ceiling(input * multiplier) / multiplier;
        }

        ///// <summary>
        ///// Converts a list to a data table
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public static DataTable ToDataTable<T>(this IList<T> data)
        //{
        //    var properties = TypeDescriptor.GetProperties(typeof(T));
        //    var table = new DataTable();
        //    if (properties.Count > 0)
        //    {
        //        foreach (PropertyDescriptor prop in properties)
        //            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        //    }
        //    else
        //    {
        //        table.Columns.Add("int", typeof(int));
        //    }
        //    foreach (T item in data)
        //    {
        //        var row = table.NewRow();
        //        if (properties.Count > 0)
        //        {
        //            foreach (PropertyDescriptor prop in properties)
        //                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
        //        }
        //        else
        //        {
        //            row["int"] = item;
        //        }

        //        table.Rows.Add(row);
        //    }
        //    return table;
        //}


        #endregion

        #region Private methods

        ///// <summary>
        ///// Throw Error
        ///// </summary>
        private static void ThrowError()
        {
            if (!isError) return;

            throw new Exception("Parse failed");
            //switch (errCode)
            //{
            //    case 1:
            //        throw new RulerException("Cannot convert empty string to int", null, 1);
            //    case 2:
            //        throw new RulerException("Failed to convert object to int", null, 2);
            //    case 3:
            //        throw new RulerException("Failed to null object to int", null, 3);
            //}
        }

        #endregion


    }
}
