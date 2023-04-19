using Avalonia.Platform;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AvaloniaVisualStudioTitleBar.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string AppName => "Avalonia.Visual Studio Title Bar Demo";

        /// <summary>
        /// Gets the modern style.
        /// </summary>
        /// <value>The modern style.</value>
        public ModernStyleDataModel ModernStyle { get; private set; } = new ModernStyleDataModel();

        public object[] Styles => Enum.GetValues(typeof(ModernStyleType)).Cast<object>().ToArray();

        public object Style
        {
            get => ModernStyle.Style;
            set => ModernStyle.Style = value != null ? (ModernStyleType)value :ModernStyleType.Default;
        }

    }


    #region Demo Support
    /// <summary>
    /// Enum ModernStyleType
    /// </summary>
    public enum ModernStyleType
    {
        /// <summary>
        /// The default
        /// </summary>
        Default,

        /// <summary>
        /// The windows metro
        /// </summary>
        WindowsMetro,

        /// <summary>
        /// The mac os
        /// </summary>
        MacOS,

        /// <summary>
        /// The classic
        /// </summary>
        Classic
    }

    #region Attributes    
    /// <summary>
    /// Class DependsOnPropertyAttribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependsOnPropertyAttribute : Attribute
    {
        /// <summary>
        /// The dependency properties
        /// </summary>
        public readonly string[] DependencyProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependsOnPropertyAttribute"/> class.
        /// </summary>
        /// <param name="propertyNames">The property names.</param>
        public DependsOnPropertyAttribute(params string[] propertyNames)
        {
            DependencyProperties = propertyNames;
        }
    }
    #endregion

    #region ReactiveObject
    /// <summary>
    /// Interface IReactiveObject
    /// Implements the <see cref="BluePrint.Common.ComponentModel.INotifyPropertyChanged" />
    /// Implements the <see cref="BluePrint.Common.ComponentModel.INotifyPropertyChanging" />
    /// </summary>
    /// <seealso cref="BluePrint.Common.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="BluePrint.Common.ComponentModel.INotifyPropertyChanging" />
    public interface IReactiveObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        void RaisePropertyChanged(string propertyName);
    }

    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>T[].</returns>
        public static T[] GetCustomAttributes<T>(this MemberInfo memberInfo, bool inherit = true)
            where T : Attribute
        {
            var Attrs = memberInfo.GetCustomAttributes(typeof(T), inherit);

            if (Attrs != null && Attrs.Length > 0)
            {
                return Attrs.Cast<T>().ToArray();
            }

            return new T[0];
        }
    }

    /// <summary>
    /// Class ReactiveObject.
    /// Implements the <see cref="BluePrint.Common.ComponentModel.INotifyPropertyChanged" />
    /// </summary>
    /// <seealso cref="BluePrint.Common.ComponentModel.INotifyPropertyChanged" />
    public class ReactiveObject : IReactiveObject
    {
        #region Properties        
        private Stack<string> ProcessStack = new Stack<string>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveObject"/> class.
        /// </summary>
        public ReactiveObject()
        {
            PropertyChanged += ProcessPropertyChanged;

            AutoCollectDependsInfo();
        }

        private static Dictionary<System.Type, Dictionary<string, List<string>>> MetaCaches = new Dictionary<Type, Dictionary<string, List<string>>>();

        /// <summary>
        /// Automatics the collect depends information.
        /// </summary>
        private void AutoCollectDependsInfo()
        {
            var type = GetType();
            if (MetaCaches.TryGetValue(type, out var cache))
            {
                return;
            }

            cache = new Dictionary<string, List<string>>();

            foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                foreach (var attr in property.GetCustomAttributes<DependsOnPropertyAttribute>())
                {
                    foreach (var name in attr.DependencyProperties)
                    {
                        if (!cache.TryGetValue(name, out var relevance))
                        {
                            relevance = new List<string>();
                            cache.Add(name, relevance);
                        }

                        relevance.Add(property.Name);
                    }
                }
            }

            if (cache.Count > 0)
            {
                MetaCaches[type] = cache;
            }
            else
            {
                MetaCaches[type] = null;
            }
        }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Dictionary&lt;System.String, List&lt;System.String&gt;&gt;.</returns>
        private static Dictionary<string, List<string>> GetCache(Type type)
        {
            MetaCaches.TryGetValue(type, out var cache);
            return cache;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Methods                        
        /// <summary>
        /// Processes the property changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void ProcessPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ProcessStack.Contains(e.PropertyName))
            {
                return;
            }

            var RelativeProperties = GetCache(GetType());

            if (RelativeProperties != null && RelativeProperties.TryGetValue(e.PropertyName, out var relevance))
            {
                try
                {
                    ProcessStack.Push(e.PropertyName);

                    foreach (var r in relevance)
                    {
                        RaisePropertyChanged(r);
                    }
                }
                finally
                {
                    ProcessStack.Pop();
                }
            }
        }

        #endregion
    }
    #endregion

    #region Extensions
    /// <summary>
    /// Class ReactiveObjectExtensions.
    /// </summary>
    public static class ReactiveObjectExtensions
    {
        /// <summary>
        /// Raises the and set if changed.
        /// </summary>
        /// <typeparam name="TObj">The type of the t object.</typeparam>
        /// <typeparam name="TRet">The type of the t ret.</typeparam>
        /// <param name="reactiveObject">The reactive object.</param>
        /// <param name="backingField">The backing field.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>TRet.</returns>
        public static TRet RaiseAndSetIfChanged<TObj, TRet>(this TObj reactiveObject,
            ref TRet backingField,
            TRet newValue,
            [CallerMemberName] string propertyName = null
            )
            where TObj : IReactiveObject
        {
            if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
            {
                return newValue;
            }

            backingField = newValue;
            reactiveObject.RaisePropertyChanged(propertyName);

            return newValue;
        }

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <typeparam name="TSender">The type of the t sender.</typeparam>
        /// <param name="reactiveObject">The reactive object.</param>
        /// <param name="propertyName">Name of the property.</param>
        public static void RaisePropertyChanged<TSender>(this TSender reactiveObject, [CallerMemberName] string propertyName = null)
            where TSender : IReactiveObject
        {
            if (propertyName is not null)
            {
                reactiveObject.RaisePropertyChanged(propertyName);
            }
        }
    }
    #endregion

    /// <summary>
    /// Class ModernStyleDataModel.
    /// Implements the <see cref="ReactiveObject" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    public class ModernStyleDataModel : ReactiveObject
    {
        bool ExtendClientAreaToDecorationsHintCore = false;
        /// <summary>
        /// Gets or sets a value indicating whether [extend client area to decorations hint].
        /// </summary>
        /// <value><c>true</c> if [extend client area to decorations hint]; otherwise, <c>false</c>.</value>
        public bool ExtendClientAreaToDecorationsHint
        {
            get => ExtendClientAreaToDecorationsHintCore;
            set => this.RaiseAndSetIfChanged(ref ExtendClientAreaToDecorationsHintCore, value);
        }

        int ExtendClientAreaTitleBarHeightHintCore = 0;
        /// <summary>
        /// Gets or sets the extend client area title bar height hint.
        /// </summary>
        /// <value>The extend client area title bar height hint.</value>
        public int ExtendClientAreaTitleBarHeightHint
        {
            get => ExtendClientAreaTitleBarHeightHintCore;
            set => this.RaiseAndSetIfChanged(ref ExtendClientAreaTitleBarHeightHintCore, value);
        }

        /// <summary>
        /// The extend client area chrome hints core
        /// </summary>
        ExtendClientAreaChromeHints ExtendClientAreaChromeHintsCore = ExtendClientAreaChromeHints.Default;

        /// <summary>
        /// Gets or sets the extend client area chrome hints.
        /// </summary>
        /// <value>The extend client area chrome hints.</value>
        public ExtendClientAreaChromeHints ExtendClientAreaChromeHints
        {
            get => ExtendClientAreaChromeHintsCore;
            set => this.RaiseAndSetIfChanged(ref ExtendClientAreaChromeHintsCore, value);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is windows style.
        /// </summary>
        /// <value><c>true</c> if this instance is windows style; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Style))]
        public bool IsWindowsStyle => Style == ModernStyleType.WindowsMetro || (Style == ModernStyleType.Default && OperatingSystem.IsWindows());
        /// <summary>
        /// Gets a value indicating whether this instance is mac os style.
        /// </summary>
        /// <value><c>true</c> if this instance is mac os style; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Style))]
        public bool IsMacOSStyle => Style == ModernStyleType.MacOS || (Style == ModernStyleType.Default && OperatingSystem.IsMacOS());

        /// <summary>
        /// Gets a value indicating whether this instance is classic style.
        /// </summary>
        /// <value><c>true</c> if this instance is classic style; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Style))]
        public bool IsClassicStyle => Style == ModernStyleType.Classic || (Style == ModernStyleType.Default && OperatingSystem.IsLinux());

        /// <summary>
        /// Gets a value indicating whether this instance is modern style.
        /// </summary>
        /// <value><c>true</c> if this instance is modern style; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Style))]
        public bool IsModernStyle => IsWindowsStyle || IsMacOSStyle;

        ModernStyleType StyleCore = ModernStyleType.Default;

        /// <summary>
        /// Gets or sets the style.
        /// </summary>
        /// <value>The style.</value>
        public ModernStyleType Style
        {
            get => StyleCore;
            set => this.RaiseAndSetIfChanged(ref StyleCore, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernStyleDataModel"/> class.
        /// </summary>
        public ModernStyleDataModel()
        {
            StyleCore = ApplicationSettings.Default.ModernStyle;

            this.PropertyChanged += OnPropertyChanged;

            // force reset values
            RaisePropertyChanged(nameof(Style));

            ApplicationSettings.Default.PropertyChanged += OnApplicationSettingChanged;
        }

        private void OnApplicationSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationSettings.Default.ModernStyle))
            {
                this.Style = ApplicationSettings.Default.ModernStyle;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Style))
            {
                if (IsMacOSStyle || IsWindowsStyle)
                {
                    ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                    ExtendClientAreaTitleBarHeightHint = -1;
                    ExtendClientAreaToDecorationsHint = true;
                }
                else
                {
                    ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                    ExtendClientAreaTitleBarHeightHint = 0;
                    ExtendClientAreaToDecorationsHint = false;
                }
            }
        }
    }
    public class ApplicationSettings : ReactiveObject
    {
        public static readonly ApplicationSettings Default = new ApplicationSettings();

        ModernStyleType _ModernStyle = ModernStyleType.Default;
        public ModernStyleType ModernStyle
        {
            get => _ModernStyle;
            set => this.RaiseAndSetIfChanged(ref _ModernStyle, value);
        }

        public void Save()
        {
            // todo...
        }
    }
    #endregion


}