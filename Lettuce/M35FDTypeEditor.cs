using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tomato.Hardware;

namespace Lettuce
{
    public class M35FDTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context.Instance.GetType() != typeof(M35FD))
                return base.GetEditStyle(context);
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context.Instance.GetType() != typeof(M35FD))
                return base.EditValue(provider, value);
            Form window = Lettuce.Program.Windows[context.Instance as Device];
            window.Show();
            window.BringToFront();
            window.Focus();
            return value;
        }
    }
}
