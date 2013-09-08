﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Newtonsoft.Json.Linq;

namespace Inceptum.AppServer.TestApp
{
    public class TestConf
    {
        public TestConf(string value)
        {
            Value = value;
        }

        public string Value
        {
            get; private set; }
    }
    public class TestApp:IHostedApplication
    {
        private TestConf m_Config;
        private ILogger m_Logger;
        private JObject m_JObject;

        public TestApp(ILogger logger,TestConf config)
        {
            m_Logger = logger??NullLogger.Instance;
            m_Config = config;
        }

        public void Start()
        {
            m_Logger.InfoFormat("Test App");
            m_Logger.InfoFormat("Value from config: '{0}'", m_Config.Value);
            m_Logger.InfoFormat("Value from app.config: '{0}'", ConfigurationManager.AppSettings["appConfigSetting"]);
            m_JObject = JObject.Parse("{}");
            bool fail;
            if (bool.TryParse(ConfigurationManager.AppSettings["fail"], out fail) && fail)
            {
                new Thread(() =>
                {
                    m_Logger.Error("FAIL!!!");
                    throw new Exception();
                }).Start();
            }

            bool hangOnStart;
            if (bool.TryParse(ConfigurationManager.AppSettings["hangOnStart"], out hangOnStart) && hangOnStart)
            {
                Console.ReadLine();
            }
            m_Logger.InfoFormat("log record");
        }

        public string DoSomething(DateTime dateValue, string stringValue, int intValue, decimal decimalValue, bool boolValue)
        {
            return string.Format("Something is done. Date {0}", dateValue);
        }
    }
}
