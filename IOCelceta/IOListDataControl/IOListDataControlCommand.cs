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
        public static RoutedUICommand ModifyElement { get; private set; }
        public static RoutedUICommand MoveElementUp { get; private set; }
        public static RoutedUICommand MoveElementDown { get; private set; }
        public static RoutedUICommand InsertElementBefore { get; private set; }

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
            InputGestureCollection gestureModifyElement = new InputGestureCollection
            {
                new KeyGesture(Key.M, ModifierKeys.Control, "Ctrl+M")
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

            AddElement = new RoutedUICommand("Add", "Add", typeof(IOListDataControlCommand), gestureAddElement);
            RemoveElement = new RoutedUICommand("Remove", "Remove", typeof(IOListDataControlCommand), gestureRemoveElement);
            ModifyElement = new RoutedUICommand("Modify", "Modify", typeof(IOListDataControlCommand), gestureModifyElement);

            MoveElementUp = new RoutedUICommand("Move Up", "MoveUp", typeof(IOListDataControlCommand), gesturMoveElementUp);
            MoveElementDown = new RoutedUICommand("Move Down", "MoveDown", typeof(IOListDataControlCommand), gestureMoveElementDown);
            InsertElementBefore = new RoutedUICommand("Insert Before", "InsertBefore", typeof(IOListDataControlCommand), gesturInsertElementBefore);
        }
    }
}
