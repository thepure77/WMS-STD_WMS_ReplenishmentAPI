using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

namespace ReplenishmentBusiness.Commons
{
    public static class ModelConverter
    {
        public static string sJson<T>(this T obj)
        {
            try
            {
                return JsonConvert.SerializeObject((object)obj, new JsonSerializerSettings()
                {
                    ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver()
                });
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static T sJson<T>(this string obj) where T : new()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(obj);
            }
            catch (Exception ex)
            {
                return new T();
            }
        }

        public static string sJson(this string obj)
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(obj);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static List<T> sJsonList<T>(this string obj)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<T>>(obj);
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public static object sWebException(this WebException ex)
        {
            if (ex.Response == null)
                return (object)null;
            return JsonConvert.DeserializeObject(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
        }

        public static T sParse<T>(this object obj, T defaultValue)
        {
            try
            {
                if (typeof(T) == typeof(DateTime))
                    return (T)Convert.ChangeType(obj, typeof(T));
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (Exception ex)
            {
                return defaultValue;
            }
        }

        public static T sParse<T>(this object obj) where T : new()
        {
            try
            {
                T obj1 = (T)Convert.ChangeType(obj, typeof(T));
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (Exception ex)
            {
                return new T();
            }
        }

        public static List<T> sParse<T>(T value, int count)
        {
            List<T> objList = new List<T>();
            for (int index = 0; index < count; ++index)
                objList.Add(value);
            return objList;
        }

        public static string toDateTimeString(this string date)
        {
            try
            {
                string str1 = date;
                string str2 = str1.Substring(0, 4);
                string str3 = str1.Substring(4, 2);
                string str4 = str1.Substring(6, 2);
                string str5 = "00";
                string str6 = "00";
                if (str1.Length > 10)
                {
                    str5 = str1.Substring(8, 2);
                    str6 = str1.Substring(10, 2);
                }
                return str4 + "/" + str3 + "/" + str2 + " " + str5 + ":" + str6;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string toString(this DateTime? date)
        {
            if (date.HasValue)
                return date.Value.toString();
            return "";
        }

        public static string toString(this DateTime date)
        {
            DateTime dateTime = date;
            try
            {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                return dateTime.ToString("yyyyMMddHHmmss", (IFormatProvider)cultureInfo);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public class betweenDate
        {
            public DateTime start { get; set; }

            public DateTime end { get; set; }
        }

        public static betweenDate toBetweenDate(this string date)
        {
            try
            {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                betweenDate betweenDate = new betweenDate();
                date = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2);
                DateTime dateTime1 = DateTime.Parse(date, (IFormatProvider)cultureInfo);
                DateTime dateTime2 = DateTime.Parse(date, (IFormatProvider)cultureInfo);
                dateTime2 = dateTime2.AddHours(23.0);
                dateTime2 = dateTime2.AddMinutes(59.0);
                betweenDate.start = dateTime1;
                betweenDate.end = dateTime2;
                return betweenDate;
            }
            catch (Exception ex)
            {
                return (betweenDate)null;
            }
        }

        public static DateTime? toCVDate(this string date)
        {
            try
            {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                date = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2);
                return new DateTime?(DateTime.Parse(date, (IFormatProvider)cultureInfo));
            }
            catch (Exception ex)
            {
                return new DateTime?();
            }
        }

        public static string toCVDateString(this string date)
        {
            try
            {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                date = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2);
                return date;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static DateTime toDateDefault(this string date)
        {
            return date.toCVDate() ?? DateTime.Now;
        }

        public static string ToISOString(this DateTime obj)
        {
            return obj.ToString("yyyy-MM-ddThh:mm:ss");
        }
    }
}
