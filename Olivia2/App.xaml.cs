using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Globalization;

using Acly.Olivia2.Model;
using Acly.Olivia2.Persistence;

namespace Acly.Olivia2 {

    public partial class App : Application {

        public App() {
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

		protected override void OnLoadCompleted(System.Windows.Navigation.NavigationEventArgs e) {
			base.OnLoadCompleted(e);
		}

		protected override void OnStartup(StartupEventArgs e) {
			EventManager.RegisterClassHandler(typeof(TextBox),
				  TextBox.GotFocusEvent,
				  new RoutedEventHandler(TextBox_GotFocus));
			base.OnStartup(e);
		}

		void TextBox_GotFocus(object sender, RoutedEventArgs e) {
			(sender as TextBox).SelectAll();
		}

		protected override void  OnExit(ExitEventArgs e) {
			var serializer = (Serializer)FindResource("Serializer");
			serializer.Shutdown(); // Wait in case there is a save in progress
			while ( serializer.IsRunning ) Thread.Sleep(0);
			base.OnExit(e);
		}
    }
}
