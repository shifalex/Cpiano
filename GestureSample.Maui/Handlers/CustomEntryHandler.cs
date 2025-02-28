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
            var textField = base.CreatePlatformView();
            // Set the keyboard type to NumberPad and ReturnKeyType to Done.
            textField.KeyboardType = UIKeyboardType.NumberPad;
            textField.ReturnKeyType = UIReturnKeyType.Done;

            // When the Return key is pressed, trigger the Completed event.
            textField.ShouldReturn = (tf) =>
            {
                if (VirtualView is IEntryController entryController)
                {
                    entryController.SendCompleted();
                }
                tf.ResignFirstResponder();
                return true;
            };

            // Only add the accessory toolbar on devices that are not iPads.
            if (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad)
            {
                UIToolbar toolbar = new UIToolbar();
                toolbar.SizeToFit();

                var flexSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
                var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) =>
                {
                    if (VirtualView is IEntryController entryController)
                    {
                        entryController.SendCompleted();
                    }
                    textField.ResignFirstResponder();
                });
                toolbar.SetItems(new UIBarButtonItem[] { flexSpace, doneButton }, true);
                textField.InputAccessoryView = toolbar;
            }
            else
            {
                // On iPad, no accessory toolbar is needed.
                textField.InputAccessoryView = null;
            }

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