using Microsoft.AspNetCore.Components.Forms;
namespace SuggestionAppUI.Components;

public class MyInputRadioGroup<TValue> : InputRadioGroup<TValue>
{
    private string _name;
    private string _fieldClass;

    protected override void OnParametersSet()
    {
        var fieldClass = EditContext?.FieldCssClass(FieldIdentifier) ?? string.Empty;
        if (fieldClass == _fieldClass && Name == _name)
        {
            return;
        }

        _fieldClass = fieldClass;
        Name = _name;
        base.OnParametersSet();
    }
}
