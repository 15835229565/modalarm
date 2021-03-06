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
 * Summary  : Server module user interface
 * 
 * Author   : Alexandr Kolodkin
 * Created  : 2017
 * Modified : 2018
 */

using Scada.Server.Modules.Alarm;

namespace Scada.Server.Modules
{
    /// <summary>
    /// Server module user interface
    /// <para>Пользовательский интерфейс серверного модуля</para>
    /// </summary>
    public class ModAlarmView : ModView
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public ModAlarmView()
        {
            CanShowProps = true;
        }

        /// <summary>
        /// Получить описание модуля
        /// </summary>
        public override string Descr
        {
            get
            {
                return Localization.UseRussian ? "Серверный модуль для звуковой сигнализации." : "Server module for the sound alarm.";
            }
        }

        /// <summary>
        /// Отобразить свойства модуля
        /// </summary>
        public override void ShowProps()
        {
            FrmAlarmConfig.ShowDialog(AppDirs, ServerComm);
        }
    }
}
