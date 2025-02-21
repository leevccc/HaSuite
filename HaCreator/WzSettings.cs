﻿/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01

 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Color = Microsoft.Xna.Framework.Color;

namespace HaCreator {
	public class WzSettingsManager {
		private readonly string settingFilePath;

		private readonly Type userSettingsType;
		private readonly Type appSettingsType;

		private const string USER_SETTING_JSON = "UserSettings";
		private const string APP_SETTING_JSON = "ApplicationSettings";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wzPath"></param>
		/// <param name="userSettingsType"></param>
		/// <param name="appSettingsType"></param>
		public WzSettingsManager(string wzPath, Type userSettingsType, Type appSettingsType) {
			settingFilePath = wzPath;
			this.userSettingsType = userSettingsType;
			this.appSettingsType = appSettingsType;
		}

		#region Loading

		/// <summary>
		/// Load UserSettings and ApplicationSettings
		/// </summary>
		public void LoadSettings() {
			if (File.Exists(settingFilePath)) {
				var strJsonConfig = File.ReadAllText(settingFilePath);

				try {
					var mainJson = (JObject) JsonConvert.DeserializeObject(strJsonConfig);

					LoadSettingsJson((JObject) mainJson[USER_SETTING_JSON], userSettingsType);
					LoadSettingsJson((JObject) mainJson[APP_SETTING_JSON], appSettingsType);
				} catch {
					// its fine if loading isnt possible
					// fallback to default
				}
			}
			// do nothing, default setting is in the value as specified in WzSettings.cs
		}

		/// <summary>
		/// Load settings json
		/// </summary>
		/// <param name="json"></param>
		/// <param name="settingsHolderType"></param>
		private void LoadSettingsJson(JObject json, Type settingsHolderType) {
			foreach (var fieldInfo in settingsHolderType.GetFields(BindingFlags.Public | BindingFlags.Static)) {
				var fieldName = fieldInfo.Name;
				var jsonHoldingObject = (JObject) json[fieldName];

				LoadField(fieldInfo, jsonHoldingObject);
			}
		}

		/// <summary>
		/// Loads the individual field into object
		/// </summary>
		/// <param name="fieldInfo"></param>
		/// <param name="jsonHoldingObject"></param>
		/// <exception cref="Exception"></exception>
		private void LoadField(FieldInfo fieldInfo, JObject jsonHoldingObject) {
			if (jsonHoldingObject == null) {
				// does nothing to this field if json does not contain anything
				// fallback to default as specified in WzSettings.json
			} else if (fieldInfo.FieldType.BaseType != null && fieldInfo.FieldType.BaseType.FullName == "System.Enum") {
				fieldInfo.SetValue(null, (int) jsonHoldingObject["value"]);
			} else {
				var fieldType = (string) jsonHoldingObject["type"];

				switch (fieldType) {
					case "Microsoft.Xna.Framework.Color": {
						var value = (uint) jsonHoldingObject["value"];
						var xnaColor = new Color(value);

						fieldInfo.SetValue(null, xnaColor);
						break;
					}
					case "System.Drawing.Color": {
						var value = (int) jsonHoldingObject["value"];
						var color = System.Drawing.Color.FromArgb(value);

						fieldInfo.SetValue(null, color);
						break;
					}
					case "System.Int32": {
						var value = (int) jsonHoldingObject["value"];

						fieldInfo.SetValue(null, value);
						break;
					}
					case "System.Double": {
						var value = (double) jsonHoldingObject["value"];

						fieldInfo.SetValue(null, value);
						break;
					}
					case "System.Single": {
						var value = (float) jsonHoldingObject["value"];

						fieldInfo.SetValue(null, value);
						break;
					}
					case "System.Drawing.Size": {
						var valueHeight = (int) jsonHoldingObject["valueHeight"];
						var valueWidth = (int) jsonHoldingObject["valueWidth"];

						var size = new Size(valueHeight, valueWidth);

						fieldInfo.SetValue(null, size);
						break;
					}
					case "System.String": {
						var value = (string) jsonHoldingObject["value"];

						fieldInfo.SetValue(null, value);
						break;
					}
					case "System.Drawing.Bitmap": {
						var base64Image = (string) jsonHoldingObject["value"];
						var byteImage = Convert.FromBase64String(base64Image);

						Bitmap bmp;
						using (var ms = new MemoryStream(byteImage)) {
							bmp = new Bitmap(ms);
						}

						fieldInfo.SetValue(null, bmp);
						break;
					}
					case "System.Boolean": {
						var value = (bool) jsonHoldingObject["value"];

						fieldInfo.SetValue(null, value);
						break;
					}
					default: {
						throw new Exception("Unsupported data type for WzSettings.");
					}
				}
			}
		}

		#endregion

		#region Saving

		public void SaveSettings() {
			var userSettingJson = SaveSettingsJson(userSettingsType);
			var appSettingJson = SaveSettingsJson(appSettingsType);

			var mainJson = new JObject();
			mainJson.Add(USER_SETTING_JSON, userSettingJson);
			mainJson.Add(APP_SETTING_JSON, appSettingJson);

			var settingsExist = File.Exists(settingFilePath);
			if (settingsExist) {
				File.Delete(settingFilePath);
			}

			using (var file = File.CreateText(settingFilePath)) {
				var serializer = new JsonSerializer();
				serializer.Serialize(file, mainJson);
			}
		}

		/// <summary>
		/// Saves the settings image to json
		/// </summary>
		/// <param name="settingsHolderType"></param>
		private JObject SaveSettingsJson(Type settingsHolderType) {
			var jObjHolder = new JObject();

			foreach (var fieldInfo in settingsHolderType.GetFields(BindingFlags.Public | BindingFlags.Static)) {
				SaveField(fieldInfo, jObjHolder);
			}

			return jObjHolder;
		}

		/// <summary>
		/// Saves the individual field into json object
		/// </summary>
		/// <param name="fieldInfo"></param>
		/// <param name="jObjHolder"></param>
		/// <exception cref="Exception"></exception>
		private void SaveField(FieldInfo fieldInfo, JObject jObjHolder) {
			var settingName = fieldInfo.Name;

			var fieldJsonObject = new JObject();
			jObjHolder.Add(settingName, fieldJsonObject);

			fieldJsonObject.Add("type", fieldInfo.FieldType.FullName); // i.e System.Int

			if (fieldInfo.FieldType.BaseType != null && fieldInfo.FieldType.BaseType.FullName == "System.Enum") {
				fieldJsonObject.Add("value", (int) fieldInfo.GetValue(null));
			} else {
				switch (fieldInfo.FieldType.FullName) {
					//case "Microsoft.Xna.Framework.Graphics.Color":
					case "Microsoft.Xna.Framework.Color": {
						var xnaColor =
							(Color) fieldInfo.GetValue(null);
						var uValue = xnaColor.PackedValue;

						fieldJsonObject.Add("value", uValue);
						break;
					}
					case "System.Drawing.Color": {
						var argbColor = ((System.Drawing.Color) fieldInfo.GetValue(null)).ToArgb();

						fieldJsonObject.Add("value", argbColor);
						break;
					}
					case "System.Int32": {
						var intVal = (int) fieldInfo.GetValue(null);

						fieldJsonObject.Add("value", intVal);
						break;
					}
					case "System.Double": {
						var dValue = (double) fieldInfo.GetValue(null);

						fieldJsonObject.Add("value", dValue);
						break;
					}
					case "System.Single": {
						var fValue = (float) fieldInfo.GetValue(null);

						fieldJsonObject.Add("value", fValue);
						break;
					}
					case "System.Drawing.Size": {
						var size = (Size) fieldInfo.GetValue(null);

						fieldJsonObject.Add("valueHeight", size.Height);
						fieldJsonObject.Add("valueWidth", size.Width);
						break;
					}
					case "System.String": {
						var strValue = (string) fieldInfo.GetValue(null);

						fieldJsonObject.Add("value", strValue);
						;
						break;
					}
					case "System.Drawing.Bitmap": {
						var bitmap = (Bitmap) fieldInfo.GetValue(null);

						var converter = new ImageConverter();
						var byteImage = (byte[]) converter.ConvertTo(bitmap, typeof(byte[]));
						var base64Image = Convert.ToBase64String(byteImage, 0, byteImage.Length,
							Base64FormattingOptions.None);

						fieldJsonObject.Add("value", base64Image);
						break;
					}
					case "System.Boolean": {
						var bValue = (bool) fieldInfo.GetValue(null);

						fieldJsonObject.Add("value", bValue);
						break;
					}
					default: {
						throw new Exception("Unsupported data type for WzSettings.");
					}
				}
			}
		}

		#endregion
	}
}