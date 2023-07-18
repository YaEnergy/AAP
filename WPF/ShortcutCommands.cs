using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP.UI
{
    public static class FileShortcutCommands
    {
        public static RoutedCommand ExportAsShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control) }
        };

        public static RoutedCommand CopyToClipboardShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift) }
        };
    }

    public static class EditShortcutCommands
    {
        public static RoutedCommand SelectLayerShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.Enter, ModifierKeys.Control | ModifierKeys.Alt) }
        };

        public static RoutedCommand CancelSelectionShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.A, ModifierKeys.Shift | ModifierKeys.Control) }
        };

        public static RoutedCommand CropArtShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.Enter, ModifierKeys.Control) }
        };

        public static RoutedCommand CropLayerShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.Enter, ModifierKeys.Control | ModifierKeys.Shift) }
        };

    }

    public static class CanvasShortcutCommands
    {
        public static RoutedCommand EnlargeTextSizeShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.I, ModifierKeys.Alt) }
        };

        public static RoutedCommand ShrinkTextSizeShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.O, ModifierKeys.Alt) }
        };

        public static RoutedCommand ResetTextSizeShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.P, ModifierKeys.Alt) }
        };
    }
}
