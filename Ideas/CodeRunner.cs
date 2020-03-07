namespace Fluid.Ideas {

class A {
   string Text { get; set; }

   internal A(string? text) {
      Text = text;
   }
}

void Main() {
   var a = new A(null);
}

}

