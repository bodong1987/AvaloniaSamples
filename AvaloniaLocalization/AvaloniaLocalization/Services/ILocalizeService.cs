using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaLocalization.Services
{
    /// <summary>
    /// Class CultureInfoData.
    /// </summary>
    public class CultureInfoData
    {
        /// <summary>
        /// The culture information
        /// </summary>
        public readonly CultureInfo CultureInfo;

        /// <summary>
        /// The path
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Initializes a new instance of the <see cref="CultureInfoData" /> class.
        /// </summary>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="path">The path.</param>
        public CultureInfoData(CultureInfo cultureInfo, string path)
        {
            CultureInfo = cultureInfo;
            Path = path;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return CultureInfo.NativeName;
        }
    }

    public interface ILocalizeService : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the available cultures.
        /// </summary>
        /// <value>The available cultures.</value>
        List<CultureInfoData> AvailableCultures { get; }

        /// <summary>
        /// Gets or sets the selected culture.
        /// </summary>
        /// <value>The selected culture.</value>
        CultureInfoData SelectedCulture { get; set; }

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.String.</returns>
        string this[string key] { get; }

        /// <summary>
        /// Occurs when [on culture changed].
        /// </summary>
        event EventHandler OnCultureChanged;
    }
}
