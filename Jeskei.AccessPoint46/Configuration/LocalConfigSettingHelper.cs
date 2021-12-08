using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Collections;

namespace Jeskei.AccessPoint.Modules.Configuration
{
    class LocalConfigSettingHelper
    {
        const string appConfigNodeName = "appSettings";
        const string appConfigFileName = "LocalJeskeiSettings.cfg";

        public static Dictionary<string, object> GetValues()
        {
            Dictionary<string, object> settings = new Dictionary<string, object>();

            try
            {
                if (!File.Exists(Path.GetTempPath() + appConfigFileName))
                    if (!File.Exists("c:\\AccessPoint\\" + appConfigFileName))
                        return null;

                //string to hold the name of the  
                //config file for the assembly 
                string cfgFile = "c:\\AccessPoint\\" + appConfigFileName;

                //create a new XML Document 
                XmlDocument doc = new XmlDocument();

                //load an XML document by using the 
                //XMLTextReader class of the XML Namespace 
                //Now open the cfgFile 
                doc.Load(new XmlTextReader(cfgFile));

                //retrieve a list of nodes in the document 
                XmlNode configNode = doc.SelectSingleNode(string.Format("//{0}", appConfigNodeName));

                if (configNode != null)
                {
                    DictionarySectionHandler handler = new DictionarySectionHandler();

                    //return the new handler 
                    IDictionary configSection = (IDictionary)handler.Create(null, null, configNode);

                    foreach (string key in configSection.Keys)
                    {
                        settings.Add(key, (string)configSection[key]);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return settings;
        }
    }
}
