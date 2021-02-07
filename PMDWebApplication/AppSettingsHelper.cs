using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PMDWebApplication
{

    //https://stacktoheap.com/blog/2013/01/20/using-typeconverters-to-get-appsettings-in-net/
    //Useful link to describe retrieving other value in web.config file
    public class AppSettingsHelper
    {
        public static T Get<T>(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(appSetting)) throw new NullReferenceException(key);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(appSetting));
        }

  /*      public static Set(string key, string value)
        {
            var appSetting = ConfigurationManager.AppSettings[key];

            if (appSetting[key] == null)
            {
                appSetting.Add(key, value);
            }
            else
            {
                appSetting[key].Value = value;
            }

            if (string.IsNullOrWhiteSpace(appSetting)) throw new NullReferenceException(key);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(appSetting));
        }*/
    }
}