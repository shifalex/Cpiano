#if IOS
using UIKit;
using Microsoft.Maui.Platform;
#endif
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Platform;

namespace GestureSample.Maui
{
    public class CustomEntryHandler : EntryHandler
    {
#if IOS
        protected override MauiTextField CreatePlatformView()
        {
            // Get the platform view from the base implementation.
            var textField = base.CreatePlatformView();
            // Set the native keyboard to NumberPad (which doesn't include a return key by default)
            textField.KeyboardType = UIKeyboardType.NumberPad;

            // Create a UIToolbar to serve as an accessory view with a "Done" button.
            UIToolbar toolbar = new UIToolbar();
            toolbar.SizeToFit();

            // Create a flexible space item so the Done button is right-aligned.
            var flexSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            // Create a Done button that dismisses the keyboard.
            var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) =>
            {
                textField.ResignFirstResponder();
                textField.SendActionForControlEvents(UIControlEvent.EditingDidEndOnExit);
            });
            toolbar.SetItems(new UIBarButtonItem[] { flexSpace, doneButton }, true);
            // Assign the toolbar as the input accessory view.
            textField.InputAccessoryView = toolbar;

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