﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.WPF;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Views.Windows;
using Unity;

namespace SysadminsLV.Asn1Editor {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        static readonly Logger _logger = new Logger();
        public App() {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        public static IUnityContainer Container { get; private set; }

        void OnDispatcherUnhandledException(Object s, DispatcherUnhandledExceptionEventArgs e) {
            _logger.Write(e.Exception);
        }
        public static void Write(Exception e) {
            _logger.Write(e);
        }
        public static void Write(String s) {
            _logger.Write(s);
        }
        protected override void OnStartup(StartupEventArgs e) {
            _logger.Write("******************************** Started ********************************");
            _logger.Write($"Process: {Process.GetCurrentProcess().ProcessName}");
            _logger.Write($"PID    : {Process.GetCurrentProcess().Id}");
            _logger.Write($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
            configureUnity();
            readOids();
            parseArguments(e.Args);
            base.OnStartup(e);
            Container.Resolve<MainWindow>().Show();
        }
        protected override void OnExit(ExitEventArgs e) {
            _logger.Dispose();
            base.OnExit(e);
        }
        void readOids() {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!File.Exists(path + @"\OID.txt")) { return; }
            String[] strings = File.ReadAllLines(path + @"\OID.txt");
            foreach (String[] tokens in strings.Select(str => str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))) {
                try {
                    MainWindowVM.OIDs.Add(tokens[0], tokens[1].Trim());
                } catch { }
            }
        }
        void parseArguments(IReadOnlyList<String> args) {
            for (Int32 i = 0; i < args.Count;) {
                switch (args[i].ToLower()) {
                    case "-path":  // open from a file
                        i++;
                        if (args.Count <= i) {
                            throw new ArgumentException(args[i]);
                        }
                        Container.Resolve<IMainWindowVM>().OpenExisting(args[i]);
                        return;
                    case "-raw":  // base64 raw string
                        i++;
                        if (args.Count <= i) {
                            throw new ArgumentException(args[i]);
                        }
                        Container.Resolve<IMainWindowVM>().OpenRaw(args[i]);
                        return;
                    default:
                        Console.WriteLine($"Unknown parameter '{args[i]}'");
                        return;
                }
            }
        }
        void configureUnity() {
            Container = new UnityContainer();
            Container.RegisterType<MainWindow>();

            Container.RegisterType<IWindowFactory, WindowFactory>();
            Container.RegisterType<IAppCommands, AppCommands>();
            Container.RegisterType<ITreeCommands, TreeViewCommands>();
            Container.RegisterSingleton<IDataSource, DataSource>();
            Container.RegisterType<ITagDataEditor, TagDataEditor>();
            // view models
            Container.RegisterSingleton<IMainWindowVM, MainWindowVM>();
            Container.RegisterType<ITextViewerVM, TextViewerVM>();
            Container.RegisterType<ITreeViewVM, TreeViewVM>();
            Container.RegisterType<ITagDataEditorVM, TagDataEditorVM>();
        }

    }
}
