using SkiaSharp.Views.Maui.Controls.Hosting;

namespace DellikCorp.Libs.ColorPicker
{
    public static class DIExtensions
    {
        public static MauiAppBuilder UseColorPicker(this MauiAppBuilder builder)
        {
            builder.UseSkiaSharp();
            return builder;
        }
    }
}
