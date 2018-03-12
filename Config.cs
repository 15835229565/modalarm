﻿/*
 * Copyright 2017-2018 Alexandr Kolodkin
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ModAlarm
 * Summary  : Module configuration
 * 
 * Author   : Alexandr Kolodkin
 * Created  : 2017
 * Modified : 2018
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Scada.Server.Modules
{
    /// <summary>
    /// Module configuration
    /// <para>Конфигурация модуля</para>
    /// </summary>
    internal class Config
    {
        /// <summary>
        /// Имя файла конфигурации
        /// </summary>
        private const string configFileName = "ModAlarm.xml";


        /// <summary>
        /// Список каналов и аудиофайлов
        /// </summary>
        public SortedDictionary<int,string> channels = new SortedDictionary<int, string>();


        /// <summary>
        /// Конструктор, ограничивающий создание объекта без параметров
        /// </summary>
        private Config()
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public Config(string configDir)
        {
            fileName = ScadaUtils.NormalDir(configDir) + configFileName;
            SetToDefault();
        }


        /// <summary>
        /// Получить полное имя файла конфигурации
        /// </summary>
        public string fileName { get; private set; }


        /// <summary>
        /// Установить значения параметров конфигурации по умолчанию
        /// </summary>
        private void SetToDefault()
        {
            channels.Clear();
        }

        /// <summary>
        /// Загрузить конфигурацию модуля
        /// </summary>
        public bool Load(out string errMsg)
        {
            SetToDefault();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);

                XmlNode root = xmlDoc.DocumentElement;
                XmlNodeList alerts = root.ChildNodes;

                foreach (XmlNode alert in alerts)
                {
                    int channel = alert.GetChildAsInt("Channel");
                    string soundFileName = alert.GetChildAsString("Sound");

                    if (channels.ContainsKey(channel) == false)
                    {
                        channels.Add(channel, soundFileName);
                    }
                }

                errMsg = "";
                return true;
            }
            catch (FileNotFoundException ex)
            {
                errMsg = ModPhrases.LoadModSettingsError + ": " + ex.Message + 
                    Environment.NewLine + ModPhrases.ConfigureModule;
                return false;
            }
            catch (Exception ex)
            {
                errMsg = ModPhrases.LoadModSettingsError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Сохранить конфигурацию модуля
        /// </summary>
        public bool Save(out string errMsg)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDecl);

                XmlElement root = xmlDoc.CreateElement("ModAlarm");
                xmlDoc.AppendChild(root);

                foreach(KeyValuePair<int, string> channel in channels)
                {
                    XmlElement alert = xmlDoc.CreateElement("Alarm");
                    alert.AppendElem("Channel", channel.Key);
                    alert.AppendElem("Sound", channel.Value);
                    root.AppendChild(alert);
                }

                xmlDoc.Save(fileName);
                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ModPhrases.SaveModSettingsError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Клонировать конфигурацию модуля
        /// </summary>
        public Config Clone()
        {
            Config configCopy = new Config();
            configCopy.fileName = fileName;
            configCopy.channels = channels;

            return configCopy;
        }
    }
}
