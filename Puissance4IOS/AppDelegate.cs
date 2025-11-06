using Foundation;
using UIKit;

namespace Puissance4IOS;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        Window = new UIWindow(UIScreen.MainScreen.Bounds)
        {
            RootViewController = new GameViewController()
        };
        Window.MakeKeyAndVisible();

        return true;
    }
}
