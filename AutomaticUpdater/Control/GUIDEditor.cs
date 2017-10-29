using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

internal class GUIDEditor : UITypeEditor
{
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
        if (context != null)
        {
            return UITypeEditorEditStyle.Modal;
        }
        return base.GetEditStyle(context);
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
        if ((context != null) && (provider != null))
        {
            // Access the Property Browser's UI display service
            IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null)
            {
                DialogResult dr = DialogResult.Yes;
                // Pass the UI editor dialog the current property value
                if (value != null)
                {
                    dr = MessageBox.Show(
                        "Are you sure you want to overwrite the existing GUID? (You should only do this if you copied the AutomaticUpdate control for a new separate application.)",
                        "Overwrite existing GUID?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                }

                if (dr == DialogResult.Yes)
                    return Guid.NewGuid().ToString();
            }
        }

        return base.EditValue(context, provider, value);
    }
}