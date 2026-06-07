using ASOIU_3.Data;

namespace ASOIU_3.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        DatabaseInitializer.Initialize();
        Application.Run(new MainForm());
    }
}
