#if IOS
using UIKit;
using Microsoft.Maui.Platform;
#endif
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;

namespace GestureSample.Maui
{
    public class CustomEntryHandler : EntryHandler
    {
#if IOS
private bool _hasSentCompleted = false;
        protected override MauiTextField CreatePlatformView()
        {
            var textField = base.CreatePlatformView();
            
           if (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad)
    {
        textField.KeyboardType = UIKeyboardType.NumberPad;
        textField.ReturnKeyType = UIReturnKeyType.Done;
        
        textField.ShouldReturn = (tf) =>
        {
            if (!_hasSentCompleted && VirtualView is IEntryController entryController)
            {
                _hasSentCompleted = true;
                entryController.SendCompleted();
            }
            tf.ResignFirstResponder();
            return true;
        };

        UIToolbar toolbar = new UIToolbar();
        toolbar.SizeToFit();

        var flexSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
        var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) =>
        {
            if (!_hasSentCompleted && VirtualView is IEntryController entryController)
            {
                _hasSentCompleted = true;
                entryController.SendCompleted();
            }
            textField.ResignFirstResponder();
        });
        toolbar.SetItems(new UIBarButtonItem[] { flexSpace, doneButton }, true);
        textField.InputAccessoryView = toolbar;
    }

    // Reset flag when focus is lost, so that subsequent completions can occur.
    textField.EditingDidEnd += (s, e) =>
    {
        _hasSentCompleted = false;
    };

            return textField;
        }
#elif ANDROID
        protected override void ConnectHandler(AndroidX.AppCompat.Widget.AppCompatEditText nativeView)
        {
            base.ConnectHandler(nativeView);

            var shape = new Android.Graphics.Drawables.GradientDrawable();
            shape.SetCornerRadius(20); // Adjust as needed
            shape.SetStroke(2, Android.Graphics.Color.Black); // Border thickness and color
            shape.SetColor(Android.Graphics.Color.White); // Background color

            // Remove the underline by setting the background to our custom shape.
            nativeView.Background = shape;
            nativeView.SetPadding(20, 20, 20, 20);
        }
#endif
    }
}