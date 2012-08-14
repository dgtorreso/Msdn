using System;

namespace Formulario
{
	public partial class Ventana : Gtk.Window
	{
		public Ventana () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}
	}
}

