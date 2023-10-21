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
        /// <summary>
        /// ApplicationCommands.SaveAs has no input gestures. 
        /// </summary>
        public readonly static RoutedCommand SaveAsShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift) }
        };

        public readonly static RoutedCommand ExportAsShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control) }
        };

        public readonly static RoutedCommand CopyToClipboardShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift) }
        };

        public readonly static RoutedCommand ImportFileShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift) }
        };
    }

    public static class EditShortcutCommands
    {
        public readonly static RoutedCommand SelectLayerShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Alt) }
        };

        public readonly static RoutedCommand CancelSelectionShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.A, ModifierKeys.Shift | ModifierKeys.Control) }
        };

        public readonly static RoutedCommand CropArtShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.Enter, ModifierKeys.Control) }
        };

        public readonly static RoutedCommand CropLayerShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.Enter, ModifierKeys.Control | ModifierKeys.Shift) }
        };


    }

    public static class DrawShortcutCommands
    {
        public readonly static RoutedCommand FillSelectionShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.F, ModifierKeys.Alt) }
        };

    }

    public static class CanvasShortcutCommands
    {
        public readonly static RoutedCommand EnlargeTextSizeShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.I, ModifierKeys.Alt) }
        };

        public readonly static RoutedCommand ShrinkTextSizeShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.O, ModifierKeys.Alt) }
        };

        public readonly static RoutedCommand ResetTextSizeShortcut = new()
        {
            InputGestures = { new KeyGesture(Key.P, ModifierKeys.Alt) }
        };

        public readonly static RoutedCommand DrawToolShortCut = new()
        {
            InputGestures = { new KeyGesture(Key.D, ModifierKeys.Control | ModifierKeys.Shift) }
        };

        public readonly static RoutedCommand EraserToolShortCut = new()
        {
            InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Shift) }
        };

        public readonly static RoutedCommand SelectToolShortCut = new()
        {
            InputGestures = { new KeyGesture(Key.W, ModifierKeys.Control | ModifierKeys.Shift) }
        };

        public readonly static RoutedCommand MoveToolShortCut = new()
        {
            InputGestures = { new KeyGesture(Key.M, ModifierKeys.Control | ModifierKeys.Shift) }
        };

        public readonly static RoutedCommand LineToolShortCut = new()
        {
            InputGestures = { new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift) }
        };

        public readonly static RoutedCommand BucketToolShortCut = new()
        {
            InputGestures = { new KeyGesture(Key.B, ModifierKeys.Control | ModifierKeys.Shift) }
        };

        public readonly static RoutedCommand TextToolShortCut = new()
        {
            InputGestures = { new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift) }
        };

    }
}
