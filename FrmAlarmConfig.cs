﻿/*
 * Copyright 2017-2018 Alexandr Kolodkin <alexandr.kolodkin@gmail.com>
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
 * Summary  : Config form
 * 
 * Author   : Alexandr Kolodkin
 * Created  : 2017
 * Modified : 2018
 */

using Scada.Client;
using Scada.UI;
using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Scada.Server.Modules.Alarm
{
    /// <summary>
    /// Module configuration form
    /// <para>Форма конфигурации</para>
    /// </summary>
    public partial class FrmAlarmConfig : Form
    {
        private AppDirs appDirs;         // директории приложения
        private ServerComm serverComm;   // объект для обмена данными со SCADA-Сервером

        private Config config;           // конфигурация модуля
        private Config configCopy;       // копия конфигурации модуля для реализации отмены изменений
        private bool modified;           // признак изменения конфигурации
        private bool changing;           // происходит изменение значений элементов управления

        private int lastChannel = 0;     // последний добавленный канал
        private string lastPath = "";    // последний добавленный аудиофайл

        private string msgAlreadyExists; // сообщение "Данный канал уже исползуется. Именить его?"
        private string msgWarning;       // заголовок сообщения "Предупреждение"

        /// <summary>
        /// Конструктор, ограничивающий создание формы без параметров
        /// </summary>
        public FrmAlarmConfig()
        {
            InitializeComponent();

            config = null;
            configCopy = null;
            modified = false;
            changing = false;
        }


        /// <summary>
        /// Получить или установить признак изменения конфигурации
        /// </summary>
        private bool Modified
        {
            get
            {
                return modified;
            }
            set
            {
                modified = value;
                btnSave.Enabled = modified;
                btnCancel.Enabled = modified;
            }
        }


        /// <summary>
        /// Отобразить форму модально
        /// </summary>
        public static void ShowDialog(AppDirs appDirs, ServerComm serverComm)
        {
            FrmAlarmConfig frmAlarmConfig = new FrmAlarmConfig();
            frmAlarmConfig.appDirs = appDirs;
            frmAlarmConfig.serverComm = serverComm;
            frmAlarmConfig.ShowDialog();
        }


        /// <summary>
        /// Сохранить конфигурацию модуля
        /// </summary>
        private bool SaveConfig()
        {
            if (Modified)
            {
                string errMsg;
                if (config.Save(out errMsg))
                {
                    Modified = false;
                    return true;
                }
                else
                {
                    ScadaUiUtils.ShowError(errMsg);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Отобразить конфигурацию
        /// </summary>
        private void ConfigToControls()
        {
            changing = true;

            inputChannels.Items.Clear();

            foreach (KeyValuePair<int, string> channel in config.channels)
            {
                ListViewItem item = new ListViewItem(channel.Key.ToString());
                item.SubItems.Add(channel.Value);
                inputChannels.Items.Add(item);
            }
            changing = false;
        }


        /// <summary>
        /// Отобразить конфигурацию
        /// </summary>
        private void FrmAlarmConfig_Load(object sender, EventArgs e)
        {
            // локализация модуля
            string errMsg;
            if (!Localization.UseRussian)
            {
                if (Localization.LoadDictionaries(appDirs.LangDir, "ModAlarm", out errMsg))
                {
                    Translator.TranslateForm(this, "Scada.Server.Modules.Alarm.FrmAlarmConfig");

                    inputChannels.Columns[0].Text = Localization.Dictionaries["Scada.Server.Modules.Alarm.FrmAlarmConfig"]
                        .GetPhrase("columnChannel", inputChannels.Columns[0].Text);

                    inputChannels.Columns[1].Text = Localization.Dictionaries["Scada.Server.Modules.Alarm.FrmAlarmConfig"]
                        .GetPhrase("columnSoundFile", inputChannels.Columns[0].Text);

                    msgAlreadyExists = Localization.Dictionaries["Scada.Server.Modules.Alarm.FrmAlarmConfig"]
                        .GetPhrase("msgAlreadyExists", "Данный канал уже исползуется. Изменить его?");

                    msgWarning = Localization.Dictionaries["Scada.Server.Modules.Alarm.FrmAlarmConfig"]
                        .GetPhrase("msgWarning", "Предупреждение");
                }
                else
                    ScadaUiUtils.ShowError(errMsg);
            }

            // загрузка конфигурации
            config = new Config(appDirs.ConfigDir);
            if (File.Exists(config.fileName) && !config.Load(out errMsg)) ScadaUiUtils.ShowError(errMsg);

            // создание копии конфигурации
            configCopy = config.Clone();

            // отображение конфигурации
            ConfigToControls();

            // последний добавленный канал
            lastChannel = config.channels.Last().Key;
            lastPath = config.channels.Last().Value;

            // снятие признака изменения конфигурации
            Modified = false;
        }


        private void linkAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:alexandr.kolodkin@gmail.com");
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            // сохранение конфигурации модуля
            SaveConfig();
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            // отмена изменений конфигурации
            config = configCopy;
            configCopy = config.Clone();
            ConfigToControls();
            Modified = false;
        }


        private void FrmAlarmConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Modified)
            {
                DialogResult result = MessageBox.Show(ModPhrases.SaveModSettingsConfirm,
                    CommonPhrases.QuestionCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        if (!SaveConfig())
                            e.Cancel = true;
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!changing)
            {
                FrmAddAlarm dialog = new FrmAddAlarm(appDirs);

                dialog.SoundFilePath = lastPath;
                dialog.Channel = lastChannel + 1;

                while (true) // цикл, пока не получим условие для выхода
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (config.channels.ContainsKey(dialog.Channel))
                        {
                            switch (MessageBox.Show(msgAlreadyExists, msgWarning, MessageBoxButtons.YesNoCancel))
                            {
                                case DialogResult.No:
                                    continue;

                                case DialogResult.Cancel:
                                    return;

                                case DialogResult.Yes:
                                    config.channels.Remove(dialog.Channel);
                                    break;
                            }
                        }

                        if (config.AddChannel(dialog.Channel, dialog.SoundFilePath))
                        {
                            lastPath = dialog.SoundFilePath;
                            lastChannel = dialog.Channel;
                            Modified = true;
                            ConfigToControls();
                        }

                        return;
                    }
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (!changing)
            {
                foreach (ListViewItem item in inputChannels.SelectedItems)
                {
                    config.channels.Remove(Convert.ToInt32(item.SubItems[0].Text));
                    inputChannels.Items.Remove(item);

                    Modified = true;
                }
            }
        }
    }
}
