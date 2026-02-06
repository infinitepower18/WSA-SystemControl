namespace WSA_System_Control
{
    public class AndroidApp
    {
        public string PackageName { get; set; }
        public string AppName { get; set; }
        public Image? AppIcon { get; set; }

        public AndroidApp(string packageName, string appName = null, Image? appIcon = null)
        {
            PackageName = packageName;
            AppName = appName ?? packageName;
            AppIcon = appIcon;
        }

        public override string ToString() => AppName;
    }
}
