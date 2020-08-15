using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Html;

namespace Fluid {
public class MainWindow : Window {

   public MainWindow() {
      InitializeComponent();
   }

   private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
   }
   
}
}