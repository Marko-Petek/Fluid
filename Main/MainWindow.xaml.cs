using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Html;

namespace Fluid {
public class MainWindow : Window {

   private HtmlControl HtmlCon { get; set; }
   public MainWindow() {
      InitializeComponent();
   }

   private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
      HtmlCon = this.FindControl<HtmlControl>("HtmlCon");
      HtmlCon.Text = "<p>A Markdown string with <strong>emphasis</strong>.</p>";
   }
   
}
}