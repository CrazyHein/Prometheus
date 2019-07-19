using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    class IOListDataControlCommand
    {
        public static RoutedUICommand AddElement { get; private set; }
        public static RoutedUICommand RemoveElement { get; private set; }
        public static RoutedUICommand EditElement { get; private set; }
        public static RoutedUICommand MoveElementUp { get; private set; }
        public static RoutedUICommand MoveElementDown { get; private set; }
        public static RoutedUICommand InsertElementBefore { get; private set; }
        public static RoutedUICommand ReplaceElement { get; private set; }

        static IOListDataControlCommand()
        {
            InputGestureCollection gestureAddElement = new InputGestureCollection
            {
                new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P")
            };
            InputGestureCollection gestureRemoveElement = new InputGestureCollection
            {
                new KeyGesture(Key.R, ModifierKeys.Control, "Ctrl+R")
            };
            InputGestureCollection gestureEditElement = new InputGestureCollection
            {
                new KeyGesture(Key.M, ModifierKeys.Control, "Ctrl+E")
            };
            InputGestureCollection gesturMoveElementUp = new InputGestureCollection
            {
                new KeyGesture(Key.U, ModifierKeys.Control, "Ctrl+U")
            };
            InputGestureCollection gestureMoveElementDown = new InputGestureCollection
            {
                new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D")
            };
            InputGestureCollection gesturInsertElementBefore = new InputGestureCollection
            {
                new KeyGesture(Key.I, ModifierKeys.Control, "Ctrl+I")
            };
            InputGestureCollection gesturReplaceElementElement = new InputGestureCollection
            {
                new KeyGesture(Key.M, ModifierKeys.Control, "Ctrl+M")
            };

            AddElement = new RoutedUICommand("Add", "Add an element to the last position", typeof(IOListDataControlCommand), gestureAddElement);
            RemoveElement = new RoutedUICommand("Remove", "Remove the selected element", typeof(IOListDataControlCommand), gestureRemoveElement);
            EditElement = new RoutedUICommand("Edit", "Edit the selected element", typeof(IOListDataControlCommand), gestureEditElement);

            MoveElementUp = new RoutedUICommand("Move Up", "Move up the selected element", typeof(IOListDataControlCommand), gesturMoveElementUp);
            MoveElementDown = new RoutedUICommand("Move Down", "Move down the selected element", typeof(IOListDataControlCommand), gestureMoveElementDown);
            InsertElementBefore = new RoutedUICommand("InsertBefore", "Insert an element Before the selected one", typeof(IOListDataControlCommand), gesturInsertElementBefore);
            ReplaceElement = new RoutedUICommand("Replace", "Replace the element with the selected one", typeof(IOListDataControlCommand), gesturReplaceElementElement);
        }
    }
}
