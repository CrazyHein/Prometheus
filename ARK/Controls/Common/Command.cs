using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common
{
    internal class Command
    {
        public static RoutedUICommand New { get; private set; }
        public static RoutedUICommand Open { get; private set; }
        public static RoutedUICommand Save { get; private set; }
        public static RoutedUICommand SaveAs { get; private set; }
        public static RoutedUICommand Close { get; private set; }
        public static RoutedUICommand Quit { get; private set; }
        public static RoutedUICommand About { get; private set; }
        public static RoutedUICommand ApplyChanges { get; private set; }
        public static RoutedUICommand DiscardChanges { get; private set; }

        public static RoutedCommand Summary { get; private set; }

        public static RoutedUICommand Add { get; private set; }
        public static RoutedUICommand Edit { get; private set; }
        public static RoutedUICommand Remove { get; private set; }
        public static RoutedUICommand Insert { get; private set; }

        public static RoutedUICommand Copy { get; private set; }
        public static RoutedUICommand Paste { get; private set; }
        public static RoutedUICommand Paste2 { get; private set; }

        public static RoutedUICommand PasteBefore { get; private set; }
        public static RoutedUICommand Propagate { get; private set; }

        public static RoutedUICommand ExpressionHelper { get; private set; }

        public static RoutedUICommand SwitchControlBlockType { get; private set; }

        public static RoutedUICommand Build { get; private set; }

        static Command()
        {
            InputGestureCollection gestureNew = new InputGestureCollection
            {
                new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N")
            };
            New = new RoutedUICommand("New", "Create a New File", typeof(Command), gestureNew);

            InputGestureCollection gestureOpen = new InputGestureCollection
            {
                new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O")
            };
            Open = new RoutedUICommand("Open", "Open Existing File", typeof(Command), gestureOpen);

            InputGestureCollection gestureSave = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S")
            };
            Save = new RoutedUICommand("Save", "Save File", typeof(Command), gestureSave);

            InputGestureCollection gestureSaveAs = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Shift|ModifierKeys.Control, "Ctrl+Shift+S")
            };
            SaveAs = new RoutedUICommand("Save As", "Save File As", typeof(Command), gestureSaveAs);

            Close = new RoutedUICommand("Close", "Close File", typeof(Command));

            InputGestureCollection gestureQuit = new InputGestureCollection
            {
                new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl+Q")
            };
            Quit = new RoutedUICommand("Quit", "Quit the Application", typeof(Command), gestureQuit);

            InputGestureCollection gestureAbout = new InputGestureCollection
            {
                new KeyGesture(Key.F1)
            };
            About = new RoutedUICommand("About", "About", typeof(Command), gestureAbout);

            InputGestureCollection gestureApplyChanges = new InputGestureCollection
            {
                new KeyGesture(Key.F1, ModifierKeys.Control, "Ctrl+F1")
            };
            ApplyChanges = new RoutedUICommand("Apply Changes", "Apply All Changes", typeof(Command), gestureApplyChanges);

            InputGestureCollection gestureDiscardChanges = new InputGestureCollection
            {
                new KeyGesture(Key.F2, ModifierKeys.Control, "Ctrl+F2")
            };
            DiscardChanges = new RoutedUICommand("Discard Changes", "Discard All Changes", typeof(Command), gestureDiscardChanges);

            InputGestureCollection gestureSummary = new InputGestureCollection
            {
                new KeyGesture(Key.F5, ModifierKeys.Control, "Ctrl+F5")
            };
            Summary = new RoutedUICommand("Summary", "Display Summary", typeof(Command), gestureSummary);

            Add = new RoutedUICommand("Add", "Add", typeof(Command));
            Edit = new RoutedUICommand("Edit", "Edit", typeof(Command));
            Remove = new RoutedUICommand("Remove", "Remove", typeof(Command));
            Insert = new RoutedUICommand("Insert", "Insert", typeof(Command));
            PasteBefore = new RoutedUICommand("Paste Before", "Insert Object Found in Clipboard", typeof(Command));
            Propagate = new RoutedUICommand("Propagate", "Propagate", typeof(Command));

            InputGestureCollection gestureCopy = new InputGestureCollection
            {
                new KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C")
            };
            Copy = new RoutedUICommand("Copy", "Copy", typeof(Command), gestureCopy);

            InputGestureCollection gesturePaste = new InputGestureCollection
            {
                new KeyGesture(Key.V, ModifierKeys.Control, "Ctrl+V")
            };
            Paste = new RoutedUICommand("Paste", "Paste", typeof(Command), gesturePaste);

            InputGestureCollection gesturePaste2 = new InputGestureCollection
            {
                new KeyGesture(Key.B, ModifierKeys.Control, "Ctrl+B")
            };
            Paste2 = new RoutedUICommand("Paste", "Paste", typeof(Command), gesturePaste2);

            ExpressionHelper = new RoutedUICommand("Expression Helper", "Open Expression Helper Dialog", typeof(Command));

            SwitchControlBlockType = new RoutedUICommand("Control Block Type", "Switch Control Block Type", typeof(Command));

            InputGestureCollection gestureBuild = new InputGestureCollection
            {
                new KeyGesture(Key.F5)
            };
            Build = new RoutedUICommand("Build", "Build Script", typeof(Command), gestureBuild);
        }
    }
}
