using Avalonia.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaLocalization.Services
{
    internal class LocalizeService : ILocalizeService
    {
        #region Properties
        public string CultureName { get; private set; }
        public string Language => CultureName;

        /// <summary>
        /// The local texts
        /// </summary>
        private Dictionary<string, string> LocalTexts = null;
        #endregion

        #region Interfaces
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when [on culture changed].
        /// </summary>
        public event EventHandler OnCultureChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Localization
        public string this[string key]
        {
            get
            {
                if (LocalTexts != null && LocalTexts.TryGetValue(key, out var text))
                {
                    return text;
                }

                return key;
            }
        }

        /// <summary>
        /// Gets the available cultures.
        /// </summary>
        /// <value>The available cultures.</value>
        public List<CultureInfoData> AvailableCultures { get; private set; } = new List<CultureInfoData>();

        CultureInfoData SelectedCultureCore;
        /// <summary>
        /// Gets or sets the selected culture.
        /// </summary>
        /// <value>The selected culture.</value>
        public CultureInfoData SelectedCulture
        {
            get => SelectedCultureCore;
            set
            {
                if(SelectedCultureCore != value)
                {
                    SelectedCultureCore = value;

                    OnSelectCultureChanged();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizeService"/> class.
        /// </summary>
        public LocalizeService()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(assemblyPath);

            var locDirectory = Path.Combine(directory, "assets/localization");
            CultureInfoData currentInfo = null;

            if (Directory.Exists(locDirectory))
            {
                var files = Directory.GetFiles(locDirectory, "*.json", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var name = Path.GetFileNameWithoutExtension(file);

                    var cultureInfo = CultureInfo.GetCultureInfo(name);

                    if (cultureInfo != null)
                    {
                        var info = new CultureInfoData(cultureInfo, file);

                        if (cultureInfo.Name == CultureInfo.CurrentCulture.Name)
                        {
                            currentInfo = info;
                        }

                        AvailableCultures.Add(info);
                    }
                }
            }

            if(currentInfo != null)
            {
                SelectedCulture = currentInfo;
            }
        }

        private void OnSelectCultureChanged()
        {
            if (SelectedCulture == null)
            {
                LocalTexts = null;
                PostReload();
                return;
            }

            try
            {
                LoadAllSources(SelectedCulture.Path);

                PostReload();
            }
            catch (Exception ee)
            {
                Debug.WriteLine(string.Format("Failed load localization file :{0}\n{1}\n{2}", SelectedCulture.Path, ee.Message, ee.StackTrace));
            }
        }

        private void LoadAllSources(string appLanguageFile)
        {
            LocalTexts = new Dictionary<string, string>();

            List<string> pathes = new List<string>()
                {
                    appLanguageFile
                };

            // add additional path here if you need
            foreach (var plugin in new string[] {})
            {
                var p = Path.Combine(plugin, "assets/localization", Path.GetFileName(appLanguageFile));

                if (File.Exists(p))
                {
                    pathes.Add(p);
                }
            }

            foreach (var path in pathes)
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        var tempDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());

                        if (tempDict != null)
                        {
                            foreach (var pair in tempDict)
                            {
                                if (!LocalTexts.ContainsKey(pair.Key))
                                {
                                    LocalTexts.Add(pair.Key, pair.Value);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PostReload()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            OnCultureChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
